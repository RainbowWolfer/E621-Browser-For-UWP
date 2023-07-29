using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Networking.BackgroundTransfer;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using YiffBrowser.Helpers;
using YiffBrowser.Models.E621;
using YiffBrowser.Services.Locals;
using YiffBrowser.Services.Networks;

namespace YiffBrowser.Services.Downloads {
	public static class DownloadManager {

		public static readonly BackgroundDownloader downloader;

		public static readonly ObservableCollection<DownloadInstance> waitPool = new();
		public static readonly ObservableCollection<DownloadInstance> downloadingPool = new();
		public static readonly ObservableCollection<DownloadInstance> completedPool = new();

		private static readonly DispatcherTimer timer = new();

		static DownloadManager() {
			downloader = new BackgroundDownloader();
			downloader.SetRequestHeader("User-Agent", NetCode.USERAGENT);

			timer.Interval = TimeSpan.FromSeconds(1);
			timer.Tick += DispatcherTimer_Tick;
			timer.Start();
		}

		private static void DispatcherTimer_Tick(object sender, object e) {
			const int MAX_DOWNLOADING = 10;
			if (downloadingPool.Count < MAX_DOWNLOADING) {
				int needAddCount = MAX_DOWNLOADING - downloadingPool.Count;
				DownloadInstance[] adds = waitPool.Take(needAddCount).ToArray();
				foreach (DownloadInstance item in adds) {
					waitPool.Remove(item);
					downloadingPool.Add(item);
					item.StartDownload();
					item.OnProgressed += Item_OnProgressed;
				}
			}
		}

		private static void Item_OnProgressed(DownloadInstance sender, DownloadProgress args) {
			if (args.HasCompleted) {
				sender.OnProgressed -= Item_OnProgressed;
				downloadingPool.Remove(sender);
				completedPool.Add(sender);
			}
		}

		public static async Task RegisterDownload(E621Post post, string folderName = null) {
			string filename = $"{post.ID}.{post.File.Ext}";
			StorageFolder folder = await GetFolder(folderName);
			StorageFile file = await folder.CreateFileAsync(filename, CreationCollisionOption.OpenIfExists);
			Uri uri = new(post.File.URL);
			DownloadOperation download = downloader.CreateDownload(uri, file);
			waitPool.Add(new DownloadInstance(post, download));
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
