using E621Downloader.Models.Locals;
using E621Downloader.Models.Posts;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Networking.BackgroundTransfer;
using Windows.Storage;
using Windows.UI.Xaml.Controls;

namespace E621Downloader.Models.Download {
	public delegate void OnDownloadFinishEventHandler();
	public static class DownloadsManager {
		//public static event OnDownloadFinishEventHandler OnDownloadFinish;
		public const string DEFAULTTITLE = "Default";

		public static readonly List<DownloadInstance> downloads;
		public static readonly BackgroundDownloader downloader;

		public static readonly List<DownloadsGroup> groups;


		static DownloadsManager() {
			downloads = new List<DownloadInstance>();
			downloader = new BackgroundDownloader();
			groups = new List<DownloadsGroup>();
		}

		public static bool HasDownloading() {
			foreach(DownloadInstance item in downloads) {
				if(item.Status != BackgroundTransferStatus.Completed) {
					return true;
				}
			}
			return false;
		}

		public static async Task<bool> CheckDownloadAvailableWithDialog(Action failedAcion = null) {
			MainPage.HideInstantDialog();
			if(Local.DownloadFolder == null) {
				if(await new ContentDialog() {
					Title = "Error",
					Content = "Download Folder Not Found.\nWould you like to get it set?",
					PrimaryButtonText = "Go To Settings",
					SecondaryButtonText = "Back",
				}.ShowAsync() == ContentDialogResult.Primary) {
					MainPage.NavigateTo(PageTag.Settings);
				}
				failedAcion?.Invoke();
				return false;
			}
			return true;
		}

		public static bool CheckDownloadAvailable() => Local.DownloadFolder != null;

		public static async Task<bool?> RegisterDownloads(CancellationToken token, IEnumerable<Post> posts, IEnumerable<string> tags, Action<string> onProgress = null) {
			return await RegisterDownloads(token, posts, DownloadsGroup.GetGroupTitle(tags), onProgress);
		}

		public static async Task<bool?> RegisterDownloads(CancellationToken token, IEnumerable<Post> posts, string groupTitle, Action<string> onProgress = null) {
			if(!CheckDownloadAvailable()) {
				return false;
			}
			if(string.IsNullOrWhiteSpace(groupTitle)) {
				groupTitle = DEFAULTTITLE;
			}
			onProgress?.Invoke($"Handling Downloads\nGetting Folder - ({groupTitle})");
			if(token.IsCancellationRequested) {
				return null;
			}
			StorageFolder folder = await Local.DownloadFolder.CreateFolderAsync(groupTitle, CreationCollisionOption.OpenIfExists);
			if(folder == null) {
				return false;
			}
			int index = 1;
			foreach(Post item in posts) {
				if(string.IsNullOrEmpty(item.file.url)) {
					continue;
				}
				groupTitle = groupTitle.Replace(":", ";");
				string filename = $"{item.id}.{item.file.ext}";
				if(string.IsNullOrEmpty(groupTitle)) {
					groupTitle = DEFAULTTITLE;
				}
				onProgress?.Invoke($"Handling Downloads\t{index}/{posts.Count()}\nCreating Download File - ({filename})");
				if(token.IsCancellationRequested) {
					return null;
				}
				StorageFile file = await folder.CreateFileAsync(filename, CreationCollisionOption.ReplaceExisting);
				RegisterDownload(item, new Uri(item.file.url), file, groupTitle);
				index++;
			}
			return true;
		}

		public static async Task<bool> RegisterDownload(Post post, IEnumerable<string> tags) {
			return await RegisterDownload(post, DownloadsGroup.GetGroupTitle(tags));
		}

		public static async Task<bool> RegisterDownload(Post post, string groupTitle = DEFAULTTITLE) {
			if(string.IsNullOrEmpty(post.file.url)) {
				return false;
			}
			if(!CheckDownloadAvailable()) {
				return false;
			}
			groupTitle = groupTitle.Replace(":", "-");
			string filename = $"{post.id}.{post.file.ext}";
			if(string.IsNullOrEmpty(groupTitle)) {
				groupTitle = DEFAULTTITLE;
			}
			StorageFolder folder = await Local.DownloadFolder.CreateFolderAsync(groupTitle, CreationCollisionOption.OpenIfExists);
			StorageFile file = await folder.CreateFileAsync(filename, CreationCollisionOption.ReplaceExisting);
			RegisterDownload(post, new Uri(post.file.url), file, groupTitle);
			return true;
		}

		private static DownloadInstance RegisterDownload(Post post, Uri uri, StorageFile file, string groupTitle = DEFAULTTITLE) {
			var instance = new DownloadInstance(post, groupTitle, downloader.CreateDownload(uri, file));
			DownloadsGroup group = FindGroup(groupTitle);
			if(group == null) {
				groups.Add(new DownloadsGroup(groupTitle, instance));
			} else {
				if(group.FindByPost(post) != null) {
					return null;
				}
				group.AddInstance(instance);
			}
			downloads.Add(instance);
			MetaFile meta = Local.CreateMetaFile(file, post, groupTitle);
			instance.metaFile = meta;
			instance.StartDownload();
			return instance;
		}

		public static void RemoveDownloads(DownloadsGroup group) {
			if(groups.Contains(group)) {
				groups.Remove(group);
			}
			group.downloads.ForEach(d => d.Cancel());
		}

		//public static DownloadInstance RestoreCompletedDownload(Post post) {

		//	return null;
		//}

		//public static async Task RestoreIncompletedDownloads() {
		//	var list = await Local.GetAllMetaFiles();
		//	foreach(MetaFile meta in list) {
		//		if(meta.FinishedDownloading) {
		//			continue;
		//		}
		//		Post post = meta.MyPost;
		//		string groupName = meta.Group;
		//		RegisterDownload(post, groupName);
		//	}
		//}

		public static DownloadsGroup FindGroup(string title) {
			return groups.Find(g => g.Title == title);
		}

		//public static void Sort() {
		//	foreach(DownloadsGroup g in groups) {
		//		g.downloads.Sort((a, b) => {
		//			if(a == b) {
		//				return 0;
		//			}
		//			return 1;
		//		});
		//	}
		//}

		//public static List<DownloadsGroup> GetDownloadingGroups(){
		//	return groups.Where(g=>g.);
		//}

	}
}
