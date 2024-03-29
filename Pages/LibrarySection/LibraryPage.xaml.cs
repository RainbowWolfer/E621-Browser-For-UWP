﻿using E621Downloader.Models.Debugging;
using E621Downloader.Models.Inerfaces;
using E621Downloader.Models.Locals;
using E621Downloader.Models.Utilities;
using E621Downloader.Views.LibrarySection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using NavigationView = Microsoft.UI.Xaml.Controls.NavigationView;
using NavigationViewItem = Microsoft.UI.Xaml.Controls.NavigationViewItem;
using NavigationViewItemInvokedEventArgs = Microsoft.UI.Xaml.Controls.NavigationViewItemInvokedEventArgs;

namespace E621Downloader.Pages.LibrarySection {
	public sealed partial class LibraryPage: Page, IPage {
		public static LibraryPage Instance { get; private set; }

		private int size;
		public int Size {
			get => size;
			private set {
				size = value;
				Current?.UpdateSize(size);
			}
		}

		public ILibraryGridPage Current { get; private set; }

		private ItemsGroupViewType viewType = ItemsGroupViewType.ListView;

		public ItemsGroupViewType ViewType {
			get => viewType;
			set {
				viewType = value;
				if(Current != null) {
					Current.GetGroupView().ViewType = viewType;
					//if(IsCurrentFoldersTab()) {
					//	LocalSettings.Current.library_viewType_folders = viewType;
					//} else {
					//	LocalSettings.Current.library_viewType_images = viewType;
					//}
					//LocalSettings.Save();
				}
			}
		}

		public LibraryFilterArgs FilterArgs { get; private set; }
		public LibraryFoldersArgs RootFoldersArgs { get; private set; }
		public Dictionary<StorageFolder, LibraryImagesArgs> ImagesArgs { get; } = new Dictionary<StorageFolder, LibraryImagesArgs>();

		public LibraryPage() {
			Instance = this;
			this.InitializeComponent();
			this.NavigationCacheMode = NavigationCacheMode.Enabled;
			FilterArgs = new LibraryFilterArgs() {
				Parent = this,
				Title = "Filter".Language(),
			};
			RootFoldersArgs = new LibraryFoldersArgs() {
				Parent = this,
				Title = "Root".Language(),
				RootFolder = Local.DownloadFolder,
			};
		}

		protected override void OnNavigatedTo(NavigationEventArgs e) {
			base.OnNavigatedTo(e);
			if(!SettingsPage.isDownloadPathChangingHandled) {
				RootFoldersArgs = new LibraryFoldersArgs() {
					Parent = this,
					Title = "Root".Language(),
					RootFolder = Local.DownloadFolder,
				};
				NavigateToHome();
				ClearTabs();
				SettingsPage.isDownloadPathChangingHandled = true;
			} else {
				if(e.Parameter is string folderName) {
					FindNewTab(folderName);
					//Navigate(typeof(Explorer), new object[] { folderName, this });
					//ToTab();
				} else if(Current == null) {
					//TabsListView.SelectedIndex = 1;
					NavigateToHome();
					SettingsPage.isDownloadPathChangingHandled = true;
				}
			}
			MainPage.ClearLibraryPageParameter();
		}

		public async void FindNewTab(string folderName) {
			//if(RootFoldersArgs.Folders == null) {
			//}
			StorageFolder[] folders = await Local.GetDownloadsFolders();
			RootFoldersArgs.Folders = folders.ToList();
			StorageFolder folder = RootFoldersArgs.Folders.Find(f => f.Name == folderName);
			if(folder == null) {
				return;
			}
			ToTab(folder);
		}

		public void ClearTabs() {
			List<NavigationViewItem> itemsToDelete = new();
			foreach(var item in MainNavigationView.MenuItems.Where(i => i is NavigationViewItem).Cast<NavigationViewItem>()) {
				if(item == RootItem || item == FilterItem) {
					continue;
				}
				itemsToDelete.Add(item);
			}
			foreach(NavigationViewItem item in itemsToDelete) {
				MainNavigationView.MenuItems.Remove(item);
			}
		}

