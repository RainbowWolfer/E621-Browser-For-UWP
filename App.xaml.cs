﻿using E621Downloader.Models.E621;
using E621Downloader.Models.Locals;
using E621Downloader.Models.Utilities;
using E621Downloader.Pages;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Crashes;
using Microsoft.Toolkit.Uwp.Helpers;
using System;
using System.Collections.Generic;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Resources.Core;
using Windows.Storage;
using Windows.System;
using Windows.UI.StartScreen;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using YiffBrowser;

namespace E621Downloader {
	public sealed partial class App : Application {
		//WUwPNbGDrfXnQoHfvU1nR3TD;
		public static App Instance { get; private set; }

		public const string IS_LIGHT_THEME = "IsLightTheme";

		public static Dictionary<string, E621Post> PostsPool { get; } = new();
		public static Dictionary<int, E621Pool> PoolsPool { get; } = new();

		public static PostsList PostsList { get; private set; } = new PostsList();
		public static BitmapImage DefaultAvatar { get; } = new BitmapImage(new Uri("ms-appx:///Assets/esix2.jpg"));

		public static ushort WindowsVersion => SystemInformation.Instance.OperatingSystemVersion.Build;
		public static bool IsWindows11 => WindowsVersion > 22000;

		public App() {
			AppCenter.Start("{Your App Secret}", typeof(Crashes));

			Instance = this;
			this.InitializeComponent();
			this.Suspending += OnSuspending;

			InitializeTheme();

			switch (GetStoredTheme()) {
				case ElementTheme.Light:
					Current.RequestedTheme = ApplicationTheme.Light;
					break;
				case ElementTheme.Dark:
					Current.RequestedTheme = ApplicationTheme.Dark;
					break;
			}

			//Crashes.GenerateTestCrash();
			//SetJumpList();
		}

		public static void InitializeTheme() {
			if (ApplicationData.Current.LocalSettings.Values[IS_LIGHT_THEME] is null) {
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
			if (Instance == null) {
				return ElementTheme.Default;
			}

			if (ApplicationData.Current.LocalSettings.Values[IS_LIGHT_THEME] is int theme) {
				if (theme == 1) {
					return ElementTheme.Light;
				} else if (theme == 2) {
					return ElementTheme.Dark;
				} else {
					return ElementTheme.Default;
				}
			} else {
				return ElementTheme.Default;
			}
		}

		public static void SetStoredTheme(ElementTheme theme) {
			if (Instance == null) {
				return;
			}
			switch (theme) {
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
			if (LocalLanguage.Current.language != null) {
				ResourceContext.SetGlobalQualifierValue("Language", LocalLanguage.Current.GetSystemLanguage());
			}
			//else {
			//	var systemLanguage = GlobalizationPreferences.Languages.FirstOrDefault() ?? "en-US";
			//	ResourceContext.SetGlobalQualifierValue("Language", systemLanguage);
			//	//ApplicationLanguages.PrimaryLanguageOverride = GlobalizationPreferences.Languages.FirstOrDefault();
			//}
			if (Window.Current.Content is not Frame rootFrame) {
				rootFrame = new Frame();

				rootFrame.NavigationFailed += OnNavigationFailed;

				if (e.PreviousExecutionState == ApplicationExecutionState.Terminated) {
					//TODO: Load state from previously suspended application
				}

				Window.Current.Content = rootFrame;
			}

			if (e.PrelaunchActivated == false) {
				if (rootFrame.Content == null) {
					bool didAppCrash = await Crashes.HasCrashedInLastSessionAsync();
					if (didAppCrash) {
						rootFrame.Navigate(typeof(DefaultPage), e.Arguments);
					} else {
#if Release
						rootFrame.Navigate(typeof(MainPage), e.Arguments);
#else
						rootFrame.Navigate(typeof(YiffHomePage), e.Arguments);
#endif
					}
				}

				Window.Current.Activate();
			}

			Window.Current.CoreWindow.KeyDown += (sender, args) => {
				if (ShouldSkipHotkeysDisable(args.VirtualKey) || (LocalSettings.Current?.enableHotKeys ?? true)) {
					KeyListener.RegisterKeyDown(args.VirtualKey);
				}
			};

			Window.Current.CoreWindow.KeyUp += (sender, args) => {
				if (ShouldSkipHotkeysDisable(args.VirtualKey) || (LocalSettings.Current?.enableHotKeys ?? true)) {
					KeyListener.RegisterKeyUp(args.VirtualKey);
				}
			};
		}

		private bool ShouldSkipHotkeysDisable(VirtualKey key) {
			return key is VirtualKey.Enter or VirtualKey.Escape or VirtualKey.F11 or VirtualKey.F12;
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
			if (!JumpList.IsSupported()) {
				return;
			}
			var jumpList = await JumpList.LoadCurrentAsync();
			jumpList.SystemGroupKind = JumpListSystemGroupKind.None;

			jumpList.Items.Clear();

			JumpListItem item_home = JumpListItem.CreateWithArguments("Home", "Home");
			item_home.GroupName = "Pages";
			item_home.Logo = new Uri("ms-appx:///Icons/Twitter-icon.png");
			JumpListItem item_picture = JumpListItem.CreateWithArguments("Picture", "Picture");
			item_picture.GroupName = "Pages";

			jumpList.Items.Add(item_home);
			jumpList.Items.Add(item_picture);

			await jumpList.SaveAsync();
		}

	}

	public enum LOR {
		Left, Right
	}
}
