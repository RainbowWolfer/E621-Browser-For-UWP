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
		public List<Post> posts;
		public const float HolderScale = 1;

		public string[] tags;
		public int currentPage;

		public int ItemSize { get => 50; }

		public bool isHeightFixed;

		public PostsBrowser() {
			Instance = this;
			this.InitializeComponent();
			this.posts = new List<Post>();
			this.NavigationCacheMode = NavigationCacheMode.Enabled;
			Initialize();
		}

		private void Initialize() {
			currentPage = 1;
			string[] tags = { "rating:s", "wallpaper", "order:score" };
			posts = Post.GetPostsByTags(currentPage, tags);
			LoadPosts(posts, tags);
		}

		protected override void OnNavigatedTo(NavigationEventArgs e) {
			base.OnNavigatedTo(e);
		}

		private const int PREFEREDHEIGHT = 200;
		private int loaded;
		public void LoadPosts(List<Post> posts, params string[] tags) {
			if(posts == null) {
				return;
			}
			this.posts = posts;
			if(tags.Length != 0) {
				this.tags = tags;
				MainPage.ChangeCurrenttTags(tags);
			}
			//currentPage = 1;
			loaded = 0;
			MyWrapGrid.Children.Clear();
			LoadsTextBlock.Text = "Articles : 0/" + this.posts.Count;
			for(int i = 0; i < this.posts.Count; i++) {
				Post item = this.posts[i];
				var holder = new ImageHolder(item);
				MyWrapGrid.Children.Add(holder);
				SetImageItemSize(isHeightFixed, holder, item.sample);
				//holder.OnImagedLoaded += (b) => SetImageItemSize(isHeightFixed, holder, b);
				holder.OnImagedLoaded += (b) => LoadsTextBlock.Text = "Articles : " + ++loaded + "/" + this.posts.Count;
			}
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
				VariableSizedWrapGrid.SetColumnSpan(holder, span_col);
				VariableSizedWrapGrid.SetRowSpan(holder, span_row);
			} else {
				int fixedHeightSpan = width / 100;
				int fixedWidthSpan = height / 100;
				int span_row = (int)(fixedHeightSpan * HolderScale);
				int span_col = (int)(fixedWidthSpan * HolderScale);
				VariableSizedWrapGrid.SetColumnSpan(holder, span_row);
				VariableSizedWrapGrid.SetRowSpan(holder, span_col);
			}
			Debug.WriteLine(VariableSizedWrapGrid.GetRowSpan(holder) + "_" + VariableSizedWrapGrid.GetColumnSpan(holder));
		}

		private void RefreshButton_Tapped(object sender, TappedRoutedEventArgs e) {
			LoadPosts(posts);
		}

		private void PrevButton_Tapped(object sender, TappedRoutedEventArgs e) {
			if(currentPage <= 1) {
				return;
			}
			posts = Post.GetPostsByTags(--currentPage, tags);
			LoadPosts(posts);
			CurrentPageTextBlock.Text = "Current Page : " + currentPage;
		}

		private void NextButton_Tapped(object sender, TappedRoutedEventArgs e) {
			posts = Post.GetPostsByTags(++currentPage, tags);
			LoadPosts(posts);
			CurrentPageTextBlock.Text = "Current Page : " + currentPage;
		}

		private async void PageJumpTextBox_KeyDown(object sender, KeyRoutedEventArgs e) {
			if(e.Key == VirtualKey.Enter) {
				if(int.TryParse(PageJumpTextBox.Text, out int page)) {
					CurrentPageTextBlock.Text = "Current Page : " + page;
					currentPage = page;
					posts = Post.GetPostsByTags(currentPage, tags);
					if(posts.Count == 0) {
						await MainPage.CreatePopupDialog("Articles Error", "Articles return 0");
						return;
					}
					LoadPosts(posts);
				} else {
					await MainPage.CreatePopupDialog("Int Parse Error", "Plase Enter a Valid Number");
				}
			}
		}

		private async void JumpPageSubmitButton_Tapped(object sender, TappedRoutedEventArgs e) {
			if(int.TryParse(PageJumpTextBox.Text, out int page)) {
				CurrentPageTextBlock.Text = "Current Page : " + page;
				currentPage = page;
				MainPage.CreateInstantDialog("Please Wait", "Loading...");
				//while(!MainPage.IsShowingInstanceDialog) {
				//	await Task.Delay(20);
				//}
				await Task.Delay(20);
				posts = Post.GetPostsByTags(currentPage, tags);
				MainPage.HideInstantDialog();
				LoadPosts(posts);
			} else {
				await MainPage.CreatePopupDialog("Int Parse Error", "Plase Enter a Valid Number");
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
	}
}
