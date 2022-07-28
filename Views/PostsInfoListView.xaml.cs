using E621Downloader.Models.Posts;
using E621Downloader.Models;
using E621Downloader.Pages;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using Windows.ApplicationModel.DataTransfer;

namespace E621Downloader.Views {
	public sealed partial class PostsInfoListView: UserControl {

		private readonly ObservableCollection<PostsInfoList> poststInfolists = new();

		public PostsInfoListView() {
			this.InitializeComponent();
		}

		public void Clear() {
			poststInfolists.Clear();
		}

		public void UpdatePostsInfo() {
			
		}

		public void UpdatePostsInfo(PostsTab tab) {
			List<PostInfoLine> deletes = new();
			foreach(Post item in tab.Unsupported) {
				deletes.Add(new PostInfoLine(item.id, "File type".Language() + $": {item.file.ext}"));
			}

			List<PostInfoLine> hots = new();
			int count = 0;
			foreach(KeyValuePair<string, long> item in tab.HotTags) {
				hots.Add(new PostInfoLine(item.Key, $"{item.Value}"));
				if(count++ > 20) {
					break;
				}
			}

			List<PostInfoLine> blacks = new();
			foreach(KeyValuePair<string, long> item in tab.BlackTags) {
				blacks.Add(new PostInfoLine(item.Key, $"{item.Value}"));
			}

			poststInfolists.Clear();
			poststInfolists.Add(new PostsInfoList("Deleted Posts".Language(), deletes));
			poststInfolists.Add(new PostsInfoList("Blacklist".Language(), blacks));
			poststInfolists.Add(new PostsInfoList("Hot Tags (Top 20)".Language(), hots));

			MyPostsInfoListView.SelectedIndex = 0;
		}

		private void PostsInfoListView_ItemClick(object sender, ItemClickEventArgs e) {
			if(MyPostsInfoListView.ContainerFromItem(e.ClickedItem) is ListViewItem item) {
				item.IsSelected = true;
			}
		}

		private void CopyItem_Click(object sender, RoutedEventArgs e) {
			string name = (string)((MenuFlyoutItem)sender).Tag;
			DataPackage dataPackage = new() {
				RequestedOperation = DataPackageOperation.Copy
			};
			dataPackage.SetText($"{name}");
			Clipboard.SetContent(dataPackage);
		}
	}


	public class PostsInfoList: ObservableCollection<PostInfoLine> {
		public string Key { get; set; }
		public PostsInfoList(string key) : base() {
			this.Key = key;
		}
		public PostsInfoList(string key, List<PostInfoLine> content) : base() {
			this.Key = key;
			content.ForEach(s => this.Add(s));
		}
	}

	public struct PostInfoLine {
		public string Name { get; set; }
		public string Detail { get; set; }
		public PostInfoLine(string name, string detail) {
			Name = name;
			Detail = detail;
		}
		public override string ToString() {
			return $"Name ({Name}) - Detail ({Detail})";
		}
	}

}
