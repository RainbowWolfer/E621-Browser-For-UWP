using E621Downloader.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

namespace E621Downloader.Pages {
	public sealed partial class GridViewPostsBrowser: Page {
		public readonly ObservableCollection<E621Article> articles;
		public GridViewPostsBrowser() {
			this.InitializeComponent();
			articles = new ObservableCollection<E621Article>();
			//Task.Run(() => LoadPosts(Data.GetPostsByTags(1, "rating:s", "wallpaper", "order:score")));

		}

		protected override void OnNavigatedTo(NavigationEventArgs e) {
			base.OnNavigatedTo(e);

		}

		public void LoadPosts(E621Article[] articles) {
			foreach(E621Article item in articles) {
				this.articles.Add(item);
				//MyGridView.Items.Add(new Image);
			}
		}

		private void Image_ImageOpened(object sender, RoutedEventArgs e) {
			var self = sender as Image;
			int width = (self.Source as BitmapImage).PixelWidth;
			int height = (self.Source as BitmapImage).PixelHeight;
			self.Width = 200 * width / height;
			Debug.WriteLine(200 * width / height);
		}
	}
}