		public void ToTab(StorageFolder folder) {
			bool found = false;
			NavigationViewItem viewItem = null;
			foreach(var item in MainNavigationView.MenuItems.Where(i => i is NavigationViewItem).Cast<NavigationViewItem>()) {
				if(item == RootItem || item == FilterItem) {
					continue;
				}
				if(folder.DisplayName == item.Content as string) {
					found = true;
					viewItem = item;
					break;
				}
			}

			if(!found) {
				NavigationViewItem newMenuItem = CreateMenuItem(folder);
				MenuFlyout flyout = new();
				MenuFlyoutItem delete_item = new() {
					Icon = new FontIcon() {
						Glyph = "\uE10A",
					},
					Text = "Close".Language(),
				};
				delete_item.Click += (s, e) => Delete_Item_Click(newMenuItem);
				flyout.Items.Add(delete_item);
				newMenuItem.ContextFlyout = flyout;
				MainNavigationView.MenuItems.Add(newMenuItem);
				viewItem = newMenuItem;
			}

			NavigateToFolder(viewItem, folder);
		}

		private NavigationViewItem GetCurrent() {
			return MainNavigationView.MenuItems.Where(i => i is NavigationViewItem).Cast<NavigationViewItem>().FirstOrDefault(i => i.IsSelected);
		}

		private void Delete_Item_Click(NavigationViewItem item) {
			if(MainNavigationView.MenuItems.Contains(item)) {
				MainNavigationView.MenuItems.Remove(item);
				if(!MainNavigationView.MenuItems.Where(i => i is NavigationViewItem).Cast<NavigationViewItem>().Any(i => i.IsSelected)) {
					NavigateToHome();
				}
			}
		}

		private NavigationViewItem CreateMenuItem(StorageFolder folder) {
			return new NavigationViewItem() {
				Content = folder.DisplayName,
				Icon = new FontIcon() {
					Glyph = "\uED25",
				},
				Tag = folder,
			};
		}

		private void NavigateToFolder(NavigationViewItem item, StorageFolder folder) {
			if(folder == null || GetCurrent() == item) {
				return;
			}
			UpdateToolBar(false);
			MainNavigationView.SelectedItem = item;
			LibraryImagesArgs args;
			if(ImagesArgs.ContainsKey(folder)) {
				args = ImagesArgs[folder];
			} else {
				args = new LibraryImagesArgs() {
					Parent = this,
					Title = folder.DisplayName,
					Belonger = folder,
					Files = null,
				};
				ImagesArgs.Add(folder, args);
			}

			MainFrame.Navigate(typeof(Explorer), args, new EntranceNavigationTransitionInfo());
			Current = MainFrame.Content as ILibraryGridPage;
		}

		private void UpdateToolBar(bool isFolder) {
			if(isFolder) {
				MyResizeBar.SetSize(LocalSettings.Current.librarSizeView_folders, false);
				ViewType = LocalSettings.Current.library_viewType_folders;
			} else {
				MyResizeBar.SetSize(LocalSettings.Current.librarSizeView_images, false);
				ViewType = LocalSettings.Current.library_viewType_images;
			}
			MyResizeBar.UpdateValue();
			UpdateViewType();
		}

		private bool IsCurrentFoldersTab() {
			return Current is Explorer ex && ex.Args is LibraryFoldersArgs;
		}

		private void ResizeBar_OnSizeChanged(int value, bool save) {
			Size = value;
			if(!save) {
				return;
			}
			if(IsCurrentFoldersTab()) {
				LocalSettings.Current.librarSizeView_folders = value;
			} else {
				LocalSettings.Current.librarSizeView_images = value;
			}
			DelayedSaveLocalSetting();
		}

		private CancellationTokenSource localSaveCTS;
		private async void DelayedSaveLocalSetting() {
			try {
				if(localSaveCTS != null) {
					localSaveCTS.Cancel();
					localSaveCTS.Dispose();
				}
				localSaveCTS = new CancellationTokenSource();
				await Task.Delay(500, localSaveCTS.Token);
				await Local.WriteLocalSettings();
			} catch(OperationCanceledException) { }
		}

