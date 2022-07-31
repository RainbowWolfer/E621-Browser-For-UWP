using E621Downloader.Models;
using E621Downloader.Models.Download;
using E621Downloader.Models.Inerfaces;
using E621Downloader.Models.Locals;
using E621Downloader.Models.Networks;
using E621Downloader.Views;
using E621Downloader.Views.ListingManager;
using E621Downloader.Views.SettingsSection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation.Metadata;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.Storage.Pickers;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Shell;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;

namespace E621Downloader.Pages {
	public sealed partial class SettingsPage: Page, IPage {
		public static bool isDownloadPathChangingHandled = true;
		public string Version => $"{App.GetAppVersion()}";

		private bool internalChanges = true;

		private readonly ElementTheme initialTheme;

		private CoreCursor cursorBeforePointerEntered = null;

		private bool hasInitialized = false;

		public SettingsPage() {
			this.InitializeComponent();
			this.NavigationCacheMode = NavigationCacheMode.Required;
			ClearDownloadPathButton.IsEnabled = Local.DownloadFolder != null;
			initialTheme = App.GetStoredTheme();

			AddPointerHelpEvent(LanguagePanel);
			AddPointerHelpEvent(CycleListToggle);
			AddPointerHelpEvent(ConcatTagToggle);
			AddPointerHelpEvent(MediaPlayToggle);
			AddPointerHelpEvent(MediaAutoPlayToggle);
			AddPointerHelpEvent(GifAutoPlayToggle);

			WindowsVersionText.Text = $"{(App.IsWindows11 ? "Windows 11" : "Windows 10")} ({App.WindowsVersion})";

			hasInitialized = true;
		}

		private void AddPointerHelpEvent(UIElement ui) {
			ui.PointerEntered += (_, _) => Help_PointerEntered(true);
			ui.PointerExited += (_, _) => Help_PointerEntered(false);
		}

		private void Help_PointerEntered(bool enter) {
			if(enter) {
				cursorBeforePointerEntered = Window.Current.CoreWindow.PointerCursor;
				Window.Current.CoreWindow.PointerCursor = new CoreCursor(CoreCursorType.Help, 0);
			} else {
				Window.Current.CoreWindow.PointerCursor = cursorBeforePointerEntered;
			}
		}

		protected override void OnNavigatedTo(NavigationEventArgs e) {
			base.OnNavigatedTo(e);
			LocalStateHyperContentText.Text = Local.LocalFolder.Path;

			DownloadPathTextBlock.Text = Local.DownloadFolder == null ? "No Download Path Selected".Language() : Local.DownloadFolder.Path;
			UpdateDownloadPathButtonStyle();
			CustomHostToggle.IsOn = LocalSettings.Current.customHostEnable;
			CycleListToggle.IsOn = LocalSettings.Current.cycleList;
			CustomHostButton.IsEnabled = LocalSettings.Current.customHostEnable;
			CustomHostButton.Content = LocalSettings.Current.customHostEnable ? string.IsNullOrWhiteSpace(LocalSettings.Current.customHost) ? "Host" : LocalSettings.Current.customHost : "E926.net";
			ConcatTagToggle.IsOn = LocalSettings.Current.concatTags;
			MediaPlayToggle.IsOn = LocalSettings.Current.mediaBackgroundPlay;
			MediaAutoPlayToggle.IsOn = LocalSettings.Current.mediaAutoPlay;
			HotKeysToggle.IsOn = LocalSettings.Current.enableHotKeys;
			GifAutoPlayToggle.IsOn = LocalSettings.Current.enableGifAutoPlay;
			MediaAutoMuteToggle.IsOn = LocalSettings.Current.mediaAutoMute;
			DebugToggle.IsOn = LocalSettings.Current.enableDebugPanel;
			EnableDebugPanel = DebugToggle.IsOn;

			RandomTagMaxSlider.Value = LocalSettings.Current.randomTagMaxCount;
			RandomTagMaxText.Text = Methods.NumberToK(LocalSettings.Current.randomTagMaxCount);

			LanguageComboBox.SelectedIndex = LocalLanguage.Current.language switch {
				Models.Locals.Language.English => 0,
				Models.Locals.Language.Chinese => 1,
				_ => 0,
			};

			switch(App.GetStoredTheme()) {
				case ElementTheme.Default:
					SystemThemeButton.IsChecked = true;
					break;
				case ElementTheme.Light:
					LightThemeButton.IsChecked = true;
					break;
				case ElementTheme.Dark:
					DarkThemeButton.IsChecked = true;
					break;
				default:
					break;
			}

			internalChanges = false;
		}

