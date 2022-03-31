using E621Downloader.Models.Locals;
using E621Downloader.Views.LibrarySection;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using NavigationView = Microsoft.UI.Xaml.Controls.NavigationView;
using NavigationViewItem = Microsoft.UI.Xaml.Controls.NavigationViewItem;
using NavigationViewItemInvokedEventArgs = Microsoft.UI.Xaml.Controls.NavigationViewItemInvokedEventArgs;

namespace E621Downloader.Pages.LibrarySection {
	public sealed partial class LibraryPage: Page {
		public static LibraryPage Instance;
		public const string ROOTSTRING = "ROOT";
		public const string FILTERSTRING = "Filter";
		//public ObservableCollection<LibraryTab> tabs;

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
		private OrderEnum order = OrderEnum.Asc;
		private OrderType orderType = OrderType.Name;

		public ItemsGroupViewType ViewType {
			get => viewType;
			set {
				viewType = value;
				if(Current != null) {
					Current.GetGroupView().ViewType = viewType;
					Current.GetGroupView().Update();
				}
			}
		}
		public OrderEnum Order {
			get => order;
			set {
				order = value;
			}
		}
		public OrderType OrderType {
			get => orderType;
			set {
				orderType = value;
			}
		}
		public LibraryFilterArgs FilterArgs { get; }
		public LibraryFoldersArgs RootFoldersArgs { get; }
		public Dictionary<StorageFolder, LibraryImagesArgs> ImagesArgs { get; } = new Dictionary<StorageFolder, LibraryImagesArgs>();

		//private readonly LibraryTab home;
		//private readonly LibraryTab filter;
		public LibraryPage() {
			Instance = this;
			this.InitializeComponent();
			this.NavigationCacheMode = NavigationCacheMode.Enabled;
			//home = new LibraryTab(null, Symbol.Home, HOMESTRING, false);
			//filter = new LibraryTab(null, Symbol.Filter, FILTERSTRING, false);
			//tabs = new ObservableCollection<LibraryTab>() {
			//	filter,
			//	home,
			//};
			//UpdateOrderText();

			FilterArgs = new LibraryFilterArgs() {
				Parent = this,
				Title = "Filter",
			};

			RootFoldersArgs = new LibraryFoldersArgs() {
				Parent = this,
				Title = "Home",
				RootFolder = Local.DownloadFolder,
			};
		}

		protected override void OnNavigatedTo(NavigationEventArgs e) {
			base.OnNavigatedTo(e);
			if(!SettingsPage.isDownloadPathChangingHandled) {
				//TabsListView.SelectedIndex = 1;
				NavigateToHome();
				SettingsPage.isDownloadPathChangingHandled = true;
			} else {
				if(e.Parameter is string folderName) {
					//Navigate(typeof(Explorer), new object[] { folderName, this });
				} else if(Current == null) {
					//TabsListView.SelectedIndex = 1;
					NavigateToHome();
					SettingsPage.isDownloadPathChangingHandled = true;
				}
			}
			MainPage.ClearLibraryPageParameter();
		}

		//private void HamburgerButton_Tapped(object sender, TappedRoutedEventArgs e) {
		//	MySplitView.IsPaneOpen = !MySplitView.IsPaneOpen;
		//}



		//public void Navigate(Type targetPage, object param) {
		//MainFrame.Navigate(targetPage, param, new EntranceNavigationTransitionInfo());
		//if(param is object[] objs && objs.Length > 0) {
		//	if(objs[0] is LibraryTab tab) {
		//		TitleTextBlock.Text = tab.title;
		//		if(tab.folder != null) {
		//			FlyoutItem_Size.Visibility = Visibility.Visible;
		//			FlyoutItem_Type.Visibility = Visibility.Visible;
		//		} else {
		//			FlyoutItem_Size.Visibility = Visibility.Collapsed;
		//			FlyoutItem_Type.Visibility = Visibility.Collapsed;
		//		}
		//	} else if(objs[0] is ItemBlock block) {
		//		TitleTextBlock.Text = block.Name;
		//		FlyoutItem_Size.Visibility = Visibility.Visible;
		//		FlyoutItem_Type.Visibility = Visibility.Visible;
		//	} else if(objs[0] is string folderName) {
		//		TitleTextBlock.Text = folderName;
		//		FlyoutItem_Size.Visibility = Visibility.Visible;
		//		FlyoutItem_Type.Visibility = Visibility.Visible;
		//	}
		//}
		//}

