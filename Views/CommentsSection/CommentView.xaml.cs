using E621Downloader.Models.Networks;
using E621Downloader.Models.Posts;
using E621Downloader.Pages;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.Core;
using Windows.Storage.Streams;
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
		public CancellationTokenSource Cts { get; private set; }
		public CommentView(E621Comment comment) {
			this.InitializeComponent();
			this.Comment = comment;
			this.DataContextChanged += (s, c) => Bindings.Update();
		}

		public async Task LoadAvatar() {
			Cts = new CancellationTokenSource();
			AvatorLoadingRing.IsActive = true;
			User = await E621User.GetAsync(Comment.creator_id, Cts.Token);
			string url = "";
			if(User != null) {
				url = await E621User.GetAvatarURLAsync(User, Cts.Token);
			}
			BitmapImage bi;
			if(!string.IsNullOrEmpty(url)) {
				HttpResult<InMemoryRandomAccessStream> result = await Data.ReadImageStreamAsync(url, Cts.Token);
				if(result.Result == HttpResultType.Success) {
					bi = new BitmapImage();
					await bi.SetSourceAsync(result.Content);
					ToolTipService.SetToolTip(Avatar, $"Post: {User.avatar_id}");
					Avatar.Tapped += Avatar_Tapped;
				} else {
					bi = App.DefaultAvatar;
					ToolTipService.SetToolTip(Avatar, $"Avatar Load Fail\n{result.Helper}");
				}
			} else {
				bi = App.DefaultAvatar;
				ToolTipService.SetToolTip(Avatar, "No Avatar");
			}
			AvatorLoadingRing.IsActive = false;
			Avatar.Source = bi;
		}

		public void EnableLoadingRing() {
			AvatorLoadingRing.IsActive = true;
		}

		private async void Avatar_Tapped(object sender, TappedRoutedEventArgs e) {
			if(User == null || User.avatar_id == null) {
				return;
			}
			MainPage.CreateInstantDialog("Please Wait...", $"Loading Post: {User.avatar_id}");
			Post post = await Post.GetPostByIDAsync(Cts.Token, User.avatar_id);
			MainPage.HideInstantDialog();
			MainPage.NavigateToPicturePage(post);
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
