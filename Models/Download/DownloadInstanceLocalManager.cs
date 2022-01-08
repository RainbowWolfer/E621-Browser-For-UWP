using E621Downloader.Models.Locals;
using E621Downloader.Models.Posts;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Networking.BackgroundTransfer;
using Windows.Storage;

namespace E621Downloader.Models.Download {
	[Obsolete("May be deleted later", true)]
	public static class DownloadInstanceLocalManager {
		public static async void SaveLocal() {
			var locals = new List<DownloadInstanceLocal>();
			foreach(var item in DownloadsManager.downloads) {
				locals.Add(new DownloadInstanceLocal(item));
			}
			string json = JsonConvert.SerializeObject(locals);
			await FileIO.WriteTextAsync(Local.DownloadsInfoFile, json);
		}

		//public static async void ReadLocal() {
		//	Stream stream = await Local.DownloadsInfoFile.OpenStreamForReadAsync();
		//	StreamReader reader = new StreamReader(stream);
		//	var ReList = JsonConvert.DeserializeObject<List<DownloadInstanceLocal>>(reader.ReadToEnd());
		//	foreach(DownloadInstanceLocal item in ReList) {
		//		if(item.isCompleted) {
		//			DownloadsManager.RestoreCompletedDownload(item.post);
		//		} else {
		//			await DownloadsManager.RegisterDownload(item.post);
		//		}
		//	}
		//}

		public static void Restore() {
			//DownloadsManager.
			var test = new BackgroundDownloader().CreateDownload(null, null);
		}

		public class DownloadInstanceLocal {
			public Post post;

			//public string postID;
			//public string uri;
			public string groupName;
			public bool isCompleted;

			public DownloadInstanceLocal(Post post, string groupName, bool isCompleted) {
				//this.postID = postID;
				this.post = post;
				this.groupName = groupName;
				this.isCompleted = isCompleted;
			}
			public DownloadInstanceLocal(DownloadInstance instance) {
				//this.postID = instance.PostRef.id.ToString();
				//this.uri = instance.PostRef.file.url;
				this.post = instance.PostRef;
				this.groupName = instance.GroupName;
				this.isCompleted = instance.DownloadProgress == 1;
			}
		}
	}
}
