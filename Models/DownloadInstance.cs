using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Networking.BackgroundTransfer;

namespace E621Downloader.Models {
	public class DownloadInstance {
		public Post PostRef { get; private set; }

		public DownloadOperation Operation { get; private set; }

		public BackgroundTransferStatus Status { get => Operation.Progress.Status; }

		public double Progress {
			get {
				ulong received = Operation.Progress.BytesReceived;
				ulong total = Operation.Progress.TotalBytesToReceive;
				if(total == 0) {
					return 0;
				} else {
					return received / (double)total;
				}
			}
		}

		public int Percentage => (int)Math.Ceiling(Progress * 100);

		public string ReceivedKB => (Operation.Progress.BytesReceived / 1000).ToString();
		public string TotalKB => (Operation.Progress.TotalBytesToReceive / 1000).ToString();

		public DownloadInstance(Post post, DownloadOperation operation) {
			PostRef = post;
			Operation = operation;
		}

		public async void StartDownload(Action<DownloadOperation> action) {
			await Operation.StartAsync().AsTask(new CancellationTokenSource().Token, new Progress<DownloadOperation>(action));
		}

	}
}
