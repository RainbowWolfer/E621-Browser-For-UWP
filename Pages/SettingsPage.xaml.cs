using E621Downloader.Models;
using E621Downloader.Models.Download;
using E621Downloader.Models.Locals;
using E621Downloader.Models.Networks;
using E621Downloader.Views;
using E621Downloader.Views.ListingManager;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel.Contacts;
using Windows.ApplicationModel.Email;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.Storage.Pickers;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace E621Downloader.Pages {
	public sealed partial class SettingsPage: Page {
		public static bool isDownloadPathChangingHandled = true;
		public string Version => "Version: " + Assembly.GetExecutingAssembly().GetName().Version.ToString();

		private bool internalChanges = true;
		public SettingsPage() {
			this.InitializeComponent();
			ClearDownloadPathButton.IsEnabled = Local.DownloadFolder != null;
		}

		protected override void OnNavigatedTo(NavigationEventArgs e) {
			base.OnNavigatedTo(e);
			DownloadPathTextBlock.Text = Local.DownloadFolder == null ? "No Download Path Selected" : Local.DownloadFolder.Path;
			CustomHostToggle.IsOn = LocalSettings.Current.customHostEnable;
			CycleListToggle.IsOn = LocalSettings.Current.cycleList;
			CustomHostButton.IsEnabled = LocalSettings.Current.customHostEnable;
			ConcatTagToggle.IsOn = LocalSettings.Current.concatTags;
			MediaPlayToggle.IsOn = LocalSettings.Current.mediaBackgroundPlay;
			MediaAutoPlayToggle.IsOn = LocalSettings.Current.mediaAutoPlay;
			HotKeysToggle.IsOn = LocalSettings.Current.enableHotKeys;
			internalChanges = false;
		}

		private async void BlackListButton_Click(object sender, RoutedEventArgs e) {
			BlackListButton.IsEnabled = false;
			var list = Local.Listing.LocalBlackLists.ToList();
			list.Insert(0, Local.Listing.CloudBlackList);
			await PopupListingManager("Black List", list);
			BlackListButton.IsEnabled = true;
		}

		private async void FollowListButton_Click(object sender, RoutedEventArgs e) {
			FollowListButton.IsEnabled = false;
			await PopupListingManager("Follow List", Local.Listing.LocalFollowingLists);
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
					newName = $"Paste List - {count++}";
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
				CloseButtonText = "Back",
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
			FolderPicker pick = new FolderPicker() { FileTypeFilter = { "*" } };
			StorageFolder result = await pick.PickSingleFolderAsync();
			if(result != null) {
				string token = StorageApplicationPermissions.FutureAccessList.Add(result);
				Debug.WriteLine(token);
				await Local.WriteTokenToFile(token);
				DownloadPathTextBlock.Text = Local.DownloadFolder.Path;
				ClearDownloadPathButton.IsEnabled = true;
				isDownloadPathChangingHandled = false;
			}
		}

		private async void ClearDownloadPathButton_Tapped(object sender, TappedRoutedEventArgs e) {
			if(DownloadsManager.HasDownloading()) {
				await MainPage.CreatePopupDialog("Warning", "There is something downloading.\ncannot clear download path.");
				return;
			}
			Local.ClearToken(Local.GetToken());
			if(Local.GetToken() == null) {
				ClearDownloadPathButton.IsEnabled = false;
			}
			DownloadPathTextBlock.Text = "No Path Selected";
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

		private async void HotKeysButton_Tapped(object sender, TappedRoutedEventArgs e) {
			if(await new ContentDialog() {
				Title = "Hot Keys",
				Content = new HotKeysManager(),
				CloseButtonText = "Back",
			}.ShowAsync() == ContentDialogResult.Primary) {

			}
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
			ContentDialog dialog = new ContentDialog() {
				Title = "Custom Host",
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

		private CoreCursor cursorBeforePointerEntered = null;
		private void TextBlock_PointerEntered(object sender, PointerRoutedEventArgs e) {
			cursorBeforePointerEntered = Window.Current.CoreWindow.PointerCursor;
			Window.Current.CoreWindow.PointerCursor = new CoreCursor(CoreCursorType.Help, 0);
		}

		private void TextBlock_PointerExited(object sender, PointerRoutedEventArgs e) {
			Window.Current.CoreWindow.PointerCursor = cursorBeforePointerEntered;
		}

		private async void OfficialSiteButton_Tapped(object sender, TappedRoutedEventArgs e) {
			if(!await Launcher.LaunchUriAsync(new Uri($"https://{Data.GetHost()}"))) {
				await MainPage.CreatePopupDialog("Error", "Could not Open Default Browser");
			}
		}

		private async void EmailButton_Tapped(object sender, TappedRoutedEventArgs e) {
			await ComposeEmail("[E1547 For UWP] Subject Here", "");
		}

		private async Task ComposeEmail(string subject, string messageBody) {
			var emailMessage = new EmailMessage {
				Subject = subject,
				Body = messageBody,
			};
			emailMessage.To.Add(new EmailRecipient("RainbowWolfer@Outlook.com", "RainbowWolfer"));
			await EmailManager.ShowComposeNewEmailAsync(emailMessage);
		}

		//private void LightButton_Checked(object sender, RoutedEventArgs e) {
		//	var box = sender as RadioButton;
		//	if(!box.IsChecked.Value) {
		//		return;
		//	}
		//	MainPage.Instance.RequestedTheme = ElementTheme.Light;
		//}
		//private void DarkButton_Checked(object sender, RoutedEventArgs e) {
		//	var box = sender as RadioButton;
		//	if(!box.IsChecked.Value) {
		//		return;
		//	}
		//	MainPage.Instance.RequestedTheme = ElementTheme.Dark;
		//}
		//private void FollowSystemButton_Checked(object sender, RoutedEventArgs e) {
		//	var box = sender as RadioButton;
		//	if(!box.IsChecked.Value) {
		//		return;
		//	}
		//	MainPage.Instance.RequestedTheme = ElementTheme.Default;
		//}
	}
}
