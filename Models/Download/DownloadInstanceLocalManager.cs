using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E621Downloader.Models.Download {
	public static class DownloadInstanceLocalManager {
		public static void SaveLocal() {
			foreach(var item in DownloadsManager.downloads) {

			}
		}

		private class DownloadInstanceLocal {
			public string postID;
			public string groupName;
		}
	}
}
