using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace E621Downloader.Pages.LibrarySection {
	public sealed partial class LibraryNoFolderPage: Page {
		public LibraryNoFolderPage() {
			this.InitializeComponent();
		}

		private void HintActionButton_Click(object sender, RoutedEventArgs e) {
			MainPage.NavigateTo(PageTag.Settings);
		}
	}
}
