using E621Downloader.Models;
using E621Downloader.Models.Locals;
using E621Downloader.Views.LibrarySection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.System;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

namespace E621Downloader.Pages.LibrarySection {
	public sealed partial class Explorer: Page, ILibraryGridPage {
		private LibraryPage libraryPage;
		private LibraryPassArgs args;

		private bool isLoading;

		public int ItemWidth => libraryPage.Size;
		public int ItemHeight => libraryPage.Size - 30;

		public bool IsLoading {
			get => isLoading;
			set {
				isLoading = value;
				if(isLoading) {
					TitleBar.EnableRefreshButton = false;
					TitleBar.EnableSortButtons = false;
					LoadingGrid.Visibility = Visibility.Visible;
					GroupView.ClearNoDataGrid();
				} else {
					TitleBar.EnableRefreshButton = true;
					TitleBar.EnableSortButtons = true;
					LoadingGrid.Visibility = Visibility.Collapsed;
				}
			}
		}

		public Explorer() {
			this.InitializeComponent();

			TitleBar.OnExplorerClick += TitleBar_OnExplorerClick;
			TitleBar.OnRefresh += TitleBar_OnRefresh;
			TitleBar.OnSearchInput += TitleBar_OnSearchInput;
			TitleBar.OnSearchSubmit += TitleBar_OnSearchSubmit;
			TitleBar.OnOrderTypeChanged += TitleBar_OnOrderTypeChanged;
			TitleBar.OnAsecDesOrderChanged += TitleBar_OnAsecDesOrderChanged;
		}

		protected override async void OnNavigatedTo(NavigationEventArgs e) {
			base.OnNavigatedTo(e);
			if(e.Parameter is LibraryPassArgs args) {
				this.args = args;
				if(this.args.NeedRefresh) {
					TitleBar.ShowLocalChangedHintText = true;
				}
				libraryPage = args.Parent;
				TitleBar.Title = args.Title;

				GroupView.Library = libraryPage;
				GroupView.ViewType = libraryPage.ViewType;

				if(args is LibraryImagesArgs imagesArgs) {
					if(imagesArgs.Files == null) {
						await LoadImages(imagesArgs);
					}
					UpdateImagesAsync(imagesArgs);
					TitleBar.IsFolderBar = false;
				} else if(args is LibraryFoldersArgs folderArgs) {
					if(folderArgs.Folders == null) {
						await Task.Delay(200);// just have this time for animation to play (or not)
						await LoadDownloadFolders(folderArgs);
					}
					UpdateFolders(folderArgs);
					TitleBar.IsFolderBar = true;
				} else if(args is LibraryFilterArgs filterArgs) {
					if(filterArgs.Files != null) {
						UpdateImagesAsync(filterArgs);
					}
					TitleBar.IsFolderBar = false;
				}
			}
		}

		private void TitleBar_OnSearchInput(VirtualKey key) {
			if(key == MainPage.SEARCH_KEY) {
				MainPage.Instance.DelayInputKeyListener();
			}
		}

		private async void TitleBar_OnAsecDesOrderChanged(OrderEnum orderEnum) {
			libraryPage.Order = orderEnum;
			await Refresh(false);
		}

		private async void TitleBar_OnOrderTypeChanged(OrderType orderType) {
			libraryPage.OrderType = orderType;
			await Refresh(false);
		}

		private async void TitleBar_OnSearchSubmit(string text) {
			await Refresh(false, text);
		}

		private async void TitleBar_OnRefresh() {
			await Refresh();
		}

		private async void TitleBar_OnExplorerClick() {
			StorageFolder folder = null;
			if(args is LibraryFoldersArgs foldersArgs) {
				folder = foldersArgs.RootFolder;
			} else if(args is LibraryImagesArgs imagesArgs) {
				folder = imagesArgs.Belonger;
			}
			if(folder == null) {
				return;
			}
			await Launcher.LaunchFolderAsync(folder, new FolderLauncherOptions() {
				DesiredRemainingView = ViewSizePreference.UseMore,
			});
		}

		private async Task Refresh(bool load = true, string matchedName = null) {
			TitleBar.ShowLocalChangedHintText = false;
			GroupView.ClearItems();
			if(args is LibraryImagesArgs imagesArgs) {
				if(load) {
					await LoadImages(imagesArgs);
				}
				UpdateImagesAsync(imagesArgs, matchedName);
			} else if(args is LibraryFoldersArgs folderArgs) {
				if(load) {
					await LoadDownloadFolders(folderArgs);
				}
				UpdateFolders(folderArgs, matchedName);
			}
		}

		public LibraryItemsGroupView GetGroupView() => GroupView;

		private async Task LoadImages(LibraryImagesArgs imagesArgs) {
			IsLoading = true;
			LoadingText.Text = "Getting Folder".Language();
			var list = await Local.GetMetaFiles(imagesArgs.Belonger, (next, i, length) => {
				LoadingText.Text = "Loading File".Language() + $"\n({next.Name})\n{i}/{length}";
			});
			imagesArgs.Files = list;
			IsLoading = false;
		}

