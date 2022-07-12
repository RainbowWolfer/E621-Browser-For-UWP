using E621Downloader.Models;
using E621Downloader.Models.Inerfaces;
using E621Downloader.Models.Locals;
using E621Downloader.Models.Posts;
using E621Downloader.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;


namespace E621Downloader.Pages {
	public sealed partial class SubscriptionPage: Page, IPage {
		public static SubscriptionPage Instance { get; private set; }
		public static string CurrentTag { get; private set; }

		private CancellationTokenSource cts;
		private readonly List<FontIcon> icons = new();
		private bool isSelecting = false;
		private int previousIndex = -1;

		private readonly ObservableCollection<FavoriteListViewItem> items = new();

		private int currentFollowingPage = 1;

		private LayoutType CurrentLayout { get; set; }
		public string CurrentListName { get; private set; }

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
					icons.ForEach(i => i.Width = 12);
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

		public int Size { get; private set; } = 300;

		public List<object> PostsList { get; private set; }

		public SubscriptionPage() {
			Instance = this;
			this.InitializeComponent();
			this.NavigationCacheMode = NavigationCacheMode.Enabled;
			LoadFollowing(1);
			MyResizeBar.SetSize(LocalSettings.Current.subscriptionSizeView);
		}

		protected override void OnNavigatedTo(NavigationEventArgs e) {
			base.OnNavigatedTo(e);
			this.FocusModeUpdate();
			UpdateFavoritesTable();
		}

		public void UpdateFavoritesTable() {
			items.Clear();
			int selectedIndex = -1;
			for(int i = 0; i < FavoritesList.Table.Count; i++) {
				FavoritesList item = FavoritesList.Table[i];
				items.Add(new FavoriteListViewItem(i, item.Name, item.Items.Count));
				if(item.Name == CurrentListName) {
					selectedIndex = i;
				}
			}
			FavoritesListView.SelectedIndex = selectedIndex;
			FavoritesTableHintText.Visibility = items.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
		}

		private async void LoadFollowing(int page) {
			if(page is <= 0 or >= 100) {
				return;
			}
			UpdateTitleAndSetTag("Following".Language());
			UpdatePage();
			RefreshContentButton.IsEnabled = false;
			SwitchLayout(LayoutType.Following);
			FollowingButton.IsChecked = true;
			if(cts != null) {
				cts.Cancel();
				cts.Dispose();
				cts = null;
			}
			cts = new CancellationTokenSource();
			LoadingRing.IsActive = true;
			MainGridView.Items.Clear();
			if(Local.Listing.GetGetDefaultFollowList().Tags.Count() == 0) {
				LoadingRing.IsActive = false;
				RefreshContentButton.IsEnabled = true;
				FavoritesListHintText.Visibility = Visibility.Visible;
			} else {
				List<Post> posts = await Post.GetPostsByTagsAsync(cts.Token, true, page, Local.Listing.GetGetDefaultFollowList().Tags.ToArray());
				if(posts == null) {
					return;
				}
				PostsList = new List<object>();
				PostsList.AddRange(posts);
				if(posts != null && cts != null) {
					foreach(Post post in posts) {
						var image = new ImageHolderForSubscriptionPage(this) {
							Height = Size,
							Width = Size,
						};
						image.LoadFromPost(post, Local.Listing.GetGetDefaultFollowList().Tags.ToArray());
						MainGridView.Items.Add(image);
					}
					LoadingRing.IsActive = false;
					FavoritesListHintText.Visibility = posts.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
				}
				RefreshContentButton.IsEnabled = true;
			}
		}

