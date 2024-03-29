﻿using E621Downloader.Models.Debugging;
using E621Downloader.Models.Inerfaces;
using E621Downloader.Models.Locals;
using E621Downloader.Models.Networks;
using E621Downloader.Models.Utilities;
using E621Downloader.Views.UserSection;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

namespace E621Downloader.Pages {
	public sealed partial class LoginPage: Page, IPage {
		private readonly Brush origin_brush_username;
		private readonly Brush origin_brush_apiKey;
		public LoginPage() {
			this.InitializeComponent();
			origin_brush_username = UsernameBox.BorderBrush;
			origin_brush_apiKey = APIBox.BorderBrush;
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
				MainPage.CreateTip(this, "Warning".Language(), "You Need to enter your username and api key".Language(), Symbol.Important, "Back".Language(), true, TeachingTipPlacementMode.Top);
				return;
			}
			MainPage.CreateInstantDialog("Please Wait".Language(), "Checking Sign In Parameters".Language());
			HttpResult<string> result = await Data.ReadURLAsync($"https://{Data.GetHost()}/favorites.json", null, username, apiKey);
			MainPage.HideInstantDialog();
			if(result.Result == HttpResultType.Success) {
				LocalSettings.Current.SetLocalUser(username, apiKey);
				LocalSettings.Save();
				MainPage.Instance.ChangeUser(username);
				MainPage.NavigateTo(PageTag.UserProfile);
			} else {
				await MainPage.CreatePopupDialog("Sign In Failed".Language(), "Please Check that your username and your api_key copied from the website are correct".Language());
			}
		}

		private async void PasteButton_Tapped(object sender, TappedRoutedEventArgs e) {
			try {
				string clip = await Clipboard.GetContent().GetTextAsync();
				if(!string.IsNullOrWhiteSpace(clip)) {
					APIBox.Text = clip;
				}
			} catch(Exception ex) {
				ErrorHistories.Add(ex);
			}
		}

		private void UsernameBox_TextChanged(object sender, TextChangedEventArgs e) {
			UsernameBox.BorderBrush = origin_brush_username;
		}

		private void APIBox_TextChanged(object sender, TextChangedEventArgs e) {
			APIBox.BorderBrush = origin_brush_apiKey;
		}

		private async void HelpItem_Click(object sender, RoutedEventArgs e) {
			await new ContentDialog() {
				Title = "Help".Language(),
				Content = new LoginHelpSection(),
				PrimaryButtonText = "Back".Language(),
			}.ShowAsync();
		}

		private async void SignUpItem_Click(object sender, RoutedEventArgs e) {
			await Methods.OpenBrowser($"https://{Data.GetHost()}/users/new");
		}

		private async void ResetPasswordItem_Click(object sender, RoutedEventArgs e) {
			await Methods.OpenBrowser($"https://{Data.GetHost()}/maintenance/user/password_reset/new");
		}

		private async void LoginReminderItem_Click(object sender, RoutedEventArgs e) {
			await Methods.OpenBrowser($"https://{Data.GetHost()}/maintenance/user/login_reminder/new");
		}

		void IPage.UpdateNavigationItem() {
			MainPage.Instance.currentTag = PageTag.UserProfile;
			MainPage.Instance.UpdateNavigationItem();
		}

		void IPage.FocusMode(bool enabled) { }

		private async void UsernameBox_Loaded(object sender, RoutedEventArgs e) {
			await Task.Delay(100);
			UsernameBox.Focus(FocusState.Programmatic);
		}
	}
}
