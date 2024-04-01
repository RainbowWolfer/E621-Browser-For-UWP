using System;
using System.IO;
using System.Threading.Tasks;
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


		public static async ValueTask<StorageFile> GetFileOrNullAsync(this IStorageFolder folder, string fileName) {
			try {
				return await folder.GetFileAsync(fileName);
			} catch (FileNotFoundException) {
				return null;
			}
		}
	}
}
