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
			BitmapImage bi;
			if(!string.IsNullOrEmpty(url)) {
				bi = new BitmapImage() {
					UriSource = new Uri(this.BaseUri, url)
				};
				//Debug.WriteLine($"avatar {url}");

				Avatar.RightTapped += (sender, e) => {
					MenuFlyout myFlyout = new MenuFlyout();
					//if(sourceSet != null) {
					//	for(int i = 0; i < sourceSet.sets.Length; i++) {
					//		SourceSet.Set ss = sourceSet.sets[i];
					//		MenuFlyoutItem fly = new MenuFlyoutItem { Text = "复制图片地址： 品质 " + ss.quality + "w" };
					//		fly.Click += (s, c) => {
					//			DataPackage dataPackage = new DataPackage();
					//			dataPackage.SetText(ss.address);
					//			Clipboard.SetContent(dataPackage);
					//		};
					//		myFlyout.Items.Add(fly);
					//	}
					//} else {
					//	//myFlyout.Items.Add(new TextBlock() { Text = "" });
					//}
					//myFlyout.Placement = FlyoutPlacementMode.Left;
					//if(myFlyout.Items.Count != 0) {
					//	FrameworkElement senderElement = sender as FrameworkElement;
					//	myFlyout.ShowAt(sender as UIElement, e.GetPosition(sender as UIElement));
					//}
				};

			} else {
				bi = new BitmapImage(new Uri("ms-appx:///Assets/esix2.jpg"));//not working
			}
			Avatar.Source = bi;
		}

		private void Avatar_ImageOpened(object sender, RoutedEventArgs e) {
			AvatorLoadingRing.IsActive = false;
			//Debug.WriteLine((sender as Image).Source);
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