		public void ToTab(StorageFolder folder) {
			bool found = false;
			foreach(var item in MainNavigationView.MenuItems.Where(i => i is NavigationViewItem).Cast<NavigationViewItem>()) {
				if(item == RootItem || item == FilterItem) {
					continue;
				}
				if(folder.DisplayName == item.Content as string) {
					MainNavigationView.SelectedItem = item;
					found = true;
					break;
				}
			}

			if(!found) {
				NavigationViewItem newMenuItem = CreateMenuItem(folder);
				MainNavigationView.MenuItems.Add(newMenuItem);
				MainNavigationView.SelectedItem = newMenuItem;
			}

			NavigateToFolder(folder);

			//foreach(LibraryTab item in tabs) {
			//	if(item.title == tabName) {
			//		TabsListView.SelectedIndex = TabsListView.Items.ToList().FindIndex(t => (t as LibraryTab).title == tabName);
			//		return;
			//	}
			//}
			//tabs.Add(new LibraryTab(folder, Symbol.Folder, tabName, true));
			//TabsListView.SelectedIndex = TabsListView.Items.Count - 1;
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

		private void NavigateToFolder(StorageFolder folder) {
			if(folder == null) {
				return;
			}
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

		//public void AddTab(string folderName) {
		//	foreach(LibraryTab item in tabs) {
		//		if(item.title == folderName) {
		//			TabsListView.SelectedIndex = TabsListView.Items.ToList().FindIndex(t => (t as LibraryTab).title == folderName);
		//			return;
		//		}
		//	}
		//	tabs.Add(new LibraryTab(folder, Symbol.Folder, folderName, true));
		//	TabsListView.SelectedIndex = TabsListView.Items.Count - 1;
		//}

		//private void TabsListView_ItemClick(object sender, ItemClickEventArgs e) {
		//	if(e.ClickedItem != null) {
		//		//var target = e.ClickedItem as LibraryTab;
		//		//Navigate(typeof(Explorer), new object[] { target, this });
		//	}
		//}

		//private void UpdateOrderText() {
		//	string first;
		//	switch(order) {
		//		case OrderEnum.Asc:
		//			first = "Order Asc : ";
		//			break;
		//		case OrderEnum.Desc:
		//			first = "Order Desc : ";
		//			break;
		//		default:
		//			throw new Exception();
		//	}
		//	string second;
		//	switch(orderType) {
		//		case OrderType.Name:
		//			second = "Name";
		//			break;
		//		case OrderType.Date:
		//			second = "Date";
		//			break;
		//		case OrderType.Size:
		//			second = "Size";
		//			break;
		//		case OrderType.Type:
		//			second = "Type - Date";
		//			break;
		//		default:
		//			throw new Exception();
		//	}
		//	//OrderButton.Content = first + second;
		//}
		//private void Order() {
		//	Order(this.order, this.orderType);
		//}
		//private void Order(OrderEnum order, OrderType type) {
		//	if(current == null) {
		//		return;
		//	}

		//	//Func<ItemBlock, string> keySelector;
		//	//switch(type) {
		//	//	case OrderType.Name:
		//	//		keySelector = o => o.IsFolder ? o.Name : o.imageFile.DisplayName;
		//	//		break;
		//	//	case OrderType.Date:
		//	//		keySelector = o => o.IsFolder ? o.parentFolder.DateCreated.ToString() : o.imageFile.DateCreated.ToString();
		//	//		break;
		//	//	case OrderType.Size:
		//	//		keySelector = o => o.meta.MyPost.file.size.ToString();
		//	//		break;
		//	//	case OrderType.Type:
		//	//		keySelector = o => o.meta.MyPost.file.ext;
		//	//		break;
		//	//	default:
		//	//		throw new Exception();
		//	//}

		//	//List<ItemBlock> newList;
		//	//switch(order) {
		//	//	case OrderEnum.Asc:
		//	//		newList = current.items.OrderBy(keySelector).ToList();
		//	//		break;
		//	//	case OrderEnum.Desc:
		//	//		newList = current.items.OrderByDescending(keySelector).ToList();
		//	//		break;
		//	//	default:
		//	//		throw new Exception();
		//	//}
		//	//for(int i = 0; i < newList.Count; i++) {
		//	//	current.items.Move(current.items.IndexOf(newList[i]), i);
		//	//}
		//}
		//private void OrderEnumButton_Tapped(object sender, TappedRoutedEventArgs e) {
		//	if(order == OrderEnum.Asc) {
		//		order = OrderEnum.Desc;
		//		//OrderEnumButton.Content = "\uE74A";
		//	} else {
		//		order = OrderEnum.Asc;
		//		//OrderEnumButton.Content = "\uE74B";
		//	}
		//	UpdateOrderText();
		//	Order();
		//}

		//private void MenuFlyoutItem_Click(object sender, RoutedEventArgs e) {
		//	var item = sender as MenuFlyoutItem;
		//	OrderType target;
		//	switch(item.Text) {
		//		case "Name":
		//			target = OrderType.Name;
		//			break;
		//		case "Date":
		//			target = OrderType.Date;
		//			break;
		//		case "Size":
		//			target = OrderType.Size;
		//			break;
		//		case "Type - Date":
		//			target = OrderType.Type;
		//			break;
		//		default:
		//			throw new Exception(item.Text + "not found");
		//	}
		//	if(orderType == target) {
		//		return;
		//	}
		//	orderType = target;
		//	UpdateOrderText();
		//	Order();
		//}

		private void RefreshButton_Tapped(object sender, TappedRoutedEventArgs e) {
			if(Current == null) {
				return;
			}
			object param;
			//if(current.CurrentItemBlock != null && current.CurrentFolderName == null && current.CurrentLibraryTab != null) {
			//	throw new Exception();
			//} else if(current.CurrentItemBlock != null) {
			//	param = current.CurrentItemBlock;
			//} else if(current.CurrentLibraryTab != null) {
			//	param = current.CurrentLibraryTab;
			//} else if(current.CurrentFolderName != null) {
			//	param = current.CurrentFolderName;
			//} else {
			//	throw new Exception();
			//}

			//Navigate(typeof(Explorer), new object[] { param, this });
		}

		public void EnableRefreshButton(bool b) {
			//RefreshPanel.Visibility = b ? Visibility.Visible : Visibility.Collapsed;
		}

		private void CloseButton_Tapped(object sender, TappedRoutedEventArgs e) {
			Button b = sender as Button;
			Debug.WriteLine(b.Parent);
		}

		private void MySearchBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args) {
			if(Current != null) {
				Current.Search(args.QueryText);
			}
		}

		private void ResizeBar_OnSizeChanged(int value) {
			Size = value;
		}

		private void MainNavigationView_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args) {
			if(args.InvokedItemContainer == RootItem) {
				NavigateToHome(false);
			} else if(args.InvokedItemContainer == FilterItem) {
				NavigateToFilter(false);
			} else {
				//JumpToPage(((NavigationViewItem)args.InvokedItemContainer).Tag as string);
				NavigateToFolder(args.InvokedItemContainer.Tag as StorageFolder);
			}
		}

