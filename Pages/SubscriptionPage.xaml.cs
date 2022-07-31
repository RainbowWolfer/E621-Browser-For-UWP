using E621Downloader.Models;
using E621Downloader.Models.Download;
using E621Downloader.Models.Inerfaces;
using E621Downloader.Models.Locals;
using E621Downloader.Models.Posts;
using E621Downloader.Views;
using E621Downloader.Views.SubscriptionSection;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;


namespace E621Downloader.Pages {
	public sealed partial class SubscriptionPage: Page, IPage {
		public static SubscriptionPage Instance { get; private set; }
		public static string DefaultDownloadGroupName => "Following".Language();
		public static string CurrentTag { get; private set; }

		private CancellationTokenSource cts;
		private readonly List<FontIcon> icons = new();
		private bool isSelecting = false;
		private int previousIndex = -1;

		private readonly ObservableCollection<FavoriteListViewItem> items = new();

		public LayoutType CurrentLayout { get; private set; }
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

		public bool ImagesSelecting {
			get => imagesSelecting;
			set {
				imagesSelecting = value;

				SelectionButton.IsChecked = imagesSelecting;
				SelectionCountText.Visibility = imagesSelecting ? Visibility.Visible : Visibility.Collapsed;
				SelectionCountText.Text = $"0/{PostsList.Count}";

				foreach(var item in MainGridView.Items.Cast<ImageHolderForSubscriptionPage>()) {
					item.IsSelected = false;
				}
			}
		}

		public List<FavoriteListViewItem> Selected => FavoritesListView.SelectedItems.Cast<FavoriteListViewItem>().ToList();

		public int Size { get; private set; } = 300;

		public List<object> PostsList { get; } = new List<object>();

		public SubscriptionsPostsTab FollowingTab { get; private set; } = null;
		//public List<SubscriptionsPostsTab> FavoriteTabs { get; } = new();

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
			UpdateFavoritesTable(false);
		}

		public void UpdateFavoritesTable(bool updateSelection = true) {
			items.Clear();
			int selectedIndex = FavoritesListView.SelectedIndex;
			for(int i = 0; i < FavoritesList.Table.Count; i++) {
				FavoritesList item = FavoritesList.Table[i];
				items.Add(new FavoriteListViewItem(i, item.Name, item.Items.Count));
				if(item.Name == CurrentListName) {
					selectedIndex = i;
				}
			}
			if(updateSelection || CurrentLayout == LayoutType.Favorites) {
				FavoritesListView.SelectedIndex = selectedIndex;
			}
			FavoritesTableHintText.Visibility = items.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
		}

