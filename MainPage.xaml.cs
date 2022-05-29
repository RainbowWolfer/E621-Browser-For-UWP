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
using Windows.ApplicationModel.Core;
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
using NavigationViewBackRequestedEventArgs = Microsoft.UI.Xaml.Controls.NavigationViewBackRequestedEventArgs;
using NavigationViewItem = Microsoft.UI.Xaml.Controls.NavigationViewItem;
using NavigationViewItemInvokedEventArgs = Microsoft.UI.Xaml.Controls.NavigationViewItemInvokedEventArgs;
using SymbolIconSource = Microsoft.UI.Xaml.Controls.SymbolIconSource;

namespace E621Downloader {
	public sealed partial class MainPage: Page {
		public event Action UserStartChanging;
		public event Action UserChangedInfoComplete;
		public event Action<BitmapImage> UserChangedAvatarComplete;

		public static MainPage Instance;
		//public PostsBrowser postsBrowser;

		public bool IsFullScreen {
			get => ApplicationView.GetForCurrentView().IsFullScreenMode;
			set {
				FullScreenIcon.Glyph = value ? "\uE73F" : "\uE740";
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
		private object parameter_library;

		public bool IsInSearchPopup { get; private set; }
		public bool IsInputting { get; private set; } = false;

		public const VirtualKey SEARCH_KEY = VirtualKey.Q;

		private bool isInMaintenance = false;

		public MainPage() {
			Instance = this;
			this.InitializeComponent();

			var coreTitleBar = CoreApplication.GetCurrentView().TitleBar;
			coreTitleBar.ExtendViewIntoTitleBar = true;
			Window.Current.SetTitleBar(AppTitleBar);

			currentTag = PageTag.Welcome;
			KeyListener.SubmitInstance(new KeyListenerInstance(async key => {
				if(IsInputting || isInMaintenance) {
					return;
				}
				if(key == SEARCH_KEY && !IsInSearchPopup) {
					try {
						await PopupSearch();
					} catch {
						IsInSearchPopup = false;
					}
				}
			}));
			Loop();

			//ApplicationViewTitleBar titleBar = ApplicationView.GetForCurrentView().TitleBar;
		}

		protected override async void OnNavigatedTo(NavigationEventArgs e) {
			base.OnNavigatedTo(e);
			CreateInstantDialog("Please Wait", "Initializing Local Stuff");
			await Local.Initialize();

			HttpResult<string> result;
			UpdateInstanceDialogContent($"Checking Connection to {Data.GetHost()}");
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
				if(data.Contains("performing maintenance")/* || true*/) {
					MyNavigationView.IsPaneVisible = false;
					NavigateTo(PageTag.Maintenance);
					HideInstantDialog();
					isInMaintenance = true;
					return;
				} else {
					MyNavigationView.IsPaneVisible = true;
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
			}
			HideInstantDialog();
			await Task.Delay(20);
			if(number.Length > 18) {
				number = long.MaxValue.ToString();
			}
			MyFrame.Navigate(typeof(WelcomePage), string.IsNullOrWhiteSpace(number) ? (long)-1 : long.Parse(number));
			ChangeUser(LocalSettings.Current.user_username);
		}

		private int downloadDelayedTime = 0;
		private async void Loop() {
			const int DELAY = 200;
			const int WAIT_TIME = 3000;
			while(true) {
				if(DownloadsManager.HasDownloading()) {
					ChangeDownloadRingIcon(LoadingType.Loading);
					downloadDelayedTime = WAIT_TIME;
				} else {
					if(downloadDelayedTime <= 0) {
						downloadDelayedTime = 0;
						ChangeDownloadRingIcon(LoadingType.None);
					} else {
						ChangeDownloadRingIcon(LoadingType.Done);
					}
					downloadDelayedTime -= DELAY;
				}
				await Task.Delay(DELAY);
			}
		}

		private CancellationTokenSource delayInputListener_cts;
		public async void DelayInputKeyListener(int delay = 500) {
			IsInputting = true;
			if(delayInputListener_cts != null) {
				delayInputListener_cts.Cancel();
				delayInputListener_cts.Dispose();
			}
			delayInputListener_cts = new CancellationTokenSource();
			try {
				await Task.Delay(delay, delayInputListener_cts.Token);
			} catch(TaskCanceledException) {
				return;
			}
			IsInputting = false;
		}

		private bool updatingUser = false;
		public async void ChangeUser(string username) {
			if(updatingUser) {
				return;
			}
			UserPicture.ProfilePicture = App.DefaultAvatar;
			if(string.IsNullOrWhiteSpace(username)) {
				E621User.Current = null;
				ToolTipService.SetToolTip(UserIconItem, null);
				UserChangedInfoComplete?.Invoke();
				UserChangedAvatarComplete?.Invoke(UserPicture.ProfilePicture as BitmapImage);
				PicturePage.ClearVoted();
				return;
			}
			ToolTipService.SetToolTip(UserIconItem, username);
			UserStartChanging?.Invoke();
			E621User.Current = await E621User.GetAsync(username);
			UserChangedInfoComplete?.Invoke();
			string url = await E621User.GetAvatarURLAsync(E621User.Current);
			if(string.IsNullOrWhiteSpace(url)) {
				UserChangedAvatarComplete?.Invoke(UserPicture.ProfilePicture as BitmapImage);
				return;
			}
			BitmapImage image = new(new Uri(this.BaseUri, url));
			image.ImageOpened += (s, e) => {
				UserChangedAvatarComplete?.Invoke(image);
				updatingUser = false;
			};
			UserPicture.ProfilePicture = image;
		}

		public static BitmapImage GetUserIcon() {
			return (BitmapImage)Instance.UserPicture.ProfilePicture;
		}

		public static void ChangeDownloadRingIcon(LoadingType type) {
			Instance.DownloadRingIcon.LoadingType = type;
		}

		public static void ChangeCurrentTags(params string[] strs) {
			string result = E621Tag.JoinTags(strs.Where(s => !string.IsNullOrWhiteSpace(s)).ToArray());
			if(string.IsNullOrWhiteSpace(result)) {
				result = "Tags";
			}
			Instance.TextBlockSearchTags.Text = result;
		}

		public static string[] GetCurrentTags() => PostsBrowserPage.GetCurrentTags();

		public static ContentDialog InstanceDialog { get; private set; }
		public static bool IsShowingInstanceDialog { get; private set; }

		public static async void CreateInstantDialog(string title, object content) {
			if(InstanceDialog != null) {
				return;
			}
			IsShowingInstanceDialog = false;
			InstanceDialog = new ContentDialog() {
				Title = title,
				Content = content,
			};
			_ = await InstanceDialog.ShowAsync();
			IsShowingInstanceDialog = true;
		}

		public static void UpdateInstanceDialogContent(object content) {
			if(InstanceDialog == null) {
				return;
			}
			InstanceDialog.Content = content;
		}

		public static void HideInstantDialog() {
			if(InstanceDialog == null) {
				return;
			}
			InstanceDialog.Hide();
			InstanceDialog = null;
			IsShowingInstanceDialog = false;
		}

		public static async void CreateTip_SuccessDownload(Page page) {
			if(page.Content is Panel panel) {
				var tip = new TeachingTip() {
					Title = "Notification",
					Subtitle = "Successfully Start Downloading",
					IconSource = new SymbolIconSource() {
						Symbol = Symbol.Accept,
					},
					PreferredPlacement = TeachingTipPlacementMode.TopRight,
					PlacementMargin = new Thickness(20),
					IsLightDismissEnabled = false,
					ActionButtonContent = "Go to downloads",
					CloseButtonContent = "Got it",
					IsOpen = true,
				};
				tip.ActionButtonClick += (sender, args) => {
					NavigateTo(PageTag.Download);
				};
				panel.Children.Add(tip);
				await Task.Delay(5000);
				tip.IsOpen = false;
				tip.Closed += (s, e) => {
					panel.Children.Remove(tip);
				};
			} else {
				throw new Exception("Page's Content is not a valid Panel");
			}
		}

		public static async void CreateTip(Panel parent, string titile, string subtitle, Symbol? icon = null, string closeText = "Got it!", bool isLightDismissEnabled = false, TeachingTipPlacementMode placement = TeachingTipPlacementMode.TopRight, Thickness? margin = null, int delayTime = 5000) {
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
			ContentDialog dialog = new() {
				Title = title,
				Content = content,
				IsPrimaryButtonEnabled = enableButton,
				PrimaryButtonText = enableButton ? backButtonContent : "",
			};
			try {
				await dialog.ShowAsync();
			} catch { }
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

		public static void NavigateToLibrary(string folderName) {
			Instance.parameter_library = folderName;
			Instance.JumpToPage(PageTag.Library);
			ClearLibraryPageParameter();
		}

		public static void ClearPostBrowserParameter() {
			Instance.parameter_postBrowser = null;
		}

		public static void ClearPicturePageParameter() {
			Instance.parameter_picture = null;
		}

		public static void ClearLibraryPageParameter() {
			Instance.parameter_library = null;
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

		private void JumpToPage(PageTag tag, bool updateNavigationViewItem = true) {
			NavigationTransitionInfo transition = CalculateTransition(currentTag, tag);
			Type target;
			object parameter;
			switch(tag) {
				case PageTag.PostsBrowser:
					target = typeof(PostsBrowserPage);
					parameter = parameter_postBrowser;
					break;
				case PageTag.Picture:
					target = typeof(PicturePage);
					parameter = parameter_picture;
					break;
				case PageTag.Library:
					if(Local.DownloadFolder == null) {
						target = typeof(LibraryNoFolderPage);
					} else {
						target = typeof(LibraryPage);
					}
					parameter = parameter_library;
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
				case PageTag.Maintenance:
					target = typeof(MaintenancePage);
					parameter = null;
					break;
				default:
					throw new Exception("Tag Not Found");
			}

			MyFrame.Navigate(target, parameter, transition);

			currentTag = tag;
			if(updateNavigationViewItem) {
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

		private CancellationTokenSource cts = new();

		private async void CurrentTagsButton_Tapped(object sender, TappedRoutedEventArgs e) {
			await PopupSearch();
		}

		public async Task PopupSearch() {
			if(IsInSearchPopup) {
				return;
			}
			IsInSearchPopup = true;
			var dialog = new ContentDialog() {
				Title = "Manage Your Search Tags",
			};

			var view = new TagsSelectionView(dialog, PostsBrowserPage.GetCurrentTags());
			dialog.Content = view;

			await dialog.ShowAsync();
			switch(view.Result) {
				case TagsSelectionView.ResultType.None:
					break;
				case TagsSelectionView.ResultType.Search:
					string[] tags = view.GetTags();
					NavigateToPostsBrowser(1, tags);
					Local.History.AddTag(tags);
					break;
				case TagsSelectionView.ResultType.Hot:
					NavigateToPostsBrowser(1, "order:rank");
					Local.History.AddTag("order:rank");
					break;
				case TagsSelectionView.ResultType.Random:
					CreateInstantDialog("Please Waiting", "Getting Your Tag");

					//const int MAX_LOOP = 10;
					//int index = 0;
					//string tag = null;
					//List<Post> posts;
					//do {
					//	posts = await Post.GetPostsByTagsAsync(cts.Token, 1, $"limit:5", "order:random");
					//	if(posts == null || posts.Count == 0) {
					//		continue;
					//	}
					//	foreach(Post item in posts) {

					//	}
					//	index++;
					//} while(tag == null || index > MAX_LOOP);

					Post post = (await Post.GetPostsByTagsAsync(cts.Token, 1, "limit:1", "order:random"))?.FirstOrDefault();
					HideInstantDialog();
					if(post != null) {
						List<string> all = post.tags.GetAllTags();
						string tag = all[new Random().Next(all.Count)];
						NavigateToPostsBrowser(1, tag);
						Local.History.AddTag(tag);
					}
					break;
				default:
					throw new Exception("Result Type not found");
			}
			IsInSearchPopup = false;
		}

		private void MyFrame_Navigated(object sender, NavigationEventArgs e) {
			MyNavigationView.IsBackEnabled = MyFrame.CanGoBack;
		}

		private void MyNavigationView_BackRequested(NavigationView sender, NavigationViewBackRequestedEventArgs args) {
			if(!MyFrame.CanGoBack) {
				return;
			}
			MyFrame.GoBack();

		}

		private void MyFrame_NavigationFailed(object sender, NavigationFailedEventArgs e) {

		}

	}
	public enum PageTag {
		PostsBrowser = 0, Picture = 1, Library = 2, Subscription = 3, Spot = 4, Download = 5, UserProfile = 6, Welcome = 7, Settings, Maintenance
	}
}

