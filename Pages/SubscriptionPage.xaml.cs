using E621Downloader.Models.Locals;
using E621Downloader.Models.Posts;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
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
	public sealed partial class SubscriptionPage: Page {
		private CancellationTokenSource cts = new CancellationTokenSource();
		public SubscriptionPage() {
			this.InitializeComponent();

		}

		private async void Load() {
			List<Post> posts = await Post.GetPostsByTagsAsync(cts.Token, true, 1, Local.FollowList);
			foreach(Post item in posts) {
				TestText.Text += item.file.url + "\n";
			}
		}
		protected override void OnNavigatedTo(NavigationEventArgs e) {
			base.OnNavigatedTo(e);
			Load();
		}
	}
}
