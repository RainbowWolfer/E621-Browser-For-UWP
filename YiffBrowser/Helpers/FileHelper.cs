using System;
using Windows.Storage;
using Windows.System;
using Windows.UI.ViewManagement;

namespace YiffBrowser.Helpers {
	public static class FileHelper {
		public static async void OpenFolderInExplorer(this IStorageFolder folder, FolderLauncherOptions options = null) {
			if (folder == null) {
				return;
			}
			await Launcher.LaunchFolderAsync(folder, options ?? new FolderLauncherOptions() {
				DesiredRemainingView = ViewSizePreference.UseMore,
			});
		}
	}
}
