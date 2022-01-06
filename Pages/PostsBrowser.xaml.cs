using E621Downloader.Models;
using E621Downloader.Models.Download;
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
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System;
using Windows.UI;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

namespace E621Downloader.Pages {
	public sealed partial class PostsBrowser: Page {
		public static PostsBrowser Instance;
		public const float HolderScale = 1;

		public List<Post> Posts { get; private set; }
		public string[] tags;
		public int currentPage;
		public int maxPage;

		public TagsFilterSystem tagsFilterSystem;

		private TextBlock tb_ArticlesLoadCount;

		public int ItemSize { get => 50; }

		public bool isHeightFixed;

		public bool MultipleSelectionMode { get; private set; }

		private readonly string[] ignoreTypes = { "swf", };

		private PostBrowserParameter parameter;

		public PostsBrowser() {
			Instance = this;
			this.InitializeComponent();
			this.Posts = new List<Post>();
			this.NavigationCacheMode = NavigationCacheMode.Enabled;
			this.tags = Array.Empty<string>();
			this.tagsFilterSystem = new TagsFilterSystem(HotTagsListView, BlackTagsListView,
				enable => UpdateImageHolders(CalculateEnabledPosts(), false)
			);
		}

		protected override async void OnNavigatedTo(NavigationEventArgs e) {
			base.OnNavigatedTo(e);
			if(e.Parameter is PostBrowserParameter parameter) {
				this.parameter = (PostBrowserParameter)parameter.Clone();
				await LoadAsync(parameter.Page, parameter.Tags);
			} else if(this.parameter == null) {
				//string[] tags = { "rating:s", "wallpaper", "order:score" };
				//string[] tags = { "type:webm", "order:score" };
				string[] tags = { "order:score" };
				this.parameter = new PostBrowserParameter(1, tags);
				await LoadAsync(1, tags);
			}
			MainPage.ClearPostBrowserParameter();
		}

		private const int PREFEREDHEIGHT = 350;
		private int loaded;
		private void LoadPosts(List<Post> posts, params string[] tags) {
			if(posts == null) {
				return;
			}
			this.Posts = posts;
			tagsFilterSystem.RegisterBlackList(posts);
			if(tags.Length != 0) {
				this.tags = tags;
				MainPage.ChangeCurrenttTags(tags);
			}
			loaded = 0;
			tb_ArticlesLoadCount.Text = "Posts : 0/" + this.Posts.Count;

			UpdateImageHolders(CalculateEnabledPosts(), true);

			tagsFilterSystem.Update(posts);
		}
		private void UpdateImageHolders(List<Post> ps, bool refresh) {
			if(refresh) {
				MyWrapGrid.Children.Clear();
				foreach(Post item in ps) {
					//if(!tagsFilterSystem.CheckPostContainBlackList(item)) {
					//	continue;
					//}
					var holder = new ImageHolder(item, this.Posts.IndexOf(item));
					MyWrapGrid.Children.Add(holder);
					SetImageItemSize(isHeightFixed, holder, item.sample);
					holder.OnImagedLoaded += (b) => tb_ArticlesLoadCount.Text = "Posts : " + ++loaded + "/" + this.Posts.Count;
					ToolTipService.SetToolTip(holder, $"ID: {item.id}\nScore: {item.score.total}");
				}
			} else {
				Debug.WriteLine($"{this.Posts.Count} {ps.Count} {MyWrapGrid.Children.Count}");
				for(int i = 0; i < this.Posts.Count; i++) {
					ImageHolder existed = GetImageHolder(i);
					int shouleBe = ps.IndexOf(this.Posts[i]);
					if(shouleBe == -1) {
						Post item = Posts[i];
						var holder = new ImageHolder(item, this.Posts.IndexOf(item));
						SetImageItemSize(isHeightFixed, holder, item.sample);
						holder.OnImagedLoaded += (b) => tb_ArticlesLoadCount.Text = "Posts : " + ++loaded + "/" + this.Posts.Count;
						ToolTipService.SetToolTip(holder, $"ID: {item.id}\nScore: {item.score.total}");
						MyWrapGrid.Children.Insert(existed.Index, holder);
					}
					Debug.WriteLine($"{existed?.Index} {shouleBe}");
				}
			}
			//ScrollViewer.
			//MyWrapGrid.
		}

