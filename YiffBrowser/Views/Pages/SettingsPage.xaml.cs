using Prism.Commands;
using Prism.Mvvm;
using System;
using System.IO;
using System.Windows.Input;
using Windows.ApplicationModel;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.System;
using Windows.System.Profile;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml.Controls;
using YiffBrowser.Helpers;
using YiffBrowser.Services.Locals;
using YiffBrowser.Views.Controls;
using YiffBrowser.Views.Controls.PostsView;

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

		private string autoDownloadFolderPath;
		private bool isAutoDownloadFolderPathRoot;
		private bool insertSpaceBetweenTagsInAutoFolder;

		private RequestDownloadAction requestDownloadAction = RequestDownloadAction.Select;
		private DownloadFileConflictAction downloadFileConflictAction = DownloadFileConflictAction.GenerateNewName;

		public string DownloadFolderPath {
			get => downloadFolderPath;
			set => SetProperty(ref downloadFolderPath, value);
		}

		public string AutoDownloadFolderPath {
			get => autoDownloadFolderPath;
			set => SetProperty(ref autoDownloadFolderPath, value);
		}

		public bool IsAutoDownloadFolderPathRoot {
			get => isAutoDownloadFolderPathRoot;
			set => SetProperty(ref isAutoDownloadFolderPathRoot, value);
		}

		public bool InsertSpaceBetweenTagsInAutoFolder {
			get => insertSpaceBetweenTagsInAutoFolder;
			set => SetProperty(ref insertSpaceBetweenTagsInAutoFolder, value);
		}

		public RequestDownloadAction RequestDownloadAction {
			get => requestDownloadAction;
			set => SetProperty(ref requestDownloadAction, value, OnRequestDownloadActionChanged);
		}

		public DownloadFileConflictAction DownloadFileConflictAction {
			get => downloadFileConflictAction;
			set => SetProperty(ref downloadFileConflictAction, value);
		}

		public bool HasDownloadFolder {
			get => hasDownloadFolder;
			set => SetProperty(ref hasDownloadFolder, value);
		}


		private void OnRequestDownloadActionChanged() {
			if (RequestDownloadAction == RequestDownloadAction.Specify && AutoDownloadFolderPath.IsBlank()) {
				AutoDownloadFolderPath = DownloadFolderPath;
				IsAutoDownloadFolderPathRoot = true;
			}
		}


		public ICommand SelectAutoDownloadFolderCommand => new DelegateCommand(SelectAutoDownloadFolder);

		private async void SelectAutoDownloadFolder() {
			if (RequestDownloadAction != RequestDownloadAction.Specify) {
				return;
			}
			DownloadView view = new();
			ContentDialogResult dialogResult = await view.CreateContentDialog(DownloadView.parametersForAutoFolderSelectionDialog).ShowAsyncSafe();

			if (dialogResult != ContentDialogResult.Primary) {
				return;
			}

			DownloadViewResult result = view.GetResult();
			if (result == null) {
				return;
			}

			if (result.FolderPath == null) {
				AutoDownloadFolderPath = DownloadFolderPath;
				IsAutoDownloadFolderPathRoot = true;
			} else {
				AutoDownloadFolderPath = result.FolderPath;
				IsAutoDownloadFolderPathRoot = false;
			}

		}

		public ICommand ClearDownloadFolderCommand => new DelegateCommand(ClearDownloadFolder);
		public ICommand SelectDownloadFolderCommand => new DelegateCommand(SelectDownloadFolder);
		public ICommand OpenDownloadFolderInExplorerCommand => new DelegateCommand(OpenDownloadFolderInExplorer);
		public ICommand OpenAutoDownloadFolderInExplorerCommand => new DelegateCommand(OpenAutoDownloadFolderInExplorer);

		private void OpenDownloadFolderInExplorer() {
			Local.DownloadFolder.OpenFolderInExplorer();
		}
		private async void OpenAutoDownloadFolderInExplorer() {
			if (IsAutoDownloadFolderPathRoot) {
				Local.DownloadFolder.OpenFolderInExplorer();
			} else {
				string folderName = Path.GetFileName(AutoDownloadFolderPath);
				if (folderName.IsBlank()) {
					return;
				}
				StorageFolder folder = await Local.DownloadFolder.GetFolderAsync(folderName);
				folder.OpenFolderInExplorer();
			}
		}

		private async void ClearDownloadFolder() {
			if (await "Are you sure to clear download folder path? You will not lose you downloaded files. But you will need to select download folder again before you download.".CreateContentDialog(new ContentDialogParameters() {
				Title = "Clear Download Folder Path",
				PrimaryText = "Yes",
				CloseText = "No",
				DefaultButton = ContentDialogButton.Close,
			}).ShowAsyncSafe() != ContentDialogResult.Primary) {
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
		public string WindowsVersion {
			get {
				string versionString = AnalyticsInfo.VersionInfo.DeviceFamilyVersion;
				ulong version = ulong.Parse(versionString);
				ushort major = (ushort)((version & 0xFFFF000000000000) >> 48);
				ushort minor = (ushort)((version & 0x0000FFFF00000000) >> 32);
				ushort build = (ushort)((version & 0x00000000FFFF0000) >> 16);
				ushort revision = (ushort)(version & 0x000000000000FFFF);
				string fullVersion = $"{major}.{minor} ({build}.{revision})"; // e.g. "10.0 (23555.1000)"

				return $"Windows {fullVersion}";
			}
		}

		public string ProcessorArchitecture {
			get {
				ProcessorArchitecture architecture = Package.Current.Id.Architecture;
				return architecture.ToString();
			}
		}
	}

	//General
	public partial class SettingsPageViewModel {

		public void InitializeGeneral() {
			StartupTags = string.Join(" ", Local.Settings.StartupTags).Trim();
			StartupTagsChanged = false;
		}

		private bool startupTagsChanged;
		private string startupTags;
		private HostType hostType = HostType.E926;
		private int postPerPage = 75;
		private AppTheme appTheme;
		private bool showDebugPanel;
		private StartupTagsType startupTagsType = StartupTagsType.StartupTags;

		public string StartupTags {
			get => startupTags;
			set => SetProperty(ref startupTags, value, () => {
				StartupTagsChanged = true;
			});
		}

		public StartupTagsType StartupTagsType {
			get => startupTagsType;
			set => SetProperty(ref startupTagsType, value);
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

		public HostType HostType {
			get => hostType;
			set => SetProperty(ref hostType, value);
		}

		public int PostPerPage {
			get => postPerPage;
			set => SetProperty(ref postPerPage, value);
		}

		public AppTheme AppTheme {
			get => appTheme;
			set => SetProperty(ref appTheme, value);
		}

		public bool ShowDebugPanel {
			get => showDebugPanel;
			set => SetProperty(ref showDebugPanel, value);
		}


	}

	//About
	public partial class SettingsPageViewModel {
		public const string EMAIL = "RainbowWolfer@outlook.com";
		public string VersionString => YiffApp.GetAppVersion();
		public string LocalFolderPath => Local.LocalFolder.Path;

		public ICommand EmailCommand => new DelegateCommand(Email);
		public ICommand CopyEmailCommand => new DelegateCommand(CopyEmail);

		public ICommand OpenLocalFolderCommand => new DelegateCommand(OpenLocalFolder);

		private async void Email() {
			await CommonHelpers.ComposeEmail(EMAIL, "RainbowWolfer", $"[E621 Browser For UWP] Version {VersionString}" + "Subject Here", "Body Here");
		}

		private void CopyEmail() {
			EMAIL.CopyToClipboard();
		}

		private void OpenLocalFolder() {
			Local.LocalFolder.OpenFolderInExplorer();
		}

	}
}
