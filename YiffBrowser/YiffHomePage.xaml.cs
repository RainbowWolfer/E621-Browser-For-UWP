using BaseFramework;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.ApplicationModel.Core;
using Windows.UI.Core.Preview;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Media.Imaging;
using YiffBrowser.Helpers;
using YiffBrowser.Models.E621;
using YiffBrowser.Services.Downloads;
using YiffBrowser.Services.Locals;
using YiffBrowser.Services.Networks;
using YiffBrowser.Views.Controls;
using YiffBrowser.Views.Controls.LoadingViews;
using YiffBrowser.Views.Controls.Users;
using YiffBrowser.Views.Pages;
using YiffBrowser.Views.Pages.E621;
using NavigationView = Microsoft.UI.Xaml.Controls.NavigationView;
using NavigationViewItem = Microsoft.UI.Xaml.Controls.NavigationViewItem;
using NavigationViewItemInvokedEventArgs = Microsoft.UI.Xaml.Controls.NavigationViewItemInvokedEventArgs;
using NavigationViewPaneClosingEventArgs = Microsoft.UI.Xaml.Controls.NavigationViewPaneClosingEventArgs;

namespace YiffBrowser {
	public sealed partial class YiffHomePage : Page {
		public static YiffHomePage Instance { get; private set; }
		public static LoadingDialogControl LoaderControl { get; } = new LoadingDialogControl();

		public string TAG_HOME { get; } = "TAG_HOME";
		public string TAG_FAVORITES { get; } = "TAG_FAVORITES";
		public string TAG_FOLLOWS { get; } = "TAG_FOLLOWS";
		public string TAG_DOWNLOADS { get; } = "TAG_DOWNLOADS";
		public string TAG_LOCAL { get; } = "TAG_LOCAL";

		private string userAvatarURL;
		private string usernameText;

		public string UserAvatarURL {
			get => userAvatarURL;
			set {
				userAvatarURL = value;
				if (value.IsBlank()) {
					UserAvatarPicture.ProfilePicture = new BitmapImage(new Uri(YiffApp.GetResourcesString("E621/e612-Bigger.png")));
				} else {
					UserAvatarPicture.ProfilePicture = new BitmapImage(new Uri(value));
				}
			}
		}

		public string UsernameText {
			get => usernameText;
			set {
				usernameText = value;
				UserUsernameTextBlock.Text = value.NotBlankCheck() ?? "Account";
				ToolTipService.SetToolTip(UserButton, UserUsernameTextBlock.Text);
			}
		}

		public YiffHomePage() {
			Instance = this;
			this.InitializeComponent();

			CoreApplicationViewTitleBar coreTitleBar = CoreApplication.GetCurrentView().TitleBar;
			coreTitleBar.ExtendViewIntoTitleBar = true;
			Window.Current.SetTitleBar(AppTitleBar);
			coreTitleBar.IsVisibleChanged += (sender, e) => {
				if (sender.IsVisible) {
					AppTitleBar.Visibility = Visibility.Visible;
				} else {
					AppTitleBar.Visibility = Visibility.Collapsed;
				}
			};

			SystemNavigationManagerPreview.GetForCurrentView().CloseRequested += async (sender, args) => {
				if (DownloadManager.HasDownloading()) {
					args.Handled = true;
					if (await new ContentDialog() {
						Title = "Confirm",
						Content = "You have unfinished downloads",
						PrimaryButtonText = "Quit",
						CloseButtonText = "Back",
						DefaultButton = ContentDialogButton.Close,
					}.ShowAsync() == ContentDialogResult.Primary) {
						Application.Current.Exit();
					}
				}
			};

			ViewModel.HasDownloadingChanged += ViewModel_HasDownloadingChanged;
		}

		private void ViewModel_HasDownloadingChanged(bool hasDownloading) {
			if (hasDownloading) {
				DownloadProgressStoryboard.Begin();
			} else {
				DownloadProgressStoryboard.Stop();
				DownloadIconRotateTransform.Angle = 0;
			}
		}

		private void Page_Loaded(object sender, RoutedEventArgs e) {
			Initialize();
		}

		private async void Initialize() {
			LoaderControl.Set(new LoadingRingWithTextBelow("Initializing"));

			static async Task Init() {
				await Local.Initialize();
			}

			await LoaderControl.Start(Init);

			NavigateHome();

			LoadLocalUser();

			if (NewVersionService.IsFirstTime()) {
				await NewVersionService.DisableFirstTime();
				await new FirstTimeNotificationView().CreateContentDialog(new ContentDialogParameters() {
					Title = "Welcome to new version of E621 Browser UWP",
					PrimaryText = "Continue",
					DefaultButton = ContentDialogButton.Primary,
				}).ShowAsyncSafe();
			}
		}