		private ImageHolder GetImageHolder(int index) {
			return MyWrapGrid.Children.Select(c => c as ImageHolder).FirstOrDefault(i => i.Index == index);
		}

		private async Task LoadAsync(int page = 1, params string[] tags) {
			this.currentPage = page;
			MainPage.CreateInstantDialog("Please Wait", "Loading...");
			await Task.Delay(200);
			SelectToggleButton.IsChecked = false;
			List<Post> temp = await Post.GetPostsByTagsAsync(page, tags);
			if(temp.Count == 0) {
				MainPage.HideInstantDialog();
				await MainPage.CreatePopupDialog("Articles Error",
					$"Tags:({E621Tag.JoinTags(tags)}) return 0 posts");
				return;
			}
			int removed_count = temp.RemoveAll(p => ignoreTypes.Contains(p.file.ext));
			Debug.WriteLine($"Removed {removed_count} posts");
			this.Posts = temp;
			maxPage = E621Paginator.Get(tags).GetMaxPage();
			UpdatePaginator();
			LoadPosts(this.Posts, tags);
			MainPage.HideInstantDialog();
			UpdateInfoButton(tags);
		}
		private async Task Reload() {
			MainPage.CreateInstantDialog("Please Wait", "Reloading...");
			await Task.Delay(20);
			LoadPosts(this.Posts, tags);
			MainPage.HideInstantDialog();
		}

		private void UpdateInfoButton(string[] tags) {
			InfoButton.Visibility = true || tags.Where(i => !i.Contains(':')).Count() > 0 ? Visibility.Visible : Visibility.Collapsed;
		}

		private List<Post> CalculateEnabledPosts() {
			var result = new List<Post>();
			foreach(Post item in this.Posts) {
				if(!tagsFilterSystem.CheckPostContainBlackList(item)) {
					result.Add(item);
				}
			}
			return result;
		}