		private async void BlackListButton_Click(object sender, RoutedEventArgs e) {
			BlackListButton.IsEnabled = false;
			await PopupListingManager("Blacklist".Language(), Local.Listing.LocalBlackLists);
			BlackListButton.IsEnabled = true;
		}

		private async void FollowListButton_Click(object sender, RoutedEventArgs e) {
			FollowListButton.IsEnabled = false;
			await PopupListingManager("Follow List".Language(), Local.Listing.LocalFollowingLists);
			FollowListButton.IsEnabled = true;
		}

		public static async Task PopupListingManager(string title, List<SingleListing> listings) {
			var content = new BlackListManager(listings);
			content.OnNewListSubmit += async text => {
				listings.Add(new SingleListing(text));
				await Local.WriteListing();
				content.Update(listings);
				content.FocusListingsLastItem();
			};
			content.OnNewTagSubmit += async (listing, tag) => {
				listing.Tags.Add(tag);
				await Local.WriteListing();
				content.LoadTags(listing, true);
			};
			content.OnPasteImport += async array => {
				int count = listings.Count;
				string newName;
				do {
					newName = "Paste List".Language() + $" - {count++}";
				} while(listings.Any(i => i.Name == newName));
				var list = new SingleListing(newName) {
					Tags = array.ToList(),
					IsDefault = false,
					IsCloud = false,
				};
				listings.Add(list);
				await Local.WriteListing();
				content.Update(listings);
				content.FocusListingsLastItem();
			};
			content.OnListingDelete += async listing => {
				listings.Remove(listing);
				await Local.WriteListing();
				content.Update(listings);
				content.FocusListingsLastItem();
			};
			content.OnListingSetAsDefault += async listing => {
				foreach(SingleListing item in listings) {
					item.IsDefault = item == listing.Listing;
				}
				await Local.WriteListing();
			};
			content.OnListingRename += async (listing, text) => {
				listing.Name = text;
				await Local.WriteListing();
			};
			content.OnTagDelete += async (listing, tag) => {
				listing.Tags.Remove(tag.Tag);
				await Local.WriteListing();
				content.LoadTags(listing);
			};
			content.OnCloudSync += async array => {
				await Local.WriteListing();
			};
			content.OnTagsClearAll += async listing => {
				listing.Tags.Clear();
				await Local.WriteListing();
				content.LoadTags(listing);
			};
			var dialog = new ContentDialog() {
				Title = title,
				CloseButtonText = "Back".Language(),
				Content = content,
			};
			dialog.Resources["ContentDialogMaxWidth"] = 650;
			await dialog.ShowAsync();
			content.OnClose();
		}

		private async void DownloadPathButton_Tapped(object sender, TappedRoutedEventArgs e) {
			if(Local.DownloadFolder == null) {
				Debug.WriteLine("no download path");
			} else {
				Debug.WriteLine(Local.DownloadFolder.Path);
			}
			FolderPicker pick = new() { FileTypeFilter = { "*" } };
			StorageFolder result = await pick.PickSingleFolderAsync();
			if(result != null) {
				string token = StorageApplicationPermissions.FutureAccessList.Add(result);
				Debug.WriteLine(token);
				await Local.WriteTokenToFile(token);
				DownloadPathTextBlock.Text = Local.DownloadFolder.Path;
				ClearDownloadPathButton.IsEnabled = true;
				isDownloadPathChangingHandled = false;
			}
			UpdateDownloadPathButtonStyle();
		}

