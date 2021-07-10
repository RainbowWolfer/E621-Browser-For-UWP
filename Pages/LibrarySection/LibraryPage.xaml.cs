using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
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
			tabs = new ObservableCollection<LibraryTab>() {
				new LibraryTab(Symbol.Home, HOMESTRING, false),
			};
			TabsListView.SelectedIndex = 0;
		}

		private void HamburgerButton_Tapped(object sender, TappedRoutedEventArgs e) {
			MySplitView.IsPaneOpen = !MySplitView.IsPaneOpen;
		}

		private void TabsListView_SelectionChanged(object sender, SelectionChangedEventArgs e) {
			if(e.AddedItems != null && e.AddedItems.Count > 0) {
				var target = e.AddedItems[0] as LibraryTab;
				if(target.title == HOMESTRING) {
					Navigate(typeof(Explorer), new object[] { HOMESTRING, this });
				} else {

				}
			}
		}

		public void Navigate(Type targetPage, object param) {
			MainFrame.Navigate(targetPage, param, new DrillInNavigationTransitionInfo());
		}
	}
	public class LibraryTab {
		public Symbol icon;
		public string title;
		public Visibility closeButtonVisibility;
		public LibraryTab(Symbol icon, string title, bool closeButtonVisibility) {
			this.icon = icon;
			this.title = title;
			this.closeButtonVisibility = closeButtonVisibility ? Visibility.Visible : Visibility.Collapsed;
		}
	}
}
