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
				item.GroupName = title;
			}
		}

		public DownloadInstance FindByPost(Post post) {
			foreach(DownloadInstance item in downloads) {
				if(post == item.PostRef) {
					return item;
				}
			}
			return null;
		}

		public void AddInstance(DownloadInstance instance) {
			downloads.Add(instance);
		}

		public static string GetGroupTitle(IEnumerable<string> tags) {
			string result = "";
			foreach(string item in tags) {
				result += item + ' ';
			}
			return result.Trim();
		}
	}
}
