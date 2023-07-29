using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Networking.BackgroundTransfer;
using YiffBrowser.Models.E621;

namespace YiffBrowser.Services.Downloads {
	public class DownloadInstance {
		public event TypedEventHandler<DownloadInstance, DownloadProgress> OnProgressed;

		public E621Post Post { get; }
		public DownloadOperation Download { get; }

		public CancellationTokenSource CancellationTokenSource { get; } = new CancellationTokenSource();

		public DownloadInstance(E621Post post, DownloadOperation download) {
			Post = post;
			Download = download;
		}

		public async void StartDownload() {
			try {
				//OnProgressed?.Invoke(this, new DownloadProgress(0, 0));
				await Download.StartAsync().AsTask(CancellationTokenSource.Token, new Progress<DownloadOperation>(HandleDownloadProgress));

				ResponseInformation response = Download.GetResponseInformation();

			} catch (TaskCanceledException) {

			}
		}

		private void HandleDownloadProgress(DownloadOperation operation) {
			//operation.Progress
			ulong total = operation.Progress.TotalBytesToReceive;
			ulong received = operation.Progress.BytesReceived;
			Debug.WriteLine($"{((decimal)received / total) * 100}%");

			OnProgressed?.Invoke(this, new DownloadProgress(total, received));
		}
	}

	public record DownloadProgress(ulong TotalBytesToReceive, ulong BytesReceived) {
		public double Progress => (double)BytesReceived / TotalBytesToReceive * 100;
		public bool HasCompleted => BytesReceived >= TotalBytesToReceive;
	}
}
