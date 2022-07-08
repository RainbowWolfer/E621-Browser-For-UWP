﻿using E621Downloader.Models;
using E621Downloader.Models.Posts;
using E621Downloader.Models.Services;
using Microsoft.Toolkit.Uwp.UI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
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
using Windows.UI.Xaml.Navigation;

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
			foreach(var obj in objs) {
				var item = new PhotosListItem();
				item.SetPhoto(obj);
				item.IsSelected = obj == current;
				c = item;
				PhotosListView.Items.Add(new ListViewItem() {
					Content = item,
					Margin = new Thickness(0),
					Padding = new Thickness(0),
				});
				Items.Add(item);
			}
			if(c != null) {
				PutToCenter(c);
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

		private void PutToCenter(PhotosListItem item) {
			var found = PhotosListView.Items.Where(i => i is ListViewItem lvi && lvi.Content is PhotosListItem pli && pli == item).FirstOrDefault();
			//await PhotosListView.SmoothScrollIntoViewWithItemAsync(found);
			PhotosListView.ScrollToCenterOfView(found);
		}
	}
}
