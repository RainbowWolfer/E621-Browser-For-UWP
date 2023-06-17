using Prism.Mvvm;
using Windows.UI.Xaml.Controls;

namespace YiffBrowser.Views.Pages.E621 {
	public sealed partial class DownloadPage : Page {

		public DownloadPage() {
			this.InitializeComponent();
		}

	}

	public class DownloadPageViewModel : BindableBase {

		public DownloadPageViewModel() {
			Load();
		}

		private async void Load() {
			//Uri source = new Uri("https://example.com/image.jpg");
			//{
			//	// Create a StorageFile for the destination file
			//	StorageFolder downloadsFolder = await KnownFolders.GetFolderForUserAsync(null /* current user */, KnownFolderId.PicturesLibrary);
			//	StorageFile destinationFile = await downloadsFolder.CreateFileAsync("image.jpg", CreationCollisionOption.GenerateUniqueName);
			//	BackgroundTransferCompletionGroup completionGroup = new();
			//	DownloadOperation download = new DownloadOperation(source, destinationFile, completionGroup);
			//	await download.StartAsync();
			//}
			//{
			//	BackgroundDownloader downloadDownloader = new(completionGroup);
			//	await downloadDownloader.CreateDownloadAsync();
			//	//downloadDownloader.
			//}
			//{
			//	BackgroundTransferGroup group = BackgroundTransferGroup.CreateGroup("Default");
			//	group.TransferBehavior = BackgroundTransferBehavior.Serialized;
			//	BackgroundDownloader downloader = new() {
			//		TransferGroup = group,
			//	};
			//	DownloadOperation download = downloader.CreateDownload();
			//	await download.StartAsync();

			//	IReadOnlyList<DownloadOperation> v = await BackgroundDownloader.GetCurrentDownloadsForTransferGroupAsync(group);
			//	foreach (var item in v) {
			//		item.Pause();
			//	}
			//}

			//{


			//	BackgroundDownloader downloader = new BackgroundDownloader();
			//	DownloadOperation download = downloader.CreateDownload(source, destinationFile);
			//	//download.Pause
			//}
		}
	}
}
