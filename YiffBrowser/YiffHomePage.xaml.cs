using System;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Media.Imaging;
using YiffBrowser.Helpers;
using YiffBrowser.Models.E621;
using YiffBrowser.Services.Locals;
using YiffBrowser.Services.Networks;
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
		public static LoadingDialogControl LoaderControl { get; private set; }

		public string TAG_HOME { get; } = "TAG_HOME";
		public string TAG_FAVORITES { get; } = "TAG_FAVORITES";
		public string TAG_FOLLOWS { get; } = "TAG_FOLLOWS";
		public string TAG_DOWNLOADS { get; } = "TAG_DOWNLOADS";

		private string userAvatarURL;
		private string usernameText;

		public string UserAvatarURL {
			get => userAvatarURL;
			set {
				userAvatarURL = value;
				if (value.IsBlank()) {
					UserAvatarPicture.ProfilePicture = new BitmapImage(new Uri(App.GetResourcesString("E621/e612-Bigger.png")));
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
			this.InitializeComponent();
		}

		private void Root_Loaded(object sender, RoutedEventArgs e) {
			Initialize();
		}

		private async void Initialize() {
			LoadingRingWithTextBelow loader = new("Initializing");

			LoaderControl = new LoadingDialogControl(loader);

			static async Task Init() {
				await Local.Initialize();
			}

			await LoaderControl.Start(Init);
			//DownloadManager.Initialize();

			NavigateHome();

			LoadLocalUser();
		}

		private async void LoadLocalUser() {
			if (Local.Settings.CheckLocalUser()) {

				E621User user = await E621API.GetUserAsync(Local.Settings.Username);
				UsernameText = user.name;
				E621Post avatarPost = await E621API.GetPostAsync(user.avatar_id);

				App.User = user;
				App.AvatarPost = avatarPost;
				if (avatarPost != null && !avatarPost.HasNoValidURLs()) {
					UserAvatarURL = avatarPost.Sample.URL;
				} else {
					UserAvatarURL = null;
				}

			} else {
				UserAvatarURL = null;
				UsernameText = null;
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

		private async void UserButton_Click(object sender, RoutedEventArgs e) {
			if (Local.Settings.CheckLocalUser()) {
				if (App.User == null) {
					return;
				}

				UserInfoView view = new(App.User, App.AvatarPost);
				view.OnAvatarRefreshed += (s, e) => {
					UserAvatarURL = e;
				};

				ContentDialog dialog = view.CreateContentDialog(new ContentDialogParameters() {
					CloseText = "Back",
				});
				view.Dialog = dialog;
				dialog.Closing += (s, e) => {
					if (view.IsRefreshing) {
						e.Cancel = true;
					}
				};

				await dialog.ShowDialogAsync();

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

				await dialog.ShowDialogAsync();

				if (Local.Settings.CheckLocalUser()) {
					(E621User user, E621Post avatarPost) = view.GetUserResult();
					App.User = user;
					UsernameText = user.name;
					App.AvatarPost = avatarPost;
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
}
