using E621Downloader.Models;
using E621Downloader.Models.Download;
using E621Downloader.Models.Locals;
using E621Downloader.Models.Posts;
using E621Downloader.Pages.LibrarySection;
using E621Downloader.Views;
using E621Downloader.Views.CommentsSection;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.Core;
using Windows.Networking.BackgroundTransfer;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.System;
using Windows.UI;
using Windows.UI.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

namespace E621Downloader.Pages {
	public sealed partial class PicturePage: Page {
		public Post PostRef { get; private set; }
		public readonly ObservableCollection<GroupTagList> tags;
		public readonly List<E621Comment> comments;

		private bool commentsLoading;
		private bool commentsLoaded;

		public bool isMouseOn;
		public bool isMousePressed;

		private Point pressStartPosition;

		public string Title => PostRef == null ? "# No Post" :
			$"#{PostRef.id} ({PostRef.rating.ToUpper()})";

		public PicturePage() {
			this.InitializeComponent();
			this.NavigationCacheMode = NavigationCacheMode.Enabled;
			tags = new ObservableCollection<GroupTagList>();
			comments = new List<E621Comment>();
			this.DataContextChanged += (s, c) => Bindings.Update();
			MyMediaPlayer.MediaPlayer.IsLoopingEnabled = true;
		}

