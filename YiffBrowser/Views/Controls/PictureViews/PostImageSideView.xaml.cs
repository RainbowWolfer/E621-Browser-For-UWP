using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using YiffBrowser.Helpers;
using YiffBrowser.Models.E621;
using YiffBrowser.Services.Networks;
using YiffBrowser.Views.Pages.E621;

namespace YiffBrowser.Views.Controls.PictureViews {
	public sealed partial class PostImageSideView : UserControl {

		public E621Post E621Post {
			get => (E621Post)GetValue(E621PostProperty);
			set => SetValue(E621PostProperty, value);
		}

		public static readonly DependencyProperty E621PostProperty = DependencyProperty.Register(
			nameof(E621Post),
			typeof(E621Post),
			typeof(PostImageSideView),
			new PropertyMetadata(null)
		);



		public bool IsOverlayCheck {
			get => (bool)GetValue(IsOverlayCheckProperty);
			set => SetValue(IsOverlayCheckProperty, value);
		}

		public static readonly DependencyProperty IsOverlayCheckProperty = DependencyProperty.Register(
			nameof(IsOverlayCheck),
			typeof(bool),
			typeof(PostImageSideView),
			new PropertyMetadata(true)
		);



		public PostImageSideView() {
			this.InitializeComponent();
		}

		private void ImageViewItem_ImageClick(ImageViewItem sender, ImageViewItemViewModel args) {
			ViewModel.RelationsCommand?.Execute(null);
		}

		private void ImageBrush_ImageOpened(object sender, RoutedEventArgs e) {
			ImageBrush image = sender as ImageBrush;
			BitmapImage bitmap = image.ImageSource as BitmapImage;
			bitmap.DecodePixelType = DecodePixelType.Logical;
			bitmap.DecodePixelHeight = 120;
			bitmap.DecodePixelWidth = 120;
		}
	}


	public class PostImageSideViewModel : BindableBase {
		private E621Post e621Post;

		private string[] sourceURLs;
		private string description = "No Description";
		private string sourceTitle;

		private bool isLoadingComments;

		private E621Post[] childrenPost;
		private E621Post parentPost;
		private PostPoolItem[] poolItems;

		public E621Post E621Post {
			get => e621Post;
			set => SetProperty(ref e621Post, value, OnPostChanged);
		}

		public ObservableCollection<CommentItem> CommentItems { get; } = new();

		public PostPoolItem[] PoolItems {
			get => poolItems;
			set => SetProperty(ref poolItems, value);
		}
		public E621Post ParentPost {
			get => parentPost;
			set => SetProperty(ref parentPost, value);
		}
		public E621Post[] ChildrenPost {
			get => childrenPost;
			set => SetProperty(ref childrenPost, value);
		}

		public string Description {
			get => description;
			set => SetProperty(ref description, value);
		}

		public string[] SourceURLs {
			get => sourceURLs;
			set => SetProperty(ref sourceURLs, value);
		}

		public string SourceTitle {
			get => sourceTitle;
			set => SetProperty(ref sourceTitle, value);
		}

		public bool IsLoadingComments {
			get => isLoadingComments;
			set => SetProperty(ref isLoadingComments, value);
		}

		public bool IsLoadingParent {
			get => isLoadingParent;
			set => SetProperty(ref isLoadingParent, value);
		}
		public bool IsLoadingChildren {
			get => isLoadingChildren;
			set => SetProperty(ref isLoadingChildren, value);
		}

		public bool CommentOrderCheck {
			get => commentOrderCheck;
			set => SetProperty(ref commentOrderCheck, value, OnCommentOrderCheckChanged);
		}

		private void OnCommentOrderCheckChanged() {
			List<CommentItem> items;
			if (CommentOrderCheck) {//descending
				items = CommentItems.OrderByDescending(x => x.CreatedDateTime).ToList();
			} else {
				items = CommentItems.OrderBy(x => x.CreatedDateTime).ToList();
			}
			CommentItems.Clear();
			foreach (CommentItem item in items) {
				CommentItems.Add(item);
			}
		}

