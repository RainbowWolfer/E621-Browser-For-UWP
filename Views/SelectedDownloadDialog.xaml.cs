using Windows.UI.Xaml.Controls;

namespace E621Downloader.Views {
	public sealed partial class SelectedDownloadDialog: UserControl {
		public bool TodayDate => TodayDateBox.IsChecked == true;
		public SelectedDownloadDialog() {
			this.InitializeComponent();
		}
	}
}
