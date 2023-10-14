using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.Storage.Search;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using YiffBrowser.Database;
using YiffBrowser.Helpers;
using YiffBrowser.Models.E621;
using YiffBrowser.Services.Locals;

namespace YiffBrowser.Views.Pages.E621 {
	public sealed partial class LocalPage : Page {


		public LocalPage() {
			InitializeComponent();
		}

	}

	internal class LocalPageViewModel : BindableBase {
		private FolderItem selectedFolder;

		public ObservableCollection<FolderItem> Folders { get; } = [];
		public ObservableCollection<FileItem> Files { get; } = [];

		public FolderItem SelectedFolder {
			get => selectedFolder;
			set => SetProperty(ref selectedFolder, value, () => {
				LoadFolder(value);
			});
		}

		public LocalPageViewModel() {
			Initialize();
		}

		public ICommand RefreshCommand => new DelegateCommand(Initialize);

		private async void Initialize() {
			if (Local.DownloadFolder == null) {
				return;
			}

			Folders.Clear();

			IReadOnlyList<StorageFolder> folders = await Local.DownloadFolder.GetFoldersAsync(CommonFolderQuery.DefaultQuery);
			foreach (StorageFolder folder in folders) {
				FolderItem item = new(folder);
				Folders.Add(item);
				await item.Load();
			}
		}

		public async void LoadFolder(FolderItem folder) {
			IReadOnlyList<StorageFile> files = await folder.Folder.GetFilesAsync(CommonFileQuery.DefaultQuery);

			Files.Clear();
			foreach (StorageFile file in files) {
				FileItem item = new(file);
				Files.Add(item);
				await item.Load();
			}
		}

	}

	public class FileItem : BindableBase {
		private StorageItemThumbnail thumbnail;

		public StorageFile File { get; }

		public StorageItemThumbnail Thumbnail {
			get => thumbnail;
			private set => SetProperty(ref thumbnail, value);
		}

		//private BitmapImage image;
		//public BitmapImage Image {
		//	get => image;
		//	private set => SetProperty(ref image, value);
		//}

		public FileItem(StorageFile file) {
			File = file;
			DateTimeOffset date = file.DateCreated;
			FileInfo info = new(file.Path);
			//Debug.WriteLine(info.LastWriteTime);
		}

		public async Task Load() {
			Stopwatch stopwatch = Stopwatch.StartNew();
			if (int.TryParse(File.DisplayName, out int id)) {
				E621Post post = await E621DownloadDataAccess.GetPostInfo(id);
			}
			stopwatch.Stop();

			Debug.WriteLine($"DB {stopwatch.ElapsedMilliseconds}ms");

			stopwatch.Restart();
			Thumbnail = await File.GetThumbnailAsync(ThumbnailMode.SingleItem);
			stopwatch.Stop();

			Debug.WriteLine($"THUMB {stopwatch.ElapsedMilliseconds}ms");

			Debug.WriteLine($"{Thumbnail.OriginalHeight} - {Thumbnail.OriginalWidth}");
			//try {
			//	using IRandomAccessStream fileStream = await File.OpenAsync(FileAccessMode.Read);
			//	BitmapImage bitmapImage = new() {
			//		DecodePixelHeight = 200,
			//		DecodePixelWidth = 200,
			//	};

			//	await bitmapImage.SetSourceAsync(fileStream);
			//	Image = bitmapImage;
			//} catch (Exception ex) {
			//	Debug.WriteLine(ex);
			//}
		}
	}

	public class FolderItem : BindableBase {
		private StorageItemThumbnail thumbnail;

		public string FilePath { get; }
		public StorageFolder Folder { get; }
		public StorageItemThumbnail Thumbnail {
			get => thumbnail;
			private set => SetProperty(ref thumbnail, value);
		}

		public FolderItem(StorageFolder folder) {
			Folder = folder;
			FilePath = folder.Path;
			FileSystemWatcher watcher = new(folder.Path);
		}

		public async Task Load() {
			Thumbnail = await Folder.GetThumbnailAsync(ThumbnailMode.PicturesView);
		}
	}

}