		private async void ClearDownloadPathButton_Tapped(object sender, TappedRoutedEventArgs e) {
			if(DownloadsManager.HasDownloading()) {
				await MainPage.CreatePopupDialog("Warning".Language(), "There is something downloading cannot clear download path".Language());
				return;
			}
			if(await new ContentDialog() {
				Title = "Confirm".Language(),
				Content = "Are you sure to clear download path(your downloaded files will not be deleted)".Language(),
				PrimaryButtonText = "Yes".Language(),
				CloseButtonText = "No".Language(),
				DefaultButton = ContentDialogButton.Close,
			}.ShowAsync() != ContentDialogResult.Primary) {
				return;
			}
			Local.ClearToken(Local.GetToken());
			if(Local.GetToken() == null) {
				ClearDownloadPathButton.IsEnabled = false;
			}
			DownloadPathTextBlock.Text = "No Path Selected".Language();
			UpdateDownloadPathButtonStyle();
		}

		private void UpdateDownloadPathButtonStyle() {
			if(Local.DownloadFolder == null) {
				DownloadPathButton.BorderThickness = new Thickness(2);
			} else {
				DownloadPathButton.BorderThickness = new Thickness(0);
			}
		}

		private async void CustomHostToggle_Toggled(object sender, RoutedEventArgs e) {
			if(internalChanges) {
				return;
			}
			bool isOn = (sender as ToggleSwitch).IsOn;
			if(!isOn) {
				LocalSettings.Current.customHostEnable = false;
				LocalSettings.Current.customHost = "";
				LocalSettings.Save();
				CustomHostButton.IsEnabled = false;
				CustomHostButton.Content = "E926.net";
				return;
			}
			await PopupCustomHostDialog(result => {
				if(result.Confirm) {
					LocalSettings.Current.customHostEnable = isOn;
					LocalSettings.Save();
					CustomHostButton.IsEnabled = isOn;
					CustomHostButton.Content = LocalSettings.Current.customHostEnable ? string.IsNullOrWhiteSpace(LocalSettings.Current.customHost) ? "Host" : LocalSettings.Current.customHost : "E926.net";
				} else {
					internalChanges = true;
					(sender as ToggleSwitch).IsOn = false;
					internalChanges = false;
				}
			});
		}

		private void CycleListToggle_Toggled(object sender, RoutedEventArgs e) {
			if(internalChanges) {
				return;
			}
			LocalSettings.Current.cycleList = (sender as ToggleSwitch).IsOn;
			LocalSettings.Save();
		}

		private void GifAutoPlayToggle_Toggled(object sender, RoutedEventArgs e) {
			if(internalChanges) {
				return;
			}
			LocalSettings.Current.enableGifAutoPlay = (sender as ToggleSwitch).IsOn;
			LocalSettings.Save();
		}

		private void ConcatTagToggle_Toggled(object sender, RoutedEventArgs e) {
			if(internalChanges) {
				return;
			}
			LocalSettings.Current.concatTags = (sender as ToggleSwitch).IsOn;
			LocalSettings.Save();
		}

		private void MediaPlayToggle_Toggled(object sender, RoutedEventArgs e) {
			if(internalChanges) {
				return;
			}
			LocalSettings.Current.mediaBackgroundPlay = (sender as ToggleSwitch).IsOn;
			LocalSettings.Save();
		}
		private void MediaAutoPlayToggle_Toggled(object sender, RoutedEventArgs e) {
			if(internalChanges) {
				return;
			}
			LocalSettings.Current.mediaAutoPlay = (sender as ToggleSwitch).IsOn;
			LocalSettings.Save();
		}

		private void MediaAutoMuteToggle_Toggled(object sender, RoutedEventArgs e) {
			if(internalChanges) {
				return;
			}
			LocalSettings.Current.mediaAutoMute = (sender as ToggleSwitch).IsOn;
			LocalSettings.Save();
		}

		private async void HotKeysButton_Tapped(object sender, TappedRoutedEventArgs e) {
			var dialog = new ContentDialog() {
				Title = "Hot Keys".Language(),
				Content = new HotKeysManager(),
				CloseButtonText = "Back".Language(),
			};
			dialog.Resources["ContentDialogMaxWidth"] = 1000;
			await dialog.ShowAsync();
		}

