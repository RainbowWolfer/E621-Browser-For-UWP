using E621Downloader.Models;
using E621Downloader.Models.Download;
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

		public List<Post> posts;
		public string[] tags;
		public int currentPage;
		public int maxPage;

		private TextBlock tb_ArticlesLoadCount;

		public int ItemSize { get => 50; }

		public bool isHeightFixed;
		public bool ShowNullImage => App.showNullImage;

		public bool multipleSelectionMode;

		public PostsBrowser() {
			Instance = this;
			this.InitializeComponent();
			this.posts = new List<Post>();
			this.NavigationCacheMode = NavigationCacheMode.Enabled;
			this.tags = Array.Empty<string>();
			Initialize();
		}

		private async void Initialize() {
			//string[] tags = { "rating:s", "wallpaper", "order:score" };
			//string[] tags = { "type:webm", "order:score" };
			string[] tags = { "order:score" };
			//posts = Post.GetPostsByTags(currentPage, tags);
			//LoadPosts(posts, tags);
			await LoadAsync(1, tags);
		}

		protected override void OnNavigatedTo(NavigationEventArgs e) {
			base.OnNavigatedTo(e);
		}

		private const int PREFEREDHEIGHT = 200;
		private int loaded;
		private void LoadPosts(List<Post> posts, params string[] tags) {
			if(posts == null) {
				return;
			}
			this.posts = posts;
			if(tags.Length != 0) {
				this.tags = tags;
				MainPage.ChangeCurrenttTags(tags);
			}
			loaded = 0;
			MyWrapGrid.Children.Clear();
			tb_ArticlesLoadCount.Text = "Posts : 0/" + this.posts.Count;
			foreach(Post item in this.posts) {
				var holder = new ImageHolder(item);
				MyWrapGrid.Children.Add(holder);
				SetImageItemSize(isHeightFixed, holder, item.sample);
				holder.OnImagedLoaded += (b) => tb_ArticlesLoadCount.Text = "Posts : " + ++loaded + "/" + this.posts.Count;
				ToolTipService.SetToolTip(holder, $"ID: {item.id}\nScore: {item.score.total}");
			}

		}

		public async Task LoadAsync(int page = 1, params string[] tags) {
			this.currentPage = page;
			MainPage.CreateInstantDialog("Please Wait", "Loading...");
			await Task.Delay(200);
			SelectToggleButton.IsChecked = false;
			List<Post> temp = Post.GetPostsByTags(page, tags);
			if(temp.Count == 0) {
				MainPage.HideInstantDialog();
				await MainPage.CreatePopupDialog("Articles Error", "Articles return 0");
				return;
			}
			this.posts = temp;
			maxPage = E621Paginator.Get(tags).GetMaxPage();
			UpdatePaginator();
			LoadPosts(this.posts, tags);
			MainPage.HideInstantDialog();
		}
		public async Task Reload() {
			MainPage.CreateInstantDialog("Please Wait", "Reloading...");
			await Task.Delay(20);
			LoadPosts(this.posts, tags);
			MainPage.HideInstantDialog();
		}
		public void UpdatePaginator() {
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
			SelectionCountTextBlock.Text = $"{GetSelected().Count}/{posts.Count}";
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
			//Debug.WriteLine(VariableSizedWrapGrid.GetRowSpan(holder) + "_" + VariableSizedWrapGrid.GetColumnSpan(holder));
		}

		private async void RefreshButton_Tapped(object sender, TappedRoutedEventArgs e) {
			await Reload();
		}

		//private async void PrevButton_Tapped(object sender, TappedRoutedEventArgs e) {
		//	if(currentPage <= 1) {
		//		return;
		//	}
		//	await LoadAsync(--currentPage, tags);
		//	//posts = Post.GetPostsByTags(--currentPage, tags);
		//	//LoadPosts(posts);
		//	//CurrentPageTextBlock.Text = "Current Page : " + currentPage;
		//}

		//private async void NextButton_Tapped(object sender, TappedRoutedEventArgs e) {
		//	await LoadAsync(++currentPage, tags);
		//	//posts = Post.GetPostsByTags(++currentPage, tags);
		//	//LoadPosts(posts);
		//	//CurrentPageTextBlock.Text = "Current Page : " + currentPage;
		//}

		//private async void PageJumpTextBox_KeyDown(object sender, KeyRoutedEventArgs e) {
		//	if(e.Key == VirtualKey.Enter) {
		//		await JumpPageAction(PageJumpTextBox);
		//	}
		//}

		//private async void JumpPageSubmitButton_Tapped(object sender, TappedRoutedEventArgs e) {
		//	await JumpPageAction(PageJumpTextBox);
		//}
		//private async Task JumpPageAction(TextBox sender) {
		//	if(int.TryParse(sender.Text, out int page)) {
		//		if(page == currentPage) {
		//			return;
		//		}
		//		if(page > 750 || page <= 0) {
		//			await MainPage.CreatePopupDialog("Error", "Plase Enter a Number Within 0 ~ 750");
		//			return;
		//		}
		//		await LoadAsync(page, tags);
		//	} else {
		//		await MainPage.CreatePopupDialog("Int Parse Error", "Plase Enter a Valid Number");
		//		return;
		//	}
		//}

		private void FixedHeightCheckBox_Checked(object sender, RoutedEventArgs e) {
			isHeightFixed = true;
			SetAllItemsSize(true);
		}

		private void FixedHeightCheckBox_Unchecked(object sender, RoutedEventArgs e) {
			isHeightFixed = false;
			SetAllItemsSize(false);
		}

		private async void DownloadButton_Tapped(object sender, TappedRoutedEventArgs e) {
			if(multipleSelectionMode) {
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
						await Task.Delay(2);
					}
					MainPage.HideInstantDialog();
					SelectToggleButton.IsChecked = false;
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
						await Task.Delay(20);
						var all = new List<Post>();
						for(int i = 1; i <= maxPage; i++) {
							List<Post> p = Post.GetPostsByTags(i, tags);
							all.AddRange(p);
						}
						foreach(Post item in all) {
							DownloadsManager.RegisterDownload(item, tags);
							await Task.Delay(2);
						}
						MainPage.HideInstantDialog();
						break;
					case ContentDialogResult.Secondary:
						//get currentpage posts
						MainPage.CreateInstantDialog("Please Wait", "Handling Downloads");
						await Task.Delay(50);
						foreach(Post item in posts) {
							DownloadsManager.RegisterDownload(item, tags);
							await Task.Delay(2);
						}
						MainPage.HideInstantDialog();
						break;
					default:
						throw new Exception();
				}
			}
		}

		public List<ImageHolder> GetSelected() {
			if(!multipleSelectionMode) {
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
			multipleSelectionMode = true;
			SelectionCountTextBlock.Visibility = Visibility.Visible;
			SelectionCountTextBlock.Text = $"0/{posts.Count}";
		}

		private void SelectToggleButton_Unchecked(object sender, RoutedEventArgs e) {
			multipleSelectionMode = false;
			SelectionCountTextBlock.Visibility = Visibility.Collapsed;
			foreach(UIElement item in MyWrapGrid.Children) {
				if(item is ImageHolder holder) {
					holder.IsSelected = false;
				}
			}
		}
	}
}
