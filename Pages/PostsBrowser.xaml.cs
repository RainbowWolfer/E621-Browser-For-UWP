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
		public readonly ObservableCollection<E621Article> articles;
		public const float HolderScale = 1;

		private PageInfo pageInfo;

		public int ItemSize { get => 50; }

		public bool isHeightFixed;

		public PostsBrowser() {
			Instance = this;
			this.InitializeComponent();
			this.articles = new ObservableCollection<E621Article>();
			this.NavigationCacheMode = NavigationCacheMode.Enabled;
			Initialize();
		}

		private void Initialize() {
			pageInfo = new PageInfo() { currentPage = 1 };
			string[] tags = { "rating:s", "wallpaper", "order:score" };
			pageInfo.articles = Data.GetPostsByTags(pageInfo.currentPage, tags);
			LoadPosts(pageInfo.articles, tags);
		}

		protected override void OnNavigatedTo(NavigationEventArgs e) {
			base.OnNavigatedTo(e);
		}

		private const int PREFEREDHEIGHT = 200;
		private int loaded;
		public void LoadPosts(E621Article[] articles, params string[] tags) {
			if(articles == null) {
				return;
			}
			if(tags.Length != 0) {
				pageInfo.tags = tags;
				MainPage.ChangeCurrenttTags(tags);
			}
			pageInfo.currentPage = 1;
			loaded = 0;
			MyWrapGridSameHeight.Children.Clear();
			this.articles.Clear();
			articles.ToList().ForEach((p) => this.articles.Add(p));
			LoadsTextBlock.Text = "Articles : 0/" + this.articles.Count;
			for(int i = 0; i < this.articles.Count; i++) {
				E621Article item = this.articles[i];
				var holder = new ImageHolder(item);
				MyWrapGridSameHeight.Children.Add(holder);
				holder.OnImagedLoaded += (b) => SetImageItemSize(isHeightFixed, holder, b);
				holder.OnImagedLoaded += (b) => LoadsTextBlock.Text = "Articles : " + ++loaded + "/" + this.articles.Count;
			}
		}

		private void SetAllItemsSize(bool fixedHeight) {
			MyWrapGridSameHeight.Visibility = Visibility.Collapsed;
			foreach(UIElement item in MyWrapGridSameHeight.Children) {
				if(item is ImageHolder holder) {
					SetImageItemSize(fixedHeight, holder, holder.Image);
				}
			}
			MyWrapGridSameHeight.UpdateLayout();
			MyWrapGridSameHeight.Visibility = Visibility.Visible;
		}

		private void SetImageItemSize(bool fixedHeight, ImageHolder holder, BitmapImage b) {
			if(b == null) {
				return;
			}
			if(fixedHeight) {
				float ratio_hdw = b.PixelHeight / (float)b.PixelWidth;
				int span_row = PREFEREDHEIGHT / ItemSize;
				int span_col = (int)Math.Round(span_row / ratio_hdw);
				VariableSizedWrapGrid.SetColumnSpan(holder, span_col);
				VariableSizedWrapGrid.SetRowSpan(holder, span_row);
			} else {
				int fixedHeightSpan = b.PixelHeight / 100;
				int fixedWidthSpan = b.PixelWidth / 100;
				int span_row = (int)(fixedHeightSpan * HolderScale);
				int span_col = (int)(fixedWidthSpan * HolderScale);
				VariableSizedWrapGrid.SetColumnSpan(holder, span_col);
				VariableSizedWrapGrid.SetRowSpan(holder, span_row);
			}
			Debug.WriteLine(VariableSizedWrapGrid.GetRowSpan(holder) + "_" + VariableSizedWrapGrid.GetColumnSpan(holder));
		}

		private void RefreshButton_Tapped(object sender, TappedRoutedEventArgs e) {
			LoadPosts(pageInfo.articles);
		}

		private void PrevButton_Tapped(object sender, TappedRoutedEventArgs e) {
			if(pageInfo.currentPage <= 1) {
				return;
			}
			pageInfo.articles = Data.GetPostsByTags(pageInfo.currentPage - 1, pageInfo.tags);
			LoadPosts(pageInfo.articles);
			pageInfo.currentPage--;
			CurrentPageTextBlock.Text = "Current Page : " + pageInfo.currentPage;
		}

		private void NextButton_Tapped(object sender, TappedRoutedEventArgs e) {
			pageInfo.articles = Data.GetPostsByTags(pageInfo.currentPage + 1, pageInfo.tags);
			LoadPosts(pageInfo.articles);
			pageInfo.currentPage++;
			CurrentPageTextBlock.Text = "Current Page : " + pageInfo.currentPage;
		}

		private async void PageJumpTextBox_KeyDown(object sender, KeyRoutedEventArgs e) {
			if(e.Key == VirtualKey.Enter) {
				if(int.TryParse(PageJumpTextBox.Text, out int page)) {
					CurrentPageTextBlock.Text = "Current Page : " + page;
					pageInfo.currentPage = page;
					pageInfo.articles = Data.GetPostsByTags(pageInfo.currentPage, pageInfo.tags);
					if(pageInfo.articles.Length == 0) {
						await MainPage.CreatePopupDialog("Articles Error", "Articles return 0");
						return;
					}
					LoadPosts(pageInfo.articles);
				} else {
					await MainPage.CreatePopupDialog("Int Parse Error", "Plase Enter a Valid Number");
				}
			}
		}

		private async void JumpPageSubmitButton_Tapped(object sender, TappedRoutedEventArgs e) {
			if(int.TryParse(PageJumpTextBox.Text, out int page)) {
				CurrentPageTextBlock.Text = "Current Page : " + page;
				pageInfo.currentPage = page;
				MainPage.CreateInstantDialog("Please Wait", "Loading...");
				//while(!MainPage.IsShowingInstanceDialog) {
				//	await Task.Delay(20);
				//}
				await Task.Delay(20);
				pageInfo.articles = Data.GetPostsByTags(pageInfo.currentPage, pageInfo.tags);
				MainPage.HideInstantDialog();
				LoadPosts(pageInfo.articles);
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
	public class PageInfo {
		public string[] tags;
		public E621Article[] articles;
		public int currentPage;

		//max page -> paginator
	}
}
