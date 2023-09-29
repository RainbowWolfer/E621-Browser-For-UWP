﻿using E621Downloader.Models;
using E621Downloader.Models.Debugging;
using E621Downloader.Models.Download;
using E621Downloader.Models.E621;
using E621Downloader.Models.Inerfaces;
using E621Downloader.Models.Locals;
using E621Downloader.Models.Networks;
using E621Downloader.Models.Utilities;
using E621Downloader.Pages;
using E621Downloader.Pages.DownloadSection;
using E621Downloader.Pages.LibrarySection;
using E621Downloader.Views.MainSection;
using E621Downloader.Views.TagsSearch;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Storage.Streams;
using Windows.System;
using Windows.UI.Core.Preview;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
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

		private const string SETTING_ITEM_TAG = "Settings";
		public ScreenMode ScreenMode {
			get => screenMode;
			set {
				screenMode = value;
				ApplicationView view = ApplicationView.GetForCurrentView();
				switch(screenMode) {
					case ScreenMode.Normal:
						FullScreenIcon.Glyph = "\uE1D9";
						view.ExitFullScreenMode();
						ApplicationView.PreferredLaunchWindowingMode = ApplicationViewWindowingMode.Auto;
						MyNavigationView.IsPaneVisible = true;
						(MyFrame.Content as IPage)?.FocusMode(false);
						break;
					case ScreenMode.FullScreen:
						FullScreenIcon.Glyph = "\uE1D8";
						if(view.TryEnterFullScreenMode()) {
							ApplicationView.PreferredLaunchWindowingMode = ApplicationViewWindowingMode.FullScreen;
						}
						MyNavigationView.IsPaneVisible = true;
						(MyFrame.Content as IPage)?.FocusMode(false);
						break;
					case ScreenMode.Focus:
						if(view.TryEnterFullScreenMode()) {
							ApplicationView.PreferredLaunchWindowingMode = ApplicationViewWindowingMode.FullScreen;
						}
						MyNavigationView.IsPaneVisible = false;
						(MyFrame.Content as IPage)?.FocusMode(true);
						break;
					default:
						throw new NotImplementedException();
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

		private readonly List<TeachingTip> downloadsTips = new();

		public MainPage() {
			Instance = this;
			this.InitializeComponent();
			CoreApplication.GetCurrentView().CoreWindow.SizeChanged += (s, e) => {
				if(!ApplicationView.GetForCurrentView().IsFullScreenMode) {
					ScreenMode = ScreenMode.Normal;
				}
			};
			if(!ApplicationView.GetForCurrentView().IsFullScreenMode) {
				ScreenMode = ScreenMode.Normal;
			} else {
				ScreenMode = ScreenMode.FullScreen;
			}

			SettingsKey.ScopeOwner = MyNavigationView.SettingsItem as NavigationViewItem;

			CoreApplicationViewTitleBar coreTitleBar = CoreApplication.GetCurrentView().TitleBar;
			coreTitleBar.ExtendViewIntoTitleBar = true;
			Window.Current.SetTitleBar(AppTitleBar);
			coreTitleBar.IsVisibleChanged += (sender, e) => {
				if(sender.IsVisible) {
					AppTitleBar.Visibility = Visibility.Visible;
				} else {
					AppTitleBar.Visibility = Visibility.Collapsed;
				}
			};

			ApplicationViewTitleBar titleBar = ApplicationView.GetForCurrentView().TitleBar;
			//titleBar.InactiveBackgroundColor = null;

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
				if(key is VirtualKey.Escape or VirtualKey.F12 or VirtualKey.F11) {
					try {
						if(PicturePage.Instance != null) {
							PicturePage.Instance.ShowListGrid = false;
						}
						if(ScreenMode == ScreenMode.Focus) {
							ScreenMode = ScreenMode.Normal;
							PicturePage.Instance?.ExitSlideshow();
						}
					} catch(Exception ex) {
						ErrorHistories.Add(ex);
					}
				}
			}));

			SystemNavigationManagerPreview.GetForCurrentView().CloseRequested += async (sender, args) => {
				if(DownloadsManager.HasDownloading()) {
					args.Handled = true;
					if(await new ContentDialog() {
						Title = "Confirm".Language(),
						Content = "You have unfinished downloads".Language(),
						PrimaryButtonText = "Quit".Language(),
						CloseButtonText = "Back".Language(),
						DefaultButton = ContentDialogButton.Close,
					}.ShowAsync() == ContentDialogResult.Primary) {
						Application.Current.Exit();
					}
				}
			};

			Loop();
		}

		protected override async void OnNavigatedTo(NavigationEventArgs e) {
			base.OnNavigatedTo(e);
			CreateInstantDialog("Please Wait".Language(), "Initializing Local Stuff".Language());
			await Local.Initialize();

			HttpResult<string> result;
			UpdateInstanceDialogContent("Checking Connection to".Language() + $" {Data.GetHost()}");
			do {
				result = await Data.ReadURLAsync($"https://{Data.GetHost()}/", null);
				if(result.Result == HttpResultType.Success) {
					break;
				}
				HideInstantDialog();
				ContentDialogResult dialogResult = await new ContentDialog() {
					Title = "Error".Language(),
					Content = "No Internet Connection".Language(),
					SecondaryButtonText = "Retry".Language(),
					PrimaryButtonText = "Start in Off-line Mode".Language(),
				}.ShowAsync();
				if(dialogResult == ContentDialogResult.Primary) {
					break;
				}
				CreateInstantDialog("Please Wait".Language(), "Checking Internet".Language());
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
			const int WAIT_TIME = 2000;
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
		public async void ChangeUser(string username, Action loadFinish = null) {
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
			E621Post post = await E621User.GetAvatarPostAsync(E621User.Current);
			if(post == null) {
				UserChangedAvatarComplete?.Invoke(UserPicture.ProfilePicture as BitmapImage);
				return;
			}
			string url = post.preview?.url ?? post.sample?.url;
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
			loadFinish?.Invoke();
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

		public static async void CreateInstantDialog(string title, object content, string cancelText = null, Action onCancel = null) {
			if(InstanceDialog != null) {
				return;
			}
			IsShowingInstanceDialog = false;
			InstanceDialog = new ContentDialog() {
				Title = title,
				Content = content,
			};
			if(cancelText != null) {
				InstanceDialog.SecondaryButtonText = cancelText;
			}
			var result = await InstanceDialog.ShowAsync();
			if(result == ContentDialogResult.Secondary && onCancel != null) {
				onCancel.Invoke();
			}
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
					Title = "Notification".Language(),
					Subtitle = "Successfully Start Downloading".Language(),
					IconSource = new SymbolIconSource() {
						Symbol = Symbol.Accept,
					},
					PreferredPlacement = TeachingTipPlacementMode.TopRight,
					PlacementMargin = new Thickness(20),
					IsLightDismissEnabled = false,
					ActionButtonContent = "Go to downloads".Language(),
					CloseButtonContent = "Got it".Language(),
					IsOpen = true,
				};
				tip.ActionButtonClick += (sender, args) => {
					NavigateTo(PageTag.Download);
				};
				panel.Children.Add(tip);

				tip.Closed += (s, e) => {
					panel.Children.Remove(tip);
				};

				tip.CloseButtonClick += (s, e) => {
					foreach(TeachingTip item in Instance.downloadsTips) {
						item.IsOpen = false;
					}
				};

				Instance.downloadsTips.Add(tip);
				await Task.Delay(3000);
				tip.IsOpen = false;
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

		public static void CreateTip(Page page, string titile, string subtitle, Symbol? icon = null, string closeText = "Got it!", bool isLightDismissEnabled = false, TeachingTipPlacementMode placement = TeachingTipPlacementMode.TopRight, Thickness? margin = null, int delayTime = 4000) {
			if(page.Content is Panel panel) {
				CreateTip(panel, titile, subtitle, icon, closeText, isLightDismissEnabled, placement, margin, delayTime);
			} else {
				throw new Exception("Page's Content is not a valid Panel");
			}
		}

		public static async Task<ContentDialog> CreatePopupDialog(string title, object content, bool enableButton = true, string backButtonContent = null) {
			if(string.IsNullOrWhiteSpace(backButtonContent)) {
				backButtonContent = "Back".Language();
			}
			ContentDialog dialog = new() {
				Title = title,
				Content = content,
				IsPrimaryButtonEnabled = enableButton,
				PrimaryButtonText = enableButton ? backButtonContent : "",
			};
			try {
				await dialog.ShowAsync();
			} catch(Exception ex) {
				ErrorHistories.Add(ex);
			}
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

		/// <summary> Used to solve the problem of navigating to self in PicturePage </summary>
		public static void NavigateToPicturePage(object parameter = null, string[] tags = null) {
			Instance.parameter_picture = parameter;
			Instance.JumpToPage(PageTag.Picture);
			if(tags != null) {
				PicturePage.CurrentTags = tags;
			}
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
			if(args.IsSettingsInvoked) {
				tag = SETTING_ITEM_TAG;
			}
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
			} else if(tag == SETTING_ITEM_TAG) {
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
				UpdateNavigationItem();
			}
		}

		public void UpdateNavigationItem() {
			if(currentTag == PageTag.Settings) {
				MyNavigationView.SelectedItem = MyNavigationView.SettingsItem;
				return;
			}
			foreach(NavigationViewItem item in MyNavigationView.MenuItems
						.Concat(MyNavigationView.FooterMenuItems)
						.Append(MyNavigationView.SettingsItem)
						.Cast<NavigationViewItem>()) {
				if(Convert(item.Tag as string) == currentTag) {
					item.IsSelected = true;
					break;
				}
			}
		}

		public void ClearNavigationItemSelected() {
			foreach(NavigationViewItem item in MyNavigationView.MenuItems
						.Concat(MyNavigationView.FooterMenuItems)
						.Append(MyNavigationView.SettingsItem)
						.Cast<NavigationViewItem>()) {
				item.IsSelected = false;
			}
		}

		private void FullScreenButton_Tapped(object sender, TappedRoutedEventArgs e) {
			if(ScreenMode == ScreenMode.FullScreen) {
				ScreenMode = ScreenMode.Normal;
			} else {
				ScreenMode = ScreenMode.FullScreen;
			}
		}

		private void FocusModeButton_Tapped(object sender, TappedRoutedEventArgs e) {
			if(ScreenMode == ScreenMode.Focus) {
				ScreenMode = ScreenMode.Normal;
			} else {
				ScreenMode = ScreenMode.Focus;
			}
		}

		private CancellationTokenSource cts = new();
		private ScreenMode screenMode;

		private async void CurrentTagsButton_Tapped(object sender, TappedRoutedEventArgs e) {
			await PopupSearch();
		}

		public async Task PopupSearch() {
			if(IsInSearchPopup) {
				return;
			}
			IsInSearchPopup = true;
			var dialog = new ContentDialog() {
				Title = "Search Tags".Language(),
			};

			var view = new TagsSelectionView(dialog, PostsBrowserPage.GetCurrentTags());
			dialog.Content = view;

			dialog.Opened += (s, e) => {
				view.FocusTextBox();
			};

			await dialog.ShowAsync();
			switch(view.Result) {
				case TagsSelectionView.ResultType.None:
					break;
				case TagsSelectionView.ResultType.Search:
					if(view.PostSearchResult == PostSearch.None) {
						string[] tags = view.GetTags();
						NavigateToPostsBrowser(1, tags);
						Local.History.AddTag(tags);
					} else if(!string.IsNullOrWhiteSpace(view.ResultPostID)) {
						//dialog wait 
						CreateInstantDialog("Please Wait".Language(), "Getting Post".Language());
						//get post
						if(!App.PostsPool.TryGetValue(view.ResultPostID, out E621Post loadPost)) {
							loadPost = await E621Post.GetPostByIDAsync(view.ResultPostID);
						}
						if(loadPost == null || loadPost.flags.deleted) {
							HideInstantDialog();
							await new ContentDialog() {
								Title = "Error".Language(),
								Content = "Unable to get this post".Language(),
								CloseButtonText = "Back".Language(),
							}.ShowAsync();
						} else {
							//load preview
							var loader = LoadPool.SetNew(loadPost);
							if(loader.Preview == null) {
								if(!string.IsNullOrWhiteSpace(loadPost.preview.url)) {
									HttpResult<InMemoryRandomAccessStream> result = await Data.ReadImageStreamAsync(loadPost.preview.url);
									if(result.Result == HttpResultType.Success) {
										var bitmap = new BitmapImage();
										await bitmap.SetSourceAsync(result.Content);
										loader.Preview = bitmap;
									}
								}
							}

							HideInstantDialog();

							App.PostsList.UpdatePostsList(new List<E621Post>() { loadPost });
							App.PostsList.Current = loadPost;
							NavigateToPicturePage(loadPost, Array.Empty<string>());
						}
					} else {
						await new ContentDialog() {
							Title = "Error".Language(),
							Content = "Unexpected error occurred in searching".Language(),
							CloseButtonText = "Back".Language(),
						}.ShowAsync();
					}
					break;
				case TagsSelectionView.ResultType.Hot:
					NavigateToPostsBrowser(1, "order:rank");
					Local.History.AddTag("order:rank");
					break;
				case TagsSelectionView.ResultType.Random:
					CreateInstantDialog("Please Wait".Language(), "Getting Your Tag".Language());

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

					E621Post post = (await E621Post.GetPostsByTagsAsync(cts.Token, 1, "limit:1", "order:random"))?.FirstOrDefault();
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
			if(MyFrame.Content is IPage iPage) {
				iPage.UpdateNavigationItem();
			}
		}

		private void MyFrame_NavigationFailed(object sender, NavigationFailedEventArgs e) {

		}

		private void HomeKey_Invoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args) {
			if(LocalSettings.Current.enableHotKeys) {
				NavigateTo(PageTag.PostsBrowser);
			}
			args.Handled = true;
		}

		private void PictureKey_Invoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args) {
			if(LocalSettings.Current.enableHotKeys) {
				NavigateTo(PageTag.Picture);
			}
			args.Handled = true;
		}

		private void LibraryKey_Invoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args) {
			if(LocalSettings.Current.enableHotKeys) {
				NavigateTo(PageTag.Library);
			}
			args.Handled = true;
		}

		private void FavoriteKey_Invoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args) {
			if(LocalSettings.Current.enableHotKeys) {
				NavigateTo(PageTag.Subscription);
			}
			args.Handled = true;
		}

		private void SpotKey_Invoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args) {
			if(LocalSettings.Current.enableHotKeys) {
				NavigateTo(PageTag.Spot);
			}
			args.Handled = true;
		}

		private void DownloadKey_Invoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args) {
			if(LocalSettings.Current.enableHotKeys) {
				NavigateTo(PageTag.Download);
			}
			args.Handled = true;
		}

		private void SettingsKey_Invoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args) {
			if(LocalSettings.Current.enableHotKeys) {
				NavigateTo(PageTag.Settings);
			}
			args.Handled = true;
		}

		private void UserKey_Invoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args) {
			if(LocalSettings.Current.enableHotKeys) {
				NavigateTo(PageTag.UserProfile);
			}
			args.Handled = true;
		}

		private void FullscreenKey_Invoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args) {
			if(LocalSettings.Current.enableHotKeys) {
				if(ScreenMode != ScreenMode.FullScreen) {
					ScreenMode = ScreenMode.FullScreen;
				} else {
					ScreenMode = ScreenMode.Normal;
				}
			}
			args.Handled = true;
		}

		private void FocusKey_Invoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args) {
			if(LocalSettings.Current.enableHotKeys) {
				if(ScreenMode != ScreenMode.Focus) {
					ScreenMode = ScreenMode.Focus;
				} else {
					ScreenMode = ScreenMode.Normal;
				}
			}
			args.Handled = true;
		}
	}

	public enum PageTag {
		PostsBrowser = 0, Picture = 1, Library = 2, Subscription = 3, Spot = 4, Download = 5, UserProfile = 6, Welcome = 7, Settings, Maintenance
	}

	public enum ScreenMode {
		Normal, FullScreen, Focus
	}
}