		private void UpdatePaginator() {
			PaginatorPanel.Children.Clear();
			//currentPage
			//maxPage
			//minPage = 1
			//left 4 right 4

			tb_ArticlesLoadCount = new TextBlock() {
				Text = "Posts: 0/75",
				Width = 130,
				VerticalAlignment = VerticalAlignment.Center,
				HorizontalAlignment = HorizontalAlignment.Center,
				TextAlignment = TextAlignment.Center,
				FontSize = 20,
			};
			PaginatorPanel.Children.Add(tb_ArticlesLoadCount);

			FrameworkElement CreateButton(object content, Action<Button> action = null) {
				FrameworkElement result;
				if(action != null) {
					result = new Button() {
						Content = content.ToString(),
						Background = new SolidColorBrush(Colors.Transparent),
						BorderThickness = new Thickness(0),
						Height = 40,
						Width = 40,
					};
					result.Tapped += (s, e) => action.Invoke(s as Button);
				} else {
					//button.IsEnabled = false;
					result = new TextBlock() {
						Text = content.ToString(),
						Height = 40,
						Width = 40,
						TextAlignment = TextAlignment.Center,
						Margin = new Thickness(0, 0, 0, -20),
					};
				}
				if(content is int page && page == currentPage) {
					if(result is Button button) {
						button.FontWeight = FontWeights.ExtraBold;
						button.FontSize += 2;
						button.Margin = new Thickness(0, 0, 0, -15);
					} else if(result is TextBlock tb) {
						tb.FontWeight = FontWeights.ExtraBold;
						tb.FontSize += 2;
						tb.Margin = new Thickness(0, 0, 0, -15);
					}
				}
				PaginatorPanel.Children.Add(result);
				return result;
			}
			async Task NavigatePage(int i) {
				int page;
				if(i == -1) {
					page = Math.Clamp(currentPage - 1, 1, maxPage);
				} else if(i == -2) {
					page = Math.Clamp(currentPage + 1, 1, maxPage);
				} else {
					page = i;
				}
				await LoadAsync(page, tags);
			}

			async void P1(Button b) => await NavigatePage(-1);
			async void P2(Button b) => await NavigatePage(-2);

			var btns = new List<FrameworkElement> {
				CreateButton('<', currentPage > 1 ? (Action<Button>)P1 : null)
			};

			if(currentPage > 6) {
				btns.Add(CreateButton('1', async b => await NavigatePage(1)));
				btns.Add(CreateButton("..."));
				btns.Add(CreateButton(currentPage - 4, async b => await NavigatePage(currentPage - 4)));
				btns.Add(CreateButton(currentPage - 3, async b => await NavigatePage(currentPage - 3)));
				btns.Add(CreateButton(currentPage - 2, async b => await NavigatePage(currentPage - 2)));
				btns.Add(CreateButton(currentPage - 1, async b => await NavigatePage(currentPage - 1)));
			} else {
				for(int i = 1; i < currentPage; i++) {
					btns.Add(CreateButton(i, async b => await NavigatePage(i)));
				}
			}

			btns.Add(CreateButton(currentPage));

			if(maxPage - currentPage > 6) {
				btns.Add(CreateButton(currentPage + 1, async b => await NavigatePage(currentPage + 1)));
				btns.Add(CreateButton(currentPage + 2, async b => await NavigatePage(currentPage + 2)));
				btns.Add(CreateButton(currentPage + 3, async b => await NavigatePage(currentPage + 3)));
				btns.Add(CreateButton(currentPage + 4, async b => await NavigatePage(currentPage + 4)));
				btns.Add(CreateButton("..."));
			} else {
				for(int i = currentPage + 1; i < maxPage; i++) {
					btns.Add(CreateButton(i, async b => await NavigatePage(i)));
				}
			}
			if(currentPage != maxPage) {
				btns.Add(CreateButton(maxPage, async b => await NavigatePage(maxPage)));
			}

			btns.Add(CreateButton('>', currentPage < maxPage ? (Action<Button>)P2 : null));

			AutoSuggestBox box = new AutoSuggestBox() {
				QueryIcon = new SymbolIcon(Symbol.Forward),
				VerticalAlignment = VerticalAlignment.Center,
				MinWidth = 130,
				PlaceholderText = "Jump To Page",
			};
			box.QuerySubmitted += async (s, e) => {
				if(int.TryParse(s.Text, out int page)) {
					if(page < 1 || page > maxPage) {
						await MainPage.CreatePopupDialog("Error", $"({page}) can only be in 1-{maxPage}");
					} else {
						await LoadAsync(page, tags);
					}
				} else {
					await MainPage.CreatePopupDialog("Error", $"({s.Text}) is not a valid number");
				}
			};
			PaginatorPanel.Children.Add(box);
		}

		public void SelectFeedBack(ImageHolder imageHolder) {
			SelectionCountTextBlock.Text = $"{GetSelected().Count}/{Posts.Count}";
		}

		private void ShowNullImages(bool showNullImages) {
			if(MyWrapGrid == null) {
				return;
			}
			MyWrapGrid.Visibility = Visibility.Collapsed;
			foreach(UIElement item in MyWrapGrid.Children) {
				if(item is ImageHolder holder) {
					if(holder.LoadUrl != null) {
						continue;
					}
					if(showNullImages) {
						holder.Visibility = Visibility.Visible;
						VariableSizedWrapGrid.SetColumnSpan(holder, holder.SpanCol);
						VariableSizedWrapGrid.SetRowSpan(holder, holder.SpanRow);
					} else {
						holder.Visibility = Visibility.Collapsed;
						VariableSizedWrapGrid.SetColumnSpan(holder, 0);
						VariableSizedWrapGrid.SetRowSpan(holder, 0);
					}
				}
			}
			MyWrapGrid.Visibility = Visibility.Visible;
		}

