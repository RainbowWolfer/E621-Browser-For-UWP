using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
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

		private static bool pausing = false;
		private static readonly DispatcherTimer timer = new();

		static DownloadManager() {
			downloader = new BackgroundDownloader();
			downloader.SetRequestHeader("User-Agent", NetCode.USERAGENT);

			timer.Interval = TimeSpan.FromSeconds(1);
			timer.Tick += DispatcherTimer_Tick;
			timer.Start();
		}

		private static void DispatcherTimer_Tick(object sender, object e) {
			if (Pausing) {
				return;
			}
			const int MAX_DOWNLOADING = 12;
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

		private static async void Item_OnCancel(DownloadInstance sender, EventArgs args) {
			sender.OnCancel -= Item_OnCancel;
			if (downloadingPool.Contains(sender)) {
				downloadingPool.Remove(sender);
			} else if (waitPool.Contains(sender)) {
				waitPool.Remove(sender);
			}
			await sender.Download.ResultFile.DeleteAsync();
		}

		private static void Item_OnProgressed(DownloadInstance sender, DownloadProgress args) {
			Debug.WriteLine($"{args.BytesReceived} - {args.TotalBytesToReceive} - {args.Status}");
			if (args.HasCompleted && args.BytesReceived != 0 && args.TotalBytesToReceive != 0) {
				sender.OnProgressed -= Item_OnProgressed;
				downloadingPool.Remove(sender);
				completedPool.Insert(0, sender);
				sender.RequestRemoveFromComplete += (s, e) => {
					completedPool.Remove(sender);
				};
			}
		}

		public static bool Pausing {
			get => pausing;
			set {
				pausing = value;
				if (value) {
					foreach (DownloadInstance item in downloadingPool) {
						item.Pause();
					}
				} else {
					foreach (DownloadInstance item in downloadingPool) {
						item.Resume();
					}
				}
			}
		}

		public static async Task RegisterDownload(E621Post post, string folderName = null) {
			try {
				string filename = $"{post.ID}.{post.File.Ext}";

				StorageFolder folder = await GetFolder(folderName);
				StorageFile file = await folder.CreateFileAsync(filename, CreationCollisionOption.OpenIfExists);

				DownloadOperation download = downloader.CreateDownload(new Uri(post.File.URL), file);

				DownloadInstance instance = new(post, download,
					new DownloadInstanceInformation(folder, folder == Local.DownloadFolder, file)
				);

				waitPool.Add(instance);
				instance.OnCancel += Item_OnCancel;

			} catch (Exception ex) {
				Debug.WriteLine(ex.ToString());
			}
		}

		//public static void RegisterDownloads(string folderName = null, params E621Post[] posts) {

		//}

		private static async ValueTask<StorageFolder> GetFolder(string folder = null) {
			if (folder.IsBlank()) {
				return Local.DownloadFolder;
			} else {
				return await Local.DownloadFolder.CreateFolderAsync(folder, CreationCollisionOption.OpenIfExists);
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
