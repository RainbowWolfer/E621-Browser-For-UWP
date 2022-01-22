using E621Downloader.Models.Locals;
using E621Downloader.Models.Posts;
using E621Downloader.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;


namespace E621Downloader.Pages {
	public sealed partial class SubscriptionPage: Page {
		private CancellationTokenSource cts;
		private readonly List<FontIcon> icons = new List<FontIcon>();
		private bool isSelecting = false;
		private int previousIndex = -1;

		private readonly ObservableCollection<FavoriteListViewItem> items = new ObservableCollection<FavoriteListViewItem>();

		private int currentFollowingPage = 1;

		public bool IsSelecting {
			get => isSelecting;
			set {
				isSelecting = value;
				if(value) {
					FavoritesListView.SelectionMode = ListViewSelectionMode.Multiple;
					FavoritesListView.SelectedIndex = -1;
					icons.ForEach(i => i.Width = 0);
					DeleteButton.Visibility = Visibility.Visible;
					DeleteButton.IsEnabled = false;
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

		public List<FavoriteListViewItem> Selected => FavoritesListView.SelectedItems.Cast<FavoriteListViewItem>().ToList();

		public int CurrentFollowingPage {
			get => currentFollowingPage;
			set {
				currentFollowingPage = Math.Clamp(value, 1, 100);
			}
		}

		public SubscriptionPage() {
			this.InitializeComponent();
			this.NavigationCacheMode = NavigationCacheMode.Enabled;
			LoadFavoritesTable();
			LoadFollowing(1);
		}

		protected override void OnNavigatedTo(NavigationEventArgs e) {
			base.OnNavigatedTo(e);
		}

		private void LoadFavoritesTable() {
			items.Clear();
			for(int i = 0; i < FavoritesList.Table.Count; i++) {
				FavoritesList item = FavoritesList.Table[i];
				items.Add(new FavoriteListViewItem(i, item.Name, item.Items.Count));
			}
		}

		private async void LoadFollowing(int page) {
			SwitchLayout(LayoutType.Following);
			FollowingButton.IsChecked = true;
			if(cts != null) {
				cts.Cancel();
				cts.Dispose();
			}
			cts = new CancellationTokenSource();
			LoadingRing.IsActive = true;
			List<Post> posts = await Post.GetPostsByTagsAsync(cts.Token, true, page, Local.FollowList);
			if(posts != null) {
				MainGridView.Items.Clear();
				foreach(Post post in posts) {
					var image = new ImageHolderForSubscriptionPage() {
						Height = 300,
						Width = 300,
					};
					image.LoadFromPost(post);
					MainGridView.Items.Add(image);
				}
				LoadingRing.IsActive = false;
			}
		}

		private async void LoadFavorites(string listName) {
			SwitchLayout(LayoutType.Favorites);
			FollowingButton.IsChecked = false;
			if(cts != null) {
				cts.Cancel();
				cts.Dispose();
			}
			cts = new CancellationTokenSource();
			LoadingRing.IsActive = true;
			FavoritesList list = FavoritesList.Table.Find(l => l.Name == listName);
			if(list != null) {
				foreach(FavoriteItem item in list.Items) {
					var image = new ImageHolderForSubscriptionPage();
					if(item.Type == PathType.Local) {
						image.LoadFromLocal(item.Path, cts.Token);
					} else if(item.Type == PathType.PostID) {
						image.LoadFromPostID(item.Path, cts.Token);
					} else {
						throw new PathTypeException();
					}
					MainGridView.Items.Add(image);
				}
			} else {
				throw new Exception("HOW_2?");
			}
		}

		private void SwitchLayout(LayoutType type) {
			switch(type) {
				case LayoutType.Following:
					ManageButton.Visibility = Visibility.Visible;
					SortDropDown.Visibility = Visibility.Collapsed;
					RenameButton.Visibility = Visibility.Collapsed;
					DeleteContentButton.Visibility = Visibility.Collapsed;
					Paginator.Visibility = Visibility.Visible;
					break;
				case LayoutType.Favorites:
					ManageButton.Visibility = Visibility.Collapsed;
					SortDropDown.Visibility = Visibility.Visible;
					RenameButton.Visibility = Visibility.Visible;
					DeleteContentButton.Visibility = Visibility.Visible;
					Paginator.Visibility = Visibility.Collapsed;
					break;
				default:
					throw new Exception();
			}
		}

		private void HamburgerButton_Tapped(object sender, TappedRoutedEventArgs e) {
			MainSplitView.IsPaneOpen = !MainSplitView.IsPaneOpen;
		}

		private void FollowingButton_Tapped(object sender, TappedRoutedEventArgs e) {
			e.Handled = true;
			FavoritesListView.SelectedIndex = -1;
			FollowingButton.IsChecked = true;
			LoadFollowing(CurrentFollowingPage);
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

		private async void DeleteButton_Tapped(object sender, TappedRoutedEventArgs e) {
			if(await new ContentDialog() {
				Title = "Confirmation",
				Content = $"Are you sure to delete {Selected.Count} favorites list(s)",
				PrimaryButtonText = "Yes",
				CloseButtonText = "No",
			}.ShowAsync() == ContentDialogResult.Primary) {

			}
		}

		private async void AddNewButton_Tapped(object sender, TappedRoutedEventArgs e) {
			var dialog = new ContentDialog() {
				Title = "Add New Favorites Lists",
			};
			var content = new AddNewFavoritesList(dialog, items.Select(i => i.Title));
			dialog.Content = content;
			await dialog.ShowAsync();
			if(content.IsAdd) {
				FavoritesList.Table.Insert(0, new FavoritesList(content.Input));
				FavoritesList.Save();
				LoadFavoritesTable();
			}
		}

		private void SplitViewDisplayModeSwitch_Toggled(object sender, RoutedEventArgs e) {
			if(SplitViewDisplayModeSwitch.IsOn) {
				MainSplitView.DisplayMode = SplitViewDisplayMode.CompactOverlay;
			} else {
				MainSplitView.DisplayMode = SplitViewDisplayMode.CompactInline;
			}
		}

		private void RefreshButton_Tapped(object sender, TappedRoutedEventArgs e) {
			IsSelecting = false;
			SelectionToggleButton.IsChecked = false;
			LoadFavoritesTable();
		}

		private void FavoritesListView_SelectionChanged(object sender, SelectionChangedEventArgs e) {
			if(IsSelecting) {
				DeleteButton.IsEnabled = Selected.Count > 0;
			} else {
				if(e.AddedItems.Count != 0) {
					FollowingButton.IsChecked = false;
				}
				if(e.AddedItems.FirstOrDefault() is FavoriteListViewItem item) {
					LoadFavorites(item.Title);
				} else {
					throw new Exception("HOW?");
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

		private void MainGridView_ItemClick(object sender, ItemClickEventArgs e) {
			var item = e.ClickedItem as ImageHolderForSubscriptionPage;
			//item.PostRef;
		}

		private void ForwardButton_Tapped(object sender, TappedRoutedEventArgs e) {
			if(int.TryParse(PageBox.Text, out int page)) {
				LoadFollowing(page);
			}
		}

		private void LeftButton_Tapped(object sender, TappedRoutedEventArgs e) {
			LoadFollowing(--CurrentFollowingPage);
		}

		private void RightButton_Tapped(object sender, TappedRoutedEventArgs e) {
			LoadFollowing(++CurrentFollowingPage);
		}

		private enum LayoutType {
			Following, Favorites
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
