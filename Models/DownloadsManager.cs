using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Networking.BackgroundTransfer;
using Windows.Storage;

namespace E621Downloader.Models {
	public delegate void OnDownloadFinishEventHandler();
	public static class DownloadsManager {
		//public static event OnDownloadFinishEventHandler OnDownloadFinish;

		public readonly static List<DownloadInstance> downloads;

		public readonly static BackgroundDownloader downloader;

		static DownloadsManager() {
			downloads = new List<DownloadInstance>();
			downloader = new BackgroundDownloader();
		}

		public static DownloadInstance RegisterDownload(Post post, Uri uri, IStorageFile file, bool autoStart = true) {
			var instance = new DownloadInstance(post, downloader.CreateDownload(uri, file));
			downloads.Add(instance);
			if(autoStart) {
				instance.StartDownload();
			}
			return instance;
		}

	}
}
