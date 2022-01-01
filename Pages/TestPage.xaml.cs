using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
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
using Windows.UI.Xaml.Navigation;

namespace E621Downloader.Pages {
	public sealed partial class TestPage: Page {
		public TestPage() {
			this.InitializeComponent();
		}
		private void TabView_Loaded(object sender, RoutedEventArgs e) {
			for(int i = 0; i < 3; i++) {
				(sender as TabView).TabItems.Add(CreateNewTab(i));
			}
		}

		private void TabView_AddButtonClick(TabView sender, object args) {
			sender.TabItems.Add(CreateNewTab(sender.TabItems.Count));
		}

		private void TabView_TabCloseRequested(TabView sender, TabViewTabCloseRequestedEventArgs args) {
			sender.TabItems.Remove(args.Tab);
		}

		private TabViewItem CreateNewTab(int index) {
			TabViewItem newItem = new TabViewItem();

			newItem.Header = $"Document {index}";
			newItem.IconSource = new Microsoft.UI.Xaml.Controls.SymbolIconSource() { Symbol = Symbol.Document };

			// The content of the tab is often a frame that contains a page, though it could be any UIElement.
			Frame frame = new Frame();

			switch(index % 3) {
				case 0:
					frame.Navigate(typeof(PicturePage));
					break;
				case 1:
					frame.Navigate(typeof(SubscriptionPage));
					break;
				case 2:
					frame.Navigate(typeof(TestPage));
					break;
			}

			newItem.Content = frame;

			return newItem;
		}
	}
}
