using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.Foundation;
using Windows.Networking.BackgroundTransfer;
using YiffBrowser.Models.E621;

namespace YiffBrowser.Services.Downloads {
	public class DownloadInstance(E621Post post, DownloadOperation download) : BindableBase {
		private double progress;
		private ulong receivedBytes;
		private ulong totalBytesToReceive;
		private bool initializing = true;
		private bool showPreviewImage = false;
		private bool isContextMenuOpen;

		public event TypedEventHandler<DownloadInstance, DownloadProgress> OnProgressed;
		public event TypedEventHandler<DownloadInstance, EventArgs> OnCancel;

		public E621Post Post { get; } = post;
		public DownloadOperation Download { get; } = download;

		public CancellationTokenSource CancellationTokenSource { get; } = new CancellationTokenSource();

		public async void StartDownload() {
			try {
				Initializing = true;

				await Download.StartAsync().AsTask(CancellationTokenSource.Token, new Progress<DownloadOperation>(HandleDownloadProgress));

				ResponseInformation response = Download.GetResponseInformation();

			} catch (TaskCanceledException) {

			}
		}

		private void HandleDownloadProgress(DownloadOperation operation) {
			Initializing = false;

			TotalBytesToReceive = operation.Progress.TotalBytesToReceive;
			ReceivedBytes = operation.Progress.BytesReceived;

			DownloadProgress progress = new(TotalBytesToReceive, ReceivedBytes);
			OnProgressed?.Invoke(this, progress);

			Progress = progress.Progress;
		}

		#region ViewModel Stuff

		public bool Initializing {
			get => initializing;
			set => SetProperty(ref initializing, value);
		}

		public ulong ReceivedBytes {
			get => receivedBytes;
			set => SetProperty(ref receivedBytes, value);
		}

		public ulong TotalBytesToReceive {
			get => totalBytesToReceive;
			set => SetProperty(ref totalBytesToReceive, value);
		}

		public double Progress {
			get => progress;
			set => SetProperty(ref progress, value, () => RaisePropertyChanged(nameof(ProgressString)));
		}

		public string ProgressString => $"{Progress:N1}%";

		public ICommand CancelDownloadCommand => new DelegateCommand(CancelDownload);

		private void CancelDownload() {
			CancellationTokenSource.Cancel();
			OnCancel?.Invoke(this, EventArgs.Empty);
		}

		public bool ShowPreviewImage {
			get => showPreviewImage;
			set => SetProperty(ref showPreviewImage, value);
		}

		public ICommand OnPreviewLoadedCommand => new DelegateCommand(OnPreviewLoaded);

		private void OnPreviewLoaded() {
			ShowPreviewImage = true;
		}

		public bool IsContextMenuOpen {
			get => isContextMenuOpen;
			set => SetProperty(ref isContextMenuOpen, value);
		}


		#endregion

	}

	public record DownloadProgress(ulong TotalBytesToReceive, ulong BytesReceived) {
		public double Progress => (double)BytesReceived / TotalBytesToReceive * 100;
		public bool HasCompleted => BytesReceived >= TotalBytesToReceive;
	}
}
