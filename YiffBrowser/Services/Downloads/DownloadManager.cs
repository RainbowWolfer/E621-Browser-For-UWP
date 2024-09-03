using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.Networking.BackgroundTransfer;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using YiffBrowser.Database;
using YiffBrowser.Helpers;
using YiffBrowser.Models.E621;
using YiffBrowser.Services.Locals;
using YiffBrowser.Services.Networks;

namespace YiffBrowser.Services.Downloads {
	public static class DownloadManager {

		public static readonly BackgroundDownloader downloader;

		public static readonly ObservableCollection<DownloadPreparation> preparingPool = new();
		public static readonly ObservableCollection<DownloadInstance> waitPool = new();
		public static readonly ObservableCollection<DownloadInstance> downloadingPool = new();
		public static readonly ObservableCollection<DownloadInstance> completedPool = new();

		private static bool pausing = false;
		private static readonly DispatcherTimer timer = new();

		static DownloadManager() {
			downloader = new BackgroundDownloader();
			downloader.SetRequestHeader("User-Agent", NetCode.USERAGENT);
			downloader.CostPolicy = BackgroundTransferCostPolicy.Always;

			timer.Interval = TimeSpan.FromSeconds(2);
			timer.Tick += PendingToDownloading_Tick;
			timer.Tick += PreparingToPending_Tick;
			timer.Start();
		}

		private static bool doingPreparation = false;
		private static async void PreparingToPending_Tick(object sender, object e) {
			if (Pausing || doingPreparation || preparingPool.IsEmpty()) {
				return;
			}
			doingPreparation = true;
			List<DownloadPreparation> list = preparingPool.ToList();
			foreach (DownloadPreparation item in list) {
				if (item.HasRequestedCancel) {
					continue;
				}
				await PrepareDownloadToPending(item);
			}
			doingPreparation = false;
		}

		private static async Task PrepareDownloadToPending(DownloadPreparation downloadPreparation) {
			downloadPreparation.DoingTask = true;
			await PrepareDownload(downloadPreparation);
			preparingPool.Remove(downloadPreparation);
		}

		private static void PendingToDownloading_Tick(object sender, object e) {
			if (Pausing) {
				return;
			}
			const int MAX_DOWNLOADING = 12;
			if (downloadingPool.Count < MAX_DOWNLOADING) {
				int needAddCount = MAX_DOWNLOADING - downloadingPool.Count;
				DownloadInstance[] adds = waitPool.Take(needAddCount).ToArray();
				foreach (DownloadInstance item in adds) {
					waitPool.Remove(item);
					downloadingPool.Add(item);
					item.StartDownload();
					item.OnProgressed += Item_OnProgressed;
				}
			}
		}

		private static async void Item_OnCancel(DownloadInstance sender, EventArgs args) {
			sender.OnCancel -= Item_OnCancel;
			if (downloadingPool.Contains(sender)) {
				downloadingPool.Remove(sender);
			} else if (waitPool.Contains(sender)) {
				waitPool.Remove(sender);
			}
			try {
				await sender.Download.ResultFile.DeleteAsync();
			} catch (Exception ex) {
				Debug.WriteLine(ex);
			}
		}

		private static void Item_OnProgressed(DownloadInstance sender, DownloadProgress args) {
			Debug.WriteLine($"{args.BytesReceived} - {args.TotalBytesToReceive} - {args.Status}");
			if (args.HasCompleted && args.BytesReceived != 0 && args.TotalBytesToReceive != 0) {
				sender.OnProgressed -= Item_OnProgressed;
				downloadingPool.Remove(sender);
				completedPool.Insert(0, sender);
				sender.RequestRemoveFromComplete += (s, e) => {
					completedPool.Remove(sender);
				};
				UpdateFileTime(sender);
			}
		}

		//uwp cant update file creation time or modified time
		//the file that DownloadManager created has the wrong time (it is the server file time, not the actual file downloaded time)
		//so here i delete the original file then write its content to a new file
		private static async void UpdateFileTime(DownloadInstance instance) {
			try {
				StorageFolder folder = instance.Information.DestinationFolder;
				StorageFile file = instance.Information.TargetFile;

				IBuffer buffer = await FileIO.ReadBufferAsync(file);

				byte[] byteArray = new byte[buffer.Length];
				using (DataReader dataReader = DataReader.FromBuffer(buffer)) {
					dataReader.ReadBytes(byteArray);
				}

				await file.DeleteAsync();

				StorageFile newFile = await folder.CreateFileAsync(file.Name, CreationCollisionOption.ReplaceExisting);

				await FileIO.WriteBytesAsync(newFile, byteArray);

			} catch (Exception ex) {
				Debug.WriteLine(ex);
				Debugger.Break();
			}
		}

		public static bool Pausing {
			get => pausing;
			set {
				pausing = value;
				if (value) {
					foreach (DownloadInstance item in downloadingPool) {
						item.Pause();
					}
				} else {
					foreach (DownloadInstance item in downloadingPool) {
						item.Resume();
					}
				}
			}
		}

		public static void RegisterDownload(E621Post post, string folderName = null) {
			preparingPool.Add(new DownloadPreparation(post, folderName, Local.DownloadFolder.DisplayName));
		}

		public static async Task PrepareDownload(DownloadPreparation preparation) {
			E621Post post = preparation.Post;
			string folderName = preparation.FolderName;
			try {
				string filename = $"{post.ID}.{post.File.Ext}";
				DownloadInstance instance = null;
				await Task.Run(async () => {
					StorageFolder folder = await GetFolder(folderName);
					if (preparation.HasRequestedCancel) {
						return;
					}

					if (await folder.GetFileOrNullAsync(filename) is StorageFile _file && (await _file.GetBasicPropertiesAsync()).Size != 0) {
						return;
					}

					StorageFile file = await folder.CreateFileAsync(filename, CreationCollisionOption.OpenIfExists);
					if (preparation.HasRequestedCancel) {
						return;
					}

					DownloadOperation download = downloader.CreateDownload(new Uri(post.File.URL), file);
					if (preparation.HasRequestedCancel) {
						return;
					}

					instance = new DownloadInstance(post, download,
						new DownloadInstanceInformation(folder, folder == Local.DownloadFolder, file)
					);

					await E621DownloadDataAccess.AddOrUpdatePost(post);
				});

				if (preparation.HasRequestedCancel || instance == null) {
					return;
				}

				waitPool.Add(instance);
				instance.OnCancel += Item_OnCancel;

			} catch (Exception ex) {
				Debug.WriteLine(ex.ToString());
			} finally {

			}
		}

		private static async ValueTask<StorageFolder> GetFolder(string folder = null) {
			if (folder.IsBlank()) {
				return Local.DownloadFolder;
			} else {
				return await Local.DownloadFolder.CreateFolderAsync(folder, CreationCollisionOption.OpenIfExists);
			}
		}


		public static bool HasSelectedDownloadFolder() {
			return Local.DownloadFolder != null && Local.Settings.DownloadFolderToken.IsNotBlank();
		}

		public static bool HasDownloading() {
			return preparingPool.IsNotEmpty() || waitPool.IsNotEmpty() || downloadingPool.IsNotEmpty();
		}

	}
}
