using E621Downloader.Models;
using E621Downloader.Models.Download;
using E621Downloader.Models.Inerfaces;
using E621Downloader.Models.Locals;
using E621Downloader.Models.Posts;
using E621Downloader.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;
using NavigationView = Microsoft.UI.Xaml.Controls.NavigationView;
using NavigationViewItem = Microsoft.UI.Xaml.Controls.NavigationViewItem;
using NavigationViewItemInvokedEventArgs = Microsoft.UI.Xaml.Controls.NavigationViewItemInvokedEventArgs;
using NavigationViewPaneClosingEventArgs = Microsoft.UI.Xaml.Controls.NavigationViewPaneClosingEventArgs;

namespace E621Downloader.Pages {
	public sealed partial class PostsBrowserPage: Page, IPage {
		public static bool IsInMultipleSelectionMode() {
			return Instance?.MultipleSelectionMode ?? false;
		}

		public static string[] GetCurrentTags() {
			return Instance?.GetSelectedTab()?.Tags ?? Array.Empty<string>();
		}

		public static bool HasLoaded() {
			if(Instance == null) {
				return false;
			}
			PostsTab tab = Instance.GetSelectedTab();
			if(tab == null) {
				return false;
			}
			return tab.Posts != null && tab.Posts.Count > 0;
		}

		public static List<Post> GetCurrentPosts() {
			return Instance?.GetSelectedTab()?.PostsAfterBlasklist ?? new List<Post>();
		}

		public static void SetSelectionMode(bool enable) {
			if(enable) {
				Instance?.EnterSelectionMode();
			} else {
				Instance?.LeaveSelectionMode();
			}
		}

		public static void SetSelectionFeedback() {
			Instance?.SelectFeedBack();
		}

		public static PostsBrowserPage Instance { get; private set; }
		public int ItemSize { get => 50; }

		public bool MultipleSelectionMode { get; private set; }
		private bool IsAdaptive { get; set; }

		private readonly string[] ignoreTypes = { "swf", };

		private PostBrowserParameter parameter;

		//private E621Pool pool;

		private CancellationTokenSource cts_loading;
		private CancellationTokenSource cts_download;
		private bool isLoading;

		public PostsTab CurrentTab { get; private set; } = null;

		public bool IsLoading {
			get => isLoading;
			private set {
				isLoading = value;
				LoadingPanel.Visibility = isLoading ? Visibility.Visible : Visibility.Collapsed;
				MyWrapGrid.Visibility = isLoading ? Visibility.Collapsed : Visibility.Visible;

				bool overralEnable = GetSelectedTab() != null && !IsLoading;
				InfoButton.IsEnabled = overralEnable;
				MySplitViewButton.IsEnabled = overralEnable;
				ViewButton.IsEnabled = overralEnable;
				RefreshButton.IsEnabled = overralEnable;
				SelectToggleButton.IsEnabled = overralEnable;
				if(DownloadButton.Tag is bool b && b) {
					DownloadButton.IsEnabled = b && overralEnable;
				} else {
					DownloadButton.IsEnabled = overralEnable;
				}
				//uncheck selection
			}
		}

		public PostsBrowserPage() {
			Instance = this;
			this.InitializeComponent();
			this.NavigationCacheMode = NavigationCacheMode.Enabled;
			ClearTabs();
			ResizePanelWidthButton.Visibility = TabsNavigationView.IsPaneOpen ? Visibility.Visible : Visibility.Collapsed;

			IsAdaptive = LocalSettings.Current.adaptiveGrid;
		}

