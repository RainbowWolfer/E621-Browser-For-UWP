using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Networking.BackgroundTransfer;
using Windows.Storage;
using Windows.UI.Xaml.Controls;
using YiffBrowser.Helpers;
using YiffBrowser.Models.E621;
using YiffBrowser.Services.Locals;
using YiffBrowser.Services.Networks;

namespace YiffBrowser.Services.Downloads {
	internal static class DownloadManager {

		public static readonly BackgroundDownloader downloader;

		public static readonly List<DownloadInstance> downloads;

		static DownloadManager() {
			downloader = new BackgroundDownloader();
			downloader.SetRequestHeader("User-Agent", NetCode.USERAGENT);
		}


		private static async void RegisterDownload(E621Post post, string folderName = null) {
			string filename = $"{post.ID}.{post.File.Ext}";
			StorageFolder folder = await GetFolder(folderName);
			StorageFile file = await folder.CreateFileAsync(filename, CreationCollisionOption.OpenIfExists);
			Uri uri = new(post.File.URL);
			DownloadOperation download = downloader.CreateDownload(uri, file);
			downloads.Add(new DownloadInstance(post, download));
		}

		private static async ValueTask<StorageFolder> GetFolder(string folder = null) {
			if (folder.IsBlank()) {
				return Local.LocalFolder;
			} else {
				return await Local.LocalFolder.CreateFolderAsync(folder, CreationCollisionOption.OpenIfExists);
			}
		}


		public static bool HasSelectedDownloadFolder() {
			return Local.DownloadFolder != null && Local.Settings.DownloadFolderToken.IsNotBlank();
		}

		//public static bool HasDownloading() {
		//	foreach (DownloadInstance item in downloads) {
		//		if (item.Status is not BackgroundTransferStatus.Completed and
		//			not BackgroundTransferStatus.Canceled and
		//			not BackgroundTransferStatus.Error) {
		//			return true;
		//		}
		//	}
		//	return false;
		//}

	}
}
