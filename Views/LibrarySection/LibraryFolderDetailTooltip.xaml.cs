using Windows.Storage;
using Windows.UI.Xaml.Controls;

namespace E621Downloader.Views.LibrarySection {
	public sealed partial class LibraryFolderDetailTooltip: UserControl {
		private readonly StorageFolder folder;
		public LibraryFolderDetailTooltip(StorageFolder folder) {
			this.InitializeComponent();
			this.folder = folder;
			FolderNameText.Text = folder.Name;
			var date = folder.DateCreated;
			CreatedDateText.Text = $"{date.Year}-{date.Month}-{date.Day} {date.Hour}:{date.Minute}:{date.Second}";
		}
	}
}