		protected override void OnNavigatedTo(NavigationEventArgs e) {
			base.OnNavigatedTo(e);
			this.FocusModeUpdate();
			if(e.Parameter is PostBrowserParameter parameter) {
				this.parameter = (PostBrowserParameter)parameter.Clone();
				ToTab(new PostsTab() {
					Tags = parameter.Tags,
					CurrentPage = parameter.Page,
				});
				//this.pool = null;
			} else if(e.Parameter is E621Pool pool) {
				this.parameter = new PostBrowserParameter(1, pool.Tag);
				ToTab(new PostsTab() {
					Pool = pool,
				});
				//this.pool = pool;
				//go or find new tab
			} else if(this.parameter == null) {
				this.parameter = new PostBrowserParameter(1, "");
				ToTab(new PostsTab());
				//this.pool = null;
			}
			MainPage.ClearPostBrowserParameter();

			LocalSettings.Current.tabsOpenLength = LocalSettings.Current.tabsOpenLength;
			TabsNavigationView.OpenPaneLength = LocalSettings.Current.tabsOpenLength;
			PanelWidthText.Text = $"({LocalSettings.Current.tabsOpenLength})";
		}

		public async void ToTab(PostsTab tab) {
			CurrentTab = tab;
			if(tab == null) {
				IsLoading = false;
				CancelLoading();
				await Task.Delay(20);//idk but this works
				ResetLoading();
				Paginator.Reset();
				TabsNavigationView.SelectedItem = null;
				NoTabHint.Visibility = Visibility.Visible;
				NoTabHintStoryboard.Begin();
				return;
			}
			NoTabHint.Visibility = Visibility.Collapsed;
			NavigationViewItem found = null;
			foreach(NavigationViewItem item in GetAllNavitationViewItems()) {
				if(item.Tag is PostsTab itemTag && tab.JoinedTags == itemTag.JoinedTags) {
					found = item;
					break;
				}
			}
			if(found == null) {
				found = CreateMenuItem(tab);
				TabsNavigationView.MenuItems.Add(found);
				Local.History.AddTag(tab.Tags);
			}
			NavigateToItem(found);
		}

		public void ClearTabs() {
			TabsNavigationView.MenuItems.Clear();
		}

		public void DeleteTab(PostsTab tab) {
			foreach(NavigationViewItem item in GetAllNavitationViewItems()) {
				if(item.Tag is PostsTab itemTag && tab == itemTag) {
					TabsNavigationView.MenuItems.Remove(item);
					return;
				}
			}
		}

		private NavigationViewItem CreateMenuItem(PostsTab tab) {
			NavigationViewItem item = new() {
				Icon = new FontIcon() {
					Glyph = "\uE132",
				},
				Content = string.IsNullOrWhiteSpace(tab.JoinedTags) ? "( Default Tab )".Language() : tab.JoinedTags,
				Tag = tab,
			};
			ToolTipService.SetToolTip(item, item.Content);
			MenuFlyout flyout = new();
			MenuFlyoutItem delete_item = new() {
				Icon = new FontIcon() {
					Glyph = "\uE10A",
				},
				Text = "Close".Language(),
			};
			MenuFlyoutItem copy_item = new() {
				Icon = new FontIcon() {
					Glyph = "\uE8C8",
				},
				Text = "Copy".Language(),
				IsEnabled = !string.IsNullOrWhiteSpace(tab.JoinedTags),
			};
			copy_item.Click += (s, e) => {
				var dataPackage = new DataPackage() {
					RequestedOperation = DataPackageOperation.Copy
				};
				dataPackage.SetText($"{tab.JoinedTags}");
				Clipboard.SetContent(dataPackage);
			};
			delete_item.Click += (s, e) => {
				CloseTab(item);
			};
			flyout.Items.Add(copy_item);
			flyout.Items.Add(delete_item);
			item.ContextFlyout = flyout;
			return item;
		}

		private async void NavigateToItem(NavigationViewItem item) {
			if(item.Tag is not PostsTab tab) {
				return;
			}
			TabsNavigationView.SelectedItem = item;
			await LoadAsync(tab);
		}

		private List<NavigationViewItem> GetAllNavitationViewItems() {
			return TabsNavigationView.MenuItems.Where(i => i is NavigationViewItem).Cast<NavigationViewItem>().ToList();
		}

		private PostsTab GetSelectedTab() {
			foreach(NavigationViewItem item in GetAllNavitationViewItems()) {
				if(item.IsSelected && item.Tag is PostsTab tab) {
					return tab;
				}
			}
			return null;
		}

