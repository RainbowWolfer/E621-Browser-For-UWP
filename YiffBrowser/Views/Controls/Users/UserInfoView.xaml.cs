using Microsoft.Toolkit.Uwp.Helpers;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Windows.Input;
using Windows.Foundation;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using YiffBrowser.Helpers;
using YiffBrowser.Interfaces;
using YiffBrowser.Models.E621;
using YiffBrowser.Services;
using YiffBrowser.Services.Locals;
using YiffBrowser.Services.Networks;
using YiffBrowser.Views.Pages.E621;

namespace YiffBrowser.Views.Controls.Users {
	public sealed partial class UserInfoView : UserControl, IContentDialogView {
		public event TypedEventHandler<UserInfoView, string> OnAvatarRefreshed;

		public ContentDialog Dialog { get; set; }

		public UserInfoView(E621User user, E621Post avatar) {
			this.InitializeComponent();
			ViewModel.User = user;
			ViewModel.AvatarPost = avatar;
			ViewModel.RequestCloseDialog += () => Dialog?.Hide();
			ViewModel.OnAvatarRefreshed += (s) => OnAvatarRefreshed?.Invoke(this, s);

			string yellow = App.IsDarkTheme() ? "#FFD700" : "#F7B000";

			Hello1Text.Foreground = new SolidColorBrush(yellow.ToColor());
			Hello2Text.Foreground = new SolidColorBrush(yellow.ToColor());
		}

		private void HelloGrid_PointerEntered(object sender, PointerRoutedEventArgs e) {

			Hello1TextOpacityAnimation.To = 0;
			Hello2TextOpacityAnimation.To = 1;
			Hello1TransformAnimation.To = -20;
			Hello2TransformAnimation.To = 0;

			HelloTextStoryboard.Begin();
		}

		private void HelloGrid_PointerExited(object sender, PointerRoutedEventArgs e) {

			Hello1TextOpacityAnimation.To = 1;
			Hello2TextOpacityAnimation.To = 0;
			Hello1TransformAnimation.To = 0;
			Hello2TransformAnimation.To = 20;

			HelloTextStoryboard.Begin();
		}

		public bool IsRefreshing => ViewModel.IsRefreshing;

	}

	public class UserInfoViewModel : BindableBase {
		public event Action<string> OnAvatarRefreshed;
		public event Action RequestCloseDialog;

		private E621Post avatarPost;
		private E621User user;

		private string helloText1;
		private string helloText2;
		private string username;
		private string levelString;
		private DateTime createdAt;

		private string avatarURL = "/Resources/E621/e612-Bigger.png";
		private string email;
		private string userID;

		private bool isRefreshing;
		private bool isAvatarLoading;

		public E621User User {
			get => user;
			set => SetProperty(ref user, value, OnUserChanged);
		}

		public E621Post AvatarPost {
			get => avatarPost;
			set => SetProperty(ref avatarPost, value, OnAvatarPostChanged);
		}

		public string Username {
			get => username;
			set => SetProperty(ref username, value);
		}
		public string LevelString {
			get => levelString;
			set => SetProperty(ref levelString, value);
		}
		public string Email {
			get => email;
			set => SetProperty(ref email, value);
		}
		public string UserID {
			get => userID;
			set => SetProperty(ref userID, value);
		}
		public DateTime CreatedAt {
			get => createdAt;
			set => SetProperty(ref createdAt, value);
		}
		public string HelloText1 {
			get => helloText1;
			set => SetProperty(ref helloText1, value);
		}
		public string HelloText2 {
			get => helloText2;
			set => SetProperty(ref helloText2, value);
		}
		public string AvatarURL {
			get => avatarURL;
			set => SetProperty(ref avatarURL, value, OnAvatarURLChanged);
		}

		public bool IsRefreshing {
			get => isRefreshing;
			set => SetProperty(ref isRefreshing, value);
		}

		public bool IsAvatarLoading {
			get => isAvatarLoading;
			set => SetProperty(ref isAvatarLoading, value);
		}

		private void OnUserChanged() {
			KeyValuePair<string, string> pair = HelloInLanguages.GetRandomWelcomePair();
			HelloText1 = pair.Value;
			HelloText2 = $"This is 'Hello' in {pair.Key}";

			if (User == null) {
				return;
			}

			Username = User.name;
			LevelString = User.level_string;
			CreatedAt = User.created_at;
			Email = User.email;
			UserID = User.id.ToString();
		}

		private void OnAvatarPostChanged() {
			if (AvatarPost.HasNoValidURLs()) {
				AvatarURL = "/Resources/E621/e612-Bigger.png";
			} else {
				AvatarURL = AvatarPost.Sample.URL;
			}
		}

		private void OnAvatarURLChanged() {
			IsAvatarLoading = true;
		}

		public ICommand FavoritesCommand => new DelegateCommand(Favorites);
		public ICommand VotedUpCommand => new DelegateCommand(VotedUp);
		public ICommand RefreshCommand => new DelegateCommand(Refresh);
		public ICommand LogoutCommand => new DelegateCommand(Logout);

		private void Favorites() {
			if (User == null) {
				return;
			}
			RequestCloseDialog?.Invoke();
			E621HomePageViewModel.CreateNewTag($"fav:{User.name}");
		}


		private void VotedUp() {
			if (User == null) {
				return;
			}
			RequestCloseDialog?.Invoke();
			E621HomePageViewModel.CreateNewTag($"votedup:{User.name}");
		}


		private async void Refresh() {
			if (User == null) {
				return;
			}

			IsRefreshing = true;

			E621User user = await E621API.GetUserAsync(User.name);
			if (user == null) {
				IsRefreshing = false;
				return;
			}
			User = user;

			E621Post avatarPost = await E621API.GetPostAsync(User.avatar_id);
			if (avatarPost == null || avatarPost.HasNoValidURLs()) {
				IsRefreshing = false;
				return;
			}
			AvatarPost = avatarPost;

			OnAvatarRefreshed?.Invoke(AvatarPost.Sample.URL);

			IsRefreshing = false;
		}

		private void Logout() {
			RequestCloseDialog?.Invoke();
			Local.Settings.ClearLocalUser();
			OnAvatarRefreshed?.Invoke(null);
		}

		public ICommand ImageOpenedCommand => new DelegateCommand(ImageOpened);
		public ICommand ImageFailedCommand => new DelegateCommand(ImageFailed);

		private void ImageOpened() {
			IsAvatarLoading = false;
		}

		private void ImageFailed() {
			IsAvatarLoading = false;
		}

		public ICommand OpenInNewTabCommand => new DelegateCommand(OpenInNewTab);
		public ICommand CopyCommand => new DelegateCommand(Copy);

		private void OpenInNewTab() {
			if (AvatarPost == null) {
				return;
			}
			RequestCloseDialog?.Invoke();
			E621HomePageViewModel.CreatePosts($"Post {AvatarPost.ID}", new E621Post[] { AvatarPost });
		}

		private void Copy() {
			if (AvatarPost == null) {
				return;
			}

			$"https://e621.net/posts/{AvatarPost.ID}".CopyToClipboard();
		}
	}
}
