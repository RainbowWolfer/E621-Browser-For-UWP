using E621Downloader.Models.Download;
using E621Downloader.Models.Locals;
using E621Downloader.Models.Posts;
using E621Downloader.Views;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using NavigationView = Microsoft.UI.Xaml.Controls.NavigationView;
using NavigationViewItem = Microsoft.UI.Xaml.Controls.NavigationViewItem;
using NavigationViewItemInvokedEventArgs = Microsoft.UI.Xaml.Controls.NavigationViewItemInvokedEventArgs;

namespace E621Downloader.Pages {
	public sealed partial class PostsBrowserPage: Page {
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
			return Instance?.GetSelectedTab()?.Posts ?? new List<Post>();
		}

		public static void SetSelectionMode(bool enable) {
			if(enable) {
				Instance?.EnterSelectionMode();
			} else {
				Instance?.LeaveSelectionMode();
			}
		}

		public static void SetSelectionFeedback(ImageHolder imageHolder) {
			Instance?.SelectFeedBack(imageHolder);
		}

		private static PostsBrowserPage Instance;
		public int ItemSize { get => 50; }
		public readonly ObservableCollection<PostsInfoList> poststInfolists = new();

		public bool MultipleSelectionMode { get; private set; }
		private bool IsAdaptive { get; set; }

		private readonly string[] ignoreTypes = { "swf", };

		private PostBrowserParameter parameter;

		//private E621Pool pool;

		private CancellationTokenSource cts_loading;
		private CancellationTokenSource cts_download;
		private bool isLoading;

