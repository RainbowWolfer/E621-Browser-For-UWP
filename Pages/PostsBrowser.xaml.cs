using E621Downloader.Models;
using E621Downloader.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace E621Downloader.Pages {
	public sealed partial class PostsBrowser: Page {
		public static PostsBrowser Instance;
		public readonly ObservableCollection<E621Article> articles;
		public const float HolderScale = 0.5f;

		public int currentLoads;
		public int finishedLoads;

		public PostsBrowser() {
			this.InitializeComponent();
			Instance = this;
			articles = new ObservableCollection<E621Article>();
			this.NavigationCacheMode = NavigationCacheMode.Enabled;
			LoadPosts(Data.GetPostsByTags(1, "rating:s", "wallpaper", "order:score"));
		}

		protected override void OnNavigatedTo(NavigationEventArgs e) {
			base.OnNavigatedTo(e);
			currentLoads = 0;
			finishedLoads = 0;

		}
		private int loaded;
		public void LoadPosts(E621Article[] articles) {
			loaded = 0;
			while(MyWrapGrid.Children.Count > 1) {
				MyWrapGrid.Children.RemoveAt(1);
			}
			this.articles.Clear();
			articles.ToList().ForEach((p) => this.articles.Add(p));
			ArticlesCountTextBlock.Text = "Articles Count : 0/" + this.articles.Count;
			for(int i = 0; i < this.articles.Count; i++) {
				E621Article item = this.articles[i];
				//if(item.isLoaded) {
				//	continue;
				//}
				//if(i >= currentLoads) {
				//	break;
				//}
				var holder = new ImageHolder(item);
				holder.OnImagedLoaded += (b) => {
					int fixedHeightSpan = b.PixelHeight / 100;
					int fixedWidthSpan = b.PixelWidth / 100;
					int span_row = (int)(fixedHeightSpan * HolderScale);
					int span_col = (int)(fixedWidthSpan * HolderScale);
					VariableSizedWrapGrid.SetColumnSpan(holder, span_col);
					VariableSizedWrapGrid.SetRowSpan(holder, span_row);
					ArticlesCountTextBlock.Text = "Articles Count : " + ++loaded + "/" + this.articles.Count;
				};
				MyWrapGrid.Children.Add(holder);
			}

		}
	}
	public struct LoadsInfo {
		public int goalLoads;
		public int cureentLoads;
	}
}
