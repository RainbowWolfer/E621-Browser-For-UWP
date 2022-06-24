using E621Downloader.Models.Download;
using E621Downloader.Models.Locals;
using E621Downloader.Models.Networks;
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
using Windows.System;
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
		private readonly string belongingListName;
		private PathType type;
		private string path;
		public ImageHolderForSubscriptionPage(SubscriptionPage parent, string belongingListName = "") {
			this.InitializeComponent();
			this.parent = parent;
			this.belongingListName = belongingListName;
		}

		public void LoadFromPost(Post post, string[] followTags = null) {
			this.PostRef = post;
			LoadingRing.IsActive = true;
			string url = post.sample.url ?? post.preview.url;
			if(string.IsNullOrWhiteSpace(url)) {
				HintText.Visibility = Visibility.Visible;
				LoadingRing.IsActive = false;
			} else {
				MyImage.Source = new BitmapImage(new Uri(post.sample.url ?? post.preview.url));
				(MyImage.Source as BitmapImage).ImageOpened += ImageHolderForSubscriptionPage_ImageOpened;
			}
			TypeHint.PostRef = post;
			BottomInfo.PostRef = post;
			type = PathType.PostID;
			path = post.id;
			List<string> relatives = new();
			if(followTags != null && followTags.Length > 0) {
				foreach(string followTag in followTags) {
					foreach(string tag in post.tags.GetAllTags()) {
						if(tag == followTag) {
							relatives.Add(tag);
						}
					}
				}
			}
			ToolTipService.SetToolTip(this, new ToolTipContentForPost(post) {
				RelativeTags = relatives.ToArray(),
			});
			this.Tapped += (s, e) => {
				if(PostRef == null) {
					return;
				}
				App.PostsList.UpdatePostsList(parent.PostsList);
				App.PostsList.Current = post;
				MainPage.NavigateToPicturePage(this.PostRef, new string[] { SubscriptionPage.CurrentTag });
			};
			this.RightTapped += ImageHolderForSubscriptionPage_RightTappedForFollowing;
		}

		public async void LoadFromPostID(MixPost mix, CancellationToken? token = null) {
			LoadingRing.IsActive = true;
			this.PostRef = await Post.GetPostByIDAsync(token, mix.ID);
			mix.PostRef = this.PostRef;
			if(this.PostRef == null) {
				return;
			}
			string url = this.PostRef.sample.url ?? this.PostRef.preview.url;
			if(string.IsNullOrWhiteSpace(url)) {
				HintText.Visibility = Visibility.Visible;
				LoadingRing.IsActive = false;
			} else {
				MyImage.Source = new BitmapImage(new Uri(url));
				(MyImage.Source as BitmapImage).ImageOpened += ImageHolderForSubscriptionPage_ImageOpened;
			}
			TypeHint.PostRef = this.PostRef;
			BottomInfo.PostRef = this.PostRef;
			type = PathType.PostID;
			path = mix.ID;
			ToolTipService.SetToolTip(this, new ToolTipContentForPost(this.PostRef));
			this.Tapped += (s, e) => {
				if(PostRef == null) {
					return;
				}
				App.PostsList.UpdatePostsList(parent.PostsList);
				App.PostsList.Current = mix;
				MainPage.NavigateToPicturePage(this.PostRef, new string[] { SubscriptionPage.CurrentTag });
			};
			this.RightTapped += ImageHolderForSubscriptionPage_RightTappedForPostID;
		}

		public async void LoadFromLocal(MixPost mix, CancellationToken? token = null) {
			(StorageFile, MetaFile) result = await Local.GetDownloadFile(mix.LocalPath);
			StorageFile file = result.Item1;
			MetaFile meta = result.Item2;
			mix.ImageFile = file;
			mix.MetaFile = meta;
			BitmapImage bitmap = new();
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
			type = PathType.Local;
			path = file?.Path;
			ToolTipService.SetToolTip(this, new ToolTipContentForPost(this.PostRef, true));
			this.Tapped += (s, e) => {
				if(PostRef == null) {
					return;
				}
				App.PostsList.UpdatePostsList(parent.PostsList);
				App.PostsList.Current = mix;
				MainPage.NavigateToPicturePage(
					new SubscriptionImageParameter(this.PostRef, file),
					new string[] { SubscriptionPage.CurrentTag }
				);
			};
			this.RightTapped += ImageHolderForSubscriptionPage_RightTappedForLocal;
		}

		private void ImageHolderForSubscriptionPage_ImageOpened(object sender, RoutedEventArgs e) {
			LoadingRing.IsActive = false;
		}


		private void ImageHolderForSubscriptionPage_RightTappedForLocal(object sender, RightTappedRoutedEventArgs e) {
			MenuFlyout flyout = new();
			flyout.Items.Add(Item_ManageFavorites);
			flyout.Items.Add(Item_OpenInBrowser);
			flyout.Items.Add(Item_RemoveFromThis);
			flyout.ShowAt(sender as UIElement, e.GetPosition(this));
		}

		private void ImageHolderForSubscriptionPage_RightTappedForPostID(object sender, RightTappedRoutedEventArgs e) {
			MenuFlyout flyout = new();
			flyout.Items.Add(Item_ManageFavorites);
			flyout.Items.Add(Item_OpenInBrowser);
			flyout.Items.Add(Item_Download);
			flyout.Items.Add(Item_RemoveFromThis);
			flyout.ShowAt(sender as UIElement, e.GetPosition(this));
		}

		private void ImageHolderForSubscriptionPage_RightTappedForFollowing(object sender, RightTappedRoutedEventArgs e) {
			MenuFlyout flyout = new();
			flyout.Items.Add(Item_ManageFavorites);
			flyout.Items.Add(Item_OpenInBrowser);
			flyout.Items.Add(Item_Download);
			flyout.ShowAt(sender as UIElement, e.GetPosition(this));
		}

		private MenuFlyoutItem Item_RemoveFromThis {
			get {
				MenuFlyoutItem item = new() {
					Icon = new FontIcon() { Glyph = "\uE107" },
					Text = "Remove From This"
				};
				item.Click += async (sender, args) => {
					if(string.IsNullOrWhiteSpace(belongingListName)) {
						return;
					}
					if(await new ContentDialog() {
						Title = "Confirmation",
						Content = $"Are you sure to delete Post ({PostRef.id})",
						PrimaryButtonText = "Yes",
						CloseButtonText = "No",
					}.ShowAsync() == ContentDialogResult.Primary) {
						FavoritesList foundList = FavoritesList.Table.Find(l => l.Name == belongingListName);
						if(foundList != null) {
							foundList.Items.RemoveAll(i => i.Type == type && i.Path == path);
							FavoritesList.Save();
							parent.Refresh();
							parent.UpdateFavoritesTable();
						} else {
							await MainPage.CreatePopupDialog("Error", $"List ({belongingListName}) not found.");
						}
					}
				};
				return item;
			}
		}

		private MenuFlyoutItem Item_ManageFavorites {
			get {
				MenuFlyoutItem item = new() {
					Icon = new FontIcon() { Glyph = "\uE912" },
					Text = "Manage Favorites"
				};
				item.Click += async (sender, args) => {
					var dialog = new ContentDialog() {
						Title = "Favorites",
					};
					var list = new PersonalFavoritesList(dialog, type, path) {
						Width = 300,
						ShowBackButton = true,
						ShowE621FavoriteButton = true,
						IsInitialFavorited = this.PostRef.is_favorited,
					};
					dialog.Content = list;
					await dialog.ShowAsync();
				};
				return item;
			}
		}

		private MenuFlyoutItem Item_OpenInBrowser {
			get {
				MenuFlyoutItem item = new() {
					Icon = new FontIcon() { Glyph = "\uE12B" },
					Text = "Open In Browser"
				};
				item.Click += async (sender, args) => {
					if(!await Launcher.LaunchUriAsync(new Uri($"https://{Data.GetHost()}/posts/{PostRef.id}"))) {
						await MainPage.CreatePopupDialog("Error", "Could not Open Default Browser");
					}
				};
				return item;
			}
		}

		private MenuFlyoutItem Item_Download {
			get {
				MenuFlyoutItem item = new() {
					Icon = new FontIcon() { Glyph = "\uE118" },
					Text = "Download"
				};
				item.Click += async (sender, args) => {
					if(PostRef == null) {
						return;
					}
					if(await DownloadsManager.CheckDownloadAvailableWithDialog()) {
						await DownloadsManager.RegisterDownload(PostRef);
						MainPage.CreateTip_SuccessDownload(parent);
					}
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
