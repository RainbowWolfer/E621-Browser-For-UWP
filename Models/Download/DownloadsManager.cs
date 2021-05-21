using System;
using System.Collections.Generic;
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
		public const string DEFUALTTITLE = "Individual Downloads";

		public readonly static List<DownloadInstance> downloads;
		public readonly static BackgroundDownloader downloader;

		public readonly static List<DownloadsGroup> groups;


		static DownloadsManager() {
			downloads = new List<DownloadInstance>();
			downloader = new BackgroundDownloader();
			groups = new List<DownloadsGroup>() {
				new DownloadsGroup(DEFUALTTITLE,new List<DownloadInstance>(){ 
					//new DownloadInstance(),
				}),
				new DownloadsGroup("Wallpaper"),
				new DownloadsGroup("Rating:e"),
				new DownloadsGroup("Feet Sole"),
			};
		}

		public static DownloadInstance RegisterDownload(Post post, Uri uri, IStorageFile file, string groupTitle = DEFUALTTITLE) {
			var instance = new DownloadInstance(post, downloader.CreateDownload(uri, file));
			downloads.Add(instance);
			DownloadsGroup group = FindGroup(groupTitle);
			if(groupTitle == null) {
				groups.Add(new DownloadsGroup(groupTitle, instance));
			} else {
				group.AddInstance(instance);
			}
			//Local.WriteDownloadsInfo();
			return instance;
		}

		public static DownloadsGroup FindGroup(string title) {
			return groups.Find(g => g.Title == DEFUALTTITLE);
		}

		//public static List<DownloadsGroup> GetDownloadingGroups(){
		//	return groups.Where(g=>g.);
		//}

	}
}
