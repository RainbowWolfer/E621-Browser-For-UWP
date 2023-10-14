using Newtonsoft.Json.Linq;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.Storage.Search;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
using YiffBrowser.Database;
using YiffBrowser.Models.E621;
using YiffBrowser.Services.Locals;
using YiffBrowser.Views.Controls.LocalViews;

namespace YiffBrowser.Views.Pages.E621 {
	public sealed partial class LocalPage : Page {


		public LocalPage() {
			InitializeComponent();
			ViewModel.MainGrid = MainGrid;
		}

	}

	internal class LocalPageViewModel : BindableBase {
		public VariableSizedWrapGrid MainGrid { get; set; }

		public int ItemWidth { get; } = 380;
		public int ItemHeight { get; } = 50;

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
			if (Files != null) {
				Files.CollectionChanged += Files_CollectionChanged;
			}
		}

		private void Files_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
			if (e.NewItems != null) {
				foreach (FileItem item in e.NewItems.OfType<FileItem>()) {
					FileItemImageView image = new() {
						FileItem = item,
					};
					item.ViewItem = image;
					MainGrid.Children.Add(image);
				}
			}
			if (e.OldItems != null) {
				foreach (FileItem item in e.OldItems.OfType<FileItem>()) {

				}
			}
		}

		public ICommand RefreshCommand => new DelegateCommand(Initialize);

		private async void Initialize() {
			if (Local.DownloadFolder == null) {
				return;
			}

			Folders.Clear();
			Folders.Add(new FolderItem(Local.DownloadFolder));
			IReadOnlyList<StorageFolder> folders = await Local.DownloadFolder.GetFoldersAsync(CommonFolderQuery.DefaultQuery);
			foreach (StorageFolder folder in folders) {
				Folders.Add(new FolderItem(folder));
			}
		}

		public async void LoadFolder(FolderItem folder) {
			MainGrid.Children.Clear();
			IReadOnlyList<StorageFile> files = await folder.Folder.GetFilesAsync(CommonFileQuery.DefaultQuery);

			Files.Clear();
			foreach (StorageFile file in files) {
				switch (file.FileType) {
					case ".mp4":
					case ".webm":
					case ".png":
					case ".jpg":
					case ".gif":
						break;
					default:
						continue;
				}
				FileItem item = new(file);
				Files.Add(item);
				await item.Load();
			}
		}

	}

	public class FileItem : BindableBase {
		private StorageItemThumbnail thumbnail;
		private FileItemImageView viewItem;
		private E621Post post;
		private bool isLoadingPost;
		private string typeHint;

		public StorageFile File { get; }
		public FileItemImageView ViewItem {
			get => viewItem;
			set => SetProperty(ref viewItem, value, OnImageChanged);
		}

		private void OnImageChanged() {
			LoadImage();
		}

		public StorageItemThumbnail Thumbnail {
			get => thumbnail;
			private set => SetProperty(ref thumbnail, value);
		}

		public E621Post Post {
			get => post;
			set => SetProperty(ref post, value);
		}

		public bool IsLoadingPost {
			get => isLoadingPost;
			set => SetProperty(ref isLoadingPost, value);
		}

		public string TypeHint {
			get => typeHint;
			set => SetProperty(ref typeHint, value);
		}

		//private BitmapImage image;
		//public BitmapImage Image {
		//	get => image;
		//	private set => SetProperty(ref image, value);
		//}

		public FileItem(StorageFile file) {
			File = file;
			TypeHint = file.FileType switch {
				".gif" => "GIF",
				".webm" => "WEBM",
				".mp4" => "MP4",
				_ => null,
			};
			DateTimeOffset date = file.DateCreated;
			FileInfo info = new(file.Path);
		}

		private void LoadImage() {
			if (ViewItem == null || Thumbnail == null) {
				return;
			}

			double ratio = Thumbnail.OriginalWidth / (double)Thumbnail.OriginalHeight;
			double h = (380 / ratio) / 50;
			int h2 = (int)Math.Ceiling(h);

			VariableSizedWrapGrid.SetRowSpan(ViewItem, h2);
			VariableSizedWrapGrid.SetColumnSpan(ViewItem, 1);

		}

		public async Task Load() {
			if (int.TryParse(File.DisplayName, out int id)) {
				IsLoadingPost = true;
				Post = await E621DownloadDataAccess.GetPostInfo(id);
				IsLoadingPost = false;
			}
			Thumbnail = await File.GetThumbnailAsync(ThumbnailMode.SingleItem);

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
			LoadImage();
		}
	}

	public class FolderItem : BindableBase {
		private StorageItemThumbnail thumbnail;

		public string FilePath { get; }
		public StorageFolder Folder { get; }

		public bool IsRoot => Folder == Local.DownloadFolder;

		public FolderItem(StorageFolder folder) {
			Folder = folder;
			FilePath = folder.Path;
			FileSystemWatcher watcher = new(folder.Path);
		}

	}

}
