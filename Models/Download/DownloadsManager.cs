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
		public const string DEFAULTTITLE = "Individual Downloads";

		public readonly static List<DownloadInstance> downloads;
		public readonly static BackgroundDownloader downloader;

		public readonly static List<DownloadsGroup> groups;


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

		public static async Task<bool> CheckDownloadAvailable() {
			MainPage.HideInstantDialog();
			if(Local.DownloadFolder == null) {
				if(await new ContentDialog() {
					Title = "Error",
					Content = "Download Folder Not Found.\n Would you like to get it set?",
					PrimaryButtonText = "Back",
					SecondaryButtonText = "Go To Settings",
				}.ShowAsync() == ContentDialogResult.Secondary) {
					MainPage.SelectNavigationItem(PageTag.Settings);
				}
				return false;
			}
			return true;
		}

		public static void RegisterDownload(Post post, IEnumerable<string> tags) {
			RegisterDownload(post, DownloadsGroup.GetGroupTitle(tags));
		}

		public async static void RegisterDownload(Post post, string groupTitle = DEFAULTTITLE) {
			if(string.IsNullOrEmpty(post.file.url)) {
				return;
			}
			if(await CheckDownloadAvailable()) {
				groupTitle = groupTitle.Replace(":", ";");
				string filename = $"{post.id}.{post.file.ext}";
				if(string.IsNullOrEmpty(groupTitle)) {
					groupTitle = DEFAULTTITLE;
				}
				StorageFolder folder = await Local.DownloadFolder.CreateFolderAsync(groupTitle, CreationCollisionOption.OpenIfExists);
				StorageFile file = await folder.CreateFileAsync(filename, CreationCollisionOption.ReplaceExisting);
				RegisterDownload(post, new Uri(post.file.url), file, groupTitle);
			}
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

		public static DownloadInstance RestoreCompletedDownload(Post post) {

			return null;
		}

		public static async Task RestoreIncompletedDownloads() {
			var list = await Local.GetAllMetaFiles();
			foreach(MetaFile meta in list) {
				if(meta.FinishedDownloading) {
					continue;
				}
				Post post = meta.MyPost;
				string groupName = meta.Group;
				RegisterDownload(post, groupName);
			}
		}

		public static DownloadsGroup FindGroup(string title) {
			return groups.Find(g => g.Title == title);
		}

		public static void Sort() {
			foreach(DownloadsGroup g in groups) {
				g.downloads.Sort((a, b) => {
					if(a == b) {
						return 0;
					}
					return 1;
				});
			}
		}

		//public static List<DownloadsGroup> GetDownloadingGroups(){
		//	return groups.Where(g=>g.);
		//}

	}
}