		private PostsTab CurrentTab { get; set; } = null;

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
		}

		protected override void OnNavigatedTo(NavigationEventArgs e) {
			base.OnNavigatedTo(e);
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
				Content = string.IsNullOrWhiteSpace(tab.JoinedTags) ? "( Default Tab )" : tab.JoinedTags,
				Tag = tab,
			};
			ToolTipService.SetToolTip(item, item.Content);
			MenuFlyout flyout = new();
			MenuFlyoutItem delete_item = new() {
				Icon = new FontIcon() {
					Glyph = "\uE10A",
				},
				Text = "Close",
			};
			MenuFlyoutItem copy_item = new() {
				Icon = new FontIcon() {
					Glyph = "\uE8C8",
				},
				Text = "Copy",
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
				TabsNavigationView.MenuItems.Remove(item);

				List<NavigationViewItem> items = GetAllNavitationViewItems();
				if(TabsNavigationView.SelectedItem == item) {
					TabsNavigationView.SelectedItem = items.FirstOrDefault();
				}
				if(items.Count == 0) {
					ToTab(null);
				} else {
					ToTab((PostsTab)items.First().Tag);
				}
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

			LoadingText.Text = "Getting Ready";

			CancelLoading();
			await Task.Delay(100);//must be greater than 10
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
				LoadingText.Text = "Loading Posts";
			} else {
				LoadingText.Text = $"Loading Tags: ({string.Join(' ', tags)})";
			}

			List<Post> posts = await Post.GetPostsByTagsAsync(cts_loading.Token, tab.CurrentPage, tags);

			if(posts == null) {//cancelled
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

			if(cts_loading == null || cts_loading.IsCancellationRequested) {
				IsLoading = true;
				return;
			}

			if(tab.Pool == null) {
				LoadingText.Text = "Loading Paginator";
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

		private async Task UpdateImageHolders(PostsTab tab) {
			tab.LoadedCount = 0;
			Paginator.SetLoadCountText(0, tab.Posts.Count);

			List<Post> afterBlackListed = new();
			tab.BlackTags.Clear();
			foreach(Post post in tab.Posts) {
				bool foundInBlackList = false;
				foreach(string tag in post.tags.GetAllTags()) {
					if(Local.Listing.CheckBlackList(tag)) {
						foundInBlackList = true;
						if(tab.BlackTags.ContainsKey(tag)) {
							tab.BlackTags[tag]++;
						} else {
							tab.BlackTags.Add(tag, 1);
						}
						//no break to calculate all tags
					}
				}
				if(!foundInBlackList) {
					afterBlackListed.Add(post);
				}
			}
			tab.BlackTags = tab.BlackTags.OrderByDescending(t => t.Value).ToDictionary(x => x.Key, x => x.Value);

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

			UpdatePostsInfo(tab);
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

		public void UpdatePostsInfo(PostsTab tab) {
			List<PostInfoLine> deletes = new();
			foreach(Post item in tab.Unsupported) {
				deletes.Add(new PostInfoLine(item.id, $"file type: {item.file.ext}"));
			}

			List<PostInfoLine> hots = new();
			int count = 0;
			foreach(KeyValuePair<string, long> item in tab.HotTags) {
				hots.Add(new PostInfoLine(item.Key, $"{item.Value}"));
				if(count++ > 20) {
					break;
				}
			}

			List<PostInfoLine> blacks = new();
			foreach(KeyValuePair<string, long> item in tab.BlackTags) {
				blacks.Add(new PostInfoLine(item.Key, $"{item.Value}"));
			}

			poststInfolists.Clear();
			poststInfolists.Add(new PostsInfoList("Deleted Posts", deletes));
			poststInfolists.Add(new PostsInfoList("Blacklist", blacks));
			poststInfolists.Add(new PostsInfoList("Hot Tags (Top 20)", hots));

			PostsInfoListView.SelectedIndex = 0;

			//foreach(PostInfoLine line in poststInfolists.SelectMany(i => i)) {
			//	var v = PostsInfoListView.ContainerFromItem(line);
			//	if(v is ListViewItem item) {
			//		item.Margin = new Thickness(20, 0, 0, 0);
			//		Debug.WriteLine(line);
			//	}
			//}
		}

		public void SelectFeedBack(ImageHolder imageHolder) {
			SelectionCountTextBlock.Text = $"{GetSelectedImages().Count}/{CurrentTab?.Posts.Count ?? 0}";
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
				if(await new ContentDialog() {
					Title = "Download Selection",
					Content = "Do you want to download the selected",
					PrimaryButtonText = "Yes",
					CloseButtonText = "No",
				}.ShowAsync() == ContentDialogResult.Primary) {
					if(await DownloadsManager.CheckDownloadAvailableWithDialog()) {
						CancelDownloads();
						cts_download = new CancellationTokenSource();
						CreateDownloadDialog("Please Wait", "Handling Downloads");
						bool? result = await DownloadsManager.RegisterDownloads(cts_download.Token, GetSelectedImages().Select(i => i.PostRef), tab.Tags, UpdateContentText);
						if(result == true) {
							MainPage.CreateTip_SuccessDownload(this);
							SelectToggleButton.IsChecked = false;
						} else if(result == null) {
							return;
						} else {
							await MainPage.CreatePopupDialog("Error", "Downloads Failed");
						}
						HideDownloadDialog();
					}
				}
			} else {
				bool downloadResult = false;
				bool hasShownFail = false;
				if(tab.Pool != null) {
					if(await new ContentDialog() {
						Title = "Download Section",
						Content = $"Do you want to download current pool ({tab.Pool.name})",
						PrimaryButtonText = "Yes",
						CloseButtonText = "No",
					}.ShowAsync() == ContentDialogResult.Primary) {
						if(await DownloadsManager.CheckDownloadAvailableWithDialog(() => hasShownFail = true)) {
							CancelDownloads();
							cts_download = new CancellationTokenSource();
							CreateDownloadDialog("Please Wait", "Handling Downloads");
							await Task.Delay(50);
							bool? result = await DownloadsManager.RegisterDownloads(cts_download.Token, tab.Posts, tab.Pool.name, UpdateContentText);
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
						Title = "Download Selection",
						Content = pagesSelector,
						CloseButtonText = "Back",
						PrimaryButtonText = "Confirm",
					}.ShowAsync() == ContentDialogResult.Primary) {
						if(await DownloadsManager.CheckDownloadAvailableWithDialog(() => hasShownFail = true)) {
							CancelDownloads();
							cts_download = new CancellationTokenSource();
							CreateDownloadDialog("Please Wait", "Handling Downloads");
							await Task.Delay(50);
							var all = new List<Post>();
							(int from, int to) = pagesSelector.GetRange();
							if(from == to) {
								all.AddRange(tab.Posts);
							} else {
								for(int i = from; i <= to; i++) {
									UpdateContentText($"Handling Downloads\nGetting Page {i}/{to}");
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
							bool? result = await DownloadsManager.RegisterDownloads(cts_download.Token, all, tab.Tags, UpdateContentText);
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
				} else if(!hasShownFail) {
					await MainPage.CreatePopupDialog("Error", "Downloads Failed");
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
				Title = "Favorites",
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
					Title = selected.Tags.Length != 0 ? E621Tag.JoinTags(selected.Tags) : "Default",
					CloseButtonText = "Back",
				};
				var content = new CurrentTagsInformation(selected.Tags, dialog);
				dialog.Content = content;
				await dialog.ShowAsync();
				await Local.WriteListing();
			} else {
				await MainPage.CreatePopupDialog($"Pool:{selected.Pool.id}", new CurrentPoolInformation(selected.Pool));
			}
		}

		private void TabsNavigationView_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args) {
			if(args.InvokedItemContainer is NavigationViewItem item && !item.IsSelected && item.Tag is PostsTab tab) {
				ToTab(tab);
			}
		}

		private void PostsInfoListView_ItemClick(object sender, ItemClickEventArgs e) {
			if(PostsInfoListView.ContainerFromItem(e.ClickedItem) is ListViewItem item) {
				item.IsSelected = true;
			}
		}

		private void CopyItem_Click(object sender, RoutedEventArgs e) {
			string name = (string)((MenuFlyoutItem)sender).Tag;
			DataPackage dataPackage = new() {
				RequestedOperation = DataPackageOperation.Copy
			};
			dataPackage.SetText($"{name}");
			Clipboard.SetContent(dataPackage);
		}

		private async void NoTabSearchButton_Tapped(object sender, TappedRoutedEventArgs e) {
			await MainPage.Instance.PopupSearch();
		}
	}

	public class PostsTab {
		public string[] Tags { get; set; } = new string[0];
		public List<Post> Posts { get; set; } = null;
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

	public class PostsInfoList: ObservableCollection<PostInfoLine> {
		public string Key { get; set; }
		public PostsInfoList(string key) : base() {
			this.Key = key;
		}
		public PostsInfoList(string key, List<PostInfoLine> content) : base() {
			this.Key = key;
			content.ForEach(s => this.Add(s));
		}
	}

	public struct PostInfoLine {
		public string Name { get; set; }
		public string Detail { get; set; }
		public PostInfoLine(string name, string detail) {
			Name = name;
			Detail = detail;
		}
		public override string ToString() {
			return $"Name ({Name}) - Detail ({Detail})";
		}
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
