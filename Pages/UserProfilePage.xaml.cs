using E621Downloader.Models;
using E621Downloader.Models.Inerfaces;
using E621Downloader.Models.Locals;
using E621Downloader.Models.Posts;
using E621Downloader.Views;
using System;
using System.Reflection;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;

namespace E621Downloader.Pages {
	public sealed partial class UserProfilePage: Page, IPage {
		private CoreCursor cursorBeforePointerEntered = null;

		public UserProfilePage() {
			this.InitializeComponent();
			this.NavigationCacheMode = NavigationCacheMode.Enabled;
			MainPage.Instance.UserStartChanging += () => {
				AvatarLoadingRing.IsActive = true;
				InfoLoadingRing.IsActive = true;
				AvatarImage.ImageSource = null;
				UpdateUserInfo(null);
			};
			MainPage.Instance.UserChangedInfoComplete += () => {
				UpdateUserInfo(E621User.Current);
				InfoLoadingRing.IsActive = false;
			};
			MainPage.Instance.UserChangedAvatarComplete += (image) => {
				AvatarLoadingRing.IsActive = false;
				AvatarImage.ImageSource = image;
			};
		}

		protected override void OnNavigatedTo(NavigationEventArgs e) {
			base.OnNavigatedTo(e);
			var pair = WelcomeInLanguages.GetRandomWelcomePair();
			WelcomeText.Text = pair.Value;
			WelcomeDetailText.Text = "HelloLanguage".Language(pair.Key);

			UpdateUserInfo(E621User.Current);

			AvatarLoadingRing.IsActive = false;
			InfoLoadingRing.IsActive = false;
			AvatarImage.ImageSource = MainPage.GetUserIcon();
		}

		private const int MAX_ITEM_IN_PANEL = 35;
		private void UpdateUserInfo(E621User user) {
			PanelLeft.Children.Clear();
			PanelRight.Children.Clear();
			if(user != null) {
				Panel targetPanel = PanelLeft;
				FieldInfo[] array = user.GetType().GetFields();
				for(int i = 0; i < array.Length; i++) {
					FieldInfo field = array[i];
					if(field.Name == "blacklisted_tags") {
						continue;
					}
					string title = field.Name.Replace("_", " ").ToCamelCase();
					string content = field.GetValue(E621User.Current)?.ToString() ?? "None";
					targetPanel.Children.Add(new UserInfoLine(title, content));
					if(i >= MAX_ITEM_IN_PANEL) {
						targetPanel = PanelRight;
					}
				}
			} else {
				//load default
			}
		}

		private async void LogoutButton_Tapped(object sender, TappedRoutedEventArgs e) {
			if(await new ContentDialog() {
				Title = "Confirm".Language(),
				Content = "Are you sure to log out".Language(),
				PrimaryButtonText = "Sure".Language(),
				CloseButtonText = "Back".Language(),
				DefaultButton = ContentDialogButton.Close,
			}.ShowAsync() == ContentDialogResult.Primary) {
				LocalSettings.Current.SetLocalUser("", "");
				LocalSettings.Save();
				MainPage.NavigateTo(PageTag.UserProfile);
				MainPage.Instance.ChangeUser(null);
			}
		}

		private void FavoritesButton_Tapped(object sender, TappedRoutedEventArgs e) {
			MainPage.NavigateToPostsBrowser(1, $"fav:{LocalSettings.Current.user_username}");
		}

		private void RefreshButton_Tapped(object sender, TappedRoutedEventArgs e) {
			MainPage.Instance.ChangeUser(LocalSettings.Current.user_username);
		}

		private void VotedButton_Tapped(object sender, TappedRoutedEventArgs e) {
			MainPage.NavigateToPostsBrowser(1, $"votedup:anything");
		}

		private void WelcomeTextGrid_PointerEntered(object sender, PointerRoutedEventArgs e) {
			cursorBeforePointerEntered = Window.Current.CoreWindow.PointerCursor;
			Window.Current.CoreWindow.PointerCursor = new CoreCursor(CoreCursorType.Help, 0);

			WelcomeTextHeightAnimation.From = WelcomeDetailText.Height;
			WelcomeTextHeightAnimation.To = 20;
			WelcomeTextOpacityAnimation.From = WelcomeDetailText.Opacity;
			WelcomeTextOpacityAnimation.To = 1;
			WelcomeTextStoryboard.Begin();
		}

		private void WelcomeTextGrid_PointerExited(object sender, PointerRoutedEventArgs e) {
			Window.Current.CoreWindow.PointerCursor = cursorBeforePointerEntered;

			WelcomeTextHeightAnimation.From = WelcomeDetailText.Height;
			WelcomeTextHeightAnimation.To = 0;
			WelcomeTextOpacityAnimation.From = WelcomeDetailText.Opacity;
			WelcomeTextOpacityAnimation.To = 0;
			WelcomeTextStoryboard.Begin();
		}

		void IPage.UpdateNavigationItem() {
			MainPage.Instance.currentTag = PageTag.UserProfile;
			MainPage.Instance.UpdateNavigationItem();
		}

		void IPage.FocusMode(bool enabled) { }
	}
}