		public void NavigateToHome(bool updateListMenuItem = true) {
			MainFrame.Navigate(typeof(Explorer), RootFoldersArgs, new EntranceNavigationTransitionInfo());
			if(updateListMenuItem) {
				MainNavigationView.SelectedItem = RootItem;
			}
			Current = MainFrame.Content as ILibraryGridPage;
		}

		public void NavigateToFilter(bool updateListMenuItem = true) {
			MainFrame.Navigate(typeof(LibraryFilterPage), FilterArgs, new EntranceNavigationTransitionInfo());
			if(updateListMenuItem) {
				MainNavigationView.SelectedItem = FilterItem;
			}
			Current = MainFrame.Content as ILibraryGridPage;
		}
		//private void JumpToPage(NavigationViewItem item) {
		//	MainNavigationView.SelectedItem = item;
		//}

		//private void JumpToPage(string folderName) {
		//	NavigationViewItem found = MainNavigationView.MenuItems.Cast<NavigationViewItem>().FirstOrDefault(i => folderName == i.Tag as string);
		//	MainNavigationView.SelectedItem = found;
		//}

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
		}

		private void ListViewTypeToggle_Click(object sender, RoutedEventArgs e) {
			ListViewTypeToggle.IsChecked = true;
			GridViewTypeToggle.IsChecked = false;
			ViewType = ItemsGroupViewType.ListView;
		}

		private void GridViewTypeToggle_Click(object sender, RoutedEventArgs e) {
			ListViewTypeToggle.IsChecked = false;
			GridViewTypeToggle.IsChecked = true;
			ViewType = ItemsGroupViewType.GridView;
		}
	}
	//public class LibraryTab {
	//	public Symbol icon;
	//	public string title;
	//	public StorageFolder folder;
	//	public Visibility closeButtonVisibility;
	//	public LibraryTab(StorageFolder folder, Symbol icon, string title, bool closeButtonVisibility) {
	//		this.folder = folder;
	//		this.icon = icon;
	//		this.title = title;
	//		this.closeButtonVisibility = closeButtonVisibility ? Visibility.Visible : Visibility.Collapsed;
	//	}
	//}
	public enum OrderType {
		Name, Date, Size, Type
	}

	public enum OrderEnum {
		Asc, Desc
	}

	public interface ILibraryImagesArgs {
		List<(MetaFile meta, BitmapImage bitmap, StorageFile file)> Files { get; set; }
	}

	public abstract class LibraryPassArgs {
		public LibraryPage Parent { get; set; } = null;
		public string Title { get; set; } = "Title Not Set";
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

