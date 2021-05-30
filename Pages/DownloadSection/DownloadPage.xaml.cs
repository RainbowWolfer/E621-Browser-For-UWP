using E621Downloader.Models;
using E621Downloader.Models.Download;
using E621Downloader.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Networking.BackgroundTransfer;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

namespace E621Downloader.Pages.DownloadSection {
	public sealed partial class DownloadPage: Page {
		public static DownloadPage Instance;

		public DownloadPage() {
			Instance = this;
			this.InitializeComponent();
			this.NavigationCacheMode = NavigationCacheMode.Enabled;
			MainFrame.Navigate(typeof(DownloadOverview), null, new EntranceNavigationTransitionInfo());
			MyNavigationView.MenuItems.Clear();
		}

		protected override void OnNavigatedTo(NavigationEventArgs e) {
			base.OnNavigatedTo(e);
			if(MainFrame.Content is DownloadOverview overview) {
				overview.Refresh();
			} else if(MainFrame.Content is DownloadDetailsPage details) {
				details.Refresh();
			} else {
				Debug.WriteLine("NOTHING");
			}
		}

		public void NavigateTo(Type type, object parameter = null) {
			MainFrame.Navigate(type, parameter, new EntranceNavigationTransitionInfo());

		}

		public void EnableTitleButton(bool enable) {
			TitleButton.IsChecked = !enable;
			TitleButton.IsHitTestVisible = enable;
		}

		public void SelectTitle(string title) {
			if(!(MyNavigationView.MenuItems.ToList().Find(i => ((i as NavigationViewItem).Content as StackPanel).Tag as string == title) is NavigationViewItem found)) {
				var stackPanel = new StackPanel() {
					Orientation = Orientation.Horizontal,
					Tag = title,
				};
				stackPanel.Children.Add(new TextBlock() {
					Text = title
				});
				var closeButton = new Button() {
					Background = new SolidColorBrush(Colors.Transparent),
					BorderThickness = new Thickness(0),
					FontFamily = new FontFamily("Segoe MDL2 Assets"),
					Content = "\uE10A",
				};
				stackPanel.Children.Add(closeButton);
				found = new NavigationViewItem() {
					Icon = new FontIcon {
						FontFamily = new FontFamily("Segoe MDL2 Assets"),
						Glyph = "\uE9F9",
					},
					Content = stackPanel
				};


				closeButton.Tapped += (s, e) => {
					MyNavigationView.MenuItems.Remove(found);
					if(MyNavigationView.MenuItems.Count >= 1) {
						MyNavigationView.SelectedItem = MyNavigationView.MenuItems[0];
					} else {
						MainFrame.Navigate(typeof(DownloadOverview), null, new EntranceNavigationTransitionInfo());
					}
				};

				MyNavigationView.MenuItems.Add(found);
			}
			MyNavigationView.SelectedItem = found;
		}

		private void AutoSuggestBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args) {

		}

		private void TitleButton_Tapped(object sender, TappedRoutedEventArgs e) {
			EnableTitleButton(false);
			MyNavigationView.SelectedItem = null;

			MainFrame.Navigate(typeof(DownloadOverview), null, new EntranceNavigationTransitionInfo());

		}

		private void NavigationView_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args) {
			EnableTitleButton(true);
			DownloadsGroup group = DownloadsManager.FindGroup((args.InvokedItemContainer.Content as StackPanel).Tag as string);
			if(group != null) {
				MainFrame.Navigate(typeof(DownloadDetailsPage), group.downloads, new EntranceNavigationTransitionInfo());
			}
		}

		private void MainFrame_Navigating(object sender, NavigatingCancelEventArgs e) {

		}

		private void MainFrame_Navigated(object sender, NavigationEventArgs e) {

		}
	}
}
