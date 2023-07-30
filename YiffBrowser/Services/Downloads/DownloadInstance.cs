using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.Foundation;
using Windows.Networking.BackgroundTransfer;
using Windows.Storage;
using YiffBrowser.Helpers;
using YiffBrowser.Models.E621;

namespace YiffBrowser.Services.Downloads {
	public class DownloadInstance(E621Post post, DownloadOperation download, DownloadInstanceInformation info) : BindableBase {
		private double progress;
		private ulong receivedBytes;
		private ulong totalBytesToReceive;
		private bool initializing = true;
		private bool showPreviewImage = false;
		private bool isContextMenuOpen;
		private bool pausing;
		private bool hasStarted = false;

		public event TypedEventHandler<DownloadInstance, DownloadProgress> OnProgressed;
		public event TypedEventHandler<DownloadInstance, EventArgs> OnCancel;

		public E621Post Post { get; } = post;
		public DownloadOperation Download { get; } = download;
		public DownloadInstanceInformation Information { get; } = info;

		public CancellationTokenSource CancellationTokenSource { get; } = new CancellationTokenSource();

		public async void StartDownload() {
			HasStarted = true;
			try {
				Initializing = true;

				await Download.StartAsync().AsTask(CancellationTokenSource.Token, new Progress<DownloadOperation>(HandleDownloadProgress));

				//Download.AttachAsync();

				ResponseInformation response = Download.GetResponseInformation();

			} catch (TaskCanceledException) {

			} catch (FileNotFoundException ex) {
				Debug.WriteLine(ex);
			}
		}

		private void HandleDownloadProgress(DownloadOperation operation) {
			Initializing = false;

			TotalBytesToReceive = operation.Progress.TotalBytesToReceive;
			ReceivedBytes = operation.Progress.BytesReceived;

			DownloadProgress progress = new(TotalBytesToReceive, ReceivedBytes, operation.Progress.Status);
			OnProgressed?.Invoke(this, progress);

			Progress = progress.Progress;
		}

		public void Pause() {
			Pausing = true;
			Download.Pause();
		}

		public void Resume() {
			Pausing = false;
			Download.Resume();

			Initializing = true;
		}

		#region ViewModel Stuff

		public bool ShowProgressRing => !Initializing || !Pausing;

		public bool HasStarted {
			get => hasStarted;
			set => SetProperty(ref hasStarted, value);
		}

		public bool Pausing {
			get => pausing;
			set => SetProperty(ref pausing, value, () => {
				RaisePropertyChanged(nameof(ShowProgressRing));
			});
		}

		public bool Initializing {
			get => initializing;
			set => SetProperty(ref initializing, value, () => {
				RaisePropertyChanged(nameof(ShowProgressRing));
			});
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

		public void CancelDownload() {
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

		public FileType FileType => Post.GetFileType();

		public string FileTypeString => FileType.ToString().ToUpper();

		#endregion

	}

	public record DownloadProgress(ulong TotalBytesToReceive, ulong BytesReceived, BackgroundTransferStatus Status) {
		public double Progress => (double)BytesReceived / TotalBytesToReceive * 100;
		public bool HasCompleted => BytesReceived >= TotalBytesToReceive;
	}

	public record DownloadInstanceInformation(StorageFolder DestinationFolder, bool IsRoot);
}