		private void OnPostChanged() {
			Description = E621Post.Description.NotBlankCheck() ?? "No Description";
			SourceURLs = E621Post.Sources.ToArray();
			if (SourceURLs.IsEmpty()) {
				SourceTitle = "No Source";
			} else if (SourceURLs.Length == 1) {
				SourceTitle = "Source";
			} else {
				SourceTitle = "Sources";
			}

			PoolItems = E621Post.Pools.Select(x => new PostPoolItem(x, this)).ToArray();

			LoadComments();
			LoadRelations();
		}

		private CancellationTokenSource cts1;
		private CancellationTokenSource cts2;
		private bool isLoadingParent;
		private bool isLoadingChildren;
		private bool commentOrderCheck;

		private async void LoadRelations() {
			IsLoadingParent = true;
			IsLoadingChildren = true;

			ParentPost = null;
			ChildrenPost = null;

			cts2?.Cancel();
			cts2 = new CancellationTokenSource();

			string parentID = E621Post.Relationships.ParentId;
			List<string> childrenIDs = E621Post.Relationships.Children;

			if (parentID.IsNotBlank()) {
				E621Post parent = await E621API.GetPostAsync(parentID, cts2.Token);
				if (cts2.IsCancellationRequested) {
					return;
				}
				ParentPost = parent;
			}

			IsLoadingParent = false;

			if (childrenIDs.IsNotEmpty()) {
				List<E621Post> list = new();
				foreach (string id in childrenIDs) {
					if (cts2.IsCancellationRequested) {
						IsLoadingChildren = false;
						return;
					}
					E621Post post = await E621API.GetPostAsync(id, cts2.Token);
					if (post != null) {
						list.Add(post);
					}
				}
				ChildrenPost = list.ToArray();
			}

			IsLoadingChildren = false;

		}

		private async void LoadComments() {
			cts1?.Cancel();
			cts1 = new CancellationTokenSource();

			IsLoadingComments = true;

			CommentItems.Clear();

			E621Comment[] comments = await E621API.GetCommentsAsync(E621Post.ID, cts1.Token);
			foreach (E621Comment comment in comments ?? Array.Empty<E621Comment>()) {
				CommentItem item = new() {
					E621Comment = comment,
					cts = cts1,
				};
				CommentItems.Add(item);
			}

			LoadCommentUsersPool(CommentItems.ToList(), cts1);

			RaisePropertyChanged(nameof(CommentItems));
			IsLoadingComments = false;
		}

		private static async void LoadCommentUsersPool(List<CommentItem> tasksPool, CancellationTokenSource cts) {
			foreach (CommentItem item in tasksPool) {
				if (cts.IsCancellationRequested) {
					return;
				}
				await item.LoadUserStuff();
				await Task.Delay(500);
			}
		}

		public ICommand RelationsCommand => new DelegateCommand(Relations);

		private void Relations() {
			if (IsLoadingChildren || IsLoadingParent) {
				return;
			}

			List<E621Post> posts = new() { E621Post };
			if (ParentPost != null) {
				posts.Add(ParentPost);
			}
			if (ChildrenPost.IsNotEmpty()) {
				posts.AddRange(ChildrenPost);
			}

			E621HomePageViewModel.CreatePosts($"Relations of {E621Post.ID}", posts);
		}

		public ICommand ItemClickCommand => new DelegateCommand<ItemClickEventArgs>(ItemClick);

		private void ItemClick(ItemClickEventArgs args) {
			PostPoolItem item = (PostPoolItem)args.ClickedItem;
			if (item.IsLoading) {
				return;
			}

			E621HomePageViewModel.CreatePool(item.Pool);
		}
	}


	public class CommentItem : BindableBase {
		public CancellationTokenSource cts;

		private E621Comment e621Comment = null;

		private bool isLoadingAvatar;

		private string userAvatarURL;
		private string username;
		private string levelString;
		private DateTime createdDateTime;
		private int score;

		private string textContent;

		private E621User e621User;
		private E621Post avatarPost;

		public E621Comment E621Comment {
			get => e621Comment;
			set => SetProperty(ref e621Comment, value, OnCommentChanged);
		}

