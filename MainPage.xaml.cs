using E621Downloader.Models;
using E621Downloader.Pages;
using E621Downloader.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.System;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

namespace E621Downloader {
	public sealed partial class MainPage: Page {
		private const string url = "https://e621.net/posts?tags=skyleesfm+order%3Ascore";
		public PostsBrowser postsBrowser;

		public MainPage() {
			this.InitializeComponent();
			MyFrame.Navigate(typeof(PostsBrowser), null, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromRight });
			
		}

		private async void Page_Loaded(object sender, RoutedEventArgs e) {
			string result = await ReadFromTestFile();
			//LoadPosts(Data.GetPostsByTags(1, ""));
			(MyNavigationView.MenuItems[0] as NavigationViewItem).IsSelected = true;
		}

		private async Task<string> ReadFromTestFile() {
			StorageFolder InstallationFolder = Package.Current.InstalledLocation;
			StorageFile file = await InstallationFolder.GetFileAsync(@"Assets\TestText_Copy.txt");
			return File.ReadAllText(file.Path);
		}
		private async void SearchButton_Tapped(object sender, TappedRoutedEventArgs e) {
			var dialog = new ContentDialog() {
				Title = "Search Section",
				PrimaryButtonText = "Confirm",
				SecondaryButtonText = "Cancel",
			};
			dialog.Content = new SearchPopup(dialog);
			dialog.KeyDown += (s, c) => {
				if(c.Key == VirtualKey.Escape) {
					dialog.Hide();
				}
			};
			ContentDialogResult result = await dialog.ShowAsync();
			if(result == ContentDialogResult.Primary) {
				string text = (dialog.Content as SearchPopup).GetSearchText();
				//LoadPosts(Data.GetPostsByTags(1, text));
				PostsBrowser.Instance.LoadPosts(Data.GetPostsByTags(1, text));
			}
		}
	}
}

