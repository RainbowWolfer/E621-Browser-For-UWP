using E621Downloader.Models;
using E621Downloader.Models.Download;
using E621Downloader.Models.Inerfaces;
using System;
using System.Diagnostics;
using System.Linq;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

namespace E621Downloader.Pages.DownloadSection {
	public sealed partial class DownloadPage: Page, IPage {
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
			this.FocusModeUpdate();
			RefreshCurrentContent();
		}

		public void RefreshCurrentContent() {
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
			TitleButton.BorderThickness = new Thickness(enable ? 2 : 0);
		}

		public void SelectTitle(string title) {
			if(MyNavigationView.MenuItems.ToList().Find(i => ((i as NavigationViewItem).Content as StackPanel).Tag as string == title) is not NavigationViewItem found) {
				var stackPanel = new StackPanel() {
					Orientation = Orientation.Horizontal,
					Tag = title,
				};
				stackPanel.Children.Add(new TextBlock() {
					Text = title,
					VerticalAlignment = VerticalAlignment.Center,
				});
				var closeButton = new Button() {
					Background = new SolidColorBrush(Colors.Transparent),
					BorderThickness = new Thickness(0),
					Margin = new Thickness(5, 0, 0, 0),
					Padding = new Thickness(7),
					Content = new FontIcon() {
						Glyph = "\uE10A",
						FontSize = 14,
					},
				};
				stackPanel.Children.Add(closeButton);
				found = new NavigationViewItem() {
					Icon = new FontIcon {
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

		private void TitleButton_Tapped(object sender, TappedRoutedEventArgs e) {
			EnableTitleButton(false);
			MyNavigationView.SelectedItem = null;

			MainFrame.Navigate(typeof(DownloadOverview), null, new EntranceNavigationTransitionInfo());

		}

		private void NavigationView_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args) {
			EnableTitleButton(true);
			DownloadsGroup group = DownloadsManager.FindGroup((args.InvokedItemContainer.Content as StackPanel).Tag as string);
			if(group != null) {
				MainFrame.Navigate(typeof(DownloadDetailsPage), group, new EntranceNavigationTransitionInfo());
			}
		}

		private void MainFrame_Navigating(object sender, NavigatingCancelEventArgs e) {

		}

		private void MainFrame_Navigated(object sender, NavigationEventArgs e) {

		}

		void IPage.UpdateNavigationItem() {
			MainPage.Instance.currentTag = PageTag.Download;
			MainPage.Instance.UpdateNavigationItem();
		}

		void IPage.FocusMode(bool enable) { }
	}
}