		private void MainNavigationView_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args) {
			//if(Current.IsLoading) {
			//	return;
			//}
			if(args.InvokedItemContainer == RootItem) {
				NavigateToHome(false);
			} else if(args.InvokedItemContainer == FilterItem) {
				NavigateToFilter(false);
			} else {
				var item = (NavigationViewItem)args.InvokedItemContainer;
				NavigateToFolder(item, item.Tag as StorageFolder);
			}
		}

		public void NavigateToHome(bool updateListMenuItem = true) {
			if(GetCurrent() == RootItem) {
				return;
			}
			UpdateToolBar(true);
			MainFrame.Navigate(typeof(Explorer), RootFoldersArgs, new EntranceNavigationTransitionInfo());
			if(updateListMenuItem) {
				MainNavigationView.SelectedItem = RootItem;
			}
			Current = MainFrame.Content as ILibraryGridPage;
		}

		public void NavigateToFilter(bool updateListMenuItem = true) {
			if(GetCurrent() == FilterItem) {
				return;
			}
			UpdateToolBar(true);
			MainFrame.Navigate(typeof(LibraryFilterPage), FilterArgs, new EntranceNavigationTransitionInfo());
			if(updateListMenuItem) {
				MainNavigationView.SelectedItem = FilterItem;
			}
			Current = MainFrame.Content as ILibraryGridPage;
		}

		private void ViewTypeMinorButton_Click(object sender, RoutedEventArgs e) {
			switch(ViewType) {
				case ItemsGroupViewType.ListView:
					ViewTypeMinorIcon.Glyph = "\uE154";
					ListViewTypeToggle.IsChecked = false;
					GridViewTypeToggle.IsChecked = true;
					ViewType = ItemsGroupViewType.GridView;
					break;
				case ItemsGroupViewType.GridView:
					ViewTypeMinorIcon.Glyph = "\uE14C";
					ListViewTypeToggle.IsChecked = true;
					GridViewTypeToggle.IsChecked = false;
					ViewType = ItemsGroupViewType.ListView;
					break;
				default:
					throw new Exception();
			}
			Current.GetGroupView().Update();
			SaveViewType();
		}

		private void UpdateViewType() {
			switch(ViewType) {
				case ItemsGroupViewType.ListView:
					ViewTypeMinorIcon.Glyph = "\uE14C";
					ListViewTypeToggle.IsChecked = true;
					GridViewTypeToggle.IsChecked = false;
					break;
				case ItemsGroupViewType.GridView:
					ViewTypeMinorIcon.Glyph = "\uE154";
					ListViewTypeToggle.IsChecked = false;
					GridViewTypeToggle.IsChecked = true;
					break;
				default:
					throw new Exception();
			}
		}

		private void ListViewTypeToggle_Click(object sender, RoutedEventArgs e) {
			ViewTypeMinorIcon.Glyph = "\uE14C";
			ListViewTypeToggle.IsChecked = true;
			GridViewTypeToggle.IsChecked = false;
			ViewType = ItemsGroupViewType.ListView;
			SaveViewType();
			Current.GetGroupView().Update();
		}

		private void GridViewTypeToggle_Click(object sender, RoutedEventArgs e) {
			ViewTypeMinorIcon.Glyph = "\uE154";
			ListViewTypeToggle.IsChecked = false;
			GridViewTypeToggle.IsChecked = true;
			ViewType = ItemsGroupViewType.GridView;
			SaveViewType();
			Current.GetGroupView().Update();
		}

		private void SaveViewType() {
			if(IsCurrentFoldersTab()) {
				LocalSettings.Current.library_viewType_folders = viewType;
			} else {
				LocalSettings.Current.library_viewType_images = viewType;
			}
			LocalSettings.Save();
		}

