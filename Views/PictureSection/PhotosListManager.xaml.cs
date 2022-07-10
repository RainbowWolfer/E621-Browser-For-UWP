using E621Downloader.Models;
using E621Downloader.Models.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace E621Downloader.Views.PictureSection {
	public sealed partial class PhotosListManager: UserControl {
		private List<PhotosListItem> Items { get; } = new();

		public PhotosListManager() {
			this.InitializeComponent();
		}

		public void SetPhotos(List<object> objs, object current) {
			if(GetCurrentList().CompareItemsEqual(objs)) {
				foreach(PhotosListItem item in Items) {
					item.IsSelected = item.Photo == current;
					if(item.IsSelected) {
						PutToCenter(item);
					}
				};
				return;
			}
			PhotosListView.Items.Clear();
			Items.Clear();
			PhotosListItem c = null;
			int selectedIndex = -1;
			for(int i = 0; i < objs.Count; i++) {
				object obj = objs[i];
				var item = new PhotosListItem();
				item.SetPhoto(obj);
				item.IsSelected = obj == current;
				if(item.IsSelected) {
					c = item;
					selectedIndex = i;
				}
				PhotosListView.Items.Add(new ListViewItem() {
					Content = item,
					Margin = new Thickness(0),
					Padding = new Thickness(0),
				});
				Items.Add(item);
			}
			List<Task> tasks = Items.Select(i => i.LoadAsync()).ToList();
			Load(tasks, selectedIndex);
			if(c != null) {
				PutToCenter(c);
			}
		}

		private async void Load(List<Task> tasks, int selectedIndex) {
			List<Pair<Task, int>> pool = new();
			tasks.ForEach(t => pool.Add(new Pair<Task, int>(t, -1)));
			for(int i = 0; i < pool.Count; i++) {
				pool[i].Value = Math.Abs(selectedIndex - i);
			}
			var order = pool.OrderBy(t => t.Value).Select(i => i.Key).ToList();

			List<Task>[] groups = order.Partition(5);
			foreach(List<Task> item in groups) {
				await Task.WhenAll(item);
			}
		}

		public List<object> GetCurrentList() {
			var result = new List<object>();
			foreach(var item in PhotosListView.Items) {
				if(item is ListViewItem i && i.Content is PhotosListItem li) {
					result.Add(li.Photo);
				}
			}
			return result;
		}

		private void PhotosListView_ItemClick(object sender, ItemClickEventArgs e) {
			if(e.ClickedItem is PhotosListItem item) {
				MainPage.NavigateToPicturePage(item.Photo);
				foreach(PhotosListItem i in Items) {
					i.IsSelected = i == item;
				};
				PutToCenter(item);
			}
		}

		private async void PutToCenter(PhotosListItem item) {
			var found = PhotosListView.Items.Where(i => i is ListViewItem lvi && lvi.Content is PhotosListItem pli && pli == item).FirstOrDefault();
			//var found = PhotosListView.Items.Where(i => i is ListViewItem lvi && lvi == item);
			//await PhotosListView.SmoothScrollIntoViewWithItemAsync(found);

			await Dispatcher.RunAsync(CoreDispatcherPriority.High, () => {
				PhotosListView.ScrollToCenterOfView(found);
			});
		}
	}
}
