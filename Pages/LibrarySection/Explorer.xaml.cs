using E621Downloader.Models.Debugging;
using E621Downloader.Models.Locals;
using E621Downloader.Models.Utilities;
using E621Downloader.Views.LibrarySection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.System;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

namespace E621Downloader.Pages.LibrarySection {
	public sealed partial class Explorer: Page, ILibraryGridPage {
		private LibraryPage libraryPage;
		public LibraryPassArgs Args { get; private set; }

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
					TitleBar.EnableClearEmptyFile = false;
					LoadingGrid.Visibility = Visibility.Visible;
					GroupView.ClearNoDataGrid();
				} else {
					TitleBar.EnableRefreshButton = true;
					TitleBar.EnableSortButtons = true;
					TitleBar.EnableClearEmptyFile = true;
					LoadingGrid.Visibility = Visibility.Collapsed;
				}
			}
		}

		public Explorer() {
			this.InitializeComponent();
			//this.NavigationCacheMode = NavigationCacheMode.Required;
			TitleBar.OnExplorerClick += TitleBar_OnExplorerClick;
			TitleBar.OnRefresh += TitleBar_OnRefresh;
			TitleBar.OnSearchInput += TitleBar_OnSearchInput;
			TitleBar.OnSearchSubmit += TitleBar_OnSearchSubmit;
			TitleBar.OnOrderTypeChanged += TitleBar_OnOrderTypeChanged;
			TitleBar.OnAsecDesOrderChanged += TitleBar_OnAsecDesOrderChanged;
			TitleBar.OnEmptyFileClear += TitleBar_OnEmptyFileClear;
			TitleBar.EnableClearEmptyFile = true;
		}

		protected override async void OnNavigatedTo(NavigationEventArgs e) {
			base.OnNavigatedTo(e);
			if(e.Parameter is LibraryPassArgs args) {
				this.Args = args;
				if(this.Args.NeedRefresh) {
					TitleBar.ShowLocalChangedHintText = true;
				}
				libraryPage = args.Parent;
				TitleBar.Title = args.Title;

				GroupView.Library = libraryPage;
				GroupView.ViewType = libraryPage.ViewType;

				TitleBar.SetOrderItem(args.Order, args.OrderType);

				if(args is LibraryImagesArgs imagesArgs) {
					if(imagesArgs.Files == null) {
						await LoadImages(imagesArgs);
					}
					TitleBar.IsFolderBar = false;
					await UpdateImagesAsync(imagesArgs);
				} else if(args is LibraryFoldersArgs folderArgs) {
					if(folderArgs.Folders == null) {
						await Task.Delay(200);// just have this time for animation to play (or not)
						await LoadDownloadFolders(folderArgs);
					}
					TitleBar.IsFolderBar = true;
					await UpdateFoldersAsync(folderArgs);
				} else if(args is LibraryFilterArgs filterArgs) {
					TitleBar.IsFolderBar = false;
					if(filterArgs.Files != null) {
						await UpdateImagesAsync(filterArgs);
					}
				}
			}
		}

		private CancellationTokenSource clearEmptyCTS;
		private async void TitleBar_OnEmptyFileClear() {
			MainPage.CreateInstantDialog("Please Wait".Language(), "Clearing in action".Language(), "Cancel".Language(), () => {
				if(clearEmptyCTS != null) {
					clearEmptyCTS.Cancel();
					clearEmptyCTS.Dispose();
					clearEmptyCTS = null;
				}
			});
			int affectedCount = -1;
			bool canceled = false;
			clearEmptyCTS = new CancellationTokenSource();
			if(Args is LibraryImagesArgs files) {
				affectedCount = await ClearEmptyFilesInFolder(files.Belonger, clearEmptyCTS.Token);
				canceled = affectedCount == -2;
			} else if(Args is LibraryFoldersArgs folder) {
				affectedCount = 0;
				foreach(StorageFolder item in folder.Folders) {
					int count = await ClearEmptyFilesInFolder(item, clearEmptyCTS.Token);
					if(count == -2) {
						canceled = true;
						break;
					}
					affectedCount += count;
				}
			}
			MainPage.HideInstantDialog();
			async void InlineRefresh() {
				if(affectedCount > 0) {
					await Refresh();
				}
			}
			InlineRefresh();
			await new ContentDialog() {
				Title = "Notification".Language(),
				Content = canceled ? "Canceled".Language() : "Cleared {{0}} Files".Language(affectedCount),
				CloseButtonText = "Close".Language(),
			}.ShowAsync();
		}

		private async Task<int> ClearEmptyFilesInFolder(StorageFolder folder, CancellationToken token = default) {
			int affectedCount = 0;
			Dictionary<StorageFile, StorageFile> fileToMeta = new();
			IReadOnlyList<StorageFile> files = await folder.GetFilesAsync();
			int max = files.Count;
			for(int i = 0; i < max; i++) {
				StorageFile file = files[i];
				MainPage.UpdateInstanceDialogContent("Clearing in action".Language() + "\n" + "Folder".Language() + $" : {folder.Name}\n" + "Checking File ({{0}}) {{1}}/{{2}}".Language(file.Name, i, max));
				BasicProperties properties = await file.GetBasicPropertiesAsync();
				if(properties.Size == 0) {
					fileToMeta.Add(file, null);
					//find corresponding meta
					try {
						StorageFile metaFile = await folder.GetFileAsync($"{file.DisplayName}.meta");
						if(metaFile != null) {
							fileToMeta[file] = metaFile;
						}
					} catch(FileNotFoundException) { }
				}

				if(token.IsCancellationRequested) {
					return -2;
				}
			}

			foreach(KeyValuePair<StorageFile, StorageFile> item in fileToMeta) {
				item.Value?.DeleteAsync();
				item.Key?.DeleteAsync();
				affectedCount++;
			}

			return affectedCount;
		}

		private void TitleBar_OnSearchInput(VirtualKey key) {
			if(key == MainPage.SEARCH_KEY) {
				MainPage.Instance.DelayInputKeyListener();
			}
		}

		private async void TitleBar_OnAsecDesOrderChanged(OrderEnum orderEnum) {
			Args.Order = orderEnum;
			await Refresh(false);
		}

		private async void TitleBar_OnOrderTypeChanged(OrderType orderType) {
			Args.OrderType = orderType;
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
			if(Args is LibraryFoldersArgs foldersArgs) {
				folder = foldersArgs.RootFolder;
			} else if(Args is LibraryImagesArgs imagesArgs) {
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
			Args.NeedRefresh = false;
			GroupView.ClearItems();
			if(Args is LibraryImagesArgs imagesArgs) {
				if(load) {
					await LoadImages(imagesArgs);
				}
				await UpdateImagesAsync(imagesArgs, matchedName);
			} else if(Args is LibraryFoldersArgs folderArgs) {
				if(load) {
					await LoadDownloadFolders(folderArgs);
				}
				await UpdateFoldersAsync(folderArgs, matchedName);
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
			Args.NeedRefresh = false;
			TitleBar.ShowLocalChangedHintText = false;
		}

		private async Task LoadDownloadFolders(LibraryFoldersArgs folderArgs) {
			IsLoading = true;
			LoadingText.Text = "Loading Folders".Language();
			if(Local.DownloadFolder != null) {
				StorageFolder[] folders = await Local.GetDownloadsFolders();
				folderArgs.Folders = folders.ToList();
			}
			IsLoading = false;
			Args.NeedRefresh = false;
			TitleBar.ShowLocalChangedHintText = false;
		}

		private async ValueTask UpdateImagesAsync(ILibraryImagesArgs args, string matchedName = null) {
			if(IsLoading) {
				return;
			}
			List<(MetaFile meta, BitmapImage bitmap, StorageFile file)> files = args.Files;
			if(!string.IsNullOrWhiteSpace(matchedName)) {
				files = files.Where(f => f.file.Name.Contains(matchedName)).ToList();
			}
			files = (await OrderImagesAsync(this, files, Args.OrderType, Args.Order, () => {
				IsLoading = true;
				LoadingText.Text = "Sorting".Language();
			}, () => {
				IsLoading = false;
			})).ToList();
			GroupView.SetImages(files);
		}

		private async ValueTask UpdateFoldersAsync(LibraryFoldersArgs args, string matchedName = null) {
			if(IsLoading) {
				return;
			}
			List<StorageFolder> folders = args.Folders;
			if(!string.IsNullOrWhiteSpace(matchedName)) {
				folders = folders.Where(f => f.Name.Contains(matchedName)).ToList();
			}
			folders = (await OrderFolders(this, folders, Args.OrderType, Args.Order, () => {
				IsLoading = true;
				LoadingText.Text = "Sorting".Language();
			}, () => {
				IsLoading = false;
			})).ToList();
			GroupView.SetFolders(folders);
		}

		public static async ValueTask<IEnumerable<StorageFolder>> OrderFolders(ILibraryGridPage page, IEnumerable<StorageFolder> folders, OrderType type, OrderEnum order, Action startSorting = null, Action finishSorting = null) {
			if(page.IsLoading) {
				return folders;
			}
			startSorting?.Invoke();
			Func<StorageFolder, object> keySelector;
			switch(type) {
				case OrderType.Name:
					keySelector = s => s.Name;
					break;
				case OrderType.Date:
					Dictionary<StorageFolder, DateTimeOffset> modifiedTime = new();
					foreach(StorageFolder item in folders) {
						try {
							modifiedTime.Add(item, (await item.GetBasicPropertiesAsync()).DateModified);
						} catch(Exception ex) {
							ErrorHistories.Add(ex);
							finishSorting?.Invoke();
							return folders;
						}
					}
					keySelector = s => modifiedTime[s];
					break;
				case OrderType.Size:
				case OrderType.Type:
				case OrderType.NumberOfFiles:
				case OrderType.Score:
				default:
					finishSorting?.Invoke();
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
					finishSorting?.Invoke();
					return folders;
			}

			finishSorting?.Invoke();
			return list;
		}

		public static async ValueTask<IEnumerable<(MetaFile meta, BitmapImage bitmap, StorageFile file)>> OrderImagesAsync(ILibraryGridPage page, IEnumerable<(MetaFile meta, BitmapImage bitmap, StorageFile file)> images, OrderType type, OrderEnum order, Action startSorting = null, Action finishSorting = null) {
			if(images == null || page.IsLoading) {
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
						try {
							modifiedTime.Add(item, (await item.GetBasicPropertiesAsync()).DateModified);
						} catch(Exception ex) {
							ErrorHistories.Add(ex);
							finishSorting?.Invoke();
							return images;
						}
					}
					keySelector = s => modifiedTime[s.file];
					break;
				case OrderType.Size:
					keySelector = s => s.meta.MyPost.file.size;
					break;
				case OrderType.Type:
					keySelector = s => s.file.FileType;
					break;
				case OrderType.Score:
					keySelector = s => s.meta.MyPost.score.total;
					break;
				case OrderType.NumberOfFiles:
				default:
					finishSorting?.Invoke();
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
					finishSorting?.Invoke();
					return images;
			}

			finishSorting?.Invoke();
			return list;
		}

		public static void CreateItemContextMenu() {

		}

		public void RefreshRequest() {
			Args.NeedRefresh = true;
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
