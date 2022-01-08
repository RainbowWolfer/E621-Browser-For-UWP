using E621Downloader.Models.Posts;
using E621Downloader.Pages;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
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
		private string test = "Start";
		private async void LoadAvatar() {
			AvatorLoadingRing.IsActive = true;
			User = await E621User.GetAsync(Comment.creator_id);
			test += "\n1";
			string url = "";
			if(User != null) {
				url = await E621User.GetAvatorURLAsync(User);
				test += "\n2";
			}
			BitmapImage bi;
			if(!string.IsNullOrEmpty(url)) {
				test += "\n3";
				bi = new BitmapImage() {
					UriSource = new Uri(this.BaseUri, url)
				};
				//Debug.WriteLine($"avatar {url}");

				Avatar.Tapped += async (sender, e) => {
					if(User == null || User.avatar_id == null) {
						return;
					}
					MainPage.CreateInstantDialog("Please Wait...", $"Loading Post: {User.avatar_id}");
					Post post = await Post.GetPostByID(User.avatar_id.Value);
					MainPage.HideInstantDialog();
					MainPage.Instance.parameter_picture = post;
					MainPage.NavigateToPicturePage();
				};
				ToolTipService.SetToolTip(Avatar, $"Post: {User.avatar_id}");
				test += "\n4";
			} else {
				bi = new BitmapImage(new Uri("ms-appx:///Assets/esix2.jpg"));//not working
				test += "\n5";
			}
			Avatar.Source = bi;
			//AvatorLoadingRing.IsActive = false;
			test += "\n6";
			ToolTipService.SetToolTip(ReportButton, test);
		}

		private void Avatar_ImageOpened(object sender, RoutedEventArgs e) {
			AvatorLoadingRing.IsActive = false;
			//Debug.WriteLine((sender as Image).Source);
			test += "\nopened";
			ToolTipService.SetToolTip(ReportButton, test);
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