		private NavigationViewItem GetSelectedTabItem() {
			foreach(NavigationViewItem item in GetAllNavitationViewItems()) {
				if(item.IsSelected) {
					return item;
				}
			}
			return null;
		}

		private void CloseTab(NavigationViewItem item) {
			if(item == null) {
				return;
			}

			var current = (NavigationViewItem)TabsNavigationView.SelectedItem;
			int index = GetAllNavitationViewItems().IndexOf(current);
			TabsNavigationView.MenuItems.Remove(item);

			List<NavigationViewItem> items = GetAllNavitationViewItems();
			if(items.Count == 0) {
				ToTab(null);
				TabsNavigationView.SelectedItem = null;
			} else if(current == item) {
				var target = items[index - 1];
				ToTab((PostsTab)target.Tag);
				TabsNavigationView.SelectedItem = target;
			}

		}

		private void CancelLoading() {
			if(cts_loading != null) {
				cts_loading.Cancel();
				cts_loading.Dispose();
			}
			cts_loading = null;
		}

		private void ResetLoading() {
			NoTabHint.Visibility = Visibility.Collapsed;
			AllBlackListHint.Visibility = Visibility.Collapsed;
			NoDataHint.Visibility = Visibility.Collapsed;
			MyWrapGrid.Children.Clear();

			SelectToggleButton.IsChecked = false;
			Paginator.Reset();
			LoadingText.Text = "";
		}

		private async Task LoadAsync(PostsTab tab, bool refresh = false) {
			IsLoading = true;
			ResetLoading();

			LoadingText.Text = "Getting Ready".Language();

			CancelLoading();
			await Task.Delay(100);//must be greater than 10 (Not Tested)
			cts_loading = new CancellationTokenSource();

			if(tab.Posts != null && !refresh) {
				IsLoading = false;
				if(tab.Pool == null) {
					Paginator.LoadPaginator(tab.CurrentPage, tab.MaxPage);
					AssignPaginatorAction(tab);
				}
				await UpdateImageHolders(tab);
				return;
			}

			string[] tags = tab.Pool == null ? tab.Tags : new string[1] { tab.Pool.Tag };
			tags = tags.Where(t => !string.IsNullOrWhiteSpace(t)).ToArray();
			UpdateInfoButton(tags);
			MainPage.ChangeCurrentTags(tab.Tags);

			if(tags == null || tags.Length == 0) {
				LoadingText.Text = "Loading Posts".Language();
			} else {
				LoadingText.Text = "Loading Tags".Language() + $": ({string.Join(' ', tags)})";
			}

			List<Post> posts = await Post.GetPostsByTagsAsync(cts_loading.Token, tab.CurrentPage, tags);

			if(posts == null) {//canceled
				IsLoading = true;
				return;
			}

			tab.Unsupported = new List<Post>();
			foreach(Post item in posts) {
				if(ignoreTypes.Contains(item.file.ext)) {
					tab.Unsupported.Add(item);
				}
			}
			posts.RemoveAll(p => ignoreTypes.Contains(p.file.ext));

			tab.Posts = posts;
			App.PostsPool.AddToPostsPool(posts);

			if(cts_loading == null || cts_loading.IsCancellationRequested) {
				IsLoading = true;
				return;
			}

			if(tab.Pool == null) {
				LoadingText.Text = "Loading Paginator".Language();
				await Paginator.LoadPaginator(tags, cts_loading.Token);
				AssignPaginatorAction(tab);
				tab.CurrentPage = Paginator.CurrentPage;
				tab.MaxPage = Paginator.MaxPage;
			}
			IsLoading = false;
			await UpdateImageHolders(tab);
		}

		private void AssignPaginatorAction(PostsTab tab) {
			Paginator.OnPageNavigate = async page => {
				Paginator.CurrentPage = page;
				tab.CurrentPage = page;
				await LoadAsync(tab, true);
			};
			Paginator.OnRefresh = async () => {
				await Paginator.LoadPaginator(tab.Tags, cts_loading.Token);
			};
		}

		private async Task Reload() {
			var selected = GetSelectedTab();
			if(selected == null) {
				return;
			}
			await LoadAsync(selected, true);
		}

