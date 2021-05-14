using E621Downloader.Models;
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
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

namespace E621Downloader.Pages {
	public sealed partial class PicturePage: Page {
		public Post PostRef { get; private set; }
		public readonly ObservableCollection<GroupTagList> tags;

		public string Title {
			get {
				if(PostRef == null) {
					return "No Post Were Selected.";
				} else {
					return string.Format("Posts: {0}  UpVote: {1}  DownVote: {2}", PostRef.id, PostRef.score.up, PostRef.score.down);
				}
			}
		}

		public PicturePage() {
			this.InitializeComponent();
			this.NavigationCacheMode = NavigationCacheMode.Enabled;
			tags = new ObservableCollection<GroupTagList>();
			this.DataContextChanged += (s, c) => Bindings.Update();
		}
		protected override void OnNavigatedTo(NavigationEventArgs e) {
			base.OnNavigatedTo(e);
			if(e.Parameter == null) {
				MyProgressRing.IsActive = false;
				return;
			}
			MyProgressRing.IsActive = true;
			PostRef = e.Parameter as Post;
			if(PostRef == null) {
				return;
			}

			MainImage.Source = new BitmapImage(new Uri(PostRef.file.url));

			TagsListView.ScrollIntoView(TagsListView.Items[0]);

			RemoveGroup();
			AddNewGroup("Artist", PostRef.tags.artist);
			AddNewGroup("Copyright", PostRef.tags.copyright);
			AddNewGroup("Species", PostRef.tags.species);
			AddNewGroup("General", PostRef.tags.general);
			AddNewGroup("Character", PostRef.tags.character);
			AddNewGroup("Meta", PostRef.tags.meta);
			AddNewGroup("Invalid", PostRef.tags.invalid);
			AddNewGroup("Lore", PostRef.tags.lore);
		}

		private void RemoveGroup() {
			tags.Clear();
		}

		private void AddNewGroup(string title, List<string> content) {
			if(content == null) {
				return;
			}
			if(content.Count == 0) {
				return;
			}
			var list = new GroupTagList() {
				Key = title
			};
			foreach(string item in content) {
				list.Add(item);
			}
			tags.Add(list);
		}

		private void MainImage_ImageOpened(object sender, RoutedEventArgs e) {
			MyProgressRing.IsActive = false;
		}

		private void MyScrollViewer_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e) {
			//var scrollViewer = sender as ScrollViewer;
			//if(scrollViewer.ZoomFactor != 1) {
			//	scrollViewer.ChangeView(scrollViewer.ActualWidth / 2, scrollViewer.ActualHeight / 2, 1);
			//} else if(scrollViewer.ZoomFactor == 1) {
			//	scrollViewer.ChangeView(scrollViewer.ActualWidth / 2, scrollViewer.ActualHeight / 2, 2);
			//}
		}
	}
	public class GroupTagList: List<string> {
		public string Key { get; set; }
	}

}
