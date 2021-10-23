using E621Downloader.Models.Posts;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

namespace E621Downloader.Views.CommentsSection {
	public sealed partial class CommentView: UserControl {
		public E621Comment Comment { get; set; }
		public E621User User { get; set; }
		public CommentView(E621Comment comment) {
			this.InitializeComponent();
			this.Comment = comment;
			this.DataContextChanged += (s, c) => Bindings.Update();
			LoadAvatar();
		}

		private async void LoadAvatar() {
			User = await E621User.GetAsync(Comment.creator_id);
			string url = "";
			if(User != null) {
				url = await E621User.GetAvatorURL(User);
			}
			if(!string.IsNullOrEmpty(url)) {
				BitmapImage bi = new BitmapImage {
					UriSource = new Uri(this.BaseUri, url)
				};
				Avatar.Source = bi;
			}
		}

		private void Avatar_ImageOpened(object sender, RoutedEventArgs e) {
			AvatorLoadingRing.IsActive = false;
		}

		private void DownVoteButton_Tapped(object sender, TappedRoutedEventArgs e) {

		}

		private void UpVoteButton_Tapped(object sender, TappedRoutedEventArgs e) {

		}

		private void ReplyButton_Tapped(object sender, TappedRoutedEventArgs e) {

		}

		private void ReportButton_Tapped(object sender, TappedRoutedEventArgs e) {

		}
	}
}
