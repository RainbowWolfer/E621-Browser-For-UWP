using E621Downloader.Models.Posts;
using System;
using System.Collections.Generic;
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

namespace E621Downloader.Views {
	public sealed partial class ImageHolderForSubscriptionPage: UserControl {
		public Post PostRef { get; private set; }
		public ImageHolderForSubscriptionPage(Post post) {
			this.InitializeComponent();
			this.PostRef = post;
			LoadingRing.IsActive = true;
			MyImage.Source = new BitmapImage(new Uri(post.sample.url ?? post.preview.url));
			(MyImage.Source as BitmapImage).ImageOpened += ImageHolderForSubscriptionPage_ImageOpened;
			TypeHint.PostRef = post;
			BottomInfo.PostRef = post;
		}

		private void ImageHolderForSubscriptionPage_ImageOpened(object sender, RoutedEventArgs e) {
			LoadingRing.IsActive = false;
		}
	}
}