		private async void LoadFollowing(int page, bool refresh = false) {
			if(CurrentLayout == LayoutType.Following && FollowingTab != null && !refresh && FollowingTab?.Page == page) {
				return;
			}
			UpdateLoadCountText(true);
			DownloadButton.IsEnabled = false;
			ImagesSelecting = false;
			MyPostsInfoListView.Clear();
			if(page is <= 0 or >= 100) {
				return;
			}
			UpdateTitleAndSetTag("Following".Language());
			UpdatePage(page);
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
				MyPostsInfoListView.SetEmpty();
			} else {
				List<Post> posts;
				if(FollowingTab != null && FollowingTab.Page == page && !refresh) {
					posts = FollowingTab.Posts;
				} else {
					posts = await Post.GetPostsByTagsAsync(cts.Token, true, page, Local.Listing.GetGetDefaultFollowList().Tags.ToArray());
					FollowingTab = new SubscriptionsPostsTab() {
						Page = page,
					};
				}
				if(posts == null) {
					return;
				}
				FollowingTab.Posts = posts;
				posts = FollowingTab.PostsAfterBlasklist;
				MyPostsInfoListView.UpdatePostsInfo(FollowingTab);
				if(posts != null && cts != null && CurrentLayout == LayoutType.Following) {
					PostsList.Clear();
					PostsList.AddRange(posts);
					foreach(Post post in posts) {
						ImageHolderForSubscriptionPage image = new(this) {
							Height = Size,
							Width = Size,
							OnLoaded = () => UpdateLoadCountText(),
						};
						image.LoadFromPost(post, Local.Listing.GetGetDefaultFollowList().Tags.ToArray());
						MainGridView.Items.Add(image);
					}
					LoadingRing.IsActive = false;
					FavoritesListHintText.Visibility = posts.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
				}
				RefreshContentButton.IsEnabled = true;
			}
			DownloadButton.IsEnabled = PostsList.Count > 0;
		}

		private void LoadFavorites(string listName) {
			UpdateLoadCountText(true);
			DownloadButton.IsEnabled = false;
			ImagesSelecting = false;
			MyPostsInfoListView.Clear();
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
			PostsList.Clear();
			MyPostsInfoListView.Clear();
			if(list == null || list.Items.Count == 0) {
				MyPostsInfoListView.SetEmpty();
			}
			if(list != null && cts != null && CurrentLayout == LayoutType.Favorites) {
				MainGridView.Items.Clear();
				foreach(FavoriteItem item in list.Items) {
					ImageHolderForSubscriptionPage image = new(this, listName) {
						Height = Size,
						Width = Size,
					};
					var mix = new MixPost(item.Type, item.Path);
					if(item.Type == PathType.Local) {
						image.LoadFromLocal(mix, cts.Token, UpdatePostsInfoInFavorites);
					} else if(item.Type == PathType.PostID) {
						image.LoadFromPostID(mix, cts.Token, UpdatePostsInfoInFavorites);
					} else {
						throw new PathTypeException();
					}
					PostsList.Add(mix);
					MainGridView.Items.Add(image);
				}
				UpdateLoadCountText(PostsList.Count);
				//MyPostsInfoListView.UpdateLayout(list.Items.Select(i=>i.));
			}
			LoadingRing.IsActive = false;
			FavoritesListHintText.Visibility = list.Items.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
			RefreshContentButton.IsEnabled = true;
			DownloadButton.IsEnabled = PostsList.Count > 0;
		}

		private void UpdatePostsInfoInFavorites(Post post) {
			IEnumerable<Post> posts = PostsList.Where(p => p is MixPost mix).Cast<MixPost>().Select(x => x.PostRef).Where(x => x != null);
			posts = posts.Append(post);
			MyPostsInfoListView.UpdatePostsInfo(posts);
		}

		public void Refresh() {
			switch(CurrentLayout) {
				case LayoutType.Following:
					LoadFollowing(1, true);
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
					FollowingButton.IsChecked = true;
					FavoritesListHintText.Visibility = Visibility.Collapsed;
					ManageButton.Visibility = Visibility.Visible;
					SortDropDown.Visibility = Visibility.Collapsed;
					RenameButton.Visibility = Visibility.Collapsed;
					DeleteContentButton.Visibility = Visibility.Collapsed;
					Paginator.Visibility = Visibility.Visible;
					break;
				case LayoutType.Favorites:
					FollowingButton.IsChecked = false;
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
			LoadFollowing(FollowingTab?.Page ?? 1);
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
				DefaultButton = ContentDialogButton.Close,
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
			ImagesSelecting = false;
			SelectionToggleButton.IsChecked = false;
			UpdateFavoritesTable(false);
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
			FollowingButton.Width = 40;
		}

		private void MainSplitView_PaneOpening(SplitView sender, object args) {
			if(FavoritesListView == null) {
				return;
			}
			for(int i = 0; i < FavoritesListView.Items.Count; i++) {
				ListViewItem item = (ListViewItem)FavoritesListView.ContainerFromIndex(i);
				item.Width = 290;
			}
			FollowingButton.Width = 285;
		}

		private void RefreshContentButton_Tapped(object sender, TappedRoutedEventArgs e) {
			Refresh();
		}

		private async void DeleteContentButton_Tapped(object sender, TappedRoutedEventArgs e) {
			if(await new ContentDialog() {
				Title = "Confirm".Language(),
				Content = "Are you sure to delete list".Language() + $" ({CurrentListName})",
				PrimaryButtonText = "Yes".Language(),
				CloseButtonText = "No".Language(),
				DefaultButton = ContentDialogButton.Close,
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
			if(FollowingTab != null) {
				LoadFollowing(FollowingTab.Page - 1);
			}
		}

		private void RightButton_Tapped(object sender, TappedRoutedEventArgs e) {
			if(FollowingTab != null) {
				LoadFollowing(FollowingTab.Page + 1);
			}
		}

		private void UpdatePage(int page) {
			PageText.Text = $"{page}";
		}

		private void ResizeBar_OnSizeChanged(int value, bool save) {
			Size = value;
			if(MainGridView == null) {
				return;
			}
			foreach(ImageHolderForSubscriptionPage item in MainGridView.Items.Cast<ImageHolderForSubscriptionPage>()) {
				item.Height = value;
				item.Width = value;
			}
			if(!save) {
				return;
			}
			LocalSettings.Current.subscriptionSizeView = value;
			DelayedSave();
		}

		private CancellationTokenSource saveCTS;
		private bool imagesSelecting;

		private async void DelayedSave() {
			try {
				if(saveCTS != null) {
					saveCTS.Cancel();
					saveCTS.Dispose();
				}
				saveCTS = new CancellationTokenSource();
				await Task.Delay(500, saveCTS.Token);
				await Local.WriteLocalSettings();
			} catch(OperationCanceledException) { }
		}

		private void UpdateLoadCountText(bool start = false) {
			if(start) {
				LoadCountText.Text = $"(0/{PostsList.Count})";
			} else {
				int loaded = MainGridView.Items.Cast<ImageHolderForSubscriptionPage>().Count(i => i.IsImageLoaded);
				int all = PostsList.Count;
				LoadCountText.Text = $"({loaded}/{all})";
			}
		}

		private void UpdateLoadCountText(int count) {
			LoadCountText.Text = $"({count})";
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

		public enum LayoutType {
			Following, Favorites
		}

		private void SelectionButton_Click(object sender, RoutedEventArgs e) {
			ImagesSelecting = !ImagesSelecting;
		}

		public void UpdateSelectedCountText() {
			int selected = MainGridView.Items.Cast<ImageHolderForSubscriptionPage>().Count(i => i.IsSelected);
			int all = MainGridView.Items.Count;
			SelectionCountText.Text = $"{selected}/{all}";
		}

		private CancellationTokenSource downloadCts;
		private async void DownloadButton_Click(object sender, RoutedEventArgs e) {
			if(PostsList.Count == 0) {
				return;
			}
			if(!await DownloadsManager.CheckDownloadAvailableWithDialog()) {
				return;
			}
			IEnumerable<ImageHolderForSubscriptionPage> all;
			if(ImagesSelecting) {
				all = MainGridView.Items.Cast<ImageHolderForSubscriptionPage>().Where(i => i.IsSelected);
			} else {
				all = MainGridView.Items.Cast<ImageHolderForSubscriptionPage>();
			}
			var local = all.Where(a => a.IsLocal);
			var selectedDownloadDialog = new SubscriptionDownloadDialog() {
				IsSelectedDownload = ImagesSelecting,
				Numbers = (all.Count(), local.Count()),
			};
			if(ImagesSelecting && all.Count() <= 0) {
				return;
			}
			if(await new ContentDialog() {
				Title = "Download Selection".Language(),
				Content = selectedDownloadDialog,
				PrimaryButtonText = "Yes".Language(),
				CloseButtonText = "No".Language(),
			}.ShowAsync() == ContentDialogResult.Primary) {
				CancelDownload();
				downloadCts = new CancellationTokenSource();
				CreateDownloadDialog("Please Wait".Language(), "Handling Downloads".Language());

				IEnumerable<Post> posts;
				if(ImagesSelecting) {
					posts = all.Where(a => !a.IsLocal && a.IsSelected).Select(a => a.PostRef);
				} else {
					posts = all.Where(a => !a.IsLocal).Select(a => a.PostRef);
				}

				string title;
				if(CurrentLayout == LayoutType.Favorites) {
					title = $"Favorite - {CurrentListName}";
				} else if(CurrentLayout == LayoutType.Following) {
					title = DefaultDownloadGroupName;
				} else {
					throw new Exception($"Current Layout unknown ({CurrentLayout})");
				}

				bool? result = await DownloadsManager.RegisterDownloads(downloadCts.Token, posts, title, selectedDownloadDialog.TodayDate, UpdateContentText);

				if(result == true) {
					ImagesSelecting = false;
					MainPage.CreateTip_SuccessDownload(this);
				} else if(result == null) {
					return;
				} else {
					await MainPage.CreatePopupDialog("Error".Language(), "Downloads Failed".Language());
				}
				HideDownloadDialog();
			}
		}

		private ContentDialog downloadDialog;
		private DownloadCancellableDialog dialogContent;

		private async void CreateDownloadDialog(string title, string text) {
			if(downloadDialog != null) {
				return;
			}
			downloadDialog = new ContentDialog() {
				Title = title,
			};
			dialogContent = new DownloadCancellableDialog() {
				Text = text,
				OnCancel = () => {
					CancelDownload();
					HideDownloadDialog();
				},
			};
			downloadDialog.Content = dialogContent;
			await downloadDialog.ShowAsync();
		}

		private void UpdateContentText(string text) {
			if(dialogContent == null) {
				return;
			}
			dialogContent.Text = text;
		}

		private void HideDownloadDialog() {
			if(downloadDialog == null) {
				return;
			}
			downloadDialog.Hide();
			downloadDialog = null;
			dialogContent = null;
		}

		private void CancelDownload() {
			if(downloadCts != null) {
				downloadCts.Cancel();
				downloadCts.Dispose();
			}
			downloadCts = null;
		}

		private void PostsInfoButton_Click(object sender, RoutedEventArgs e) {
			SideSplitView.IsPaneOpen = true;
		}

		private async void PageInputText_KeyDown(object sender, KeyRoutedEventArgs e) {
			if(e.Key == MainPage.SEARCH_KEY) {
				MainPage.Instance.DelayInputKeyListener();
			}
			if(e.Key == VirtualKey.Enter) {
				await Forward(PageInputText.Text);
				PageInputText.Text = "";
			} else if(e.Key == VirtualKey.Escape) {
				PageInputText.Text = "";
			}
		}

		private async Task Forward(string text) {
			const int MAX_PAGE = 75;
			if(int.TryParse(text, out int page)) {
				if(page < 1 || page > MAX_PAGE) {
					await MainPage.CreatePopupDialog("Error".Language(), "({{0}}) can only be in 1-{{1}}".Language(page, MAX_PAGE));
				} else {
					LoadFollowing(page);
				}
			} else {
				await MainPage.CreatePopupDialog("Error".Language(), "({{0}}) is not a valid number".Language(text));
			}
		}

		private async void ForwardButton_Click(object sender, RoutedEventArgs e) {
			await Forward(PageInputText.Text);
			PageInputText.Text = "";
		}

		private void BackFirstButton_Click(object sender, RoutedEventArgs e) {
			LoadFollowing(1);
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

	public class SubscriptionsPostsTab {
		private List<Post> posts;

		//public SubscriptionPage.LayoutType Layout { get; set; }
		public List<Post> Posts {
			get => posts;
			set {
				posts = value;
				AllTags.Clear();
				Unsupported.Clear();
				BlackTags.Clear();
				HotTags.Clear();
				foreach(Post item in posts) {
					if(PostsBrowserPage.IgnoreTypes.Contains(item.file.ext)) {
						Unsupported.Add(item);
					}
				}
				posts.RemoveAll(p => PostsBrowserPage.IgnoreTypes.Contains(p.file.ext));
				App.PostsPool.AddToPostsPool(posts);
				PostsAfterBlasklist = PostsBrowserPage.FilterBlacklist(posts, tag => {
					if(BlackTags.ContainsKey(tag)) {
						BlackTags[tag]++;
					} else {
						BlackTags.Add(tag, 1);
					}
				});

				BlackTags = BlackTags.OrderByDescending(t => t.Value).ToDictionary(x => x.Key, x => x.Value);
				foreach(Post item in posts) {
					foreach(string tag in item.tags.GetAllTags()) {
						if(AllTags.ContainsKey(tag)) {
							AllTags[tag]++;
						} else {
							AllTags.Add(tag, 1);
						}
					}
				}
				HotTags = AllTags.OrderByDescending(o => o.Value).ToDictionary(x => x.Key, x => x.Value);
			}
		}

		public List<Post> PostsAfterBlasklist { get; private set; } = new();
		public List<Post> Unsupported { get; private set; } = new();
		public Dictionary<string, long> BlackTags { get; private set; } = new();
		public Dictionary<string, long> AllTags { get; private set; } = new();
		public Dictionary<string, long> HotTags { get; private set; } = new();

		public int Page { get; set; }

		public SubscriptionsPostsTab() {

		}
	}
}