		private List<Post> FilterBlacklist(List<Post> posts, Action<string> onBlacklistTagFound = null) {
			List<Post> afterBlacklisted = new();
			foreach(Post post in posts) {
				bool foundInBlackList = false;
				foreach(string tag in post.tags.GetAllTags()) {
					if(Local.Listing.CheckBlackList(tag)) {
						foundInBlackList = true;
						onBlacklistTagFound?.Invoke(tag);
						//no break to calculate all tags
					}
				}
				if(!foundInBlackList) {
					afterBlacklisted.Add(post);
				}
			}
			return afterBlacklisted;
		}

		private async Task UpdateImageHolders(PostsTab tab) {
			tab.LoadedCount = 0;
			Paginator.SetLoadCountText(0, tab.Posts.Count);

			tab.BlackTags.Clear();
			List<Post> afterBlackListed = FilterBlacklist(tab.Posts, tag => {
				if(tab.BlackTags.ContainsKey(tag)) {
					tab.BlackTags[tag]++;
				} else {
					tab.BlackTags.Add(tag, 1);
				}
			});
			tab.BlackTags = tab.BlackTags.OrderByDescending(t => t.Value).ToDictionary(x => x.Key, x => x.Value);
			tab.PostsAfterBlasklist = afterBlackListed;
			tab.AllTags.Clear();
			foreach(Post item in tab.Posts) {
				foreach(string tag in item.tags.GetAllTags()) {
					if(tab.AllTags.ContainsKey(tag)) {
						tab.AllTags[tag]++;
					} else {
						tab.AllTags.Add(tag, 1);
					}
				}
			}

			tab.HotTags = tab.AllTags.OrderByDescending(o => o.Value).ToDictionary(x => x.Key, x => x.Value);

			MyPostsInfoListView.UpdatePostsInfo(tab);
			DownloadButton.Tag = true;

			MyWrapGrid.Children.Clear();
			if(tab.Posts.Count == 0) {
				AllBlackListHint.Visibility = Visibility.Collapsed;
				NoDataHint.Visibility = Visibility.Visible;
				DownloadButton.Tag = false;
				NoDataStoryboard.Begin();
				return;
			} else if(afterBlackListed.Count == 0) {
				AllBlackListHint.Visibility = Visibility.Visible;
				NoDataHint.Visibility = Visibility.Collapsed;
				DownloadButton.Tag = false;
				AllBlackListStoryboard.Begin();
				return;
			}

			for(int i = 0; i < afterBlackListed.Count; i++) {
				if(cts_loading == null || cts_loading.IsCancellationRequested) {
					return;
				}
				try {
					await Task.Delay(10, cts_loading.Token);
				} catch(TaskCanceledException) {
					return;
				}
				Post item = afterBlackListed[i];
				ImageHolder holder = new(this, item, i, PathType.PostID, item.id) {
					BelongedTags = tab.Tags,
				};
				MyWrapGrid.Children.Add(holder);
				if(IsAdaptive) {
					SetImageItemSize(!IsAdaptive, holder, item.sample, LocalSettings.Current.adaptiveSizeMultiplier);
				} else {
					SetImageItemSize(!IsAdaptive, holder, item.sample, LocalSettings.Current.fixedHeight);
				}
				holder.OnImagedLoaded += (b) => {
					Paginator.SetLoadCountText(++tab.LoadedCount, tab.Posts.Count);
				};
				holder.BeginEntranceAnimation();
			}
		}

		public void SelectFeedBack() {
			SelectionCountTextBlock.Text = $"{GetSelectedImages().Count}/{CurrentTab?.Posts?.Count ?? 0}";
		}

		public void SetAllItemsSize(bool fixedHeight, double value) {
			MyWrapGrid.Visibility = Visibility.Collapsed;
			foreach(UIElement item in MyWrapGrid.Children) {
				if(item is ImageHolder holder) {
					SetImageItemSize(fixedHeight, holder, holder.PostRef.sample, value);
				}
			}
			MyWrapGrid.UpdateLayout();
			MyWrapGrid.Visibility = Visibility.Visible;
		}

