using E621Downloader.Models.Download;
using E621Downloader.Models.Locals;
using E621Downloader.Models.Posts;
using E621Downloader.Pages;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

namespace E621Downloader.Views {
	public sealed partial class ImageHolderForSubscriptionPage: UserControl {
		public Post PostRef { get; private set; }
		private readonly SubscriptionPage parent;
		public ImageHolderForSubscriptionPage(SubscriptionPage parent) {
			this.InitializeComponent();
			this.parent = parent;
		}

		public void LoadFromPost(Post post) {
			this.PostRef = post;
			LoadingRing.IsActive = true;
			MyImage.Source = new BitmapImage(new Uri(post.sample.url ?? post.preview.url));
			(MyImage.Source as BitmapImage).ImageOpened += ImageHolderForSubscriptionPage_ImageOpened;
			TypeHint.PostRef = post;
			BottomInfo.PostRef = post;
			ToolTipService.SetToolTip(this, new ToolTipContentForPost(post));
			this.Tapped += (s, e) => {
				if(PostRef == null) {
					return;
				}
				App.postsList.UpdatePostsList(parent.PostsList);
				App.postsList.Current = post;
				MainPage.NavigateToPicturePage(this.PostRef);
			};
			this.RightTapped += ImageHolderForSubscriptionPage_RightTappedForFollowing;
		}

		public async void LoadFromPostID(MixPost mix, CancellationToken? token = null) {
			this.PostRef = await Post.GetPostByIDAsync(token, mix.ID);
			mix.PostRef = this.PostRef;
			if(this.PostRef == null) {
				return;
			}
			MyImage.Source = new BitmapImage(new Uri(this.PostRef.sample.url ?? this.PostRef.preview.url));
			(MyImage.Source as BitmapImage).ImageOpened += ImageHolderForSubscriptionPage_ImageOpened;
			TypeHint.PostRef = this.PostRef;
			BottomInfo.PostRef = this.PostRef;
			ToolTipService.SetToolTip(this, new ToolTipContentForPost(this.PostRef));
			this.Tapped += (s, e) => {
				if(PostRef == null) {
					return;
				}
				App.postsList.UpdatePostsList(parent.PostsList);
				App.postsList.Current = mix;
				MainPage.NavigateToPicturePage(this.PostRef);
			};
			this.RightTapped += ImageHolderForSubscriptionPage_RightTappedForPostID;
		}

		public async void LoadFromLocal(MixPost mix, CancellationToken? token = null) {
			(StorageFile, MetaFile) result = await Local.GetDownloadFile(mix.LocalPath);
			StorageFile file = result.Item1;
			MetaFile meta = result.Item2;
			mix.ImageFile = file;
			mix.MetaFile = meta;
			BitmapImage bitmap = new BitmapImage();
			ThumbnailMode mode = ThumbnailMode.SingleItem;
			if(new string[] { ".webm" }.Contains(file?.FileType)) {
				mode = ThumbnailMode.SingleItem;
			} else if(new string[] { ".jpg", ".png" }.Contains(file?.FileType)) {
				mode = ThumbnailMode.PicturesView;
			}
			using(StorageItemThumbnail thumbnail = await file?.GetThumbnailAsync(mode)) {
				if(thumbnail != null) {
					using(Stream stream = thumbnail.AsStreamForRead()) {
						bitmap.SetSource(stream.AsRandomAccessStream());
					}
				}
			}
			bitmap.ImageOpened += ImageHolderForSubscriptionPage_ImageOpened;
			MyImage.Source = bitmap;
			this.PostRef = meta?.MyPost;
			TypeHint.PostRef = this.PostRef;
			BottomInfo.PostRef = this.PostRef;
			ToolTipService.SetToolTip(this, new ToolTipContentForPost(this.PostRef, true));
			this.Tapped += (s, e) => {
				if(PostRef == null) {
					return;
				}
				App.postsList.UpdatePostsList(parent.PostsList);
				App.postsList.Current = mix;
				MainPage.NavigateToPicturePage(new SubscriptionImageParameter(this.PostRef, file));
			};
			this.RightTapped += ImageHolderForSubscriptionPage_RightTappedForLocal;
		}

		private void ImageHolderForSubscriptionPage_ImageOpened(object sender, RoutedEventArgs e) {
			LoadingRing.IsActive = false;
		}


		private void ImageHolderForSubscriptionPage_RightTappedForLocal(object sender, RightTappedRoutedEventArgs e) {
			MenuFlyout flyout = new MenuFlyout();
			flyout.Items.Add(Item_RemoveFromThis);
			flyout.Items.Add(Item_ManageFavorites);
			flyout.Items.Add(Item_OpenInBrowser);
			flyout.ShowAt(sender as UIElement, e.GetPosition(this));
		}

		private void ImageHolderForSubscriptionPage_RightTappedForPostID(object sender, RightTappedRoutedEventArgs e) {
			MenuFlyout flyout = new MenuFlyout();
			flyout.Items.Add(Item_RemoveFromThis);
			flyout.Items.Add(Item_ManageFavorites);
			flyout.Items.Add(Item_OpenInBrowser);
			flyout.Items.Add(Item_Download);
			flyout.ShowAt(sender as UIElement, e.GetPosition(this));
		}

		private void ImageHolderForSubscriptionPage_RightTappedForFollowing(object sender, RightTappedRoutedEventArgs e) {
			MenuFlyout flyout = new MenuFlyout();
			flyout.Items.Add(Item_ManageFavorites);
			flyout.Items.Add(Item_OpenInBrowser);
			flyout.Items.Add(Item_Download);
			flyout.ShowAt(sender as UIElement, e.GetPosition(this));
		}

		private MenuFlyoutItem Item_RemoveFromThis {
			get {
				MenuFlyoutItem item = new MenuFlyoutItem {
					Icon = new FontIcon() { Glyph = "\uE107" },
					Text = "Remove From This"
				};
				item.Click += (sender, args) => {

				};
				return item;
			}
		}

		private MenuFlyoutItem Item_ManageFavorites {
			get {
				MenuFlyoutItem item = new MenuFlyoutItem {
					Icon = new FontIcon() { Glyph = "\uE912" },
					Text = "Manage Favorites"
				};
				item.Click += (sender, args) => {

				};
				return item;
			}
		}

		private MenuFlyoutItem Item_OpenInBrowser {
			get {
				MenuFlyoutItem item = new MenuFlyoutItem {
					Icon = new FontIcon() { Glyph = "\uE12B" },
					Text = "Open In Browser"
				};
				item.Click += (sender, args) => {

				};
				return item;
			}
		}

		private MenuFlyoutItem Item_Download {
			get {
				MenuFlyoutItem item = new MenuFlyoutItem {
					Icon = new FontIcon() { Glyph = "\uE118" },
					Text = "Download"
				};
				item.Click += async (sender, args) => {
					if(PostRef == null) {
						return;
					}
					await DownloadsManager.RegisterDownload(PostRef);
					MainPage.CreateTip_SuccessDownload(parent);
				};
				return item;
			}
		}

	}

	public class SubscriptionImageParameter: ILocalImage {
		private Post imagePost;
		private StorageFile imageFile;
		public Post ImagePost => imagePost;
		public StorageFile ImageFile => imageFile;
		public SubscriptionImageParameter(Post imagePost, StorageFile imageFile) {
			this.imagePost = imagePost;
			this.imageFile = imageFile;
		}
	}
}
