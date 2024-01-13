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
using Windows.Foundation;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.Storage.Search;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using YiffBrowser.Database;
using YiffBrowser.Models.E621;
using YiffBrowser.Services.Locals;
using YiffBrowser.Services.Networks;
using YiffBrowser.Views.Controls.LocalViews;

namespace YiffBrowser.Views.Pages.E621 {
	public sealed partial class LocalPage : Page {


		public LocalPage() {
			InitializeComponent();
			ViewModel.MainGrid = MainGrid;
			ViewModel.LeftColumnDefinition = LeftColumnDefinition;
			ViewModel.RootView = RootView;
			ViewModel.DetailView = DetailView;
		}

		private void MainScrollViewer_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e) {
			Debug.WriteLine($"{MainScrollViewer.VerticalOffset} / {MainScrollViewer.ScrollableHeight}");
		}
	}

	internal class LocalPageViewModel : BindableBase {
		public VariableSizedWrapGrid MainGrid { get; set; }
		public ColumnDefinition LeftColumnDefinition { get; set; }
		public Grid RootView { get; set; }
		public Grid DetailView { get; set; }

		public int ItemWidth { get; } = 380;
		public int ItemHeight { get; } = 50;

		private FolderItem selectedFolder = null;
		private bool showFolderSideDock = true;
		private FileItem selectedFile;
		private bool isLoadingFolders;

		public FileItemDetailViewModel DetailViewModel { get; } = new FileItemDetailViewModel();

		public ObservableCollection<FolderItem> Folders { get; } = [];
		public ObservableCollection<FileItem> Files { get; } = [];

		public FolderItem SelectedFolder {
			get => selectedFolder;
			set => SetProperty(ref selectedFolder, value, () => {
				LoadFolder(value);
			});
		}

		public FileItem SelectedFile {
			get => selectedFile;
			set => SetProperty(ref selectedFile, value, () => {
				DetailViewModel.FileItem = value;
			});
		}

		public bool IsLoadingFolders {
			get => isLoadingFolders;
			set => SetProperty(ref isLoadingFolders, value);
		}

		public bool ShowFolderSideDock {
			get => showFolderSideDock;
			set => SetProperty(ref showFolderSideDock, value, OnShowFolderSideDockChanged);
		}

		public ICommand ToggleFolderSideDockCommand => new DelegateCommand(() => {
			ShowFolderSideDock = !ShowFolderSideDock;
		});

		private double DockLastWidth { set; get; }
		private void OnShowFolderSideDockChanged() {
			if (ShowFolderSideDock) {
				LeftColumnDefinition.Width = new GridLength(DockLastWidth, GridUnitType.Pixel);
			} else {
				DockLastWidth = LeftColumnDefinition.Width.Value;
				LeftColumnDefinition.Width = new GridLength(0);
			}
		}


		public LocalPageViewModel() {
			Initialize();
			if (Files != null) {
				Files.CollectionChanged += Files_CollectionChanged;
			}

			DetailViewModel.ControlViewModel.BackCommand = new DelegateCommand(DetailBack);
		}

		private void DetailBack() {
			RootView.Visibility = Visibility.Visible;
			DetailView.Visibility = Visibility.Collapsed;
			SelectedFile = null;
		}

		private void Files_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
			if (e.NewItems != null) {
				foreach (FileItem item in e.NewItems.OfType<FileItem>()) {
					FileItemImageView image = new() {
						FileItem = item,
					};
					item.ViewItem = image;
					item.ClickCommand = new DelegateCommand<FileItem>(ItemClick);
					MainGrid.Children.Add(image);
				}
			}
			if (e.OldItems != null) {
				foreach (FileItem item in e.OldItems.OfType<FileItem>()) {

				}
			}
		}

		private void ClearAllFiles() {
			foreach (FileItem item in Files) {

			}
			MainGrid.Children.Clear();
			Files.Clear();
		}

		private void ItemClick(FileItem item) {
			RootView.Visibility = Visibility.Collapsed;
			DetailView.Visibility = Visibility.Visible;
			SelectedFile = item;
		}

		public ICommand RefreshCommand => new DelegateCommand(Initialize);

		private async void Initialize() {
			if (Local.DownloadFolder == null) {
				return;
			}

			IsLoadingFolders = true;

			Folders.Clear();
			Folders.Add(new FolderItem(Local.DownloadFolder));
			IReadOnlyList<StorageFolder> folders = await Local.DownloadFolder.GetFoldersAsync(CommonFolderQuery.DefaultQuery);
			foreach (StorageFolder folder in folders) {
				Folders.Add(new FolderItem(folder));
			}

			IsLoadingFolders = false;
		}

		public async void LoadFolder(FolderItem folder) {
			ClearAllFiles();

			IReadOnlyList<StorageFile> files = await folder.Folder.GetFilesAsync(CommonFileQuery.DefaultQuery);

			int index = 0;
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
				if (index++ < 100) {
					break;
				}
			}
		}

	}

	public class FileItem : BindableBase {
		public event TypedEventHandler<FileItem, E621Post> PostChanged;

		private StorageItemThumbnail thumbnail;
		private FileItemImageView viewItem;
		private E621Post post;
		private bool isLoadingPost;
		private string typeHint;
		private ICommand clickCommand;

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
			set => SetProperty(ref post, value, () => {
				PostChanged?.Invoke(this, value);
			});
		}

		public bool IsLoadingPost {
			get => isLoadingPost;
			set => SetProperty(ref isLoadingPost, value);
		}

		public string TypeHint {
			get => typeHint;
			set => SetProperty(ref typeHint, value);
		}

		public ICommand ClickCommand {
			get => clickCommand;
			set => SetProperty(ref clickCommand, value);
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
				if (Post == null) {
					Post = await E621API.GetPostAsync(id);
					//write to database
				}
				IsLoadingPost = false;
			}
			Thumbnail = await File.GetThumbnailAsync(ThumbnailMode.SingleItem);

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