		public E621User E621User {
			get => e621User;
			set => SetProperty(ref e621User, value);
		}

		public E621Post AvatarPost {
			get => avatarPost;
			set => SetProperty(ref avatarPost, value);
		}

		public string UserAvatarURL {
			get => userAvatarURL;
			set => SetProperty(ref userAvatarURL, value);
		}

		public string Username {
			get => username;
			set => SetProperty(ref username, value);
		}

		public string LevelString {
			get => levelString;
			set => SetProperty(ref levelString, value);
		}

		public DateTime CreatedDateTime {
			get => createdDateTime;
			set => SetProperty(ref createdDateTime, value);
		}

		public int Score {
			get => score;
			set => SetProperty(ref score, value);
		}

		public string TextContent {
			get => textContent;
			set => SetProperty(ref textContent, value);
		}

		public bool IsLoadingAvatar {
			get => isLoadingAvatar;
			set => SetProperty(ref isLoadingAvatar, value);
		}

		private void OnCommentChanged() {
			if (E621Comment == null) {
				return;
			}

			Username = E621Comment.creator_name;
			Score = E621Comment.score;
			CreatedDateTime = E621Comment.created_at;
			TextContent = E621Comment.body;

		}

		public async Task LoadUserStuff() {
			IsLoadingAvatar = true;

			E621User = await E621API.GetUserAsync(E621Comment.creator_id, cts.Token);
			if (E621User == null) {
				IsLoadingAvatar = false;
				return;
			}

			LevelString = E621User.level_string;

			AvatarPost = await E621API.GetPostAsync(E621User.avatar_id, cts.Token);
			if (AvatarPost == null) {
				IsLoadingAvatar = false;
				return;
			}

			if (AvatarPost.HasNoValidURLs()) {
				IsLoadingAvatar = false;
				return;
			}

			UserAvatarURL = AvatarPost.Sample.URL;

			IsLoadingAvatar = false;
		}

		public ICommand CopyCommand => new DelegateCommand(Copy);
		public ICommand OpenInNewTabCommand => new DelegateCommand(OpenInNewTab);

		private void Copy() {
			$"https://www.e621.net/posts/{E621User.avatar_id}".CopyToClipboard();
		}

		private void OpenInNewTab() {
			if (AvatarPost != null) {
				E621HomePageViewModel.CreatePosts($"Post {AvatarPost.ID}", new E621Post[] { AvatarPost });
			}
		}
	}

	public class PostPoolItem : BindableBase {
		public PostImageSideViewModel ParentViewModel { get; set; }

		private E621Pool pool;
		private bool isLoading;

		private string poolID;

		private string poolName;
		private string toolTip;

		public string PoolID {
			get => poolID;
			set => SetProperty(ref poolID, value, OnPoolIDChanged);
		}

		public string PoolName {
			get => poolName;
			set => SetProperty(ref poolName, value);
		}

		public string ToolTip {
			get => toolTip;
			set => SetProperty(ref toolTip, value);
		}

		public E621Pool Pool {
			get => pool;
			set => SetProperty(ref pool, value, OnPoolChanged);
		}

		public bool IsLoading {
			get => isLoading;
			set => SetProperty(ref isLoading, value);
		}

		public PostPoolItem(string item, PostImageSideViewModel parentViewModel) {
			this.PoolID = item;
			ParentViewModel = parentViewModel;
		}

		private async void OnPoolIDChanged() {
			IsLoading = true;

			Pool = await E621API.GetPoolAsync(PoolID);

			IsLoading = false;
		}

		private void OnPoolChanged() {
			if (Pool == null) {
				return;
			}

			PoolName = Pool.Name;
			ToolTip = $"";
		}

		public ICommand CopyCommand => new DelegateCommand(Copy);
		public ICommand InfoCommand => new DelegateCommand(Info);

		private void Copy() {
			$"https://e621.net/pools/{PoolID}".CopyToClipboard();
		}

		private async void Info() {
			await PoolInfoView.ShowAsDialog(Pool);
		}
	}
}
