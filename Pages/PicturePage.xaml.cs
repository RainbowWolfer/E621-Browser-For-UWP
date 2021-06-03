using E621Downloader.Models;
using E621Downloader.Models.Download;
using E621Downloader.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.Core;
using Windows.Networking.BackgroundTransfer;
using Windows.Storage;
using Windows.UI.Input;
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

		public bool isMouseOn;
		public bool isMousePressed;

		private Point pressStartPosition;

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
			if(PostRef == null && PostsBrowser.Instance.posts != null && PostsBrowser.Instance.posts.Count > 0) {
				PostRef = PostsBrowser.Instance.posts[0];
			} else {
				if(e.Parameter == null) {
					MyProgressRing.IsActive = false;
					DebugButton.Visibility = Visibility.Collapsed;
					CopyButton.Visibility = Visibility.Collapsed;
					DownloadButton.Visibility = Visibility.Collapsed;
					return;
				}
				if(PostRef == e.Parameter as Post) {
					return;
				}
				PostRef = e.Parameter as Post;
				if(PostRef == null) {
					return;
				}
			}
			DebugButton.Visibility = Visibility.Visible;
			CopyButton.Visibility = Visibility.Visible;
			DownloadButton.Visibility = Visibility.Visible;
			if(PostRef.file.ext.ToLower().Trim() == "webm") {
				MyProgressRing.IsActive = false;
				MyMediaPlayer.Visibility = Visibility.Visible;
				MyScrollViewer.Visibility = Visibility.Collapsed;
				MyMediaPlayer.Source = MediaSource.CreateFromUri(new Uri(PostRef.file.url));
			} else {
				MyProgressRing.IsActive = true;
				MyMediaPlayer.Visibility = Visibility.Collapsed;
				MyScrollViewer.Visibility = Visibility.Visible;
				MainImage.Source = new BitmapImage(new Uri(PostRef.file.url));
			}

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
			tags.Add(new GroupTagList(title, content));
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


		private void MainImage_PointerWheelChanged(object sender, PointerRoutedEventArgs e) {
			//Debug.WriteLine("PointerWheelChanged");
		}

		private void MainImage_PointerPressed(object sender, PointerRoutedEventArgs e) {
			isMousePressed = true;
			pressStartPosition = e.GetCurrentPoint(sender as UIElement).Position;
		}

		private void MainImage_PointerReleased(object sender, PointerRoutedEventArgs e) {
			isMousePressed = false;
		}

		private void MainImage_PointerMoved(object sender, PointerRoutedEventArgs e) {
			if(isMouseOn && isMousePressed) {
				//PointerPoint pointer = e.GetCurrentPoint(sender as UIElement);
				//Point p = Diff(pointer.Position, pressStartPosition);
				//Debug.WriteLine(p);
				//MyScrollViewer.ChangeView(-p.X, -p.Y, null);
			}
		}

		private void MainImage_PointerEntered(object sender, PointerRoutedEventArgs e) {
			isMouseOn = true;
		}

		private void MainImage_PointerExited(object sender, PointerRoutedEventArgs e) {
			isMouseOn = false;
		}

		private static Point Diff(Point a, Point b) {
			return new Point(a.X - b.X, a.Y - b.Y);
		}

		private void DeleteButton_Loaded(object sender, RoutedEventArgs e) {
			if(!Local.FollowList.Contains((string)(sender as Button).Tag)) {
				(sender as Button).Visibility = Visibility.Collapsed;
			}
		}

		private async void TagsListView_ItemClick(object sender, ItemClickEventArgs e) {
			string tag = e.ClickedItem as string;

			MainPage.SelectNavigationItem(PageTag.Home);
			await Task.Delay(200);

			await PostsBrowser.Instance.LoadAsync(1, tag);
		}

		private void BlackListButton_Tapped(object sender, TappedRoutedEventArgs e) {
			var btn = sender as Button;
			string tag = btn.Tag as string;

		}

		private void FollowListButton_Tapped(object sender, TappedRoutedEventArgs e) {
			var btn = sender as Button;
			string tag = btn.Tag as string;

		}

		private void BlackListButton_Loaded(object sender, RoutedEventArgs e) {
			var btn = sender as Button;
			string tag = btn.Tag as string;
			if(Local.BlackList.Contains(tag)) {
				btn.Content = "\uEA43";
				ToolTipService.SetToolTip(btn, "Remove From BlackList");
			} else {
				btn.Content = "\uF8AB";
				ToolTipService.SetToolTip(btn, "Add To BlackList");
			}
		}

		private void FollowListButton_Loaded(object sender, RoutedEventArgs e) {
			var btn = sender as Button;
			string tag = btn.Tag as string;
			if(Local.FollowList.Contains(tag)) {
				btn.Content = "\uE74D";
				ToolTipService.SetToolTip(btn, "Delete From FollowList");
			} else {
				btn.Content = "\uF8AA";
				ToolTipService.SetToolTip(btn, "Add To FollowList");
			}
		}

		private async void DebugButton_Tapped(object sender, TappedRoutedEventArgs e) {
			if(PostRef == null) {
				return;
			}
			var dialog = new ContentDialog() {
				Title = "Debug Info",
				Content = new PostDebugView(PostRef),
				PrimaryButtonText = "Back",
			};
			await dialog.ShowAsync();
		}

		private void DownloadButton_Tapped(object sender, TappedRoutedEventArgs e) {
			DownloadsManager.RegisterDownload(PostRef);
		}

		private void CopyButton_Tapped(object sender, TappedRoutedEventArgs e) {

		}
	}
	public class GroupTagList: List<string> {
		public string Key { get; set; }
		public GroupTagList(string key) : base() {
			this.Key = key;
		}
		public GroupTagList(string key, List<string> content) : base() {
			this.Key = key;
			content.ForEach(s => this.Add(s));
		}
	}

}
