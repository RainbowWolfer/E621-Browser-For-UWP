using E621Downloader.Models;
using E621Downloader.Views;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.Storage.Pickers;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace E621Downloader.Pages {
	public sealed partial class SettingsPage: Page {
		public string Version {
			get => "Version: " + Assembly.GetExecutingAssembly().GetName().Version.ToString();
		}
		public SettingsPage() {
			this.InitializeComponent();
		}

		private async void BlackListButton_Tapped(object sender, TappedRoutedEventArgs e) {
			var result = await PopUp("Black List", Local.BlackList);
			if(result.Item1 == ContentDialogResult.Primary) {
				if(!App.CompareTwoArray(result.Item2, result.Item3)) {
					Local.WriteBlackList(result.Item3);
				}
			}
		}

		private async void FollowListButton_Tapped(object sender, TappedRoutedEventArgs e) {
			var result = await PopUp("Follow List", Local.FollowList);
			if(result.Item1 == ContentDialogResult.Primary) {
				if(!App.CompareTwoArray(result.Item2, result.Item3)) {
					Local.WriteFollowList(result.Item3);
				}
			}
		}


		private async Task<(ContentDialogResult, string[], string[])> PopUp(string title, string[] list) {
			string[] oldValue = list;
			ContentDialog dialog = new ContentDialog() {
				Title = title,
				PrimaryButtonText = "Confirm",
				SecondaryButtonText = "Cancel",
			};
			var manager = new ListManager(oldValue, dialog);
			dialog.Content = manager;
			ContentDialogResult result = await dialog.ShowAsync();
			string[] newValue = manager.GetCurrentTags();
			return (result, oldValue, newValue);
		}

		private async void DownloadPathButton_Tapped(object sender, TappedRoutedEventArgs e) {
			if(Local.downloadFolder == null) {
				Debug.WriteLine("no download path");
			} else {
				Debug.WriteLine(Local.downloadFolder.Path);
			}
			FolderPicker pick = new FolderPicker() { FileTypeFilter = { "*" } };
			StorageFolder result = await pick.PickSingleFolderAsync();
			if(result != null) {
				string token = StorageApplicationPermissions.FutureAccessList.Add(result);
				Debug.WriteLine(token);
				Local.WriteToken(token);
			}
		}
	}
}
