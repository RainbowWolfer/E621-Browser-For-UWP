using System;
using System.Diagnostics;
using Windows.Networking.BackgroundTransfer;

namespace E621Downloader.Models {
	public class DownloadInstance {
		public Post PostRef { get; private set; }

		public DownloadOperation Operation { get; private set; }

		public BackgroundTransferStatus Status { get => Operation.Progress.Status; }

		public double DownloadProgress { get; set; }

		public DownloadInstance(Post post, DownloadOperation operation) {
			PostRef = post;
			Operation = operation;
		}
		public DownloadInstance(Post post, DownloadOperation operation, Action<BackgroundTransferRangesDownloadedEventArgs> action) {
			PostRef = post;
			Operation = operation;
			Operation.RangesDownloaded += (s, e) => action?.Invoke(e);
		}

		public void AddDownloadingCallBack(Action<BackgroundTransferRangesDownloadedEventArgs> action) {
			Operation.RangesDownloaded += (s, e) => action?.Invoke(e);
		}

		public async void StartDownload() {
			//Operation.RangesDownloaded += (s, e) => {
				
			//};
			await Operation.StartAsync();
		}

	}
}