		private void SetAllItemsSize(bool fixedHeight) {
			MyWrapGrid.Visibility = Visibility.Collapsed;
			foreach(UIElement item in MyWrapGrid.Children) {
				if(item is ImageHolder holder) {
					SetImageItemSize(fixedHeight, holder, holder.PostRef.sample);
				}
			}
			MyWrapGrid.UpdateLayout();
			MyWrapGrid.Visibility = Visibility.Visible;
		}

		private void SetImageItemSize(bool fixedHeight, ImageHolder holder, Sample sample) {
			int height = sample.height;
			int width = sample.width;
			if(fixedHeight) {
				float ratio_hdw = height / (float)width;
				int span_row = PREFEREDHEIGHT / ItemSize;
				int span_col = (int)Math.Round(span_row / ratio_hdw);
				holder.SpanCol = span_col;
				holder.SpanRow = span_row;
			} else {
				int fixedHeightSpan = width / 100;
				int fixedWidthSpan = height / 100;
				int span_row = (int)(fixedHeightSpan * HolderScale);
				int span_col = (int)(fixedWidthSpan * HolderScale);
				holder.SpanCol = span_row;
				holder.SpanRow = span_col;
			}
		}

		private async void RefreshButton_Tapped(object sender, TappedRoutedEventArgs e) {
			await Reload();
		}
		private void FixedHeightCheckBox_Checked(object sender, RoutedEventArgs e) {
			isHeightFixed = true;
			SetAllItemsSize(true);
		}

		private void FixedHeightCheckBox_Unchecked(object sender, RoutedEventArgs e) {
			isHeightFixed = false;
			SetAllItemsSize(false);
		}
		private const int delay = 1;
		private async void DownloadButton_Tapped(object sender, TappedRoutedEventArgs e) {
			if(MultipleSelectionMode) {
				if(await new ContentDialog() {
					Title = "Download Selection",
					Content = "Do you want to download the selected",
					PrimaryButtonText = "Yes",
					CloseButtonText = "No",
				}.ShowAsync() == ContentDialogResult.Primary) {
					MainPage.CreateInstantDialog("Please Wait", "Handling Downloads");
					await Task.Delay(50);
					foreach(ImageHolder item in GetSelected()) {
						DownloadsManager.RegisterDownload(item.PostRef, tags);
						await Task.Delay(delay);
					}
					MainPage.HideInstantDialog();
					SelectToggleButton.IsChecked = false;
					ShowTeachingTip();
				}
			} else {
				var dialog = new ContentDialog() {
					Title = "Download Selection",
					Content = new TextBlock() {
						Text = "Do you want to download for current page or for whole tag(s)?",
						TextWrapping = TextWrapping.WrapWholeWords,
						FontSize = 24,
					},
					CloseButtonText = "Back",
					PrimaryButtonText = "Whole Tag(s)",
					SecondaryButtonText = "Current Page",
				};
				ContentDialogResult result = await dialog.ShowAsync();
				switch(result) {
					case ContentDialogResult.None:
						break;
					case ContentDialogResult.Primary:
						MainPage.CreateInstantDialog("Please Wait", "Handling Downloads");
						await Task.Delay(50);
						var all = new List<Post>();
						for(int i = 1; i <= maxPage; i++) {
							List<Post> p = await Post.GetPostsByTagsAsync(i, tags);
							all.AddRange(p);
						}
						foreach(Post item in all) {
							DownloadsManager.RegisterDownload(item, tags);
							await Task.Delay(delay);
						}
						MainPage.HideInstantDialog();
						break;
					case ContentDialogResult.Secondary:
						//get currentpage posts
						MainPage.CreateInstantDialog("Please Wait", "Handling Downloads");
						await Task.Delay(50);
						foreach(Post item in Posts) {
							DownloadsManager.RegisterDownload(item, tags);
							await Task.Delay(delay);
						}
						MainPage.HideInstantDialog();
						break;
					default:
						throw new Exception();
				}
				ShowTeachingTip();
			}
		}
		public void ShowTeachingTip() {
			ToggleThemeTeachingTip2.IsOpen = true;
		}

