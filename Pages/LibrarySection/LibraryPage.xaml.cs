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
using Windows.UI.Xaml.Navigation;

namespace E621Downloader.Pages.LibrarySection {
	public sealed partial class LibraryPage: Page {
		public static LibraryPage Instance;
		public const string HOMESTRING = "Home";
		public const string FILTERSTRING = "Filter";
		public ObservableCollection<LibraryTab> tabs;

		public Explorer current;

		private OrderEnum order = OrderEnum.Asc;
		private OrderType orderType = OrderType.Name;

		private readonly LibraryTab home;
		private readonly LibraryTab filter;
		public LibraryPage() {
			Instance = this;
			this.InitializeComponent();
			this.NavigationCacheMode = NavigationCacheMode.Enabled;
			home = new LibraryTab(null, Symbol.Home, HOMESTRING, false);
			filter = new LibraryTab(null, Symbol.Filter, FILTERSTRING, false);
			tabs = new ObservableCollection<LibraryTab>() {
				filter,
				home,
			};
			TabsListView.SelectedIndex = 1;
			NavigateToHome();
			SettingsPage.isDownloadPathChangingHandled = true;

			UpdateOrderText();
		}

		protected override void OnNavigatedTo(NavigationEventArgs e) {
			base.OnNavigatedTo(e);
			if(!SettingsPage.isDownloadPathChangingHandled) {
				Navigate(typeof(Explorer), new object[] { home, this });
				SettingsPage.isDownloadPathChangingHandled = true;
			}
		}

		private void HamburgerButton_Tapped(object sender, TappedRoutedEventArgs e) {
			MySplitView.IsPaneOpen = !MySplitView.IsPaneOpen;
		}

		public void NavigateToHome() {
			Navigate(typeof(Explorer), new object[] { home, this });
		}

		public void Navigate(Type targetPage, object param) {
			MainFrame.Navigate(targetPage, param, new DrillInNavigationTransitionInfo());
			if(param is object[] objs && objs.Length > 0) {
				if(objs[0] is LibraryTab tab) {
					TitleTextBlock.Text = tab.title;
					if(tab.folder != null) {
						FlyoutItem_Size.Visibility = Visibility.Visible;
						FlyoutItem_Type.Visibility = Visibility.Visible;
					} else {
						FlyoutItem_Size.Visibility = Visibility.Collapsed;
						FlyoutItem_Type.Visibility = Visibility.Collapsed;
					}
				} else if(objs[0] is ItemBlock block) {
					TitleTextBlock.Text = block.Name;
					FlyoutItem_Size.Visibility = Visibility.Visible;
					FlyoutItem_Type.Visibility = Visibility.Visible;
				}
			}
		}

		public void ToTab(StorageFolder folder, string tabName) {
			foreach(LibraryTab item in tabs) {
				if(item.title == tabName) {
					TabsListView.SelectedIndex = TabsListView.Items.ToList().FindIndex(t => (t as LibraryTab).title == tabName);
					return;
				}
			}
			tabs.Add(new LibraryTab(folder, Symbol.Folder, tabName, true));
			TabsListView.SelectedIndex = TabsListView.Items.Count - 1;
		}

		private void TabsListView_ItemClick(object sender, ItemClickEventArgs e) {
			if(e.ClickedItem != null) {
				var target = e.ClickedItem as LibraryTab;
				Navigate(typeof(Explorer), new object[] { target, this });
			}
		}