		private async void LoadLocalUser() {

			if (Local.Settings.CheckLocalUser()) {

				E621User user = await E621API.GetUserAsync(Local.Settings.GetCurrentUser().Username);
				YiffApp.User = user;
				if (user != null) {
					UsernameText = user.name;
				} else {
					UsernameText = null;
					UserAvatarURL = null;
					return;
				}

				E621Post avatarPost = await E621API.GetPostAsync(user.avatar_id);
				YiffApp.AvatarPost = avatarPost;

				if (avatarPost != null && !avatarPost.HasNoValidURLs()) {
					UserAvatarURL = avatarPost.Sample.URL;
				} else {
					UserAvatarURL = null;
				}

			} else {
				UsernameText = null;
				UserAvatarURL = null;
			}


		}


		private void MainNavigationView_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args) {
			NavigationViewItem item = args.InvokedItemContainer as NavigationViewItem;
			string tag = item.Tag as string;

			Type targetType;
			if (args.IsSettingsInvoked) {
				targetType = typeof(SettingsPage);
			} else {
				if (tag == TAG_HOME) {
					targetType = typeof(E621HomePage);
				} else if (tag == TAG_DOWNLOADS) {
					targetType = typeof(DownloadPage);
				} else if (tag == TAG_LOCAL) {
					targetType = typeof(LocalPage);
				} else {
					targetType = typeof(TestPage);
					//throw new NotSupportedException($"{tag}");
				}
			}
			MainFrame.Navigate(targetType, null, args.RecommendedNavigationTransitionInfo);
		}


		public void NavigateHome() {
			MainNavigationView.SelectedItem = ItemHome;
			MainFrame.Navigate(typeof(E621HomePage), null, new EntranceNavigationTransitionInfo());
		}

		public void NavigateDownload() {
			MainNavigationView.SelectedItem = ItemDownload;
			MainFrame.Navigate(typeof(DownloadPage), null, new EntranceNavigationTransitionInfo());
		}

		public void NavigateSettings() {
			MainNavigationView.SelectedItem = MainNavigationView.SettingsItem;
			MainFrame.Navigate(typeof(SettingsPage), null, new EntranceNavigationTransitionInfo());
		}

		private async void UserButton_Click(object sender, RoutedEventArgs e) {
			if (Local.Settings.CheckLocalUser()) {
				if (YiffApp.User == null) {
					return;
				}

				UserInfoView view = new(YiffApp.User, YiffApp.AvatarPost);
				view.OnAvatarRefreshed += (s, e) => {
					UserAvatarURL = e;
				};

				ContentDialog dialog = view.CreateContentDialog(new ContentDialogParameters() {
					CloseText = "Back",
					MaxWidth = ContentDialogParameters.DEFAULT_MAX_WIDTH,
				});
				view.Dialog = dialog;
				dialog.Closing += (s, e) => {
					if (view.IsRefreshing) {
						e.Cancel = true;
					}
				};

				await dialog.ShowAsyncSafe();

			} else {
				LoginView view = new();

				ContentDialog dialog = view.CreateContentDialog(new ContentDialogParameters() {
					Title = "Sign in",
					PrimaryText = "Submit",
					CloseText = "Back",
					DefaultButton = ContentDialogButton.Primary,
				});

				view.Dialog = dialog;

				dialog.Closing += (s, e) => {
					if (e.Result == ContentDialogResult.Primary) {
						view.Submit();
						e.Cancel = true;
					} else if (e.Result == ContentDialogResult.None) {
						if (view.IsLoading) {
							e.Cancel = true;
						}
					}
				};

				await dialog.ShowAsyncSafe();

				if (Local.Settings.CheckLocalUser()) {
					(E621User user, E621Post avatarPost) = view.GetUserResult();
					YiffApp.User = user;
					UsernameText = user.name;
					YiffApp.AvatarPost = avatarPost;
					if (avatarPost != null && !avatarPost.HasNoValidURLs()) {
						UserAvatarURL = avatarPost.Sample.URL;
					} else {
						UserAvatarURL = null;
					}
				}
			}
		}

		private void MainNavigationView_PaneOpening(NavigationView sender, object args) {
			UserButton.Width = 312;
		}

		private void MainNavigationView_PaneClosing(NavigationView sender, NavigationViewPaneClosingEventArgs args) {
			UserButton.Width = 40;
		}

	}


	internal class YiffHomePageViewModel : BindableBase {
		public event Action<bool> HasDownloadingChanged;

		private bool hasDownloading;

		public bool HasDownloading {
			get => hasDownloading;
			set => SetProperty(ref hasDownloading, value, OnHasDownloadingChanged);
		}

		private void OnHasDownloadingChanged() {
			HasDownloadingChanged?.Invoke(HasDownloading);
		}

		public ICommand TitleDownloadButtonCommand => new DelegateCommand(TitleDownloadButton);

		private void TitleDownloadButton() {
			YiffHomePage.Instance.NavigateDownload();
		}

		public YiffHomePageViewModel() {
			CheckDownloadingLoop();
		}

		private async void CheckDownloadingLoop() {
			while (true) {
				HasDownloading = DownloadManager.HasDownloading();

				await Task.Delay(200);
			}
		}

	}
}