		public List<ImageHolder> GetSelected() {
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

		public void EnterSelectionMode() {
			if(MultipleSelectionMode) {
				return;
			}
			MultipleSelectionMode = true;
			SelectionCountTextBlock.Visibility = Visibility.Visible;
			SelectionCountTextBlock.Text = $"0/{Posts.Count}";
			if(!SelectToggleButton.IsChecked.Value) {
				SelectToggleButton.IsChecked = true;
			}
		}

		public void LeaveSelectionMode() {
			if(!MultipleSelectionMode) {
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
		}


		private void MySplitViewButton_Tapped(object sender, TappedRoutedEventArgs e) {
			MySplitView.IsPaneOpen = !MySplitView.IsPaneOpen;
		}

		private void TagsButton_Tapped(object sender, TappedRoutedEventArgs e) {
			if(tagsFilterSystem.hot_tags_count == 5) {
				tagsFilterSystem.hot_tags_count = 10;
			} else if(tagsFilterSystem.hot_tags_count == 10) {
				tagsFilterSystem.hot_tags_count = 15;
			} else if(tagsFilterSystem.hot_tags_count == 15) {
				tagsFilterSystem.hot_tags_count = 20;
			} else if(tagsFilterSystem.hot_tags_count == 20) {
				tagsFilterSystem.hot_tags_count = 5;
			}
			HotTagsCountText.Text = $"({tagsFilterSystem.hot_tags_count})";
			tagsFilterSystem.UpdateHotTags();
		}

		private async void HotTagsListView_ItemClick(object sender, ItemClickEventArgs e) {
			string tag = (e.ClickedItem as StackPanel).Tag as string;
			await LoadAsync(1, tag);
		}

		private async void InfoButton_Tapped(object sender, TappedRoutedEventArgs e) {
			await MainPage.CreatePopupDialog(E621Tag.JoinTags(tags), new CurrentTagsInformation(tags));
		}

		private void ToggleThemeTeachingTip2_ActionButtonClick(Microsoft.UI.Xaml.Controls.TeachingTip sender, object args) {
			Debug.WriteLine("ewq");
		}

		private void ToggleThemeTeachingTip2_Closed(Microsoft.UI.Xaml.Controls.TeachingTip sender, Microsoft.UI.Xaml.Controls.TeachingTipClosedEventArgs args) {
			Debug.WriteLine(sender.Parent);
		}
	}

	public class TagsFilterSystem {
		private readonly ListView hotTagsListView;
		private readonly ListView blackTagsListView;
		private readonly Dictionary<string, long> all_tags;//name, count
		public readonly Dictionary<string, long> black_tags;//name, count
		public readonly Dictionary<string, CheckBox> black_tags_enabled;

		private Dictionary<string, long> hot_tags;

		public int hot_tags_count = 5;

		public Action<bool> BlackListCheckBoxAction { get; private set; }

		public Dictionary<string, long> GetHotTags(int length) {
			return hot_tags.Take(length).ToDictionary(x => x.Key, x => x.Value);
		}

		public bool CheckPostContainBlackList(Post post) {
			//var list = GetEnabledBlackTags();
			IEnumerable<string> list = black_tags.Select(d => d.Key);
			foreach(string item in post.tags.GetAllTags()) {
				if(list.Contains(item)) {
					return true;
				}
			}
			return false;
		}

		public List<string> GetEnabledBlackTags() {
			var result = new List<string>();
			foreach(KeyValuePair<string, CheckBox> item in black_tags_enabled) {
				if(item.Value.IsChecked.Value) {
					result.Add(item.Key);
				}
			}
			return result;
		}

		public void Update(List<Post> posts) {
			foreach(Post item in posts) {
				foreach(string tag in item.tags.GetAllTags()) {
					if(all_tags.ContainsKey(tag)) {
						all_tags[tag]++;
					} else {
						all_tags.Add(tag, 1);
					}
				}
			}
			CalculateHotTags();
			UpdateHotTags();
			UpdateBlackListTags();
		}

		private void CalculateHotTags() {
			hot_tags = all_tags.OrderByDescending(o => o.Value).ToDictionary(x => x.Key, x => x.Value);
		}

		public void RegisterBlackList(List<Post> posts) {
			black_tags.Clear();
			foreach(Post item in posts) {
				foreach(string tag in item.tags.GetAllTags()) {
					if(Local.BlackList.Contains(tag)) {
						if(black_tags.ContainsKey(tag)) {
							black_tags[tag]++;
						} else {
							black_tags.Add(tag, 1);
						}
						break;
					}
				}
			}
		}

		public void UpdateHotTags() {
			hotTagsListView.Items.Clear();
			foreach(KeyValuePair<string, long> item in GetHotTags(hot_tags_count)) {
				StackPanel panel = new StackPanel() {
					Orientation = Orientation.Horizontal,
					Tag = item.Key,
				};
				panel.Children.Add(new TextBlock() {
					Text = item.Value.ToString(),
					Width = 50,
					TextAlignment = TextAlignment.Right,
				});
				panel.Children.Add(new TextBlock() {
					Text = item.Key,
					Margin = new Thickness(10, 0, 0, 0),
				});
				hotTagsListView.Items.Add(panel);
			}
		}

		public void UpdateBlackListTags() {
			black_tags_enabled.Clear();
			blackTagsListView.Items.Clear();
			foreach(KeyValuePair<string, long> item in black_tags) {
				StackPanel panel = new StackPanel() { Orientation = Orientation.Horizontal };
				var checkBox = new CheckBox() {
					IsChecked = true,
					Content = "",
					MinWidth = 10,
					VerticalAlignment = VerticalAlignment.Center,
				};
				checkBox.Unchecked += (s, e) => BlackListCheckBoxAction?.Invoke(false);
				checkBox.Checked += (s, e) => BlackListCheckBoxAction?.Invoke(true);
				//panel.Children.Add(checkBox);
				panel.Children.Add(new TextBlock() {
					Text = item.Value.ToString(),
					Width = 50,//20
					TextAlignment = TextAlignment.Right,
					HorizontalAlignment = HorizontalAlignment.Center,
					VerticalAlignment = VerticalAlignment.Center,
				});
				panel.Children.Add(new TextBlock() {
					Text = item.Key,
					Margin = new Thickness(10, 0, 0, 0),
					HorizontalAlignment = HorizontalAlignment.Center,
					VerticalAlignment = VerticalAlignment.Center,
				});
				blackTagsListView.Items.Add(panel);
				black_tags_enabled.Add(item.Key, checkBox);
			}
		}

		public TagsFilterSystem(ListView hotTagsListView, ListView blackTagsListView, Action<bool> blackListCheckBoxAction) {
			this.all_tags = new Dictionary<string, long>();
			this.hot_tags = new Dictionary<string, long>();
			this.black_tags = new Dictionary<string, long>();
			this.black_tags_enabled = new Dictionary<string, CheckBox>();
			this.hotTagsListView = hotTagsListView;
			this.blackTagsListView = blackTagsListView;
			this.BlackListCheckBoxAction = blackListCheckBoxAction;
		}
	}

	public class PostBrowserParameter: ICloneable {
		public int Page { get; private set; }
		public string[] Tags { get; private set; }
		public PostBrowserParameter(int page, string[] tags) {
			Page = page;
			Tags = tags;
		}

		public object Clone() {
			return MemberwiseClone();
		}
	}
}