		protected async override void OnNavigatedTo(NavigationEventArgs e) {
			base.OnNavigatedTo(e);
			object p = e.Parameter;
			if(p == null && PostRef == null && PostsBrowser.Instance != null && PostsBrowser.Instance.Posts != null && PostsBrowser.Instance.Posts.Count > 0) {
				p = PostsBrowser.Instance.Posts[0];
			}
			if(p is Post post) {
				if(PostRef == post) {
					UpdateTagsGroup(PostRef.tags);
					return;
				}
				PostRef = post;
				//DebugButton.Visibility = Visibility.Visible;
				//CopyButton.Visibility = Visibility.Visible;
				DownloadButton.Visibility = Visibility.Visible;
				DownloadText.Text = "Download";
				DownloadIcon.Glyph = "\uE118";
				DownloadButton.IsEnabled = true;
				string type = PostRef.file.ext.ToLower().Trim();
				if(type == "webm") {
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
				UpdateTagsGroup(PostRef.tags);
			} else if(p is ItemBlock itemBlock) {
				if(PostRef == itemBlock.meta.MyPost) {
					UpdateTagsGroup(PostRef.tags);
					return;
				}
				PostRef = itemBlock.meta.MyPost;
				//DebugButton.Visibility = Visibility.Visible;
				//CopyButton.Visibility = Visibility.Visible;
				DownloadButton.Visibility = Visibility.Visible;
				DownloadText.Text = "Local";
				DownloadIcon.Glyph = "\uE159";
				DownloadButton.IsEnabled = false;
				string type = PostRef.file.ext.ToLower().Trim();
				if(type == "webm") {
					MyProgressRing.IsActive = false;
					MyMediaPlayer.Visibility = Visibility.Visible;
					MyScrollViewer.Visibility = Visibility.Collapsed;

					MyMediaPlayer.Source = MediaSource.CreateFromStorageFile(itemBlock.imageFile);
				} else if(type == "anim") {
					MyMediaPlayer.Visibility = Visibility.Collapsed;
					MyScrollViewer.Visibility = Visibility.Collapsed;
					MyProgressRing.IsActive = false;
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

				UpdateTagsGroup(PostRef.tags);
			} else if(p == null) {
				MyProgressRing.IsActive = false;
				//DebugButton.Visibility = Visibility.Collapsed;
				//CopyButton.Visibility = Visibility.Collapsed;
				DownloadButton.Visibility = Visibility.Collapsed;
			}
			TitleText.Text = Title;
			UpdateRatingIcon();
			DescriptionText.Text = PostRef != null && !string.IsNullOrEmpty(PostRef.description) ? PostRef.description : "No Description";
			MainSplitView.IsPaneOpen = false;
			InformationPivot.SelectedIndex = 0;
			commentsLoaded = false;
			commentsLoading = false;
			comments.Clear();
			CommentsListView.Items.Clear();
		}

		private void UpdateRatingIcon() {
			if(PostRef == null) {
				return;
			}
			//RatingPanel.Visibility = Visibility.Visible;
			//Color color;
			//switch(PostRef.rating) {
			//	case "s":
			//		RatingIcon.Glyph = "\uF78C";
			//		RatingText.Text = "Safe";
			//		ToolTipService.SetToolTip(RatingPanel, "Rating: Safe");
			//		color = Colors.Green;
			//		break;
			//	case "q":
			//		RatingIcon.Glyph = "\uF142";
			//		RatingText.Text = "Qestionable";
			//		ToolTipService.SetToolTip(RatingPanel, "Rating: Questionable");
			//		color = Colors.Yellow;
			//		break;
			//	case "e":
			//		RatingIcon.Glyph = "\uE814";
			//		RatingText.Text = "Explicit";
			//		ToolTipService.SetToolTip(RatingPanel, "Rating: Explicit");
			//		color = Colors.Red;
			//		break;
			//	default:
			//		RatingPanel.Visibility = Visibility.Collapsed;
			//		color = Colors.White;
			//		break;
			//}
			//RatingIcon.Foreground = new SolidColorBrush(color);
			//RatingText.Foreground = new SolidColorBrush(color);
		}

		private void UpdateTagsGroup(Tags tags) {
			RemoveGroup();
			AddNewGroup("Artist", tags.artist);
			AddNewGroup("Copyright", tags.copyright);
			AddNewGroup("Species", tags.species);
			AddNewGroup("General", tags.general);
			AddNewGroup("Character", tags.character);
			AddNewGroup("Meta", tags.meta);
			AddNewGroup("Invalid", tags.invalid);
			AddNewGroup("Lore", tags.lore);
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
			comments.Clear();
			CommentsListView.Items.Clear();
			commentsLoaded = false;
			commentsLoading = true;
			LoadingSection.Visibility = Visibility.Visible;
			CommentsHint.Visibility = Visibility.Collapsed;
			E621Comment[] list = await E621Comment.GetAsync(PostRef.id);
			if(list != null && list.Length > 0) {
				foreach(E621Comment item in list) {
					comments.Add(item);
					CommentsListView.Items.Add(new CommentView(item));
				}
			} else {
				CommentsHint.Visibility = Visibility.Visible;
			}
			LoadingSection.Visibility = Visibility.Collapsed;
			commentsLoading = false;
			commentsLoaded = true;
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

		//private void DeleteButton_Loaded(object sender, RoutedEventArgs e) {
		//	if(!Local.FollowList.Contains((string)(sender as Button).Tag)) {
		//		(sender as Button).Visibility = Visibility.Collapsed;
		//	}
		//}

		private async void TagsListView_ItemClick(object sender, ItemClickEventArgs e) {
			string tag = e.ClickedItem as string;

			MainPage.SelectNavigationItem(PageTag.PostsBrowser);
			await Task.Delay(200);

			await PostsBrowser.Instance.LoadAsync(1, tag);
		}

		private void BlackListButton_Tapped(object sender, TappedRoutedEventArgs e) {
			var btn = sender as Button;
			string tag = btn.Tag as string;
			if(Local.CheckBlackList(tag)) {
				Local.RemoveBlackList(tag);
				btn.Content = "\uF8AB";
				ToolTipService.SetToolTip(btn, "Add To BlackList");
			} else {
				Local.AddBlackList(tag);
				btn.Content = "\uEA43";
				ToolTipService.SetToolTip(btn, "Remove From BlackList");

				if(Local.CheckFollowList(tag)) {
					Local.RemoveFollowList(tag);
					var followListButton = (btn.Parent as RelativePanel).Children.OfType<Button>().ToList().Find(b => b.Name == "FollowListButton");
					followListButton.Content = "\uF8AA";
					ToolTipService.SetToolTip(followListButton, "Add To FollowList");
				}
			}
		}

		private void FollowListButton_Tapped(object sender, TappedRoutedEventArgs e) {
			var btn = sender as Button;
			string tag = btn.Tag as string;
			if(Local.CheckFollowList(tag)) {
				Local.RemoveFollowList(tag);
				btn.Content = "\uF8AA";
				ToolTipService.SetToolTip(btn, "Add To FollowList");
			} else {
				Local.AddFollowList(tag);
				btn.Content = "\uE74D";
				ToolTipService.SetToolTip(btn, "Delete From FollowList");

				if(Local.CheckBlackList(tag)) {
					Local.RemoveBlackList(tag);
					var blackListButton = (btn.Parent as RelativePanel).Children.OfType<Button>().ToList().Find(b => b.Name == "BlackListButton");
					blackListButton.Content = "\uF8AB";
					ToolTipService.SetToolTip(blackListButton, "Add To BlackList");
				}
			}
		}

		private void BlackListButton_Loaded(object sender, RoutedEventArgs e) {
			var btn = sender as Button;
			string tag = btn.Tag as string;
			if(Local.CheckBlackList(tag)) {
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
			if(Local.CheckFollowList(tag)) {
				btn.Content = "\uE74D";
				ToolTipService.SetToolTip(btn, "Delete From FollowList");
			} else {
				btn.Content = "\uF8AA";
				ToolTipService.SetToolTip(btn, "Add To FollowList");
			}
		}

		private void DownloadButton_Tapped(object sender, TappedRoutedEventArgs e) {
			DownloadsManager.RegisterDownload(PostRef);
			MainPage.CreateTip(this, "Notification", "Download Successfully Began", Symbol.Accept);
		}

		private void MoreInfoButton_Tapped(object sender, TappedRoutedEventArgs e) {
			MainSplitView.IsPaneOpen = !MainSplitView.IsPaneOpen;
		}

		private async void InfoButton_Tapped(object sender, TappedRoutedEventArgs e) {
			string tag = (sender as Button).Tag as string;
			string name = tag;
			var dialog = new ContentDialog() {
				Title = $"Tag Information: {name}",
				CloseButtonText = "Back",
			};
			dialog.Content = new TagInformationDisplay(dialog, tag);
			await dialog.ShowAsync();

			//E621Tag[] e621tags = E621Tag.Get(tag);
			//string count = "0";
			//string description = "not found";
			//if(e621tags != null && e621tags.Length > 0) {
			//	count = e621tags[0].post_count.ToString();
			//	E621Wiki[] e621wikies = E621Wiki.Get(tag);
			//	if(e621wikies != null && e621wikies.Length > 0) {
			//		description = e621wikies[0].body;
			//	}
			//}
			//await new ContentDialog() {
			//	Title = $"Tag Information: {name}",
			//	Content = $"Count: {count}\nDescription: {description}",
			//	CloseButtonText = "Back",
			//}.ShowAsync();
		}

		private void SplitViewModeSwitch_Toggled(object sender, RoutedEventArgs e) {
			if(MainSplitView == null) {
				return;
			}
			MainSplitView.DisplayMode = SplitViewModeSwitch.IsOn ? SplitViewDisplayMode.Overlay : SplitViewDisplayMode.Inline;
		}

		private void LeftButton_PointerEntered(object sender, PointerRoutedEventArgs e) {
			(sender as Button).Opacity = 1;
		}

		private void LeftButton_PointerExited(object sender, PointerRoutedEventArgs e) {
			(sender as Button).Opacity = 0.2;
		}

		private void LeftButton_Tapped(object sender, TappedRoutedEventArgs e) {
			MainPage.Instance.parameter_picture = App.postsList.GoLeft();
			MainPage.NavigateToPicturePage();
		}

		private void RightButton_PointerEntered(object sender, PointerRoutedEventArgs e) {
			(sender as Button).Opacity = 1;
		}

		private void RightButton_PointerExited(object sender, PointerRoutedEventArgs e) {
			(sender as Button).Opacity = 0.2;
		}

		private void RightButton_Tapped(object sender, TappedRoutedEventArgs e) {
			MainPage.Instance.parameter_picture = App.postsList.GoRight();
			MainPage.NavigateToPicturePage();
		}

		private void InformationPivot_SelectionChanged(object sender, SelectionChangedEventArgs e) {
			if(e.AddedItems != null && e.AddedItems.Count > 0 && e.AddedItems[0] is PivotItem item && item.Header as string == "Comments" && !commentsLoaded && !commentsLoading) {
				if(PostRef != null) {
					LoadCommentsAsync();
				} else {
					CommentsHint.Visibility = Visibility.Visible;
				}
			}
		}

		private async void BrowserItem_Click(object sender, RoutedEventArgs e) {
			if(PostRef == null) {
				return;
			}
			if(!await Launcher.LaunchUriAsync(new Uri($"https://e621.net/posts/{PostRef.id}"))) {
				await MainPage.CreatePopupDialog("Error", "Could not Open Default Browser");
			}
		}

		private void CopyItem_Click(object sender, RoutedEventArgs e) {
			if(PostRef == null) {
				return;
			}
			var dataPackage = new DataPackage() {
				RequestedOperation = DataPackageOperation.Copy
			};
			dataPackage.SetText($"{PostRef.id}");
			Clipboard.SetContent(dataPackage);
		}

		private async void DebugItem_Click(object sender, RoutedEventArgs e) {
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

		private void FavoriteButton_Click(object sender, RoutedEventArgs e) {
			if(FavoriteButton.IsChecked.Value) {
				FavoriteText.Text = "Favorited";
				FavoriteIcon.Glyph = "\uEB52";
			} else {
				FavoriteText.Text = "Favorite";
				FavoriteIcon.Glyph = "\uEB51";
			}
		}

		private void ToggleTagsButton_Tapped(object sender, TappedRoutedEventArgs e) {
			double from = TagsListView.Width;
			double to;
			if(TagsListView.Width <= 125) {
				to = 250;
				ToggleTagsButtonIcon.Glyph = "\uE8A0";
			} else {
				to = 0;
				ToggleTagsButtonIcon.Glyph = "\uE89F";
			}
			TagsDisplay.Children[0].SetValue(DoubleAnimation.FromProperty, from);
			TagsDisplay.Children[0].SetValue(DoubleAnimation.ToProperty, to);
			TagsDisplay.Begin();
		}

		private void CopyItem_Tapped(object sender, TappedRoutedEventArgs e) {

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
