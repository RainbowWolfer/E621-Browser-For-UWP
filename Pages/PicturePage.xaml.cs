using E621Downloader.Models;
using E621Downloader.Models.Download;
using E621Downloader.Models.Locals;
using E621Downloader.Models.Posts;
using E621Downloader.Pages.LibrarySection;
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
using Windows.Storage.Streams;
using Windows.System;
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
		public readonly ObservableCollection<E621Comment> comments;

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
			comments = new ObservableCollection<E621Comment>();
			this.DataContextChanged += (s, c) => Bindings.Update();
			MyMediaPlayer.MediaPlayer.IsLoopingEnabled = true;
		}

		protected async override void OnNavigatedTo(NavigationEventArgs e) {
			base.OnNavigatedTo(e);
			object p = e.Parameter;
			if(p == null && PostRef == null && PostsBrowser.Instance != null && PostsBrowser.Instance.posts != null && PostsBrowser.Instance.posts.Count > 0) {
				p = PostsBrowser.Instance.posts[0];
			}
			if(p is Post post) {
				if(PostRef == post) {
					return;
				}
				PostRef = post;
				DebugButton.Visibility = Visibility.Visible;
				CopyButton.Visibility = Visibility.Visible;
				DownloadButton.Visibility = Visibility.Visible;
				DownloadButton.Content = "Download";
				DownloadButton.IsEnabled = true;
				if(PostRef.file.ext.ToLower().Trim() == "webm") {
					MyProgressRing.IsActive = false;
					MyMediaPlayer.Visibility = Visibility.Visible;
					MyScrollViewer.Visibility = Visibility.Collapsed;
					if(!string.IsNullOrEmpty(PostRef.file.url)) {
						MyMediaPlayer.Source = MediaSource.CreateFromUri(new Uri(PostRef.file.url));
					}
				} else {
					MyProgressRing.IsActive = true;
					MyMediaPlayer.Visibility = Visibility.Collapsed;
					MyScrollViewer.Visibility = Visibility.Visible;
					MainImage.Source = new BitmapImage(new Uri(PostRef.file.url));
					MyMediaPlayer.MediaPlayer.Source = null;
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
			} else if(p is ItemBlock itemBlock) {
				if(PostRef == itemBlock.meta.MyPost) {
					return;
				}
				PostRef = itemBlock.meta.MyPost;
				DebugButton.Visibility = Visibility.Visible;
				CopyButton.Visibility = Visibility.Visible;
				DownloadButton.Visibility = Visibility.Visible;
				DownloadButton.Content = "Local";
				DownloadButton.IsEnabled = false;
				if(PostRef.file.ext.ToLower().Trim() == "webm") {
					MyProgressRing.IsActive = false;
					MyMediaPlayer.Visibility = Visibility.Visible;
					MyScrollViewer.Visibility = Visibility.Collapsed;

					MyMediaPlayer.Source = MediaSource.CreateFromStorageFile(itemBlock.imageFile);
				} else {
					MyProgressRing.IsActive = true;
					MyMediaPlayer.Visibility = Visibility.Collapsed;
					MyScrollViewer.Visibility = Visibility.Visible;

					using(IRandomAccessStream randomAccessStream = await itemBlock.imageFile.OpenAsync(FileAccessMode.Read)) {
						BitmapImage result = new BitmapImage();
						await result.SetSourceAsync(randomAccessStream);
						MainImage.Source = result;
					}
					MyProgressRing.IsActive = false;
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
			} else if(p == null) {
				MyProgressRing.IsActive = false;
				DebugButton.Visibility = Visibility.Collapsed;
				CopyButton.Visibility = Visibility.Collapsed;
				DownloadButton.Visibility = Visibility.Collapsed;
			}
			LoadCommentsAsync();
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

		private async void LoadCommentsAsync() {
			LoadingSection.Visibility = Visibility.Visible;
			foreach(E621Comment item in await E621Comment.GetAsync(PostRef.id)) {
				comments.Add(item);
			}
			LoadingSection.Visibility = Visibility.Collapsed;
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

		private async void BrowserButton_Tapped(object sender, TappedRoutedEventArgs e) {
			if(PostRef == null) {
				return;
			}
			bool success = await Launcher.LaunchUriAsync(new Uri($"https://e621.net/posts/{PostRef.id}"));
		}

		private void MoreInfoButton_Tapped(object sender, TappedRoutedEventArgs e) {
			MainSplitView.IsPaneOpen = !MainSplitView.IsPaneOpen;
		}

		private async void InfoButton_Tapped(object sender, TappedRoutedEventArgs e) {
			string tag = (sender as Button).Tag as string;
			E621Tag[] e621tags = E621Tag.Get(tag);
			string name = tag;
			string count = "0";
			string description = "not found";
			if(e621tags != null & e621tags.Length > 0) {
				count = e621tags[0].post_count.ToString();
				E621Wiki[] e621wikies = E621Wiki.Get(tag);
				if(e621wikies != null && e621wikies.Length > 0) {
					description = e621wikies[0].body;
				}
			}
			await new ContentDialog() {
				Title = $"Tag Information: {name}",
				Content = $"Count: {count}\nDescription: {description}",
				CloseButtonText = "Back",
			}.ShowAsync();
		}
	}
	public class GroupTagList: ObservableCollection<string> {
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
