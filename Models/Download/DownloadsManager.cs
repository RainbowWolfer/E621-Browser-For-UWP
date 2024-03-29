﻿using E621Downloader.Models.E621;
using E621Downloader.Models.Locals;
using E621Downloader.Models.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.Networking.BackgroundTransfer;
using Windows.Storage;
using Windows.UI.Xaml.Controls;

namespace E621Downloader.Models.Download {
	public delegate void OnDownloadFinishEventHandler();
	public static class DownloadsManager {
		//public static event OnDownloadFinishEventHandler OnDownloadFinish;
		public const string DEFAULT_TITLE = "Default";

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
				if(item.Status is not BackgroundTransferStatus.Completed and
					not BackgroundTransferStatus.Canceled and
					not BackgroundTransferStatus.Error) {
					return true;
				}
			}
			return false;
		}

		public static async Task<bool> CheckDownloadAvailableWithDialog(Action failedAcion = null) {
			MainPage.HideInstantDialog();
			if(Local.DownloadFolder == null) {
				if(await new ContentDialog() {
					Title = "Error".Language(),
					Content = "DownloadFolderNotFound".Language(),
					PrimaryButtonText = "Go To Settings".Language(),
					SecondaryButtonText = "Back".Language(),
					DefaultButton = ContentDialogButton.Primary,
				}.ShowAsync() == ContentDialogResult.Primary) {
					MainPage.NavigateTo(PageTag.Settings);
				}
				failedAcion?.Invoke();
				return false;
			}
			return true;
		}

		public static bool CheckDownloadAvailable() => Local.DownloadFolder != null;

		public static async Task<bool?> RegisterDownloads(CancellationToken token, IEnumerable<E621Post> posts, IEnumerable<string> tags, bool todayDate, Action<string> onProgress = null) {
			return await RegisterDownloads(token, posts, DownloadsGroup.GetGroupTitle(tags), todayDate, onProgress);
		}

		public static async Task<bool?> RegisterDownloads(CancellationToken token, IEnumerable<E621Post> posts, string groupTitle, bool todayDate, Action<string> onProgress = null) {
			if(!CheckDownloadAvailable()) {
				return false;
			}
			if(string.IsNullOrWhiteSpace(groupTitle)) {
				groupTitle = DEFAULT_TITLE;
			}
			if(todayDate) {
				groupTitle = $"{groupTitle} ({Methods.GetTodayDate()})";
			}
			onProgress?.Invoke("Handling Downloads".Language() + "\n" + "Getting Folder".Language() + $" - ({groupTitle})");
			if(token.IsCancellationRequested) {
				return null;
			}
			StorageFolder folder = await Local.DownloadFolder.CreateFolderAsync(groupTitle, CreationCollisionOption.OpenIfExists);
			if(folder == null) {
				return false;
			}
			int index = 1;
			foreach(E621Post item in posts) {
				if(string.IsNullOrEmpty(item.file.url)) {
					continue;
				}
				groupTitle = groupTitle.Replace(":", ";");
				string filename = $"{item.id}.{item.file.ext}";
				if(string.IsNullOrEmpty(groupTitle)) {
					groupTitle = DEFAULT_TITLE;
				}
				onProgress?.Invoke("Handling Downloads".Language() + $"\t{index}/{posts.Count()}\n" + "Creating Download File".Language() + $" - ({filename})");
				if(token.IsCancellationRequested) {
					return null;
				}
				StorageFile file = await folder.CreateFileAsync(filename, CreationCollisionOption.GenerateUniqueName);
				RegisterDownload(item, new Uri(item.file.url), file, groupTitle);
				index++;
			}
			return true;
		}

		public static async Task<bool> RegisterDownload(E621Post post, IEnumerable<string> tags) {
			return await RegisterDownload(post, DownloadsGroup.GetGroupTitle(tags));
		}

		public static async Task<bool> RegisterDownload(E621Post post, string groupTitle = DEFAULT_TITLE) {
			if(string.IsNullOrEmpty(post.file.url)) {
				return false;
			}
			if(!CheckDownloadAvailable()) {
				return false;
			}
			groupTitle = groupTitle.Replace(":", "-");
			string filename = $"{post.id}.{post.file.ext}";
			if(string.IsNullOrEmpty(groupTitle)) {
				groupTitle = DEFAULT_TITLE;
			}
			StorageFolder folder = await Local.DownloadFolder.CreateFolderAsync(groupTitle, CreationCollisionOption.OpenIfExists);
			StorageFile file = await folder.CreateFileAsync(filename, CreationCollisionOption.OpenIfExists);
			RegisterDownload(post, new Uri(post.file.url), file, groupTitle);
			return true;
		}

		private static DownloadInstance RegisterDownload(E621Post post, Uri uri, StorageFile file, string groupTitle = DEFAULT_TITLE) {
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

		public static DownloadsGroup FindGroup(string title) {
			return groups.Find(g => g.Title == title);
		}


	}
}
