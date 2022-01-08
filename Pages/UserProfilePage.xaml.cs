using E621Downloader.Models.Locals;
using E621Downloader.Models.Posts;
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
		public E621User User { get; set; }
		public UserProfilePage() {
			this.InitializeComponent();
			if(User != null) {
				FieldInfo[] fields = User.GetType().GetFields();
				foreach(FieldInfo field in fields) {
					Debug.WriteLine($"{field.Name} - {field.GetValue(this)}");
				}
			}
		}
		protected override void OnNavigatedTo(NavigationEventArgs e) {
			base.OnNavigatedTo(e);
			var pair = WelcomeInLanguages.GetRandomWelcomePair();
			WelcomeText.Text = pair.Value;
			ToolTipService.SetToolTip(WelcomeText, $"\"Welcome\" in {pair.Key}");
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
				MainPage.SelectNavigationItem(PageTag.UserProfile);
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
			
		}
	}
}
