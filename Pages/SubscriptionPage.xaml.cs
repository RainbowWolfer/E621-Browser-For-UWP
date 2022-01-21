using E621Downloader.Models.Locals;
using E621Downloader.Models.Posts;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
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
	public sealed partial class SubscriptionPage: Page {
		private CancellationTokenSource cts = new CancellationTokenSource();
		private readonly List<FontIcon> icons = new List<FontIcon>();
		private bool isSelecting = false;
		private int previousIndex = -1;

		private readonly ObservableCollection<FavoriteListViewItem> items = new ObservableCollection<FavoriteListViewItem>();

		public bool IsSelecting {
			get => isSelecting;
			set {
				isSelecting = value;
				if(value) {
					FavoritesListView.SelectionMode = ListViewSelectionMode.Multiple;
					FavoritesListView.SelectedIndex = -1;
					icons.ForEach(i => i.Width = 0);
					DeleteButton.Visibility = Visibility.Visible;
				} else {
					FavoritesListView.SelectionMode = ListViewSelectionMode.Single;
					if(previousIndex != -1) {
						FavoritesListView.SelectedIndex = previousIndex;
					}
					icons.ForEach(i => i.Width = 40);
					DeleteButton.Visibility = Visibility.Collapsed;
					previousIndex = -1;
				}
			}
		}

		public SubscriptionPage() {
			this.InitializeComponent();
			for(int i = 0; i < FavoritesList.Table.Count; i++) {
				FavoritesList item = FavoritesList.Table[i];
				items.Add(new FavoriteListViewItem(i, item.Name, item.Items.Count));
			}
		}

		//private async void Load() {
		//	List<Post> posts = await Post.GetPostsByTagsAsync(cts.Token, true, 1, Local.FollowList);
		//	foreach(Post item in posts ?? new List<Post>()) {
		//		TestText.Text += item.file.url + "\n";
		//	}
		//}

		protected override void OnNavigatedTo(NavigationEventArgs e) {
			base.OnNavigatedTo(e);
			//Load();
		}

		private void HamburgerButton_Tapped(object sender, TappedRoutedEventArgs e) {
			MainSplitView.IsPaneOpen = !MainSplitView.IsPaneOpen;
		}

		private void FollowingButton_Tapped(object sender, TappedRoutedEventArgs e) {
			e.Handled = true;
			FavoritesListView.SelectedIndex = -1;
			FollowingButton.IsChecked = true;
		}

		private void SelectionToggleButton_Tapped(object sender, TappedRoutedEventArgs e) {
			e.Handled = true;
			if(SelectionToggleButton.IsChecked.Value) {
				previousIndex = FavoritesListView.SelectedIndex;
				IsSelecting = true;
			} else {
				IsSelecting = false;
			}
		}

		private void DeleteButton_Tapped(object sender, TappedRoutedEventArgs e) {

		}

		private void AddNewButton_Tapped(object sender, TappedRoutedEventArgs e) {

		}

		private void SplitViewDisplayModeSwitch_Toggled(object sender, RoutedEventArgs e) {
			if(SplitViewDisplayModeSwitch.IsOn) {
				MainSplitView.DisplayMode = SplitViewDisplayMode.CompactOverlay;
			} else {
				MainSplitView.DisplayMode = SplitViewDisplayMode.CompactInline;
			}
		}

		private void RefreshButton_Tapped(object sender, TappedRoutedEventArgs e) {

		}

		private void FavoritesListView_SelectionChanged(object sender, SelectionChangedEventArgs e) {
			if(IsSelecting) {

			} else {
				if(e.AddedItems.Count != 0) {
					FollowingButton.IsChecked = false;
				}
			}
		}

		private void FontIcon_Loaded(object sender, RoutedEventArgs e) {
			icons.Add(sender as FontIcon);

		}

		private void MainSplitView_PaneClosing(SplitView sender, SplitViewPaneClosingEventArgs args) {
			IsSelecting = false;
			SelectionToggleButton.IsChecked = false;
		}

		private void RefreshContentButton_Tapped(object sender, TappedRoutedEventArgs e) {

		}

		private void DeleteContentButton_Tapped(object sender, TappedRoutedEventArgs e) {

		}

		private void RenameButton_Tapped(object sender, TappedRoutedEventArgs e) {

		}
	}

	public class FavoriteListViewItem {
		public int Index { get; set; }
		public string Title { get; set; }
		public int Count { get; set; }
		public FavoriteListViewItem(int index, string title, int count) {
			Index = index;
			Title = title;
			Count = count;
		}
	}
}
