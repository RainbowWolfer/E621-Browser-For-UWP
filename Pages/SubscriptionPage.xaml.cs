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

		private LayoutType CurrentLayout { get; set; }
		private string currentListName;

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

		public List<object> PostsList { get; private set; }

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
			FavoritesTableHintText.Visibility = items.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
		}

		private async void LoadFollowing(int page) {
			if(page <= 0 || page >= 100) {
				return;
			}
			UpdateTitle("Following");
			RefreshContentButton.IsEnabled = false;
			SwitchLayout(LayoutType.Following);
			FollowingButton.IsChecked = true;
			if(cts != null) {
				cts.Cancel();
				cts.Dispose();
			}
			cts = new CancellationTokenSource();
			LoadingRing.IsActive = true;
			MainGridView.Items.Clear();
			List<Post> posts = await Post.GetPostsByTagsAsync(cts.Token, true, page, Local.FollowList);
			if(posts == null) {
				return;
			}
			PostsList = new List<object>();
			PostsList.AddRange(posts);
			if(posts != null) {
				foreach(Post post in posts) {
					var image = new ImageHolderForSubscriptionPage(this) {
						Height = 300,
						Width = 300,
					};
					image.LoadFromPost(post);
					MainGridView.Items.Add(image);
				}
				LoadingRing.IsActive = false;
				FavoritesListHintText.Visibility = posts.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
			}
			RefreshContentButton.IsEnabled = true;
		}

		private void LoadFavorites(string listName) {
			if(string.IsNullOrWhiteSpace(listName)) {
				return;
			}
			UpdateTitle(listName);
			RefreshContentButton.IsEnabled = false;
			currentListName = listName;
			SwitchLayout(LayoutType.Favorites);
			FollowingButton.IsChecked = false;
			if(cts != null) {
				cts.Cancel();
				cts.Dispose();
			}
			cts = new CancellationTokenSource();
			LoadingRing.IsActive = true;
			FavoritesList list = FavoritesList.Table.Find(l => l.Name == listName);
			PostsList = new List<object>();
			if(list != null) {
				MainGridView.Items.Clear();
				foreach(FavoriteItem item in list.Items) {
					var image = new ImageHolderForSubscriptionPage(this) {
						Height = 300,
						Width = 300,
					};
					var mix = new MixPost(item.Type, item.Path);
					if(item.Type == PathType.Local) {
						image.LoadFromLocal(mix, cts.Token);
					} else if(item.Type == PathType.PostID) {
						image.LoadFromPostID(mix, cts.Token);
					} else {
						throw new PathTypeException();
					}
					PostsList.Add(mix);
					MainGridView.Items.Add(image);
				}
			} else {
				throw new Exception("HOW_2?");
			}
			LoadingRing.IsActive = false;
			FavoritesListHintText.Visibility = list.Items.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
			RefreshContentButton.IsEnabled = true;
		}

		private void ClearGridView() {
			MainGridView.Items.Clear();
			LoadingRing.IsActive = false;
			FavoritesListHintText.Visibility = Visibility.Visible;
		}

		private void SwitchLayout(LayoutType type) {
			CurrentLayout = type;
			switch(type) {
				case LayoutType.Following:
					FavoritesListHintText.Visibility = Visibility.Collapsed;
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

		private void UpdateTitle(string title) {
			TitleText.Text = title;
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
				FavoritesList.Table.RemoveAll(l => Selected.Select(s => s.Title).Contains(l.Name));
				FavoritesList.Save();
				LoadFavoritesTable();
			}
		}

		private async void AddNewButton_Tapped(object sender, TappedRoutedEventArgs e) {
			var dialog = new ContentDialog() {
				Title = "Add New Favorites Lists",
			};
			var content = new FavoritesListNameModify(true, dialog, items.Select(i => i.Title));
			dialog.Content = content;
			await dialog.ShowAsync();
			if(content.Confirm) {
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
			switch(CurrentLayout) {
				case LayoutType.Following:
					LoadFollowing(1);
					break;
				case LayoutType.Favorites:
					LoadFavorites(currentListName);
					break;
				default:
					throw new Exception();
			}
		}

		private async void DeleteContentButton_Tapped(object sender, TappedRoutedEventArgs e) {
			if(await new ContentDialog() {
				Title = "Confirmation",
				Content = $"Are you sure to delete list ({currentListName})",
				PrimaryButtonText = "Yes",
				CloseButtonText = "No",
			}.ShowAsync() == ContentDialogResult.Primary) {
				FavoritesList.Table.RemoveAll(l => currentListName == l.Name);
				FavoritesList.Save();
				LoadFavoritesTable();
				if(FavoritesList.Table.Count > 1) {
					LoadFavorites(FavoritesList.Table.First().Name);
				} else {
					ClearGridView();
				}
			}
		}

		private async void RenameButton_Tapped(object sender, TappedRoutedEventArgs e) {
			var dialog = new ContentDialog() {
				Title = $"Rename - {currentListName}",
			};
			var content = new FavoritesListNameModify(false, dialog, items.Select(i => i.Title), currentListName);
			dialog.Content = content;
			await dialog.ShowAsync();
			if(content.Confirm) {
				FavoritesList found = FavoritesList.Table.Find(l => l.Name == currentListName);
				if(found != null) {
					found.Name = content.Input;
					FavoritesList.Save();
					LoadFavoritesTable();
					UpdateTitle(content.Input);
				}
			}
		}

		private async void ManageButton_Tapped(object sender, TappedRoutedEventArgs e) {
			await SettingsPage.FollowListManage(this);
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