		private void SetImageItemSize(bool fixedHeight, ImageHolder holder, Sample sample, double value) {
			int height = sample.height;
			int width = sample.width;
			if(fixedHeight) {
				float ratio_hdw = height / (float)width;
				int span_row = (int)value / ItemSize;
				int span_col = (int)Math.Round(span_row / ratio_hdw);
				holder.SpanCol = span_col;
				holder.SpanRow = span_row;
			} else {
				int fixedHeightSpan = width / 100;
				int fixedWidthSpan = height / 100;
				int span_row = (int)(fixedHeightSpan * value);
				int span_col = (int)(fixedWidthSpan * value);
				holder.SpanCol = span_row;
				holder.SpanRow = span_col;
			}
		}

		private void UpdateInfoButton(string[] tags) {
			//no need to hide it if no tags
			InfoButton.Visibility = true || tags.Where(t => !string.IsNullOrWhiteSpace(t)).Count() > 0 ? Visibility.Visible : Visibility.Collapsed;
		}

		private void ImagesSizeDialog_UpdateImagesLayout(bool isAdaptive, double value) {
			IsAdaptive = isAdaptive;
			SetAllItemsSize(!isAdaptive, value);
		}

		public void EnterSelectionMode() {
			if(MultipleSelectionMode) {
				return;
			}
			PostsTab selected = GetSelectedTab();
			if(selected == null) {
				return;
			}
			MultipleSelectionMode = true;
			SelectionCountTextBlock.Visibility = Visibility.Visible;
			SelectionCountTextBlock.Text = $"0/{selected.Posts.Count}";
			if(!SelectToggleButton.IsChecked.Value) {
				SelectToggleButton.IsChecked = true;
			}
			AddFavoritesButton.Visibility = Visibility.Collapsed;
		}

		public void LeaveSelectionMode() {
			if(!MultipleSelectionMode) {
				return;
			}
			PostsTab selected = GetSelectedTab();
			if(selected == null) {
				return;
			}
			MultipleSelectionMode = false;
			SelectionCountTextBlock.Visibility = Visibility.Collapsed;
			foreach(UIElement item in MyWrapGrid.Children) {
				if(item is ImageHolder holder) {
					holder.IsSelected = false;
				}
			}
			if(SelectToggleButton.IsChecked.Value) {
				SelectToggleButton.IsChecked = false;
			}
			AddFavoritesButton.Visibility = Visibility.Collapsed;
		}

		private async void RefreshButton_Tapped(object sender, TappedRoutedEventArgs e) {
			await Reload();
		}


