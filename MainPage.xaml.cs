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
using System.Threading;
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
using NavigationView = Microsoft.UI.Xaml.Controls.NavigationView;
using NavigationViewItem = Microsoft.UI.Xaml.Controls.NavigationViewItem;
using NavigationViewItemInvokedEventArgs = Microsoft.UI.Xaml.Controls.NavigationViewItemInvokedEventArgs;
using SymbolIconSource = Microsoft.UI.Xaml.Controls.SymbolIconSource;

namespace E621Downloader {
	public sealed partial class MainPage: Page {
		public event Action UserStartChanging;
		public event Action UserChangedInfoComplete;
		public event Action<BitmapImage> UserChangedAvatarComplete;

		public static MainPage Instance;
		public PostsBrowser postsBrowser;

		public bool IsFullScreen {
			get => ApplicationView.GetForCurrentView().IsFullScreenMode;
			set {
				FullScreenButton.Content = value ? "\uE73F" : "\uE740";
				ApplicationView view = ApplicationView.GetForCurrentView();
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

		private object parameter_picture;
		private object parameter_postBrowser;

		public MainPage() {
			Instance = this;
			this.InitializeComponent();
			currentTag = PageTag.Welcome;
		}

		protected async override void OnNavigatedTo(NavigationEventArgs e) {
			base.OnNavigatedTo(e);
			CreateInstantDialog("Please Wait", "Initializing...");
			await Local.Initialize();
			HttpResult<string> result;
			do {
				result = await Data.ReadURLAsync($"https://{Data.GetHost()}/", null);
				if(result.Result == HttpResultType.Success) {
					break;
				}
				HideInstantDialog();
				ContentDialogResult dialogResult = await new ContentDialog() {
					Title = "Error",
					Content = "No Internet Connection",
					SecondaryButtonText = "Retry",
					PrimaryButtonText = "Start in Offline Mode",
				}.ShowAsync();
				if(dialogResult == ContentDialogResult.Primary) {
					break;
				}
				CreateInstantDialog("Please Wait", "Checking Internet");
			} while(result.Result == HttpResultType.Error);
			string number = "";
			if(result.Result == HttpResultType.Success) {
				string data = result.Content;
				int start = data.IndexOf("Serving ") + 8;
				for(int i = start; i < data.Length; i++) {
					if(data[i] == ',') {
						continue;
					}
					if(char.IsDigit(data[i])) {
						number += data[i];
					}
				}
			}
			HideInstantDialog();
			await Task.Delay(20);

			MyFrame.Navigate(typeof(WelcomePage), string.IsNullOrWhiteSpace(number) ? (long)-1 : long.Parse(number));
			ChangeUser(LocalSettings.Current.user_username);
		}

		private bool updatingUser = false;
		public async void ChangeUser(string username) {
			if(updatingUser) {
				return;
			}
			UserPicture.ProfilePicture = App.DefaultAvatar;
			if(string.IsNullOrWhiteSpace(username)) {
				ToolTipService.SetToolTip(UserIconItem, null);
				UserChangedInfoComplete?.Invoke();
				UserChangedAvatarComplete?.Invoke(UserPicture.ProfilePicture as BitmapImage);
				return;
			}
			ToolTipService.SetToolTip(UserIconItem, username);
			UserStartChanging?.Invoke();
			E621User.Current = await E621User.GetAsync(username);
			UserChangedInfoComplete?.Invoke();
			string url = await E621User.GetAvatarURLAsync(E621User.Current);
			BitmapImage image = new BitmapImage(new Uri(this.BaseUri, url));
			image.ImageOpened += (s, e) => {
				UserChangedAvatarComplete?.Invoke(image);
				updatingUser = false;
			};
			UserPicture.ProfilePicture = image;
		}
		public static BitmapImage GetUserIcon() {
			return (BitmapImage)Instance.UserPicture.ProfilePicture;
		}

		public static void ChangeCurrenttTags(params string[] strs) {
			string result = "";
			foreach(string item in strs) {
				result += item + " ";
			}
			if(string.IsNullOrWhiteSpace(result)){
				result = "Tags";
			}
			Instance.TextBlockSearchTags.Text = result;
		}

		public static string[] GetCurrentTags() {
			return PostsBrowser.Instance?.Tags ?? Array.Empty<string>();
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

		public static void CreateTip_SuccessDownload(Page page) {
			CreateTip(page, "Notification", "Successfully Start Downloading", Symbol.Accept);
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

		private static NavigationTransitionInfo CalculateTransition(PageTag from, PageTag to) {
			if(from == to) {
				//return new DrillInNavigationTransitionInfo();
				return new EntranceNavigationTransitionInfo();
			} else if(from == PageTag.Welcome) {
				return new EntranceNavigationTransitionInfo();
			} else {
				return new SlideNavigationTransitionInfo() {
					Effect = (int)from - (int)to < 0 ? SlideNavigationTransitionEffect.FromRight : SlideNavigationTransitionEffect.FromLeft
				};
			}
		}
		public static void NavigateTo(PageTag tag) {
			Instance.JumpToPage(tag);
		}

		public static void NavigateToPostsBrowser(int page, params string[] tags) {
			Instance.parameter_postBrowser = new PostBrowserParameter(page, tags);
			Instance.JumpToPage(PageTag.PostsBrowser);
			ClearPostBrowserParameter();
		}
		public static void NavigateToPostsBrowser(E621Pool pool) {
			Instance.parameter_postBrowser = pool;
			Instance.JumpToPage(PageTag.PostsBrowser);
			ClearPostBrowserParameter();
		}

		/// <summary> Used to solve the problem of navtigating to self in PicturePage </summary>
		public static void NavigateToPicturePage(object parameter = null) {
			Instance.parameter_picture = parameter;
			Instance.JumpToPage(PageTag.Picture);
			ClearPicturePageParameter();
		}

		public static void ClearPostBrowserParameter() {
			Instance.parameter_postBrowser = null;
		}

		public static void ClearPicturePageParameter() {
			Instance.parameter_picture = null;
		}

		//click from NavitaionViewItem
		private void MyNavigationView_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args) {
			string tag = args.InvokedItemContainer.Tag as string;
			PageTag pageTag = Convert(tag) ?? throw new Exception("Tag Not Found");
			if(currentTag == pageTag) {
				return;
			}
			JumpToPage(pageTag, false);
		}

		private PageTag? Convert(string tag) {
			if(string.IsNullOrWhiteSpace(tag)) {
				return null;
			}
			if(int.TryParse(tag, out int tagInt)) {
				return (PageTag)tagInt;
			} else if(tag == "Settings") {
				return PageTag.Settings;
			} else {
				return null;
			}
		}

		private void JumpToPage(PageTag tag, bool updateNavigationVieItem = true) {
			NavigationTransitionInfo transition = CalculateTransition(currentTag, tag);
			Type target;
			object parameter;
			switch(tag) {
				case PageTag.PostsBrowser:
					target = typeof(PostsBrowser);
					parameter = parameter_postBrowser;
					break;
				case PageTag.Picture:
					target = typeof(PicturePage);
					parameter = parameter_picture;
					Debug.WriteLine($"PICTURE: {parameter == null}");
					break;
				case PageTag.Library:
					target = typeof(LibraryPage);
					parameter = null;
					break;
				case PageTag.Subscription:
					target = typeof(SubscriptionPage);
					parameter = null;
					break;
				case PageTag.Spot:
					target = typeof(SpotPage);
					parameter = null;
					break;
				case PageTag.Download:
					target = typeof(DownloadPage);
					parameter = null;
					break;
				case PageTag.UserProfile:
					if(LocalSettings.Current.CheckLocalUser()) {
						target = typeof(UserProfilePage);
					} else {
						target = typeof(LoginPage);
					}
					parameter = null;
					break;
				case PageTag.Welcome:
					target = typeof(WelcomePage);
					parameter = null;
					break;
				case PageTag.Settings:
					target = typeof(SettingsPage);
					parameter = null;
					break;
				default:
					throw new Exception("Tag Not Found");
			}
			MyFrame.Navigate(target, parameter, transition);
			currentTag = tag;
			if(updateNavigationVieItem) {
				foreach(NavigationViewItem item in MyNavigationView.MenuItems
					.Concat(MyNavigationView.FooterMenuItems)
					.Append(MyNavigationView.SettingsItem)
				) {
					if(Convert(item.Tag as string) == currentTag) {
						item.IsSelected = true;
						break;
					}
				}
			}
		}

		private void FullScreenButton_Tapped(object sender, TappedRoutedEventArgs e) {
			IsFullScreen = !IsFullScreen;
		}


		private CancellationTokenSource cts = new CancellationTokenSource();

		private async void CurrentTagsButton_Tapped(object sender, TappedRoutedEventArgs e) {
			var dialog = new ContentDialog() {
				Title = "Manage Your Search Tags",
			};
			var view = new TagsSelectionView(dialog, PostsBrowser.Instance?.Tags ?? Array.Empty<string>());
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
					Post post = (await Post.GetPostsByTagsAsync(cts.Token, 1, "limit:1", "order:random"))?.FirstOrDefault();
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
		PostsBrowser = 0, Picture = 1, Library = 2, Subscription = 3, Spot = 4, Download = 5, UserProfile = 6, Welcome = 7, Settings,
	}
}

