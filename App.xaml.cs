using E621Downloader.Models;
using E621Downloader.Models.Locals;
using E621Downloader.Models.Posts;
using System;
using System.Collections.Generic;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Resources.Core;
using Windows.Storage;
using Windows.UI.StartScreen;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

namespace E621Downloader {
	public sealed partial class App: Application {
		//WUwPNbGDrfXnQoHfvU1nR3TD;
		public static App Instance { get; private set; }

		public const string IS_LIGHT_THEME = "IsLightTheme";

		public static Dictionary<string, Post> PostsPool { get; } = new();

		public static PostsList PostsList { get; private set; } = new PostsList();
		public static BitmapImage DefaultAvatar { get; } = new BitmapImage(new Uri("ms-appx:///Assets/esix2.jpg"));

		public App() {
			Instance = this;
			this.InitializeComponent();
			this.Suspending += OnSuspending;

			InitializeTheme();

			switch(GetStoredTheme()) {
				case ElementTheme.Light:
					Current.RequestedTheme = ApplicationTheme.Light;
					break;
				case ElementTheme.Dark:
					Current.RequestedTheme = ApplicationTheme.Dark;
					break;
			}

		}

		public static void InitializeTheme() {
			if(ApplicationData.Current.LocalSettings.Values[IS_LIGHT_THEME] is null) {
				ApplicationData.Current.LocalSettings.Values[IS_LIGHT_THEME] = 0;
			}
		}

		public static ApplicationTheme GetApplicationTheme() {
			return GetStoredTheme() switch {
				ElementTheme.Light => ApplicationTheme.Light,
				ElementTheme.Dark => ApplicationTheme.Dark,
				_ => Current.RequestedTheme,
			};
		}

		public static ElementTheme GetStoredTheme() {
			if(Instance == null) {
				return ElementTheme.Default;
			}

			if(ApplicationData.Current.LocalSettings.Values[IS_LIGHT_THEME] is int theme) {
				if(theme == 1) {
					return ElementTheme.Light;
				} else if(theme == 2) {
					return ElementTheme.Dark;
				} else {
					return ElementTheme.Default;
				}
			} else {
				return ElementTheme.Default;
			}
		}

		public static void SetStoredTheme(ElementTheme theme) {
			if(Instance == null) {
				return;
			}
			switch(theme) {
				case ElementTheme.Default:
					ApplicationData.Current.LocalSettings.Values[IS_LIGHT_THEME] = 0;
					break;
				case ElementTheme.Light:
					ApplicationData.Current.LocalSettings.Values[IS_LIGHT_THEME] = 1;
					break;
				case ElementTheme.Dark:
					ApplicationData.Current.LocalSettings.Values[IS_LIGHT_THEME] = 2;
					break;
				default:
					break;
			}
		}

		public static string GetAppVersion() {
			Package package = Package.Current;
			PackageId packageId = package.Id;
			PackageVersion version = packageId.Version;

			return $"{version.Major}.{version.Minor}.{version.Build}.{version.Revision}";
		}

		protected override async void OnLaunched(LaunchActivatedEventArgs e) {
			//read local stored language preference
			await LocalLanguage.Initialize();
			if(LocalLanguage.Current.language != null) {
				ResourceContext.SetGlobalQualifierValue("Language", LocalLanguage.Current.GetSystemLanguage());
			}
			//else {
			//	var systemLanguage = GlobalizationPreferences.Languages.FirstOrDefault() ?? "en-US";
			//	ResourceContext.SetGlobalQualifierValue("Language", systemLanguage);
			//	//ApplicationLanguages.PrimaryLanguageOverride = GlobalizationPreferences.Languages.FirstOrDefault();
			//}
			if(Window.Current.Content is not Frame rootFrame) {
				rootFrame = new Frame();

				rootFrame.NavigationFailed += OnNavigationFailed;

				if(e.PreviousExecutionState == ApplicationExecutionState.Terminated) {
					//TODO: Load state from previously suspended application
				}

				Window.Current.Content = rootFrame;
			}

			if(e.PrelaunchActivated == false) {
				if(rootFrame.Content == null) {
					rootFrame.Navigate(typeof(MainPage), e.Arguments);
				}
				Window.Current.Activate();
			}

			Window.Current.CoreWindow.KeyDown += (sender, args) => {
				if(LocalSettings.Current?.enableHotKeys ?? true) {
					KeyListener.RegisterKeyDown(args.VirtualKey);
				}
			};

			Window.Current.CoreWindow.KeyUp += (sender, args) => {
				if(LocalSettings.Current?.enableHotKeys ?? true) {
					KeyListener.RegisterKeyUp(args.VirtualKey);
				}
			};
		}

		void OnNavigationFailed(object sender, NavigationFailedEventArgs e) {
			throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
		}

		private void OnSuspending(object sender, SuspendingEventArgs e) {
			var deferral = e.SuspendingOperation.GetDeferral();
			//TODO: Save application state and stop any background activity
			deferral.Complete();
		}

		private async void SetJumpList() {
			if(!JumpList.IsSupported()) {
				return;
			}
			var jumpList = await JumpList.LoadCurrentAsync();
			jumpList.SystemGroupKind = JumpListSystemGroupKind.None;

			jumpList.Items.Clear();

			var item = JumpListItem.CreateWithArguments("Argument", "DisplayName");
			item.Description = "Description";
			item.GroupName = "Group Name";
			item.Logo = new Uri("ms-appx:///Icons/Twitter-icon.png");

			jumpList.Items.Add(item);

			await jumpList.SaveAsync();
		}

	}

	public enum LOR {
		Left, Right
	}
}
