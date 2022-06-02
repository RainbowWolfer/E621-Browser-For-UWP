using E621Downloader.Models.Locals;
using E621Downloader.Models.Posts;
using E621Downloader.Views;
using E621Downloader.Views.FoldersSelectionSection;
using E621Downloader.Views.LibrarySection;
using E621Downloader.Views.LocalTagsManagementSection;
using E621Downloader.Views.TagsManagementSection;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.System;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

namespace E621Downloader.Pages.LibrarySection {
	public sealed partial class Explorer: Page, ILibraryGridPage {
		private LibraryPage libraryPage;
		private LibraryPassArgs args;

		private bool isLoading;
		private readonly List<string> selectedTags;

		private readonly List<Grid> itemGrids;

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
			selectedTags = new List<string>();
			itemGrids = new List<Grid>();

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
					UpdateImages(imagesArgs);
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
						UpdateImages(filterArgs);
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
				DesiredRemainingView = ViewSizePreference.UseMore
			});
		}

		private async Task Refresh(bool load = true, string matchedName = null) {
			TitleBar.ShowLocalChangedHintText = false;
			GroupView.ClearItems();
			if(args is LibraryImagesArgs imagesArgs) {
				if(load) {
					await LoadImages(imagesArgs);
				}
				UpdateImages(imagesArgs, matchedName);
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
			LoadingText.Text = "Getting Folder";
			var list = await Local.GetMetaFiles(imagesArgs.Belonger, (next, i, length) => {
				LoadingText.Text = $"Loading File\n({next.Name})\n{i}/{length}";
			});
			imagesArgs.Files = list;
			IsLoading = false;
		}

		private async Task LoadDownloadFolders(LibraryFoldersArgs folderArgs) {
			IsLoading = true;
			LoadingText.Text = "Loading Folders";
			if(Local.DownloadFolder != null) {
				StorageFolder[] folders = await Local.GetDownloadsFolders();
				folderArgs.Folders = folders.ToList();
			}
			IsLoading = false;
		}

		private void UpdateImages(ILibraryImagesArgs args, string matchedName = null) {
			List<(MetaFile meta, BitmapImage bitmap, StorageFile file)> files = args.Files;
			if(!string.IsNullOrWhiteSpace(matchedName)) {
				files = files.Where(f => f.file.Name.Contains(matchedName)).ToList();
			}
			files = OrderImages(files, libraryPage.OrderType, libraryPage.Order).ToList();
			GroupView.SetImages(files);
		}

		private void UpdateFolders(LibraryFoldersArgs args, string matchedName = null) {
			List<StorageFolder> folders = args.Folders;
			if(!string.IsNullOrWhiteSpace(matchedName)) {
				folders = folders.Where(f => f.Name.Contains(matchedName)).ToList();
			}
			folders = OrderFolders(folders, libraryPage.OrderType, libraryPage.Order).ToList();
			GroupView.SetFolders(folders);
		}

		public static IEnumerable<StorageFolder> OrderFolders(IEnumerable<StorageFolder> folders, OrderType type, OrderEnum order) {
			Func<StorageFolder, object> keySelector;

			switch(type) {
				case OrderType.Name:
					keySelector = s => s.Name;
					break;
				case OrderType.Date:
					keySelector = s => s.DateCreated;
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

			return list;
		}

		public static IEnumerable<(MetaFile meta, BitmapImage bitmap, StorageFile file)> OrderImages(IEnumerable<(MetaFile meta, BitmapImage bitmap, StorageFile file)> images, OrderType type, OrderEnum order) {
			if(images == null) {
				return images;
			}
			Func<(MetaFile meta, BitmapImage bitmap, StorageFile file), object> keySelector;
			switch(type) {
				case OrderType.Name:
					keySelector = s => s.file.Name;
					break;
				case OrderType.Date:
					keySelector = s => s.file.DateCreated;
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

			return list;
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
