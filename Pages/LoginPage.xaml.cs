using E621Downloader.Models;
using E621Downloader.Models.Locals;
using E621Downloader.Models.Networks;
using E621Downloader.Models.Posts;
using E621Downloader.Views;
using Microsoft.UI.Xaml.Controls;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace E621Downloader.Pages {
	public sealed partial class LoginPage: Page {
		private Brush origin_brush_username;
		private Brush origin_brush_apiKey;
		public LoginPage() {
			this.InitializeComponent();
			origin_brush_username = UsernameBox.BorderBrush;
			origin_brush_apiKey = APIBox.BorderBrush;
		}

		private async void HelpButton_Tapped(object sender, TappedRoutedEventArgs e) {
			await new ContentDialog() {
				Title = "Help",
				Content = new LoginHelpSection(),
				PrimaryButtonText = "Back",
			}.ShowAsync();
		}

		private async void SubmitButton_Tapped(object sender, TappedRoutedEventArgs e) {
			string username = UsernameBox.Text.Trim();
			string apiKey = APIBox.Text.Trim();
			bool hasEmpty = false;
			if(string.IsNullOrWhiteSpace(username)) {
				UsernameBox.BorderBrush = new SolidColorBrush(Colors.Red);
				hasEmpty = true;
			}
			if(string.IsNullOrWhiteSpace(apiKey)) {
				APIBox.BorderBrush = new SolidColorBrush(Colors.Red);
				hasEmpty = true;
			}
			if(hasEmpty) {
				MainPage.CreateTip(this, "Warning", "You Need to enter your username and api key", Symbol.Important, "Back", true, TeachingTipPlacementMode.Top);
				return;
			}
			MainPage.CreateInstantDialog("Please Wait", "Checking Sign In Parameters");
			HttpResult<string> result = await Data.ReadURLAsync($"https://{Data.GetHost()}/favorites.json", null, username, apiKey);
			MainPage.HideInstantDialog();
			if(result.Result == HttpResultType.Success) {
				LocalSettings.Current.SetLocalUser(username, apiKey);
				LocalSettings.Save();
				MainPage.NavigateTo(PageTag.UserProfile);
				MainPage.Instance.ChangeUser(username);
			} else {
				await MainPage.CreatePopupDialog("Sign In Failed", "Please Check that your username and your api_key copied from the website are correct");
			}
		}

		private async void PasteButton_Tapped(object sender, TappedRoutedEventArgs e) {
			string clip = await Clipboard.GetContent().GetTextAsync();
			if(!string.IsNullOrWhiteSpace(clip)) {
				APIBox.Text = clip;
			}
		}

		private void UsernameBox_TextChanged(object sender, TextChangedEventArgs e) {
			UsernameBox.BorderBrush = origin_brush_username;
		}

		private void APIBox_TextChanged(object sender, TextChangedEventArgs e) {
			APIBox.BorderBrush = origin_brush_apiKey;
		}
	}
}
