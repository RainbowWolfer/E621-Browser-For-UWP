using E621Downloader.Models;
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

		public int ItemSize { get => 50; }

		public bool isHeightFixed;
		public bool showNullImage = true;

		public PostsBrowser() {
			Instance = this;
			this.InitializeComponent();
			this.posts = new List<Post>();
			this.NavigationCacheMode = NavigationCacheMode.Enabled;
			Initialize();
		}

		private async void Initialize() {
			string[] tags = { "rating:s", "wallpaper", "order:score" };
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
			LoadsTextBlock.Text = "Articles : 0/" + this.posts.Count;
			foreach(Post item in this.posts) {
				var holder = new ImageHolder(item);
				MyWrapGrid.Children.Add(holder);
				SetImageItemSize(isHeightFixed, holder, item.sample);
				holder.OnImagedLoaded += (b) => LoadsTextBlock.Text = "Articles : " + ++loaded + "/" + this.posts.Count;
			}
		}

		public async Task LoadAsync(int page = 1, params string[] tags) {
			this.currentPage = page;
			MainPage.CreateInstantDialog("Please Wait", "Loading...");
			await Task.Delay(200);
			List<Post> temp = Post.GetPostsByTags(page, tags);
			if(temp.Count == 0) {
				MainPage.HideInstantDialog();
				await MainPage.CreatePopupDialog("Articles Error", "Articles return 0");
				return;
			}
			this.posts = temp;
			LoadPosts(this.posts, tags);
			MainPage.HideInstantDialog();
			CurrentPageTextBlock.Text = "Current Page : " + page;
		}
		public async Task Reload() {
			MainPage.CreateInstantDialog("Please Wait", "Reloading...");
			await Task.Delay(20);
			LoadPosts(this.posts, tags);
			MainPage.HideInstantDialog();
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

		private async void PrevButton_Tapped(object sender, TappedRoutedEventArgs e) {
			if(currentPage <= 1) {
				return;
			}
			await LoadAsync(--currentPage, tags);
			//posts = Post.GetPostsByTags(--currentPage, tags);
			//LoadPosts(posts);
			//CurrentPageTextBlock.Text = "Current Page : " + currentPage;
		}

		private async void NextButton_Tapped(object sender, TappedRoutedEventArgs e) {
			await LoadAsync(++currentPage, tags);
			//posts = Post.GetPostsByTags(++currentPage, tags);
			//LoadPosts(posts);
			//CurrentPageTextBlock.Text = "Current Page : " + currentPage;
		}

		private async void PageJumpTextBox_KeyDown(object sender, KeyRoutedEventArgs e) {
			if(e.Key == VirtualKey.Enter) {
				if(int.TryParse(PageJumpTextBox.Text, out int page)) {
					await LoadAsync(page, tags);
				} else {
					await MainPage.CreatePopupDialog("Int Parse Error", "Plase Enter a Valid Number");
				}
			}
		}

		private async void JumpPageSubmitButton_Tapped(object sender, TappedRoutedEventArgs e) {
			if(int.TryParse(PageJumpTextBox.Text, out int page)) {
				if(page > 750 || page <= 0) {
					await MainPage.CreatePopupDialog("Error", "Plase Enter a Number Within 0 ~ 750");
					return;
				}
				await LoadAsync(page, tags);
			} else {
				await MainPage.CreatePopupDialog("Int Parse Error", "Plase Enter a Valid Number");
				return;
			}
		}

		private void FixedHeightCheckBox_Checked(object sender, RoutedEventArgs e) {
			isHeightFixed = true;
			SetAllItemsSize(true);
		}

		private void FixedHeightCheckBox_Unchecked(object sender, RoutedEventArgs e) {
			isHeightFixed = false;
			SetAllItemsSize(false);
		}

		private void ShowNullImageCheckBox_Checked(object sender, RoutedEventArgs e) {
			showNullImage = true;
			ShowNullImages(true);
		}

		private void ShowNullImageCheckBox_Unchecked(object sender, RoutedEventArgs e) {
			showNullImage = false;
			ShowNullImages(false);
		}

		private void ShowBlackListCheckBox_Checked(object sender, RoutedEventArgs e) {

		}

		private void ShowBlackListCheckBox_Unchecked(object sender, RoutedEventArgs e) {

		}
	}
}
