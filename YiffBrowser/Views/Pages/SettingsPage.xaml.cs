using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Windows.Input;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.System;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using YiffBrowser.Services.Locals;

namespace YiffBrowser.Views.Pages {
	public sealed partial class SettingsPage : Page {
		public SettingsPage() {
			this.InitializeComponent();
		}
	}

	public class SettingsPageViewModel : BindableBase {

		private string downloadFolderPath;

		public string DownloadFolderPath {
			get => downloadFolderPath;
			set => SetProperty(ref downloadFolderPath, value);
		}

		public SettingsPageViewModel() {

		}

		public ICommand ClearDownloadFolderCommand => new DelegateCommand(ClearDownloadFolder);
		public ICommand SelectDownloadFolderCommand => new DelegateCommand(SelectDownloadFolder);

		private void ClearDownloadFolder() {
			Local.Settings.ClearDownloadFolder();
		}

		private async void SelectDownloadFolder() {
			FolderPicker pick = new() {
				FileTypeFilter = { "*" },
			};
			StorageFolder folder = await pick.PickSingleFolderAsync();
			if (folder != null) {
				Local.Settings.SetDownloadFolder(folder);
			}
		}

		public ICommand OpenAppLocalFolderCommand => new DelegateCommand(OpenAppLocalFolder);

		private async void OpenAppLocalFolder() {
			await Launcher.LaunchFolderAsync(Local.LocalFolder, new FolderLauncherOptions() {
				DesiredRemainingView = ViewSizePreference.UseMore,
			});
		}

		private void UpdateDownloadFolderPath() {
			DownloadFolderPath = Local.DownloadFolder.Path;
		}
	}
}
