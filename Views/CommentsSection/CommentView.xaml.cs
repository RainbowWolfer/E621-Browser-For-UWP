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
		private async void LoadAvatar() {
			AvatorLoadingRing.IsActive = true;
			User = await E621User.GetAsync(Comment.creator_id);
			string url = "";
			if(User != null) {
				url = await E621User.GetAvatarURLAsync(User);
			}
			BitmapImage bi;
			if(!string.IsNullOrEmpty(url)) {
				bi = new BitmapImage(new Uri(this.BaseUri, url));
				bi.ImageOpened += (s, e) => {
					Avatar.Tapped += async (sender, args) => {
						if(User == null || User.avatar_id == null) {
							return;
						}
						MainPage.CreateInstantDialog("Please Wait...", $"Loading Post: {User.avatar_id}");
						Post post = await Post.GetPostByIDAsync(User.avatar_id);
						MainPage.HideInstantDialog();
						MainPage.NavigateToPicturePage(post);
					};
					AvatorLoadingRing.IsActive = false;
					ToolTipService.SetToolTip(Avatar, $"Post: {User.avatar_id}");
				};
				bi.ImageFailed += (s, e) => {
					bi.UriSource = new Uri("ms-appx:///Assets/esix2.jpg");
					AvatorLoadingRing.IsActive = false;
					ToolTipService.SetToolTip(Avatar, e.ErrorMessage);
				};
			} else {
				bi = new BitmapImage(new Uri("ms-appx:///Assets/esix2.jpg"));
				AvatorLoadingRing.IsActive = false;
				ToolTipService.SetToolTip(Avatar, "No Avatar");
			}
			Avatar.Source = bi;
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
