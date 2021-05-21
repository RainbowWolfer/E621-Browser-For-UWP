using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E621Downloader.Models.Download {
	public class DownloadsGroup {
		public string Title { get; set; }
		public List<DownloadInstance> downloads;


		public DownloadsGroup(string title) {
			Title = title;
			this.downloads = new List<DownloadInstance>();
		}

		public DownloadsGroup(string title, List<DownloadInstance> downloads) {
			Title = title;
			this.downloads = downloads;
		}
		public DownloadsGroup(string title, params DownloadInstance[] downloads) {
			Title = title;
			this.downloads = new List<DownloadInstance>();
			foreach(var item in downloads) {
				this.downloads.Add(item);
			}
		}

		public void AddInstance(DownloadInstance instance) {
			downloads.Add(instance);
		}
	}
}
