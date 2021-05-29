using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Networking.BackgroundTransfer;
using Windows.Storage;

namespace E621Downloader.Models.Download {
	public delegate void OnDownloadFinishEventHandler();
	public static class DownloadsManager {
		//public static event OnDownloadFinishEventHandler OnDownloadFinish;
		public const string DEFAULTTITLE = "Individual Downloads";

		public readonly static List<DownloadInstance> downloads;
		public readonly static BackgroundDownloader downloader;

		public readonly static List<DownloadsGroup> groups;


		static DownloadsManager() {
			downloads = new List<DownloadInstance>();
			downloader = new BackgroundDownloader();
			groups = new List<DownloadsGroup>() {
				//new DownloadsGroup(DEFUALTTITLE,new List<DownloadInstance>(){ 
				//	//new DownloadInstance(),
				//}),
				//new DownloadsGroup("Wallpaper"),
				//new DownloadsGroup("Rating:e"),
				//new DownloadsGroup("Feet Sole"),
			};
		}
		public static void RegisterDownload(Post post, IEnumerable<string> tags) {
			RegisterDownload(post, DownloadsGroup.GetGroupTitle(tags));
		}
		public async static void RegisterDownload(Post post, string groupTitle = DEFAULTTITLE) {
			if(string.IsNullOrEmpty(post.file.url)) {
				return;
			}
			string filename = string.Format("{0}.{1}", post.id, post.file.ext);
			StorageFile file = await Local.downloadFolder.CreateFileAsync(filename, CreationCollisionOption.GenerateUniqueName);
			RegisterDownload(post, new Uri(post.file.url), file, groupTitle);
		}

		public static DownloadInstance RegisterDownload(Post post, Uri uri, IStorageFile file, string groupTitle = DEFAULTTITLE) {
			var instance = new DownloadInstance(post, downloader.CreateDownload(uri, file));
			downloads.Add(instance);
			DownloadsGroup group = FindGroup(groupTitle);
			if(group == null) {
				groups.Add(new DownloadsGroup(groupTitle, instance));
			} else {
				group.AddInstance(instance);
			}
			//Local.WriteDownloadsInfo();
			instance.StartDownload();
			return instance;
		}

		public static DownloadsGroup FindGroup(string title) {
			return groups.Find(g => g.Title == title);
		}

		//public static List<DownloadsGroup> GetDownloadingGroups(){
		//	return groups.Where(g=>g.);
		//}

	}
}
