using E621Downloader.Models;
using E621Downloader.Models.Download;
using E621Downloader.Models.Locals;
using E621Downloader.Models.Networks;
using E621Downloader.Models.Posts;
using E621Downloader.Pages;
using E621Downloader.Pages.DownloadSection;
using E621Downloader.Pages.LibrarySection;
using E621Downloader.Views;
using E621Downloader.Views.TagsManagementSection;
using Microsoft.UI.Xaml.Controls;
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
using NavigationView = Windows.UI.Xaml.Controls.NavigationView;
using NavigationViewItem = Windows.UI.Xaml.Controls.NavigationViewItem;
using NavigationViewItemInvokedEventArgs = Windows.UI.Xaml.Controls.NavigationViewItemInvokedEventArgs;
using SymbolIconSource = Microsoft.UI.Xaml.Controls.SymbolIconSource;

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

		public object parameter_picture;
		private PostBrowserParameter parameter_postBrowser;

		public MainPage() {
			Instance = this;
			this.InitializeComponent();
			currentTag = PageTag.Welcome;
		}

		protected async override void OnNavigatedTo(NavigationEventArgs e) {
			base.OnNavigatedTo(e);
			CreateInstantDialog("Please Wait", "Initializing Local Program");
			int time = 0;
			while(Local.DownloadFolder == null) {
				await Task.Delay(10);
				time += 1;
				if(time >= 200) {
					HideInstantDialog();
					await CreatePopupDialog("Warning", "Download Folder Not Found", true);
					CreateInstantDialog("Please Wait", "Initializing Local Program");
					break;
				}
			}
			string data = await Data.ReadURLAsync("https://e621.net/");
			while(string.IsNullOrEmpty(data)) {
				HideInstantDialog();
				await CreatePopupDialog("Error", "No Internet Connection", true, "Retry");
				CreateInstantDialog("Please Wait", "Checking Internet");
				data = await Data.ReadURLAsync("https://e621.net/");
			}
			int start = data.IndexOf("Serving ") + 8;
			string result = "";
			for(int i = start; i < data.Length; i++) {
				if(data[i] == ',') {
					continue;
				}
				if(char.IsDigit(data[i])) {
					result += data[i];
				}
			}
			HideInstantDialog();
			await Task.Delay(20);

			CreateInstantDialog("Please Wait", "Checking Unfinished Downloads");
			if(Local.DownloadFolder != null) {
				//await DownloadsManager.RestoreIncompletedDownloads();
			}
			HideInstantDialog();

			MyFrame.Navigate(typeof(WelcomePage), long.Parse(result));
			//(MyNavigationView.MenuItems[0] as NavigationViewItem).IsSelected = true;
		}

		public static void ChangeCurrenttTags(params string[] strs) {
			string result = "Current Tags : ";
			foreach(string item in strs) {
				result += item + " ";
			}
			Instance.TextBlockSearchTags.Text = result;
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

		public async static void CreateTip(Panel parent, string titile, string subtitle, Symbol? icon = null, string closeText = "Got it!", bool isLightDismissEnabled = false, TeachingTipPlacementMode placement = TeachingTipPlacementMode.TopRight, Thickness? margin = null, int delayTime = 5000) {
			var tip = new TeachingTip() {
				Title = titile,
				Subtitle = subtitle,
				IconSource = icon == null ? null : new SymbolIconSource() {
					Symbol = icon.Value
				},
				PreferredPlacement = placement,
				PlacementMargin = margin == null ? new Thickness(20) : margin.Value,
				IsLightDismissEnabled = isLightDismissEnabled,
				CloseButtonContent = closeText,
				IsOpen = true,
			};
			parent.Children.Add(tip);
			if(delayTime > 0) {
				await Task.Delay(delayTime);
				tip.IsOpen = false;
			}
			tip.Closed += (s, e) => {
				parent.Children.Remove(tip);
			};
		}
		public static void CreateTip(Page page, string titile, string subtitle, Symbol? icon = null, string closeText = "Got it!", bool isLightDismissEnabled = false, TeachingTipPlacementMode placement = TeachingTipPlacementMode.TopRight, Thickness? margin = null, int delayTime = 5000) {
			if(page.Content is Panel panel) {
				CreateTip(panel, titile, subtitle, icon, closeText, isLightDismissEnabled, placement, margin, delayTime);
			} else {
				throw new Exception("Page's Content is not a valid Panel");
			}
		}

		public static async Task<ContentDialog> CreatePopupDialog(string title, object content, bool enableButton = true, string backButtonContent = "Back") {
			ContentDialog dialog = new ContentDialog() {
				Title = title,
				Content = content,
				IsPrimaryButtonEnabled = enableButton,
				PrimaryButtonText = enableButton ? backButtonContent : "",
			};
			await dialog.ShowAsync();
			return dialog;
		}

		public static NavigationTransitionInfo CalculateTransition(PageTag from, PageTag to) {
			return (int)from - (int)to < 0
				? new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromRight }
				: new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromLeft };
		}

		public static void SelectNavigationItem(PageTag tag) {
			(Instance.MyNavigationView.MenuItems.ToList().Find((i) => int.Parse((string)(i as NavigationViewItem).Tag) == (int)tag) as NavigationViewItem).IsSelected = true;
		}

		public static void NavigateToPostsBrowser(int page, params string[] tags) {
			Instance.parameter_postBrowser = new PostBrowserParameter(page, tags);
			if(Instance.currentTag == PageTag.PostsBrowser) {
				Instance.MyFrame.Navigate(typeof(PostsBrowser), Instance.parameter_postBrowser, CalculateTransition(Instance.currentTag, PageTag.PostsBrowser));
			} else {
				SelectNavigationItem(PageTag.PostsBrowser);
			}
			ClearPostBrowserParameter();
		}

		public static void ClearPostBrowserParameter() {
			Instance.parameter_postBrowser = null;
		}

		/// <summary> Used to solve the problem of navtigating to self in PicturePage </summary>
		public static void NavigateToPicturePage() {
			Instance.MyFrame.Navigate(typeof(PicturePage), Instance.parameter_picture, CalculateTransition(Instance.currentTag, PageTag.Picture));
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
			if(tag == PageTag.PostsBrowser) {
				MyFrame.Navigate(typeof(PostsBrowser), parameter_postBrowser, CalculateTransition(currentTag, PageTag.PostsBrowser));
			} else if(tag == PageTag.Picture) {
				MyFrame.Navigate(typeof(PicturePage), parameter_picture, CalculateTransition(currentTag, PageTag.Picture));
			} else if(tag == PageTag.Library) {
				MyFrame.Navigate(typeof(LibraryPage), null, CalculateTransition(currentTag, PageTag.Library));
			} else if(tag == PageTag.Subscription) {
				MyFrame.Navigate(typeof(SubscriptionPage), null, CalculateTransition(currentTag, PageTag.Subscription));
			} else if(tag == PageTag.Spot) {
				MyFrame.Navigate(typeof(SpotPage), null, CalculateTransition(currentTag, PageTag.Spot));
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

		private async void CurrentTagsButton_Tapped(object sender, TappedRoutedEventArgs e) {
			var dialog = new ContentDialog() {
				Title = "Manage Your Search Tags",
			};
			var view = new TagsSelectionView(dialog, PostsBrowser.Instance?.tags ?? Array.Empty<string>());
			dialog.Content = view;

			await dialog.ShowAsync();
			switch(view.Result) {
				case TagsSelectionView.ResultType.None:
					break;
				case TagsSelectionView.ResultType.Search:
					NavigateToPostsBrowser(1, view.GetTags());
					break;
				case TagsSelectionView.ResultType.Hot:
					NavigateToPostsBrowser(1, "order:rank");
					break;
				case TagsSelectionView.ResultType.Random:
					CreateInstantDialog("Please Waiting", "Getting Your Tag");
					Post post = (await Post.GetPostsByTagsAsync(1, "limit:1", "order:random"))?.FirstOrDefault();
					HideInstantDialog();
					if(post != null) {
						List<string> all = post.tags.GetAllTags();
						string tag = all[new Random().Next(all.Count)];
						NavigateToPostsBrowser(1, tag);
					}
					break;
				default:
					throw new Exception("Result Type not found");
			}
		}
	}
	public enum PageTag {
		PostsBrowser, Picture, Library, Subscription, Spot, Download, Settings, Welcome,
	}
}