		private void HotKeysToggle_Toggled(object sender, RoutedEventArgs e) {
			if(internalChanges) {
				return;
			}
			LocalSettings.Current.enableHotKeys = (sender as ToggleSwitch).IsOn;
			LocalSettings.Save();
		}

		private async void CustomHostButton_Tapped(object sender, TappedRoutedEventArgs e) {
			await PopupCustomHostDialog();
		}

		private async Task<bool> PopupCustomHostDialog(Action<CustomHostInputDialog> OnClosing = null) {
			ContentDialog dialog = new() {
				Title = "Custom Host".Language(),
			};
			var content = new CustomHostInputDialog(dialog, LocalSettings.Current.customHost ?? "");
			dialog.Closing += (s, e) => OnClosing?.Invoke(content);
			dialog.Content = content;
			await dialog.ShowAsync();
			if(content.Confirm) {
				CustomHostButton.Content = content.InputText;
				LocalSettings.Current.customHost = content.InputText;
				LocalSettings.Save();
				return true;
			} else {
				return false;
			}
		}

		private async void OfficialSiteButton_Tapped(object sender, TappedRoutedEventArgs e) {
			await Methods.OpenBrowser($"https://{Data.GetHost()}");
		}

		private async void EmailButton_Tapped(object sender, TappedRoutedEventArgs e) {
			await Methods.ComposeEmail("[E621 Browser For UWP] " + "Subject Here".Language(), "");
		}

		private async void LocalStateHyperButton_Click(object sender, RoutedEventArgs e) {
			await Launcher.LaunchFolderAsync(Local.LocalFolder, new FolderLauncherOptions() {
				DesiredRemainingView = ViewSizePreference.UseMore
			});
		}

		private void LightThemeButton_Click(object sender, RoutedEventArgs e) {
			if(internalChanges) {
				return;
			}
			App.SetStoredTheme(ElementTheme.Light);
			if(initialTheme == ElementTheme.Light) {
				ThemeChangedHintText.Visibility = Visibility.Collapsed;
			} else {
				ThemeChangedHintText.Visibility = Visibility.Visible;
			}
		}

		private void DarkThemeButton_Click(object sender, RoutedEventArgs e) {
			if(internalChanges) {
				return;
			}
			App.SetStoredTheme(ElementTheme.Dark);
			if(initialTheme == ElementTheme.Dark) {
				ThemeChangedHintText.Visibility = Visibility.Collapsed;
			} else {
				ThemeChangedHintText.Visibility = Visibility.Visible;
			}
		}

		private void SystemThemeButton_Click(object sender, RoutedEventArgs e) {
			if(internalChanges) {
				return;
			}
			App.SetStoredTheme(ElementTheme.Default);
			if(initialTheme == ElementTheme.Default) {
				ThemeChangedHintText.Visibility = Visibility.Collapsed;
			} else {
				ThemeChangedHintText.Visibility = Visibility.Visible;
			}
		}

		private void RandomTagMaxSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e) {
			if(internalChanges) {
				return;
			}
			int value = (int)e.NewValue;
			LocalSettings.Current.randomTagMaxCount = value;
			RandomTagMaxText.Text = Methods.NumberToK(value);
			DelayedSaveSetting();
		}

		private CancellationTokenSource cts;
		private async void DelayedSaveSetting() {
			if(cts != null) {
				cts.Cancel();
				cts.Dispose();
				cts = null;
			}
			cts = new CancellationTokenSource();
			try {
				await Task.Delay(500, cts.Token);
			} catch(TaskCanceledException) {
				return;
			}
			LocalSettings.Save();
		}

		private async void HistoryButton_Click(object sender, RoutedEventArgs e) {
			var dialog = new ContentDialog() {
				Title = "History".Language(),
				CloseButtonText = "Back".Language(),
			};
			var content = new HistoryDialogView(dialog);
			dialog.Content = content;
			dialog.Closing += (s, args) => {
				content.CancelLoading();
			};
			await dialog.ShowAsync();
		}

