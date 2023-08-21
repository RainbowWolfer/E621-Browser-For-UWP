using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Linq;
using System.Windows.Input;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.System;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using YiffBrowser.Helpers;
using YiffBrowser.Services.Locals;
using YiffBrowser.Views.Controls;

namespace YiffBrowser.Views.Pages {
	public sealed partial class SettingsPage : Page {
		public SettingsPage() {
			this.InitializeComponent();
		}
	}

	//Initialize
	public partial class SettingsPageViewModel : BindableBase {

		public SettingsPageViewModel() {
			UpdateDownloadFolderPath();
			InitializeGeneral();
		}

		public ICommand OpenAppLocalFolderCommand => new DelegateCommand(OpenAppLocalFolder);

		private async void OpenAppLocalFolder() {
			await Launcher.LaunchFolderAsync(Local.LocalFolder, new FolderLauncherOptions() {
				DesiredRemainingView = ViewSizePreference.UseMore,
			});
		}

	}

	//Download
	public partial class SettingsPageViewModel {

		private string downloadFolderPath;
		private bool hasDownloadFolder;

		public string DownloadFolderPath {
			get => downloadFolderPath;
			set => SetProperty(ref downloadFolderPath, value);
		}

		public bool HasDownloadFolder {
			get => hasDownloadFolder;
			set => SetProperty(ref hasDownloadFolder, value);
		}

		public ICommand ClearDownloadFolderCommand => new DelegateCommand(ClearDownloadFolder);
		public ICommand SelectDownloadFolderCommand => new DelegateCommand(SelectDownloadFolder);
		public ICommand OpenDownloadFolderInExplorerCommand => new DelegateCommand(OpenDownloadFolderInExplorer);

		private async void OpenDownloadFolderInExplorer() {
			await Launcher.LaunchFolderAsync(Local.DownloadFolder, new FolderLauncherOptions() {
				DesiredRemainingView = ViewSizePreference.UseMore,
			});
		}

		private async void ClearDownloadFolder() {
			if (await "Are you sure to clear download folder path? You will not lose you downloaded files. But you will need to select download folder again before you download.".CreateContentDialog(new ContentDialogParameters() {
				Title = "Clear Download Folder Path",
				PrimaryText = "Yes",
				CloseText = "No",
				DefaultButton = ContentDialogButton.Close,
			}).ShowDialogAsync() != ContentDialogResult.Primary) {
				return;
			}
			Local.Settings.ClearDownloadFolder();
			UpdateDownloadFolderPath();
		}

		private async void SelectDownloadFolder() {
			FolderPicker pick = new() {
				FileTypeFilter = { "*" },
			};
			StorageFolder folder = await pick.PickSingleFolderAsync();
			if (folder != null) {
				Local.Settings.SetDownloadFolder(folder);
			}
			UpdateDownloadFolderPath();
		}

		private void UpdateDownloadFolderPath() {
			if (Local.DownloadFolder != null) {
				DownloadFolderPath = Local.DownloadFolder.Path;
				HasDownloadFolder = true;
			} else {
				DownloadFolderPath = "";
				HasDownloadFolder = false;
			}
		}
	}

	//Listing
	public partial class SettingsPageViewModel {
		public ICommand OpenBlacklistCommand => new DelegateCommand(OpenBlacklist);
		public ICommand OpenFollowListCommand => new DelegateCommand(OpenFollowList);
		public ICommand OpenPoolFollowListCommand => new DelegateCommand(OpenPoolFollowList);

		private void OpenPoolFollowList() {

		}

		private async void OpenBlacklist() {
			await ListingsManager.ShowAsDialog(false);
		}

		private async void OpenFollowList() {
			await ListingsManager.ShowAsDialog(true);
		}

	}

	//Debug
	public partial class SettingsPageViewModel {

	}

	//General
	public partial class SettingsPageViewModel {

		public void InitializeGeneral() {
			EnableStartupTags = Local.Settings.EnableStartupTags;
			StartupTags = string.Join(" ", Local.Settings.StartupTags).Trim();
			StartupTagsChanged = false;
		}

		private bool startupTagsChanged;
		private bool enableStartupTags;
		private string startupTags;

		public bool EnableStartupTags {
			get => enableStartupTags;
			set => SetProperty(ref enableStartupTags, value, () => {
				Local.Settings.EnableStartupTags = value;
				LocalSettings.Write();
			});
		}

		public string StartupTags {
			get => startupTags;
			set => SetProperty(ref startupTags, value, () => {
				StartupTagsChanged = true;
			});
		}

		public bool StartupTagsChanged {
			get => startupTagsChanged;
			set => SetProperty(ref startupTagsChanged, value);
		}

		public ICommand AcceptStartupTagsCommand => new DelegateCommand(AcceptStartupTags);

		private void AcceptStartupTags() {
			Local.Settings.StartupTags = StartupTags.Trim().Split(' ');
			LocalSettings.Write();
			StartupTagsChanged = false;
		}
	}


}
