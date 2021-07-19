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
			TabsListView.SelectedIndex = 0;
			Navigate(typeof(Explorer), new object[] { home, this });
			SettingsPage.isDownloadPathChangingHandled = true;
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

		public void Navigate(Type targetPage, object param) {
			MainFrame.Navigate(targetPage, param, new DrillInNavigationTransitionInfo());
			if(param is object[] objs && objs.Length > 0) {
				if(objs[0] is LibraryTab tab) {
					TitleTextBlock.Text = tab.title;
				} else if(objs[0] is ItemBlock block) {
					TitleTextBlock.Text = block.Name;
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

		private void Button_Tapped(object sender, TappedRoutedEventArgs e) {
			Button b = sender as Button;
			Debug.WriteLine(b.Parent);
		}

		private void TabsListView_ItemClick(object sender, ItemClickEventArgs e) {
			if(e.ClickedItem != null) {
				var target = e.ClickedItem as LibraryTab;
				Navigate(typeof(Explorer), new object[] { target, this });
			}
		}

		private void MenuFlyoutItem_Tapped(object sender, TappedRoutedEventArgs e) {
			var item = sender as MenuFlyoutItem;
			switch(item.Text) {
				case "Name":
					if(item.Text == "Order : Name") {
						break;
					}
					item.Text = "Order : Name";
					Order(OrderType.Name);
					break;
				case "Date":
					if(item.Text == "Order : Date") {
						break;
					}
					item.Text = "Order : Date";
					Order(OrderType.Date);
					break;
				case "Size":
					if(item.Text == "Order : Size") {
						break;
					}
					item.Text = "Order : Size";
					Order(OrderType.Size);
					break;
				case "Type - Date":
					if(item.Text == "Order : Type - Date") {
						break;
					}
					item.Text = "Order : Type - Date";
					Order(OrderType.Type);
					break;
				default:
					throw new Exception(item.Text + "not found");
			}
		}
		private void Order(OrderType order) {
			if(current == null) {
				return;
			}
			switch(order) {
				case OrderType.Name:
					foreach(ItemBlock item in current.items) {
						//item.meta.MyPost.file.size

					}
					break;
				case OrderType.Date:
					break;
				case OrderType.Size:
					break;
				case OrderType.Type:
					break;
				default:
					throw new Exception();
			}
		}
		private enum OrderType {
			Name, Date, Size, Type
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

