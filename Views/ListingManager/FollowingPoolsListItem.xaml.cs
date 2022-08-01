using E621Downloader.Models;
using E621Downloader.Models.E621;
using E621Downloader.Models.Networks;
using E621Downloader.Models.Utilities;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;

namespace E621Downloader.Views.ListingManager {
	public sealed partial class FollowingPoolsListItem: UserControl {
		private readonly FollowingPoolsManager parent;
		public readonly int poolID;
		private CancellationTokenSource cts;
		private E621Pool pool;

		public E621Pool Pool {
			get => pool;
			private set {
				pool = value;
				OpenItem.IsEnabled = value != null;
			}
		}

		public FollowingPoolsListItem(FollowingPoolsManager parent, int poolID) {
			this.InitializeComponent();
			this.parent = parent;
			this.poolID = poolID;
			HeaderText.Text = $"# {poolID} (...)";
		}

		public async Task Load() {
			if(App.PoolsPool.TryGetValue(poolID, out E621Pool pool) && pool != null) {
				//Pool = pool;
				Pool = await E621Pool.GetAsync(poolID);// this is only data update in app lifecycle.
			} else {
				Pool = await E621Pool.GetAsync(poolID);
				if(Pool != null) {
					App.PoolsPool.Add(poolID, Pool);
				}
			}
			UpdateInfo(Pool);
		}

		private async void UpdateInfo(E621Pool pool) {
			if(pool == null) {
				return;
			}
			HeaderText.Text = $"# {pool.id} ({pool.post_count})";
			ToolTipService.SetToolTip(MainGrid, $"{pool.name}");

			InfoTip.Title = $"Pool ID : {pool.id}";
			NameText.Text = pool.name;
			CreatedText.Text = Methods.GetDateTime(pool.created_at);
			UpdatedText.Text = Methods.GetDateTime(pool.updated_at);
			CreatorText.Text = pool.creator_name;
			CategoryText.Text = pool.category;
			DescriptionText.Text = pool.description;

			await LoadImage();
		}

		public void CancelLoading() {
			if(cts != null) {
				cts.Cancel();
				cts.Dispose();
			}
			cts = null;
		}

		private async ValueTask LoadImage() {
			E621Post post = await GetFirstPost(pool);
			if(post == null || string.IsNullOrWhiteSpace(post.sample.url)) {
				PreviewImage.Source = App.DefaultAvatar;
				LoadingBar.Visibility = Visibility.Collapsed;
				return;
			}
			LoadingBar.Visibility = Visibility.Visible;
			if(PreviewImage.Source is BitmapImage b) {
				b.UriSource = null;
			}
			PreviewImage.Source = null;
			CancelLoading();
			cts = new CancellationTokenSource();
			LoadPoolItem loader = LoadPool.SetNew(post);
			BitmapImage bit = loader.Sample;
			if(bit == null) {
				HttpResult<InMemoryRandomAccessStream> result = await Data.ReadImageStreamAsync(post.sample.url, cts.Token);
				if(result.Result == HttpResultType.Success) {
					bit = new BitmapImage();
					await bit.SetSourceAsync(result.Content);
					loader.Sample = bit;
				} else {
					bit = App.DefaultAvatar;
				}
			}
			PreviewImage.Source = bit;
			LoadingBar.Visibility = Visibility.Collapsed;
		}

		//get first that have sample url
		private async ValueTask<E621Post> GetFirstPost(E621Pool pool, int maxTryCount = 10) {
			E621Post found = null;
			for(int i = 0; i < Math.Min(pool.post_ids.Count, maxTryCount); i++) {
				var id = pool.post_ids[i];
				if(string.IsNullOrWhiteSpace(id)) {
					continue;
				}
				E621Post tmp;
				if(App.PostsPool.TryGetValue(id, out E621Post post)) {
					tmp = post;
				} else {
					tmp = await E621Post.GetPostByIDAsync(id);
					if(tmp != null) {
						App.PostsPool.Add(id, tmp);
					}
				}
				if(tmp == null || string.IsNullOrWhiteSpace(tmp.sample.url)) {
					continue;
				}
				found = tmp;
				break;
			}
			return found;
		}

		public void Click() {
			if(Pool == null) {
				return;
			}
			InfoTip.IsOpen = true;
		}

		private void InfoTip_ActionButtonClick(TeachingTip sender, object args) {
			Open();
		}

		private void OpenItem_Click(object sender, RoutedEventArgs e) {
			Open();
		}

		private void Open() {
			if(Pool == null) {
				return;
			}
			parent.CancelAllLoading();
			parent.HideDialog();
			MainPage.NavigateToPostsBrowser(Pool);
		}

		private async void DeleteItem_Click(object sender, RoutedEventArgs e) {
			await parent.DeleteItem(poolID);
		}
	}
}