		private void CancelDownloads() {
			if(cts_download != null) {
				cts_download.Cancel();
				cts_download.Dispose();
			}
			cts_download = null;
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
					CancelDownloads();
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

		private async void DownloadButton_Tapped(object sender, TappedRoutedEventArgs e) {
			PostsTab tab = GetSelectedTab();
			if(tab == null || tab.Posts == null || tab.Posts.Count == 0) {
				return;
			}
			if(MultipleSelectionMode) {
				var selectedDownloadDialog = new SelectedDownloadDialog();
				var selected = GetSelectedImages();
				if(selected.Count != 0 && await new ContentDialog() {
					Title = "Download Selection".Language(),
					Content = selectedDownloadDialog,
					PrimaryButtonText = "Yes".Language(),
					CloseButtonText = "No".Language(),
				}.ShowAsync() == ContentDialogResult.Primary) {
					if(await DownloadsManager.CheckDownloadAvailableWithDialog()) {
						CancelDownloads();
						cts_download = new CancellationTokenSource();
						CreateDownloadDialog("Please Wait".Language(), "Handling Downloads".Language());
						bool? result = await DownloadsManager.RegisterDownloads(cts_download.Token, selected.Select(i => i.PostRef), tab.Tags, selectedDownloadDialog.TodayDate, UpdateContentText);
						if(result == true) {
							MainPage.CreateTip_SuccessDownload(this);
							SelectToggleButton.IsChecked = false;
						} else if(result == null) {
							return;
						} else {
							await MainPage.CreatePopupDialog("Error".Language(), "Downloads Failed".Language());
						}
						HideDownloadDialog();
					}
				}
			} else {
				bool downloadResult = false;
				bool hasShownFail = false;
				if(tab.Pool != null) {
					if(await new ContentDialog() {
						Title = "Download Section".Language(),
						Content = "Do you want to download current pool".Language() + $" ({tab.Pool.name})",
						PrimaryButtonText = "Yes".Language(),
						CloseButtonText = "No".Language(),
					}.ShowAsync() == ContentDialogResult.Primary) {
						if(await DownloadsManager.CheckDownloadAvailableWithDialog(() => {
							hasShownFail = true;
						})) {
							CancelDownloads();
							cts_download = new CancellationTokenSource();
							CreateDownloadDialog("Please Wait".Language(), "Handling Downloads".Language());
							await Task.Delay(50);
							bool? result = await DownloadsManager.RegisterDownloads(cts_download.Token, tab.Posts, tab.Pool.name, false, UpdateContentText);
							if(result == true) {
								downloadResult = true;
							} else if(result == null) {
								return;
							}
							HideDownloadDialog();
						}
					}
				} else {
					PagesSelector pagesSelector = new() {
						Minimum = 1,
						Maximum = tab.MaxPage,
						CurrentPage = tab.CurrentPage,
					};
					if(await new ContentDialog() {
						Title = "Download Selection".Language(),
						Content = pagesSelector,
						CloseButtonText = "Back".Language(),
						PrimaryButtonText = "Confirm".Language(),
					}.ShowAsync() == ContentDialogResult.Primary) {
						if(await DownloadsManager.CheckDownloadAvailableWithDialog(() => hasShownFail = true)) {
							CancelDownloads();
							cts_download = new CancellationTokenSource();
							CreateDownloadDialog("Please Wait".Language(), "Handling Downloads".Language());
							await Task.Delay(50);
							var all = new List<Post>();
							(int from, int to) = pagesSelector.GetRange();
							if(from == to) {
								all.AddRange(tab.Posts);
							} else {
								for(int i = from; i <= to; i++) {
									UpdateContentText("Handling Downloads".Language() + "\n" + "Getting Page".Language() + $" {i}/{to}");
									if(cts_download == null) {
										HideDownloadDialog();
										return;
									}
									List<Post> p = await Post.GetPostsByTagsAsync(cts_download.Token, i, tab.Tags);
									if(p == null) {
										return;
									}
									all.AddRange(p);
								}
							}
							if(!pagesSelector.SkipBlacklist) {
								all = FilterBlacklist(all);
							}
							bool? result = await DownloadsManager.RegisterDownloads(cts_download.Token, all, tab.Tags, pagesSelector.TodayDate, UpdateContentText);
							if(result == true) {
								downloadResult = true;
							} else if(result == null) {
								return;
							}
							HideDownloadDialog();
						}
					} else {
						return;
					}
				}
				if(downloadResult) {
					MainPage.CreateTip_SuccessDownload(this);
				} else if(hasShownFail) {
					await MainPage.CreatePopupDialog("Error".Language(), "Downloads Failed".Language());
				}

			}
		}

		public List<ImageHolder> GetSelectedImages() {
			if(!MultipleSelectionMode) {
				return new List<ImageHolder>();
			}
			var result = new List<ImageHolder>();
			foreach(UIElement item in MyWrapGrid.Children) {
				if(item is ImageHolder holder && holder.IsSelected) {
					result.Add(holder);
				}
			}
			return result;
		}

		private void SelectToggleButton_Checked(object sender, RoutedEventArgs e) {
			EnterSelectionMode();
		}

		private void SelectToggleButton_Unchecked(object sender, RoutedEventArgs e) {
			LeaveSelectionMode();
		}

		private async void AddFavoritesButton_Tapped(object sender, TappedRoutedEventArgs e) {
			var dialog = new ContentDialog() {
				Title = "Favorites".Language(),
			};
			var list = new PersonalFavoritesListForMultiple(dialog, PathType.PostID, GetSelectedImages().Select(i => i.PostRef));
			dialog.Content = list;
			await dialog.ShowAsync();
		}

		private void MySplitViewButton_Tapped(object sender, TappedRoutedEventArgs e) {
			MainSplitView.IsPaneOpen = !MainSplitView.IsPaneOpen;
		}

		private async void InfoButton_Tapped(object sender, TappedRoutedEventArgs e) {
			PostsTab selected = GetSelectedTab();
			if(selected == null) {
				return;
			}
			if(selected.Pool == null) {
				ContentDialog dialog = new() {
					Title = selected.Tags.Length != 0 ? E621Tag.JoinTags(selected.Tags) : "Default".Language(),
					CloseButtonText = "Back".Language(),
				};
				var content = new CurrentTagsInformation(selected.Tags, dialog);
				dialog.Content = content;
				await dialog.ShowAsync();
				await Local.WriteListing();
			} else {
				await MainPage.CreatePopupDialog("Pool".Language() + $":{selected.Pool.id}", new CurrentPoolInformation(selected.Pool));
			}
		}

		private void TabsNavigationView_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args) {
			if(args.InvokedItemContainer is NavigationViewItem item && !item.IsSelected && item.Tag is PostsTab tab) {
				ToTab(tab);
			}
		}