		private async Task LoadDownloadFolders(LibraryFoldersArgs folderArgs) {
			IsLoading = true;
			LoadingText.Text = "Loading Folders".Language();
			if(Local.DownloadFolder != null) {
				StorageFolder[] folders = await Local.GetDownloadsFolders();
				folderArgs.Folders = folders.ToList();
			}
			IsLoading = false;
		}

		private async void UpdateImagesAsync(ILibraryImagesArgs args, string matchedName = null) {
			List<(MetaFile meta, BitmapImage bitmap, StorageFile file)> files = args.Files;
			if(!string.IsNullOrWhiteSpace(matchedName)) {
				files = files.Where(f => f.file.Name.Contains(matchedName)).ToList();
			}
			files = (await OrderImagesAsync(files, libraryPage.OrderType, libraryPage.Order, () => {
				IsLoading = true;
				LoadingText.Text = "Sorting".Language();
			}, () => {
				IsLoading = false;
			})).ToList();
			GroupView.SetImages(files);
		}

		private async void UpdateFolders(LibraryFoldersArgs args, string matchedName = null) {
			List<StorageFolder> folders = args.Folders;
			if(!string.IsNullOrWhiteSpace(matchedName)) {
				folders = folders.Where(f => f.Name.Contains(matchedName)).ToList();
			}
			folders = (await OrderFolders(folders, libraryPage.OrderType, libraryPage.Order, () => {
				IsLoading = true;
				LoadingText.Text = "Sorting".Language();
			}, () => {
				IsLoading = false;
			})).ToList();
			GroupView.SetFolders(folders);
		}

		public static async ValueTask<IEnumerable<StorageFolder>> OrderFolders(IEnumerable<StorageFolder> folders, OrderType type, OrderEnum order, Action startSorting = null, Action finishSorting = null) {
			startSorting?.Invoke();
			Func<StorageFolder, object> keySelector;
			switch(type) {
				case OrderType.Name:
					keySelector = s => s.Name;
					break;
				case OrderType.Date:
					Dictionary<StorageFolder, DateTimeOffset> modifiedTime = new();
					foreach(StorageFolder item in folders) {
						modifiedTime.Add(item, (await item.GetBasicPropertiesAsync()).DateModified);
					}
					keySelector = s => modifiedTime[s];
					break;
				case OrderType.Size:
				case OrderType.Type:
				default:
					return folders;
			}

			List<StorageFolder> list = folders.ToList();

			switch(order) {
				case OrderEnum.Asc:
					list = list.OrderBy(keySelector).ToList();
					break;
				case OrderEnum.Desc:
					list = list.OrderByDescending(keySelector).ToList();
					break;
				default:
					return folders;
			}

			finishSorting?.Invoke();
			return list;
		}

		public static async ValueTask<IEnumerable<(MetaFile meta, BitmapImage bitmap, StorageFile file)>> OrderImagesAsync(IEnumerable<(MetaFile meta, BitmapImage bitmap, StorageFile file)> images, OrderType type, OrderEnum order, Action startSorting = null, Action finishSorting = null) {
			if(images == null) {
				return images;
			}
			startSorting?.Invoke();
			Func<(MetaFile meta, BitmapImage bitmap, StorageFile file), object> keySelector;
			switch(type) {
				case OrderType.Name:
					keySelector = s => s.file.Name;
					break;
				case OrderType.Date:
					Dictionary<StorageFile, DateTimeOffset> modifiedTime = new();
					foreach(StorageFile item in images.Select(i => i.file)) {
						modifiedTime.Add(item, (await item.GetBasicPropertiesAsync()).DateModified);
					}
					keySelector = s => modifiedTime[s.file];
					break;
				case OrderType.Size:
					keySelector = s => s.meta.MyPost.file.size;
					break;
				case OrderType.Type:
					keySelector = s => s.file.FileType;
					break;
				default:
					return images;
			}

			var list = images.ToList();

			switch(order) {
				case OrderEnum.Asc:
					list = list.OrderBy(keySelector).ToList();
					break;
				case OrderEnum.Desc:
					list = list.OrderByDescending(keySelector).ToList();
					break;
				default:
					return images;
			}

			finishSorting?.Invoke();
			return list;
		}

		public static void CreateItemContextMenu() {

		}

		public void RefreshRequest() {
			args.NeedRefresh = true;
			TitleBar.ShowLocalChangedHintText = true;
		}

		public void UpdateSize(int size) {
			GroupView.UpdateSize(size);
		}

		public void DisplayHeader(bool enabled) {
			HeaderGrid.Visibility = enabled ? Visibility.Visible : Visibility.Collapsed;
		}
	}
}