		private void UpdateOrderText() {
			string first;
			switch(order) {
				case OrderEnum.Asc:
					first = "Order Asc : ";
					break;
				case OrderEnum.Desc:
					first = "Order Desc : ";
					break;
				default:
					throw new Exception();
			}
			string second;
			switch(orderType) {
				case OrderType.Name:
					second = "Name";
					break;
				case OrderType.Date:
					second = "Date";
					break;
				case OrderType.Size:
					second = "Size";
					break;
				case OrderType.Type:
					second = "Type - Date";
					break;
				default:
					throw new Exception();
			}
			OrderButton.Content = first + second;
		}
		private void Order() {
			Order(this.order, this.orderType);
		}
		private void Order(OrderEnum order, OrderType type) {
			if(current == null) {
				return;
			}

			Func<ItemBlock, string> keySelector;
			switch(type) {
				case OrderType.Name:
					keySelector = o => o.IsFolder ? o.Name : o.imageFile.DisplayName;
					break;
				case OrderType.Date:
					keySelector = o => o.IsFolder ? o.parentFolder.DateCreated.ToString() : o.imageFile.DateCreated.ToString();
					break;
				case OrderType.Size:
					keySelector = o => o.meta.MyPost.file.size.ToString();
					break;
				case OrderType.Type:
					keySelector = o => o.meta.MyPost.file.ext;
					break;
				default:
					throw new Exception();
			}

			List<ItemBlock> newList;
			switch(order) {
				case OrderEnum.Asc:
					newList = current.items.OrderBy(keySelector).ToList();
					break;
				case OrderEnum.Desc:
					newList = current.items.OrderByDescending(keySelector).ToList();
					break;
				default:
					throw new Exception();
			}
			for(int i = 0; i < newList.Count; i++) {
				current.items.Move(current.items.IndexOf(newList[i]), i);
			}
		}
		private enum OrderType {
			Name, Date, Size, Type
		}
		private enum OrderEnum {
			Asc, Desc
		}

		private void OrderEnumButton_Tapped(object sender, TappedRoutedEventArgs e) {
			if(order == OrderEnum.Asc) {
				order = OrderEnum.Desc;
				OrderEnumButton.Content = "\uE74A";
			} else {
				order = OrderEnum.Asc;
				OrderEnumButton.Content = "\uE74B";
			}
			UpdateOrderText();
			Order();
		}

		private void MenuFlyoutItem_Click(object sender, RoutedEventArgs e) {
			var item = sender as MenuFlyoutItem;
			OrderType target;
			switch(item.Text) {
				case "Name":
					target = OrderType.Name;
					break;
				case "Date":
					target = OrderType.Date;
					break;
				case "Size":
					target = OrderType.Size;
					break;
				case "Type - Date":
					target = OrderType.Type;
					break;
				default:
					throw new Exception(item.Text + "not found");
			}
			if(orderType == target) {
				return;
			}
			orderType = target;
			UpdateOrderText();
			Order();
		}

		private void RefreshButton_Tapped(object sender, TappedRoutedEventArgs e) {
			if(current == null) {
				return;
			}
			object param;
			if(current.CurrentItemBlock != null && current.CurrentLibraryTab != null) {
				throw new Exception();
			} else if(current.CurrentItemBlock != null) {
				param = current.CurrentItemBlock;
			} else if(current.CurrentLibraryTab != null) {
				param = current.CurrentLibraryTab;
			} else {
				throw new Exception();
			}

			Navigate(typeof(Explorer), new object[] { param, this });
		}

		public void EnableRefreshButton(bool b) {
			RefreshPanel.Visibility = b ? Visibility.Visible : Visibility.Collapsed;
		}

		private void CloseButton_Tapped(object sender, TappedRoutedEventArgs e) {
			Button b = sender as Button;
			Debug.WriteLine(b.Parent);
		}

		private void MySearchBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args) {
			if(current != null && current.items != null) {
				
			}
		}
	}
	public class LibraryTab {
		public Symbol icon;
		public string title;
		public StorageFolder folder;
		public Visibility closeButtonVisibility;
		public LibraryTab(StorageFolder folder, Symbol icon, string title, bool closeButtonVisibility) {
			this.folder = folder;
			this.icon = icon;
			this.title = title;
			this.closeButtonVisibility = closeButtonVisibility ? Visibility.Visible : Visibility.Collapsed;
		}
	}
}