		private async void NoTabSearchButton_Tapped(object sender, TappedRoutedEventArgs e) {
			await MainPage.Instance.PopupSearch();
		}

		private async void ResizePanelWidthButton_Click(object sender, RoutedEventArgs e) {
			LocalSettings.Current.tabsOpenLength = LocalSettings.Current.tabsOpenLength switch {
				200 => 250,
				250 => 300,
				300 => 350,
				350 => 400,
				400 => 200,
				_ => 400,
			};

			TabsNavigationView.OpenPaneLength = LocalSettings.Current.tabsOpenLength;
			PanelWidthText.Text = $"({LocalSettings.Current.tabsOpenLength})";
			await Local.WriteLocalSettings();
		}

		private void TabsNavigationView_PaneOpening(NavigationView sender, object args) {
			ResizePanelWidthButton.Visibility = Visibility.Visible;
		}

		private void TabsNavigationView_PaneClosing(NavigationView sender, NavigationViewPaneClosingEventArgs args) {
			ResizePanelWidthButton.Visibility = Visibility.Collapsed;
		}

		private async void Refresh_Invoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args) {
			if(IsLoading) {
				return;
			}
			await Reload();
			args.Handled = true;
		}

		private void CloseTab_Invoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args) {
			CloseTab(GetSelectedTabItem());
			args.Handled = true;
		}

		void IPage.UpdateNavigationItem() {
			MainPage.Instance.currentTag = PageTag.PostsBrowser;
			MainPage.Instance.UpdateNavigationItem();
		}

		void IPage.FocusMode(bool enabled) {
			//TabsNavigationView.IsPaneVisible = !enabled;
			if(enabled) {
				TabsNavigationView.IsPaneOpen = false;
			}
		}

	}

	public class PostsTab {
		public string[] Tags { get; set; } = new string[0];
		public List<Post> Posts { get; set; } = null;
		public List<Post> PostsAfterBlasklist { get; set; } = new();
		public E621Pool Pool { get; set; } = null;
		public int LoadedCount { get; set; } = 0;
		public List<Post> Unsupported { get; set; } = new();
		public Dictionary<string, long> BlackTags { get; set; } = new();
		public Dictionary<string, long> AllTags { get; set; } = new();
		public Dictionary<string, long> HotTags { get; set; } = new();
		public int MaxPage { get; set; } = 75;
		public int CurrentPage { get; set; } = 1;
		public string JoinedTags => Pool != null ? Pool.TagName : E621Tag.JoinTags(Tags);

		public PostsTab() { }

	}

	public class PostBrowserParameter: ICloneable {
		public int Page { get; private set; }
		public string[] Tags { get; private set; }
		public PostBrowserParameter(int page, params string[] tags) {
			Page = page;
			Tags = tags;
		}

		public object Clone() {
			return MemberwiseClone();
		}
	}
}
