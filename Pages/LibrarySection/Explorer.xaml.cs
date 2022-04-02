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
		//private readonly List<StorageFolder> folders;
		//private readonly List<StorageFolder> orginalFolders;
		//public ObservableCollection<ItemBlock> items;
		//private readonly List<ItemBlock> originalItems;
		//private readonly List<FontIcon> folderIcons;

		private readonly List<string> selectedTags;

		private readonly List<Grid> itemGrids;

		public int ItemWidth => libraryPage.Size;
		public int ItemHeight => libraryPage.Size - 30;

		//public int FolderFontSize => libraryPage.Size < 200 ? 60 : 120;

		//public LibraryTab CurrentLibraryTab { get; private set; }
		//public ItemBlock CurrentItemBlock { get; private set; }
		//public string CurrentFolderName { get; private set; }

		//private bool refreshNeeded;

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
			//this.DataContextChanged += (s, e) => Bindings.Update();

			//items = new ObservableCollection<ItemBlock>();
			//folders = new List<StorageFolder>();
			//originalItems = new List<ItemBlock>();
			//orginalFolders = new List<StorageFolder>();
			selectedTags = new List<string>();
			itemGrids = new List<Grid>();
			//folderIcons = new List<FontIcon>();

			TitleBar.OnExplorerClick += TitleBar_OnExplorerClick;
			TitleBar.OnRefresh += TitleBar_OnRefresh;
			TitleBar.OnSearchInput += TitleBar_OnSearchInput;
			TitleBar.OnSearchSubmit += TitleBar_OnSearchSubmit;
			TitleBar.OnOrderTypeChanged += TitleBar_OnOrderTypeChanged;
			TitleBar.OnAsecDesOrderChanged += TitleBar_OnAsecDesOrderChanged;
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

		protected async override void OnNavigatedTo(NavigationEventArgs e) {
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
			if(Local.DownloadFolder == null) {
				UpdateHintPanelToNoDownloadFolder();
			} else {
				StorageFolder[] folders = await Local.GetDownloadsFolders();
				if(folders.Length == 0) {
					UpdateHintPanelToNoFolderFound();
				}
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

		private void MyGridView_ItemClick(object sender, ItemClickEventArgs e) {
			//var target = e.ClickedItem as ItemBlock;
			//if(target.IsFolder) {
			//	libraryPage.Navigate(typeof(Explorer), new object[] { target, libraryPage });
			//	libraryPage.ToTab(folders.Find(f => f.DisplayName == target.Name), target.Name);
			//} else {
			//	App.postsList.UpdatePostsList(items.ToList());
			//	App.postsList.Current = target;
			//	MainPage.NavigateToPicturePage(target);
			//}
		}

		private void UpdateHintPanelToNoDownloadFolder() {
			UpdateHintPanel("You have not selected your download folder\nPlease go to the settings page and choose your download folder", "Go To Settings", "\uE713", () => {
				MainPage.NavigateTo(PageTag.Settings);
			});
			SetHintPanelVisible(true);
		}
		private void UpdateHintPanelToNoFolderFound() {
			UpdateHintPanel("No Download Folders Found\nWhy not go to posts browser to download some of your favorites?", "Go To Posts Browser", "\uE155", () => {
				MainPage.NavigateTo(PageTag.PostsBrowser);
			});
			SetHintPanelVisible(true);
		}

		private Action buttonAction;
		private void UpdateHintPanel(string content, string buttonText, string buttonIcon, Action onClick) {
			//HintText.Text = content;
			//HintButtonText.Text = buttonText;
			//HintButtonIcon.Glyph = buttonIcon;
			buttonAction = onClick;
		}

		private void SetHintPanelVisible(bool visible) {
			//HintPanel.Visibility = visible ? Visibility.Visible : Visibility.Collapsed;
			//MyGridView.Visibility = !visible ? Visibility.Visible : Visibility.Collapsed;
		}

		public void Search(string content) {
			//itemGrids.Clear();
			//foreach(ItemBlock o in originalItems) {
			//	if(o.Name.Contains(content)) {
			//		if(!items.Contains(o)) {
			//			items.Add(o);
			//		}
			//	} else {
			//		if(items.Contains(o)) {
			//			items.Remove(o);
			//		}
			//	}
			//}
		}

		public void ClearSearch() {

		}

		public void RefreshRequest() {
			args.NeedRefresh = true;
			TitleBar.ShowLocalChangedHintText = true;
		}

		public void UpdateSize(int size) {
			//foreach(Grid item in itemGrids) {
			//	if(item == null) {
			//		continue;
			//	}
			//	item.Height = height;
			//	item.Width = width;
			//}
			//int fontsize = 120;
			//int size = Math.Max(width, height);
			//if(size < 130) {
			//	fontsize = 30;
			//} else if(size < 200) {
			//	fontsize = 60;
			//}
			GroupView.UpdateSize(size);
			//foreach(FontIcon item in folderIcons) {
			//	item.FontSize = fontsize;
			//}
		}

		private GroupTagList defaultInput;
		private bool isLoading;
		//private async void TagsButton_Tapped(object sender, TappedRoutedEventArgs e) {
		//	var dialog = new ContentDialog() {
		//		Title = "Manage Your Search Tags",
		//	};
		//	var content = new LocalTagsManagementView(dialog, defaultInput);
		//	dialog.Content = content;
		//	await dialog.ShowAsync();

		//	if(content.HandleConfirm) {
		//		defaultInput = content.GetInput();
		//		selectedTags.Clear();
		//		string tooltip = "";
		//		string text = "Selected Tags";
		//		int count = 0;
		//		bool max = false;
		//		foreach(string item in content.GetResult()) {
		//			if(defaultInput != null && !defaultInput.Contains(item)) {
		//				defaultInput.Add(item);
		//			}
		//			selectedTags.Add(item);
		//			tooltip += item + "\n";
		//			if(count++ < 3) {
		//				text += " : " + item;
		//			} else if(!max) {
		//				max = true;
		//				text += " ...";
		//			}
		//		}
		//		//SelectedTagsText.Text = text.Trim();
		//		//SelectedTagsCount.Text = selectedTags.Count.ToString();
		//		ToolTipService.SetToolTip(sender as Button, tooltip.Trim());

		//	}
		//}

		//private async void FoldersButton_Tapped(object sender, TappedRoutedEventArgs e) {
		//if(orginalFolders == null | orginalFolders.Count == 0) {
		//	return;
		//}
		//var foldersView = new FoldersSelectionView(orginalFolders, folders);
		//if(await new ContentDialog() {
		//	Title = "Manage Your Search Tags",
		//	Content = foldersView,
		//	PrimaryButtonText = "Confirm",
		//	CloseButtonText = "Back",
		//}.ShowAsync() == ContentDialogResult.Primary) {
		//	folders.Clear();
		//	string tooltip = "";
		//	string text = "Active Folders";
		//	int count = 0;
		//	bool max = false;
		//	foreach(StorageFolder item in foldersView.GetSelected()) {
		//		folders.Add(item);
		//		tooltip += item.DisplayName + "\n";
		//		if(count++ < 3) {
		//			text += " : " + item.DisplayName;
		//		} else if(!max) {
		//			max = true;
		//			text += " ...";
		//		}
		//	}
		//	//SelectedFoldersText.Text = text.Trim();
		//	//SelectedFoldersCount.Text = folders.Count.ToString();
		//	ToolTipService.SetToolTip(sender as Button, tooltip.Trim());
		//}
		//}

		//private async void SearchButton_Tapped(object sender, TappedRoutedEventArgs e) {
		//MyProgressRing.IsActive = false;
		//MyLoadingBar.IsIndeterminate = true;
		//MyLoadingBar.Visibility = Visibility.Visible;
		//items.Clear();
		//itemGrids.Clear();
		//folderIcons.Clear();
		//await Task.Delay(100);

		//foreach(StorageFolder folder in folders) {
		//	var result = await Local.GetMetaFiles(folder.Name);
		//	foreach((MetaFile, BitmapImage, StorageFile) item in result.Item1) {
		//		if(!GetSelectedRating().Contains(item.Item1.MyPost.rating)) {
		//			continue;
		//		}
		//		int score = item.Item1.MyPost.score.total;
		//		int min = string.IsNullOrEmpty(MinScoreText.Text) ? int.MinValue : int.Parse(MinScoreText.Text);
		//		int max = string.IsNullOrEmpty(MaxScoreText.Text) ? int.MaxValue : int.Parse(MaxScoreText.Text);
		//		if(score < min || score > max) {
		//			continue;
		//		}
		//		List<string> tags = item.Item1.MyPost.tags.GetAllTags();
		//		foreach(string tag in selectedTags) {
		//			if(tags.Contains(tag)) {
		//				var myitem = new ItemBlock() {
		//					meta = item.Item1,
		//					thumbnail = item.Item2,
		//					imageFile = item.Item3,
		//				};
		//				items.Add(myitem);
		//				break;
		//			}
		//		}
		//	}
		//}

		//await Task.Delay(20);
		//MyLoadingBar.IsIndeterminate = false;
		//MyLoadingBar.Visibility = Visibility.Collapsed;
		//}
		//private string[] GetSelectedRating() {
		//var r = new List<string>();
		//if(SCheckBox.IsChecked == true) {
		//	r.Add("s");
		//}
		//if(QCheckBox.IsChecked == true) {
		//	r.Add("q");
		//}
		//if(ECheckBox.IsChecked == true) {
		//	r.Add("e");
		//}
		//return r.ToArray();
		//}

		//private void TextBox_TextChanged(object sender, TextChangedEventArgs e) {
		//	var textbox = sender as TextBox;
		//	if(!int.TryParse(textbox.Text, out int result) && textbox.Text != "" && textbox.Text != "-") {
		//		int pos = textbox.SelectionStart - 1;
		//		textbox.Text = textbox.Text.Remove(pos, 1);
		//		textbox.SelectionStart = pos;
		//	}
		//}

		//private void ItemGrid_Loaded(object sender, RoutedEventArgs e) {
		//	itemGrids.Add(sender as Grid);
		//	//if((sender as Grid).Children.First() is FontIcon icon) {
		//	//	folderIcons.Add(icon);
		//	//}
		//	//if(folderIcons.Count >= items.Count) {
		//	//	UpdateSize(libraryPage.Size, libraryPage.Size - 30);
		//	//}
		//}

		//private void HintActionButton_Tapped(object sender, TappedRoutedEventArgs e) {
		//	buttonAction?.Invoke();
		//}
	}

	//public class ItemBlock: ILocalImage {
	//	public StorageFolder parentFolder;
	//	public StorageFile imageFile;
	//	public MetaFile meta;
	//	public ItemBlock parent;
	//	public ImageSource thumbnail;

	//	public string Name => meta == null ? parentFolder.DisplayName : meta.MyPost.id.ToString();
	//	public bool IsFolder => parentFolder != null;

	//	public Visibility FolderIconVisibility => IsFolder ? Visibility.Visible : Visibility.Collapsed;
	//	public Visibility ImagePreviewVisibility => IsFolder ? Visibility.Collapsed : Visibility.Visible;

	//	public Visibility TypeBorderVisibility => meta != null && new string[] { "webm", "gif", "anim" }.Contains(meta.MyPost?.file?.ext?.ToLower()) ? Visibility.Visible : Visibility.Collapsed;
	//	public string TypeText => meta != null && meta.MyPost != null && meta.MyPost.file != null ? meta.MyPost.file.ext : "UNDEFINED";

	//	public Post ImagePost => meta?.MyPost;
	//	public StorageFile ImageFile => imageFile;
	//}
}