		private void LoadFavorites(string listName) {
			if(string.IsNullOrWhiteSpace(listName)) {
				return;
			}
			UpdateTitleAndSetTag(listName);
			RefreshContentButton.IsEnabled = false;
			CurrentListName = listName;
			SwitchLayout(LayoutType.Favorites);
			FollowingButton.IsChecked = false;
			if(cts != null) {
				cts.Cancel();
				cts.Dispose();
				cts = null;
			}
			cts = new CancellationTokenSource();
			LoadingRing.IsActive = true;
			FavoritesList list = FavoritesList.Table.Find(l => l.Name == listName);
			PostsList = new List<object>();
			if(list != null && cts != null) {
				MainGridView.Items.Clear();
				foreach(FavoriteItem item in list.Items) {
					var image = new ImageHolderForSubscriptionPage(this, listName) {
						Height = Size,
						Width = Size,
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

		public void Refresh() {
			switch(CurrentLayout) {
				case LayoutType.Following:
					LoadFollowing(1);
					break;
				case LayoutType.Favorites:
					LoadFavorites(CurrentListName);
					break;
				default:
					throw new Exception();
			}
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
					SortDropDown.Visibility = Visibility.Collapsed;//work on this later
					RenameButton.Visibility = Visibility.Visible;
					DeleteContentButton.Visibility = Visibility.Visible;
					Paginator.Visibility = Visibility.Collapsed;
					break;
				default:
					throw new Exception();
			}
		}

		private void UpdateTitleAndSetTag(string title) {
			TitleText.Text = title;
			CurrentTag = title;
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

		private void SelectionToggleButton_Click(object sender, RoutedEventArgs e) {
			//e.Handled = true;
			if(SelectionToggleButton.IsChecked.Value) {
				previousIndex = FavoritesListView.SelectedIndex;
				IsSelecting = true;
			} else {
				IsSelecting = false;
			}
		}

		private async void DeleteButton_Tapped(object sender, TappedRoutedEventArgs e) {
			if(await new ContentDialog() {
				Title = "Confirm".Language(),
				Content = "Are you sure to delete".Language() + $" {Selected.Count} " + "favorites list(s)".Language() + "?",
				PrimaryButtonText = "Yes".Language(),
				CloseButtonText = "No".Language(),
			}.ShowAsync() == ContentDialogResult.Primary) {
				FavoritesList.Table.RemoveAll(l => Selected.Select(s => s.Title).Contains(l.Name));
				FavoritesList.Save();
				UpdateFavoritesTable();
			}
		}

		private async void AddNewButton_Tapped(object sender, TappedRoutedEventArgs e) {
			var dialog = new ContentDialog() {
				Title = "Add New Favorites List".Language(),
			};
			var content = new FavoritesListNameModify(true, dialog, items.Select(i => i.Title));
			dialog.Content = content;
			await dialog.ShowAsync();
			if(content.Confirm) {
				FavoritesList.Table.Insert(0, new FavoritesList(content.Input));
				FavoritesList.Save();
				UpdateFavoritesTable();
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
			UpdateFavoritesTable();
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
			if(FavoritesListView == null) {
				return;
			}
			for(int i = 0; i < FavoritesListView.Items.Count; i++) {
				ListViewItem item = (ListViewItem)FavoritesListView.ContainerFromIndex(i);
				item.HorizontalAlignment = HorizontalAlignment.Left;
				item.Width = 45;
				item.MinWidth = 0;
			}
		}

		private void MainSplitView_PaneOpening(SplitView sender, object args) {
			if(FavoritesListView == null) {
				return;
			}
			for(int i = 0; i < FavoritesListView.Items.Count; i++) {
				ListViewItem item = (ListViewItem)FavoritesListView.ContainerFromIndex(i);
				item.Width = 290;
			}
		}

		private void RefreshContentButton_Tapped(object sender, TappedRoutedEventArgs e) {
			Refresh();
		}

		private async void DeleteContentButton_Tapped(object sender, TappedRoutedEventArgs e) {
			if(await new ContentDialog() {
				Title = "Confirmation".Language(),
				Content = "Are you sure to delete list".Language() + $" ({CurrentListName})",
				PrimaryButtonText = "Yes".Language(),
				CloseButtonText = "No".Language(),
			}.ShowAsync() == ContentDialogResult.Primary) {
				FavoritesList.Table.RemoveAll(l => CurrentListName == l.Name);
				FavoritesList.Save();
				UpdateFavoritesTable();
				if(FavoritesList.Table.Count > 1) {
					LoadFavorites(FavoritesList.Table.First().Name);
				} else {
					ClearGridView();
				}
			}
		}

		private async void RenameButton_Tapped(object sender, TappedRoutedEventArgs e) {
			var dialog = new ContentDialog() {
				Title = "Rename".Language() + $" - {CurrentListName}",
			};
			var content = new FavoritesListNameModify(false, dialog, items.Select(i => i.Title), CurrentListName);
			dialog.Content = content;
			await dialog.ShowAsync();
			if(content.Confirm) {
				FavoritesList found = FavoritesList.Table.Find(l => l.Name == CurrentListName);
				if(found != null) {
					found.Name = content.Input;
					FavoritesList.Save();
					UpdateFavoritesTable();
					UpdateTitleAndSetTag(content.Input);
				}
			}
		}

		private async void ManageButton_Tapped(object sender, TappedRoutedEventArgs e) {
			await SettingsPage.PopupListingManager("Follow List".Language(), Local.Listing.LocalFollowingLists);

		}

		private void LeftButton_Tapped(object sender, TappedRoutedEventArgs e) {
			LoadFollowing(--CurrentFollowingPage);
		}

		private void RightButton_Tapped(object sender, TappedRoutedEventArgs e) {
			LoadFollowing(++CurrentFollowingPage);
		}

		private void UpdatePage() {
			PageText.Text = $"{CurrentFollowingPage}";
		}

		private void ResizeBar_OnSizeChanged(int value, bool save) {
			Size = value;
			LocalSettings.Current.subscriptionSizeView = value;
			LocalSettings.Save();
			if(MainGridView == null) {
				return;
			}
			foreach(ImageHolderForSubscriptionPage item in MainGridView.Items.Cast<ImageHolderForSubscriptionPage>()) {
				item.Height = value;
				item.Width = value;
			}
		}

		void IPage.UpdateNavigationItem() {
			MainPage.Instance.currentTag = PageTag.Subscription;
			MainPage.Instance.UpdateNavigationItem();
		}

		void IPage.FocusMode(bool enabled) {
			if(enabled) {
				MainSplitView.IsPaneOpen = false;
			}
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