		private async void ClearWallpapersCacheButton_Click(object sender, RoutedEventArgs e) {
			var result = await new ContentDialog() {
				Title = "Confirm".Language(),
				Content = "Are you sure to delete all your wallpapers cache located in the APP settings folder".Language(),
				PrimaryButtonText = "Yes".Language(),
				SecondaryButtonText = "Open Folder".Language(),
				CloseButtonText = "No".Language(),
				DefaultButton = ContentDialogButton.Close,
			}.ShowAsync();
			if(result == ContentDialogResult.Primary) {
				MainPage.CreateInstantDialog("Please Wait".Language(), "Cleaning".Language());
				foreach(StorageFile item in await Local.WallpapersFolder.GetFilesAsync()) {
					await item.DeleteAsync();
				}
				MainPage.HideInstantDialog();
			} else if(result == ContentDialogResult.Secondary) {
				await Launcher.LaunchFolderAsync(Local.WallpapersFolder, new FolderLauncherOptions() {
					DesiredRemainingView = ViewSizePreference.UseMore
				});
			}
		}

		void IPage.UpdateNavigationItem() {
			MainPage.Instance.currentTag = PageTag.Settings;
			MainPage.Instance.UpdateNavigationItem();
		}

		void IPage.FocusMode(bool enabled) { }

		private async void PinButton_Tapped(object sender, TappedRoutedEventArgs e) {
			if(!ApiInformation.IsTypePresent("Windows.UI.Shell.TaskbarManager")) {
				return;
			}
			TaskbarManager bar = TaskbarManager.GetDefault();
			if(!bar.IsSupported) {
				return;
			}
			if(!bar.IsPinningAllowed) {
				return;
			}
			if(await bar.IsCurrentAppPinnedAsync()) {
				return;
			}
			await bar.RequestPinCurrentAppAsync();
		}

		private void LanguageComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) {
			if(internalChanges) {
				return;
			}
			if(LanguageComboBox.SelectedIndex == 0) {
				LocalLanguage.Current.language = Models.Locals.Language.English;
				LocalLanguage.Save();
			} else if(LanguageComboBox.SelectedIndex == 1) {
				LocalLanguage.Current.language = Models.Locals.Language.Chinese;
				LocalLanguage.Save();
			}
			LanguageChangeHintText.Visibility = Visibility.Visible;
		}


		public bool EnableDebugPanel {
			get => LocalSettings.Current.enableDebugPanel;
			set {
				DebugPanel.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
				if(internalChanges) {
					return;
				}
				LocalSettings.Current.enableDebugPanel = value;
				LocalSettings.Save();
			}
		}
		private void DebugToggle_Toggled(object sender, RoutedEventArgs e) {
			if(internalChanges) {
				return;
			}
			EnableDebugPanel = DebugToggle.IsOn;
		}

		private async void HttpRequestHistoriesButton_Click(object sender, RoutedEventArgs e) {
			if(!LocalSettings.Current?.enableDebugPanel ?? true) {
				return;
			}

			ContentDialog dialog = new() {
				Title = "Http Request Histories".Language(),
				Content = new HttpRequestHistoriesView(),
				CloseButtonText = "Close".Language(),
			};
			dialog.Resources["ContentDialogMaxWidth"] = 650;
			await dialog.ShowAsync();
		}

		private async void HandledExceptionsButton_Click(object sender, RoutedEventArgs e) {
			if(!LocalSettings.Current?.enableDebugPanel ?? true) {
				return;
			}
			ContentDialog dialog = new() {
				Title = "Handled Exceptions".Language(),
				Content = new HandledExceptionsView(),
				CloseButtonText = "Close".Language(),
			};
			dialog.Resources["ContentDialogMaxWidth"] = 650;
			await dialog.ShowAsync();
		}

		private void PostsPerPageSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e) {
			if(!hasInitialized) {
				return;
			}
			PostsPerPageValueText.Text = $"{e.NewValue}";
			LocalSettings.Current.postsPerPage = (int)e.NewValue;
			DelayedSaveSetting();
		}

	}
}