		public async Task ShowRenameDialog(string originName, Action<string> onRename) {
			RenameDialog.PrimaryButtonText = "Rename".Language();
			RenameDialog.CloseButtonText = "Close".Language();
			RenameDialog.Tag = onRename;
			DialogRenameBox.Tag = originName;
			DialogRenameBox.Text = originName;
			DialogRenameBox.SelectAll();
			RenameDialog.IsPrimaryButtonEnabled = false;
			if(await RenameDialog.ShowAsync() == ContentDialogResult.Primary) {
				onRename.Invoke(DialogRenameBox.Text.Trim());
			}
		}

		private void DialogRenameBox_TextChanged(object sender, TextChangedEventArgs e) {
			var originName = DialogRenameBox.Tag as string;
			var text = DialogRenameBox.Text;
			if(string.IsNullOrEmpty(text)) {
				DialogErrorText.Text = "Cannot be empty".Language();
				DialogErrorText.Visibility = Visibility.Visible;
				RenameDialog.IsPrimaryButtonEnabled = false;
			} else if(originName == text) {
				DialogErrorText.Text = "Cannot be the same as before".Language();
				DialogErrorText.Visibility = Visibility.Visible;
				RenameDialog.IsPrimaryButtonEnabled = false;
			} else {
				DialogErrorText.Visibility = Visibility.Collapsed;
				RenameDialog.IsPrimaryButtonEnabled = true;
			}
		}

		void IPage.UpdateNavigationItem() {
			MainPage.Instance.currentTag = PageTag.Library;
			MainPage.Instance.UpdateNavigationItem();
		}

		void IPage.FocusMode(bool enabled) {
			try {//leave it be, it throws parameters error.
				MainNavigationView.IsPaneVisible = !enabled;
			} catch(Exception ex) {
				ErrorHistories.Add(ex);
			}
			Current.DisplayHeader(!enabled);
		}

		private void DialogEnterKey_Invoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args) {
			args.Handled = true;
			var originName = DialogRenameBox.Tag as string;
			var text = DialogRenameBox.Text;
			if(!string.IsNullOrEmpty(text) && originName != text) {
				if(RenameDialog.Tag is Action<string> action) {
					action.Invoke(text.Trim());
				}
				RenameDialog.Hide();
			}
		}
	}

	public enum OrderType {
		Name, Date, Size, Type, NumberOfFiles, Score
	}

	public enum OrderEnum {
		Asc, Desc
	}

	public interface ILibraryImagesArgs {
		List<(MetaFile meta, BitmapImage bitmap, StorageFile file)> Files { get; set; }
	}

	public abstract class LibraryPassArgs {
		public LibraryPage Parent { get; set; } = null;
		public string Title { get; set; } = "Title Not Set".Language();
		public bool NeedRefresh { get; set; }

		public OrderEnum Order { get; set; } = OrderEnum.Desc;
		public OrderType OrderType { get; set; } = OrderType.Date;

	}

	public class LibraryFoldersArgs: LibraryPassArgs {
		public List<StorageFolder> Folders { get; set; }
		public StorageFolder RootFolder { get; set; }
	}

	public class LibraryImagesArgs: LibraryPassArgs, ILibraryImagesArgs {
		public StorageFolder Belonger { get; set; }
		public List<(MetaFile meta, BitmapImage bitmap, StorageFile file)> Files { get; set; } = null;
	}

	public class LibraryFilterArgs: LibraryPassArgs, ILibraryImagesArgs {
		public bool IsExpanded { get; set; } = true;
		public bool IsAnd { get; set; } = true;

		public bool SafeCheck { get; set; } = true;
		public bool QuestionableCheck { get; set; } = true;
		public bool ExplicitCheck { get; set; } = true;
		public bool ImageCheck { get; set; } = true;
		public bool GifCheck { get; set; } = true;
		public bool WebmCheck { get; set; } = true;

		public int MinimumScore { get; set; } = -10;
		public int MaximumScore { get; set; } = 10;

		public List<StorageFolder> SelectedFolders { get; set; } = new List<StorageFolder>();
		public List<string> SelectedTags { get; set; } = new List<string>();
		public List<(MetaFile meta, BitmapImage bitmap, StorageFile file)> Files { get; set; } = null;
		public int TotalCount => Files?.Count() ?? -1;
	}

}

