using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Windows.Input;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Storage.Streams;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Imaging;
using YiffBrowser.Helpers;
using YiffBrowser.Models.E621;
using YiffBrowser.Services.Networks;

namespace YiffBrowser.Views.Controls {
	public sealed partial class PostDetailView : UserControl {
		public event Action RequestBack;

		public E621Post E621Post {
			get => (E621Post)GetValue(E621PostProperty);
			set {
				if (E621Post == value) {
					MediaDisplayView.Play();
				}
				SetValue(E621PostProperty, value);
			}
		}

		public static readonly DependencyProperty E621PostProperty = DependencyProperty.Register(
			nameof(E621Post),
			typeof(E621Post),
			typeof(PostDetailView),
			new PropertyMetadata(null)
		);

		public ICommand ImagesListManagerItemClickCommand {
			get => (ICommand)GetValue(ImagesListManagerItemClickCommandProperty);
			set => SetValue(ImagesListManagerItemClickCommandProperty, value);
		}

		public static readonly DependencyProperty ImagesListManagerItemClickCommandProperty = DependencyProperty.Register(
			nameof(ImagesListManagerItemClickCommand),
			typeof(ICommand),
			typeof(PostDetailView),
			new PropertyMetadata(null)
		);

		public E621Post[] PostsList {
			get => (E621Post[])GetValue(PostsListProperty);
			set => SetValue(PostsListProperty, value);
		}

		public static readonly DependencyProperty PostsListProperty = DependencyProperty.Register(
			nameof(PostsList),
			typeof(E621Post[]),
			typeof(PostDetailView),
			new PropertyMetadata(Array.Empty<E621Post>())
		);

		public bool InputByPosts {
			get => (bool)GetValue(InputByPostsProperty);
			set => SetValue(InputByPostsProperty, value);
		}

		public static readonly DependencyProperty InputByPostsProperty = DependencyProperty.Register(
			nameof(InputByPosts),
			typeof(bool),
			typeof(PostDetailView),
			new PropertyMetadata(false, OnInputByPostsChanged)
		);

		private static void OnInputByPostsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
			if (d is PostDetailView view) {
				view.ViewModel.InputByPosts = (bool)e.NewValue;
			}
		}

		public string InitialImageURL {
			get => (string)GetValue(InitialImageURLProperty);
			set => SetValue(InitialImageURLProperty, value ?? string.Empty);
		}

		public static readonly DependencyProperty InitialImageURLProperty = DependencyProperty.Register(
			nameof(InitialImageURL),
			typeof(string),
			typeof(PostDetailView),
			new PropertyMetadata(string.Empty)
		);

		public PostDetailView() {
			this.InitializeComponent();
			ViewModel.OnImagesListManagerItemClick += ViewModel_OnImagesListManagerItemClick;
			IsShowingImagesListManager = false;
		}

		private void ViewModel_OnImagesListManagerItemClick(E621Post post) {
			ImagesListManagerItemClickCommand?.Execute(post);
		}

		public Image GetBackgroundImage() => BackgroundImage;
		public FrameworkElement GetCurrentImage() {
			PictureDisplayView.ResetImage();
			if (ViewModel.ShowBackgroundImage) {
				return BackgroundImage;
			} else {
				return PictureDisplayView.GetTargetImage();
			}
		}

		private void BackButton_Click(object sender, RoutedEventArgs e) {
			IsShowingImagesListManager = false;
			PauseVideo();
			RequestBack?.Invoke();
		}

		public void PauseVideo() {
			MediaDisplayView.Pause();
		}

		public void PlayVideo() {
			MediaDisplayView.Play();
		}

		public bool IsShowingImagesListManager {
			get => (bool)GetValue(IsShowingImagesListManagerProperty);
			set {
				if (ViewModel.IsImagesListManagerLocked) {
					return;
				}
				SetValue(IsShowingImagesListManagerProperty, value);
			}
		}

		public static readonly DependencyProperty IsShowingImagesListManagerProperty = DependencyProperty.Register(
			nameof(IsShowingImagesListManager),
			typeof(bool),
			typeof(PostDetailView),
			new PropertyMetadata(false, OnIsShowingImagesListManagerPropertyChanged)
		);

		private static void OnIsShowingImagesListManagerPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
			if (d is not PostDetailView view) {
				return;
			}

			bool value = (bool)e.NewValue;

			view.ImagesListManagerScroll.IsHitTestVisible = value;

			if (value) {
				view.ImagesListManagerTransformAnimation.To = 5;
			} else {
				view.ImagesListManagerTransformAnimation.To = -150;
			}
			view.ImagesListManagerTransformStoryboard.Begin();
		}

		private void MainGrid_Tapped(object sender, TappedRoutedEventArgs e) {

		}

		private Point startPosition;

		private void MainGrid_PointerPressed(object sender, PointerRoutedEventArgs e) {
			startPosition = e.GetCurrentPoint(MainGrid).Position;
			//if (e.Pointer.PointerDeviceType != PointerDeviceType.Touch) {
			//	IsShowingImagesListManager = false;
			//}
		}

		private void MainGrid_PointerReleased(object sender, PointerRoutedEventArgs e) {
			Point endPos = e.GetCurrentPoint(MainGrid).Position;
			if (startPosition.Distance(endPos) >= 20d) {
				return;
			}
			IsShowingImagesListManager = !IsShowingImagesListManager;
		}
	}

	public class PostDetailViewModel : BindableBase {
		public event Action<E621Post> OnImagesListManagerItemClick;

		private E621Post e621Post;
		private string imageURL;
		private bool isMedia;
		private int fileSize;
		private bool showBackgroundImage = true;
		private string mediaURL;
		private bool showMoreInfoSplitView;
		private string fileTypeIcon;
		private string fileTypeToolTip;
		private string ratingToolTip;
		private string idTitle;
		private Color ratingColor;
		private bool showSoundWarning;
		private Color soundWarningColor;
		private string soundWarningToolTip;
		private bool isFavoriteLoading;
		private bool hasFavorited;
		private bool ableToCopyImage;
		private bool isSidePaneOverlay = true;
		private bool inputByPosts;
		private bool isImagesListManagerLocked;
		private E621Post[] allPosts;
		private bool hasVotedUp;
		private bool hasVotedDown;
		private bool isVoteLoading;
		private int voteUp;
		private int voteDown;
		private int voteTotal;
		private string duration;

		public bool IsSidePaneOverlay {
			get => isSidePaneOverlay;
			set => SetProperty(ref isSidePaneOverlay, value);
		}

		public E621Post E621Post {
			get => e621Post;
			set => SetProperty(ref e621Post, value, OnPostChanged);
		}

		public E621Post[] AllPosts {
			get => allPosts;
			set => SetProperty(ref allPosts, value);
		}

		public string ImageURL {
			get => imageURL;
			set => SetProperty(ref imageURL, value);
		}

		public string MediaURL {
			get => mediaURL;
			set => SetProperty(ref mediaURL, value);
		}

		public int FileSize {
			get => fileSize;
			set => SetProperty(ref fileSize, value);
		}

		public bool IsMedia {
			get => isMedia;
			set => SetProperty(ref isMedia, value);
		}

		public bool ShowBackgroundImage {
			get => showBackgroundImage;
			set => SetProperty(ref showBackgroundImage, value);
		}

		public bool ShowMoreInfoSplitView {
			get => showMoreInfoSplitView;
			set => SetProperty(ref showMoreInfoSplitView, value);
		}

		public string FileTypeIcon {
			get => fileTypeIcon;
			set => SetProperty(ref fileTypeIcon, value);
		}

		public string Duration {
			get => duration;
			set => SetProperty(ref duration, value);
		}

		public string FileTypeToolTip {
			get => fileTypeToolTip;
			set => SetProperty(ref fileTypeToolTip, value);
		}

		public string RatingToolTip {
			get => ratingToolTip;
			set => SetProperty(ref ratingToolTip, value);
		}

		public string IDTitle {
			get => idTitle;
			set => SetProperty(ref idTitle, value);
		}
		public Color RatingColor {
			get => ratingColor;
			set => SetProperty(ref ratingColor, value);
		}

		public bool ShowSoundWarning {
			get => showSoundWarning;
			set => SetProperty(ref showSoundWarning, value);
		}
		public Color SoundWarningColor {
			get => soundWarningColor;
			set => SetProperty(ref soundWarningColor, value);
		}
		public string SoundWarningToolTip {
			get => soundWarningToolTip;
			set => SetProperty(ref soundWarningToolTip, value);
		}

		public bool InputByPosts {
			get => inputByPosts;
			set => SetProperty(ref inputByPosts, value);
		}

		public bool IsImagesListManagerLocked {
			get => isImagesListManagerLocked;
			set => SetProperty(ref isImagesListManagerLocked, value);
		}

		private void OnPostChanged() {
			ShowBackgroundImage = true;

			IDTitle = $"{E621Post.ID} ({E621Post.Rating.ToString().Substring(0, 1)})";
			RatingColor = E621Post.Rating.GetRatingColor();

			VoteUp = E621Post.Score.Up;
			VoteDown = E621Post.Score.Down;
			VoteTotal = E621Post.Score.Total;

			FileSize = E621Post.File.Size;
			Duration = E621Post.Duration;

			FileType type = E621Post.GetFileType();
			FileTypeIcon = type switch {
				FileType.Png or FileType.Jpg => "\uEB9F",
				FileType.Gif => "\uF4A9",
				FileType.Webm => "\uE714",
				_ => "\uE9CE",
			};
			FileTypeToolTip = $"Type: {type}";

			RatingToolTip = $"Rating: {E621Post.Rating}";

			List<string> tags = E621Post.Tags.GetAllTags();
			if (tags.Contains("sound_warning")) {
				ShowSoundWarning = true;
				SoundWarningColor = E621Rating.Explict.GetRatingColor();
				SoundWarningToolTip = "This Video Has 'sound_warning' Tag";
			} else if (tags.Contains("sound")) {
				ShowSoundWarning = true;
				SoundWarningColor = E621Rating.Questionable.GetRatingColor();
				SoundWarningToolTip = "This Video Has 'sound' Tag";
			} else {
				ShowSoundWarning = false;
			}

			AbleToCopyImage = false;

			switch (type) {
				case FileType.Png:
				case FileType.Jpg:
				case FileType.Gif:
					ImageURL = E621Post.File.URL;
					MediaURL = string.Empty;
					IsMedia = false;
					break;
				case FileType.Webm:
					ImageURL = string.Empty;
					MediaURL = E621Post.File.URL;
					IsMedia = true;
					break;
				case FileType.Anim:
					//display error
					break;
				default:
					throw new NotSupportedException();
			}

			//hasFavorited = E621Post.IsFavorited;
			//RaisePropertyChanged(nameof(HasFavorited));

			SetProperty(ref hasFavorited, E621Post.IsFavorited);

			//E621Post.
		}

		public bool AbleToCopyImage {
			get => ableToCopyImage;
			set => SetProperty(ref ableToCopyImage, value);
		}

		public ICommand OnImageLoadedCommand => new DelegateCommand<BitmapImage>(OnImageLoaded);

		private void OnImageLoaded(BitmapImage image) {
			if (image != null) {
				ShowBackgroundImage = false;
				AbleToCopyImage = true;
			} else {

			}
		}

		public bool IsFavoriteLoading {
			get => isFavoriteLoading;
			set => SetProperty(ref isFavoriteLoading, value);
		}

		public bool HasFavorited {
			get => hasFavorited;
			set => SetProperty(ref hasFavorited, value, OnHasFavoritedChanged);
		}

		public bool HasVotedUp {
			get => hasVotedUp;
			set => SetProperty(ref hasVotedUp, value, OnHasVotedUpChanged);
		}

		public bool HasVotedDown {
			get => hasVotedDown;
			set => SetProperty(ref hasVotedDown, value, OnHasVotedDownChanged);
		}

		public bool IsVoteLoading {
			get => isVoteLoading;
			set => SetProperty(ref isVoteLoading, value);
		}

		public int VoteUp {
			get => voteUp;
			set => SetProperty(ref voteUp, value);
		}
		public int VoteDown {
			get => voteDown;
			set => SetProperty(ref voteDown, value);
		}
		public int VoteTotal {
			get => voteTotal;
			set => SetProperty(ref voteTotal, value);
		}

		private async void OnHasVotedUpChanged() {
			if (IsVoteLoading) {
				return;
			}

			IsVoteLoading = true;

			DataResult<E621Vote> result;

			if (HasVotedUp) {//none -> up
				result = await E621API.VotePost(E621Post.ID, 1, true);
				HasVotedDown = false;
			} else {//up -> none
				result = await E621API.VotePost(E621Post.ID, 1, false);
			}

			if (result.ResultType != HttpResultType.Success) {
				HasVotedUp = !HasVotedUp;
			}

			if (result.Data != null) {
				VoteUp = result.Data.up;
				VoteDown = result.Data.down;
				VoteTotal = result.Data.score;
			}

			IsVoteLoading = false;
		}

		private async void OnHasVotedDownChanged() {
			if (IsVoteLoading) {
				return;
			}

			IsVoteLoading = true;

			DataResult<E621Vote> result;

			if (HasVotedDown) {//none -> down
				result = await E621API.VotePost(E621Post.ID, -1, true);
				HasVotedUp = false;
			} else {//down -> none
				result = await E621API.VotePost(E621Post.ID, -1, false);
			}

			if (result.ResultType != HttpResultType.Success) {
				HasVotedDown = !HasVotedDown;
			}

			if (result.Data != null) {
				VoteUp = result.Data.up;
				VoteDown = result.Data.down;
				VoteTotal = result.Data.score;
			}

			IsVoteLoading = false;
		}

		private async void OnHasFavoritedChanged() {
			if (IsFavoriteLoading) {
				return;
			}
			IsFavoriteLoading = true;

			//await Task.Delay(3000);

			HttpResult<string> result;
			if (HasFavorited) {
				result = await E621API.PostAddFavoriteAsync(E621Post.ID);
			} else {
				result = await E621API.PostDeleteFavoriteAsync(E621Post.ID);
			}

			if (result.Result != HttpResultType.Success) {//cancel action
				HasFavorited = !HasFavorited;
			} else {
				E621Post.IsFavorited = HasFavorited;
			}

			IsFavoriteLoading = false;
		}

		public ICommand OpenMoreInfoCommand => new DelegateCommand(OpenMoreInfo);

		private void OpenMoreInfo() {
			ShowMoreInfoSplitView = !ShowMoreInfoSplitView;
		}

		public ICommand CopyIDCommand => new DelegateCommand(CopyID);

		private void CopyID() {
			E621Post.ID.ToString().CopyToClipboard();
		}

		public ICommand OpenInBrowserCommand => new DelegateCommand(OpenInBrowser);

		private void OpenInBrowser() {
			@$"https://e621.net/posts/{E621Post.ID}".OpenInBrowser();
		}

		public ICommand CopyImageCommand => new DelegateCommand(CopyImage);

		private void CopyImage() {
			if (!AbleToCopyImage) {
				return;
			}

			DataPackage imageDataPackage = new() {
				RequestedOperation = DataPackageOperation.Copy,
			};

			imageDataPackage.SetBitmap(RandomAccessStreamReference.CreateFromUri(new Uri(E621Post.File.URL)));
			Clipboard.SetContent(imageDataPackage);
		}

		public ICommand ImagesListManagerItemClickCommand => new DelegateCommand<E621Post>(ImagesListManagerItemClick);

		private void ImagesListManagerItemClick(E621Post post) {
			OnImagesListManagerItemClick?.Invoke(post);
		}

		public ICommand NextCommand => new DelegateCommand(Next);
		public ICommand PreviousCommand => new DelegateCommand(Previous);

		private void Next() {
			int index = Array.IndexOf(AllPosts, E621Post);
			if (index == -1) {
				return;
			}

			if (index + 1 >= AllPosts.Length) {
				index = 0;
			} else {
				index++;
			}

			OnImagesListManagerItemClick?.Invoke(AllPosts[index]);
		}

		private void Previous() {
			int index = Array.IndexOf(AllPosts, E621Post);
			if (index == -1) {
				return;
			}

			if (index - 1 < 0) {
				index = AllPosts.Length - 1;
			} else {
				index--;
			}

			OnImagesListManagerItemClick?.Invoke(AllPosts[index]);
		}

		public ICommand DownloadCommand => new DelegateCommand(Download);

		private void Download() {
			
		}
	}
}
