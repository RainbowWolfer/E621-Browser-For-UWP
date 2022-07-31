using E621Downloader.Models.Locals;
using E621Downloader.Models.Posts;
using Microsoft.Toolkit.Uwp.UI;
using System;
using System.Collections.Generic;
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
using Windows.UI.Xaml.Navigation;

namespace E621Downloader.Views.ListingManager {
	public sealed partial class FollowingPoolsManager: UserControl {
		private readonly List<FollowingPoolsListItem> items = new();
		public ContentDialog Dialog { get; private set; }

		private bool canceled = false;

		public FollowingPoolsManager(ContentDialog dialog) {
			this.InitializeComponent();
			Dialog = dialog;
			items.Clear();
			MainListView.Items.Clear();
			foreach(int id in Local.Listing.FollowPoolsList) {
				var item = new FollowingPoolsListItem(this, id);
				items.Add(item);
				MainListView.Items.Add(item);
			}

			if(items.Count == 0) {
				EmptyGrid.Visibility = Visibility.Visible;
				MainListView.Visibility = Visibility.Collapsed;
			} else {
				Load();
			}
		}

		private async void Load() {
			foreach(FollowingPoolsListItem item in items) {
				if(canceled) {
					break;
				}
				await item.Load();
			}
		}

		private void ListView_ItemClick(object sender, ItemClickEventArgs e) {
			if(e.ClickedItem is FollowingPoolsListItem item) {
				item.Click();
			}
		}

		public void HideDialog() {
			canceled = true;
			Dialog.Hide();
		}

		public void CancelAllLoading() {
			foreach(FollowingPoolsListItem item in items) {
				item.CancelLoading();
			}
		}

		public async Task DeleteItem(int poolID) {
			if(Local.Listing.CheckFollowPool(poolID)) {
				await Local.Listing.RemoveFollowPool(poolID);
			}
			FollowingPoolsListItem found = items.Find(i => i.poolID == poolID);
			if(found != null) {
				items.Remove(found);
				MainListView.Items.Remove(found);
			}
		}
	}
}
