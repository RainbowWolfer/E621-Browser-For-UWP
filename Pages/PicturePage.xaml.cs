using E621Downloader.Models;
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
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

namespace E621Downloader.Pages {
	public sealed partial class PicturePage: Page {
		public Post PostRef { get; private set; }
		public ObservableCollection<string> tags;
		public PicturePage() {
			this.InitializeComponent();
			this.NavigationCacheMode = NavigationCacheMode.Enabled;
			tags = new ObservableCollection<string>();
		}
		protected override void OnNavigatedTo(NavigationEventArgs e) {
			base.OnNavigatedTo(e);
			if(e.Parameter == null) {
				MyProgressRing.IsActive = false;
				return;
			}
			MyProgressRing.IsActive = true;
			PostRef = e.Parameter as Post;
			if(PostRef == null) {
				return;
			}
			tags.Clear();
			//PostRef.tags.ToList().ForEach((s) => tags.Add(s));
			MainImage.Source = new BitmapImage(new Uri(PostRef.file.url));
		}

		private void MainImage_ImageOpened(object sender, RoutedEventArgs e) {
			MyProgressRing.IsActive = false;
		}
	}
}
