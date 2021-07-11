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
		public const string HOMESTRING = "Home";
		public ObservableCollection<LibraryTab> tabs;

		public LibraryPage() {
			this.InitializeComponent();
			this.NavigationCacheMode = NavigationCacheMode.Enabled;
			tabs = new ObservableCollection<LibraryTab>() {
				new LibraryTab(null, Symbol.Home, HOMESTRING, false),
			};
			TabsListView.SelectedIndex = 0;
		}

		private void HamburgerButton_Tapped(object sender, TappedRoutedEventArgs e) {
			MySplitView.IsPaneOpen = !MySplitView.IsPaneOpen;
		}

		private void TabsListView_SelectionChanged(object sender, SelectionChangedEventArgs e) {
			if(e.AddedItems != null && e.AddedItems.Count > 0) {
				var target = e.AddedItems[0] as LibraryTab;
				Navigate(typeof(Explorer), new object[] { target, this });
			}
		}

		public void Navigate(Type targetPage, object param) {
			MainFrame.Navigate(targetPage, param, new DrillInNavigationTransitionInfo());
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
