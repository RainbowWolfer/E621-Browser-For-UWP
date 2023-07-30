using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Windows.Input;
using Windows.UI.Xaml.Controls;
using YiffBrowser.Helpers;
using YiffBrowser.Services.Downloads;

namespace YiffBrowser.Views.Pages.E621 {
	public sealed partial class DownloadPage : Page {

		public DownloadPage() {
			this.InitializeComponent();
		}

		private void MenuFlyout_Opening(object sender, object e) {
			if (sender is MenuFlyout { Target: Grid { Tag: DownloadInstance instance } }) {
				instance.IsContextMenuOpen = true;
			}
		}

		private void MenuFlyout_Closing(Windows.UI.Xaml.Controls.Primitives.FlyoutBase sender, Windows.UI.Xaml.Controls.Primitives.FlyoutBaseClosingEventArgs args) {
			if (sender?.Target?.Tag is DownloadInstance instance) {
				instance.IsContextMenuOpen = false;
			}
		}
	}

	public class DownloadPageViewModel : BindableBase {
		private int totalUnfinishedDownloadCount;
		private bool ableToPause;
		private bool pausing;
		private double totalProgress;

		public ObservableCollection<DownloadInstance> WaitPool { get; }
		public ObservableCollection<DownloadInstance> DownloadingPool { get; }
		public ObservableCollection<DownloadInstance> CompletedPool { get; }

		public DownloadPageViewModel() {
			WaitPool = DownloadManager.waitPool;
			DownloadingPool = DownloadManager.downloadingPool;
			CompletedPool = DownloadManager.completedPool;

			DownloadingPool.CollectionChanged += DownloadingPool_CollectionChanged;
			WaitPool.CollectionChanged += WaitPool_CollectionChanged;

			RaisePropertyChanged();
			UpdateProgress();
		}

		private void WaitPool_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
			UpdateProgress();
		}

		private void DownloadingPool_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
			RaisePropertyChanged("DownloadingPool");
			RaisePropertyChanged("DownloadingPool.Count");
			UpdateProgress();
		}

		private void UpdateProgress() {
			TotalUnfinishedDownloadCount = DownloadingPool.Count + WaitPool.Count;
			if (CompletedPool.Count == 0) {
				TotalProgress = 0;
			} else {
				TotalProgress = TotalUnfinishedDownloadCount / (double)CompletedPool.Count;
			}
		}

		public bool Pausing {
			get => pausing;
			set => SetProperty(ref pausing, value);
		}
		public bool AbleToPause {
			get => ableToPause;
			set => SetProperty(ref ableToPause, value);
		}
		public int TotalUnfinishedDownloadCount {
			get => totalUnfinishedDownloadCount;
			set => SetProperty(ref totalUnfinishedDownloadCount, value);
		}

		public double TotalProgress {
			get => totalProgress;
			set => SetProperty(ref totalProgress, value);
		}

		public ICommand TogglePauseCommand => new DelegateCommand(TogglePause);

		private void TogglePause() {
			Pausing = !Pausing;
			DownloadManager.Pausing = Pausing;
		}

		public ICommand ClearAllCompletedCommand => new DelegateCommand(PerformClearAllCompleted);

		private void PerformClearAllCompleted() {
			CompletedPool.Clear();
		}

		public ICommand CancelAllCommand => new DelegateCommand(CancelAll);

		private async void CancelAll() {
			if (TotalUnfinishedDownloadCount == 0) {
				return;
			}

			if (await "Are you sure to cancel all pending and active downloads?".CreateContentDialog(new ContentDialogParameters() {
				Title = "Clear All Downloads",
				PrimaryText = "Yes",
				CloseText = "No",
				DefaultButton = ContentDialogButton.Close,
			}).ShowDialogAsync() != ContentDialogResult.Primary) {
				return;
			}

			while (WaitPool.IsNotEmpty()) {
				DownloadInstance first = WaitPool.First();
				first.CancelDownload();
			}

			while (DownloadingPool.IsNotEmpty()) {
				DownloadInstance first = DownloadingPool.First();
				first.CancelDownload();
			}

		}

	}
}
