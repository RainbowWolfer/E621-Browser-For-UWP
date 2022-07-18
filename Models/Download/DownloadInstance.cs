using E621Downloader.Models.Debugging;
using E621Downloader.Models.Locals;
using E621Downloader.Models.Posts;
using E621Downloader.Pages.LibrarySection;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Windows.Networking.BackgroundTransfer;
using Windows.UI.Xaml.Controls;

namespace E621Downloader.Models.Download {
	public class DownloadInstance {
		public Post PostRef { get; private set; }
		public string GroupName { get; set; }

		public MetaFile metaFile;

		public DownloadOperation Operation { get; private set; }

		public double DownloadProgress { get; private set; }

		public BackgroundTransferStatus Status => Operation.Progress.Status;

		public int Percentage => (int)Math.Ceiling(DownloadProgress * 100);

		public ulong BytesReceived => Operation.Progress.BytesReceived;
		public ulong TotalBytesToReceive => Operation.Progress.TotalBytesToReceive;

		public string ReceivedKB => (BytesReceived / 1000).ToString();
		public string TotalKB => (TotalBytesToReceive / 1000).ToString();

		/// <summary> Direct assign action value. Do not use plus sign on this</summary>
		public Action<double> DedicatedDownloadingAction { get; set; }
		public Action<double> SubDownloadingAction { get; set; }
		public Action DownloadCompleteAction { get; set; }
		public CancellationTokenSource Cts { get; private set; }

		public DownloadInstance(Post post, string groupName, DownloadOperation operation) {
			PostRef = post;
			GroupName = groupName;
			Operation = operation;
		}

		public async void StartDownload() {
			try {
				Cts = new CancellationTokenSource();
				await Operation.StartAsync().AsTask(Cts.Token, new Progress<DownloadOperation>(o => {
					ulong received = Operation.Progress.BytesReceived;
					ulong total = Operation.Progress.TotalBytesToReceive;
					if(total == 0) {
						DownloadProgress = -1;
					} else {
						DownloadProgress = received / (double)total;
					}
					DedicatedDownloadingAction?.Invoke(DownloadProgress);
					SubDownloadingAction?.Invoke(DownloadProgress);
					if(DownloadProgress == 1 || Status == BackgroundTransferStatus.Completed) {
						DownloadCompleteAction?.Invoke();
						metaFile.FinishedDownloading = true;
						Local.WriteMetaFile(metaFile, PostRef, GroupName);
						if(LibraryPage.Instance != null && LibraryPage.Instance.Current != null) {
							LibraryPage.Instance.Current.RefreshRequest();
						}
					}
				}));
			} catch(TaskCanceledException) {
				return;
			} catch(Exception ex) {
				MainPage.CreateTip(MainPage.Instance, $"(#{metaFile.MyPost.id}) Download Fail", $"Error Message: {ex.Message}", Symbol.Important);
				ErrorHistories.Add(ex);
			}
		}

		public void Pause() {
			try {
				Operation.Pause();
			} catch(InvalidOperationException ex) {
				Debug.WriteLine("Pause(): " + ex.Message);
				ErrorHistories.Add(ex);
			}
		}

		public void Resume() {
			if(Status == BackgroundTransferStatus.Running) {
				return;
			}
			try {
				Operation.Resume();
			} catch(InvalidOperationException ex) {
				Debug.WriteLine("Resume(): " + ex.Message);
				ErrorHistories.Add(ex);
			}
		}

		public void Cancel() {
			try {
				if(Cts != null) {
					Cts.Cancel();
					Cts.Dispose();
					Cts = null;
				}
			} catch(Exception ex) {
				Debug.WriteLine("Cancel(): " + ex.Message);
				ErrorHistories.Add(ex);
			}
		}
	}
}
