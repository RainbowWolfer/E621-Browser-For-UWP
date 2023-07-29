using Prism.Mvvm;
using System.Collections.ObjectModel;
using Windows.UI.Xaml.Controls;
using YiffBrowser.Services.Downloads;

namespace YiffBrowser.Views.Pages.E621 {
	public sealed partial class DownloadPage : Page {

		public DownloadPage() {
			this.InitializeComponent();
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
