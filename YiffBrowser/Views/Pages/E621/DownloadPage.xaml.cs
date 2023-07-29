using Prism.Mvvm;
using System.Collections.ObjectModel;
using Windows.UI.Xaml.Controls;
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

		public ObservableCollection<DownloadInstance> WaitPool { get; }
		public ObservableCollection<DownloadInstance> DownloadingPool { get; }
		public ObservableCollection<DownloadInstance> CompletedPool { get; }

		public DownloadPageViewModel() {
			WaitPool = DownloadManager.waitPool;
			DownloadingPool = DownloadManager.downloadingPool;
			CompletedPool = DownloadManager.completedPool;
		}

	}
}
