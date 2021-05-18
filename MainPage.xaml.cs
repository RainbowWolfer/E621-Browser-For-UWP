using E621Downloader.Models;
using E621Downloader.Pages;
using E621Downloader.Views;
//using Microsoft.UI.Xaml.Controls;
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
using Windows.UI.ViewManagement;
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
		public static MainPage Instance;
		private const string url = "https://e621.net/posts?tags=skyleesfm+order%3Ascore";
		public PostsBrowser postsBrowser;

		public bool IsFullScreen {
			get => ApplicationView.GetForCurrentView().IsFullScreenMode;
			set {
				FullScreenButton.Content = value ? "\uE73F" : "\uE740";
				var view = ApplicationView.GetForCurrentView();
				if(view.IsFullScreenMode) {
					view.ExitFullScreenMode();
					ApplicationView.PreferredLaunchWindowingMode = ApplicationViewWindowingMode.Auto;
				} else {
					if(view.TryEnterFullScreenMode()) {
						ApplicationView.PreferredLaunchWindowingMode = ApplicationViewWindowingMode.FullScreen;
					}
				}
			}
		}
		public PageTag currentTag;

		//private 

		//public const string HOME = "Home";
		//public const string PICTURE = "Picture";
		//public const string SLIDESHOW = "SlideShow";
		//public const string SETTINGS = "Settings";

		public object parameter_picture;

		//public string currentPage;

		public MainPage() {
			Instance = this;
			this.InitializeComponent();
			currentTag = PageTag.Settings;
			//MyFrame.Navigate(typeof(PostsBrowser), null, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromRight });
		}

		public static void ChangeCurrenttTags(params string[] strs) {
			string result = "Current Tags : ";
			foreach(string item in strs) {
				result += item + " ";
			}
			Instance.CurrentTagsTextBlock.Text = result;
		}

		public static ContentDialog InstanceDialog { get; private set; }
		public static bool IsShowingInstanceDialog { get; private set; }

		public async static void CreateInstantDialog(string title, object content) {
			if(InstanceDialog != null) {
				return;
			}
			IsShowingInstanceDialog = false;
			InstanceDialog = new ContentDialog() {
				Title = title,
				Content = content,
			};

			await InstanceDialog.ShowAsync();
			IsShowingInstanceDialog = true;
		}
		public static void HideInstantDialog() {
			if(InstanceDialog == null) {
				return;
			}
			InstanceDialog.Hide();
			InstanceDialog = null;
			IsShowingInstanceDialog = false;
		}

		public async static void CreateTip(string titile, string subtitle, int delayTime = 5000) {
			var tip = new Microsoft.UI.Xaml.Controls.TeachingTip() {
				Title = titile,
				Subtitle = subtitle,
				Target = Instance.CurrentTagsTextBlock,
				IconSource = new Microsoft.UI.Xaml.Controls.SymbolIconSource() {
					Symbol = Symbol.Accept
				},
				IsOpen = true,
			};
			await Task.Delay(delayTime);
			tip.IsOpen = false;
		}

		public static async Task<ContentDialog> CreatePopupDialog(string title, object content, bool enableButton = true) {
			ContentDialog dialog = new ContentDialog() {
				Title = title,
				Content = content,
				IsPrimaryButtonEnabled = enableButton,
				PrimaryButtonText = enableButton ? "Back" : "",
			};
			await dialog.ShowAsync();
			return dialog;
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
				SelectNavigationItem(PageTag.Home);
				await Task.Delay(100);
				string text = (dialog.Content as SearchPopup).GetSearchText();
				//LoadPosts(Data.GetPostsByTags(1, text));
				//PostsBrowser.Instance.LoadPosts(Post.GetPostsByTags(1, text), text);
				await PostsBrowser.Instance.LoadAsync(1, text);
			}
		}

		public static NavigationTransitionInfo CalculateTransition(PageTag from, PageTag to) {
			if((int)from - (int)to < 0) {
				return new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromRight };
			} else {
				return new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromLeft };
			}
		}

		public static void SelectNavigationItem(PageTag tag) {
			(Instance.MyNavigationView.MenuItems.ToList().Find((i) => int.Parse((string)(i as NavigationViewItem).Tag) == (int)tag) as NavigationViewItem).IsSelected = true;
		}

		private void MyNavigationView_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args) {
			if(args.IsSettingsInvoked) {
				if(currentTag == PageTag.Settings) {
					return;
				}
				MyFrame.Navigate(typeof(SettingsPage), null, CalculateTransition(currentTag, PageTag.Settings));
				currentTag = PageTag.Settings;
				return;
			}
			PageTag tag = (PageTag)int.Parse((string)args.InvokedItemContainer.Tag);
			if(currentTag == tag) {
				return;
			}
			if(tag == PageTag.Home) {
				MyFrame.Navigate(typeof(PostsBrowser), null, CalculateTransition(currentTag, PageTag.Home));
			} else if(tag == PageTag.Picture) {
				MyFrame.Navigate(typeof(PicturePage), parameter_picture, CalculateTransition(currentTag, PageTag.Picture));
			} else if(tag == PageTag.ShowSlider) {
				MyFrame.Navigate(typeof(SlideshowPage), null, CalculateTransition(currentTag, PageTag.ShowSlider));
			} else if(tag == PageTag.Subscription) {
				MyFrame.Navigate(typeof(SubscriptionPage), null, CalculateTransition(currentTag, PageTag.Subscription));
			} else if(tag == PageTag.Download) {
				MyFrame.Navigate(typeof(DownloadPage), null, CalculateTransition(currentTag, PageTag.Download));
			} else {
				throw new Exception("Tag Error");
			}
			currentTag = tag;
		}

		private void FullScreenButton_Tapped(object sender, TappedRoutedEventArgs e) {
			IsFullScreen = !IsFullScreen;
		}
	}
	public enum PageTag {
		Home, Picture, ShowSlider, Subscription, Download, Settings
	}
}

