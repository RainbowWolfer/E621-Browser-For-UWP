using E621Downloader.Models;
using E621Downloader.Models.Locals;
using E621Downloader.Models.Posts;
using E621Downloader.Views;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace E621Downloader.Pages {
	public sealed partial class UserProfilePage: Page {
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
			ToolTipService.SetPlacement(WelcomeText, PlacementMode.Right);
			ToolTipService.SetToolTip(WelcomeText, $"\"Hello\" in {pair.Key}");

			UpdateUserInfo(E621User.Current);

			AvatarLoadingRing.IsActive = false;
			InfoLoadingRing.IsActive = false;
			AvatarImage.ImageSource = MainPage.GetUserIcon();
		}

		private const int MAXITEMINPANEL = 35;
		private void UpdateUserInfo(E621User user) {
			PanelLeft.Children.Clear();
			PanelRight.Children.Clear();
			if(user != null) {
				Panel targetPanel = PanelLeft;
				FieldInfo[] array = user.GetType().GetFields();
				for(int i = 0; i < array.Length; i++) {
					FieldInfo field = array[i];
					string title = field.Name.Replace("_", " ").ToCamelCase();
					string content = field.GetValue(E621User.Current).ToString();
					targetPanel.Children.Add(new UserInfoLine(title, content));
					if(i >= MAXITEMINPANEL) {
						targetPanel = PanelRight;
					}
				}
			} else {
				//load default
			}
		}

		private async void LogoutButton_Tapped(object sender, TappedRoutedEventArgs e) {
			if(await new ContentDialog() {
				Title = "Confirm",
				Content = "Are you sure to log out?",
				PrimaryButtonText = "Sure",
				CloseButtonText = "Back",
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

		private CoreCursor cursorBeforePointerEntered = null;
		private void WelcomeText_PointerEntered(object sender, PointerRoutedEventArgs e) {
			cursorBeforePointerEntered = Window.Current.CoreWindow.PointerCursor;
			Window.Current.CoreWindow.PointerCursor = new CoreCursor(CoreCursorType.Help, 0);
		}

		private void WelcomeText_PointerExited(object sender, PointerRoutedEventArgs e) {
			Window.Current.CoreWindow.PointerCursor = cursorBeforePointerEntered;
		}

		private void RefreshButton_Tapped(object sender, TappedRoutedEventArgs e) {
			MainPage.Instance.ChangeUser(LocalSettings.Current.user_username);
		}
	}
}
