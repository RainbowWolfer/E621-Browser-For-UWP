using E621Downloader.Models;
using E621Downloader.Models.Download;
using E621Downloader.Models.E621;
using E621Downloader.Models.Locals;
using E621Downloader.Models.Networks;
using E621Downloader.Models.Utilities;
using E621Downloader.Pages;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Threading;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Imaging;

namespace E621Downloader.Views {
	public sealed partial class ImageHolderForSubscriptionPage: UserControl {
		public E621Post PostRef { get; private set; }
		private readonly SubscriptionPage parent;
		private readonly string belongingListName;
		private PathType type;
		private string path;
		private bool isSelected;

		private readonly ProgressLoader progress;

		public Action OnLoaded { get; set; }
		public bool IsImageLoaded { get; private set; } = false;//preview on cloud load & thumbnail on local load

		public bool IsLocal => type == PathType.Local;

		public bool IsSelected {
			get => isSelected;
			set {
				isSelected = value;
				MainGrid.BorderThickness = new Thickness(IsSelected ? 3d : 0);
				parent.UpdateSelectedCountText();
			}
		}

		public ImageHolderForSubscriptionPage(SubscriptionPage parent, string belongingListName = "") {
			this.InitializeComponent();
			this.progress = new ProgressLoader(LoadingRing);
			this.parent = parent;
			this.belongingListName = belongingListName;
			LocalBorder.Translation += new Vector3(0, 0, 8);
		}

		//can only be used in following layout
		public void LoadFromPost(E621Post post, string[] followTags = null) {
			this.PostRef = post;
			LoadingRing.IsActive = true;
			if(App.PostsPool.ContainsKey(post.id)) {
				App.PostsPool[post.id] = post;
			} else {
				App.PostsPool.Add(post.id, post);
			}

			string url = post.sample.url ?? post.preview.url;
			if(string.IsNullOrWhiteSpace(url)) {
				HintText.Visibility = Visibility.Visible;
				LoadingRing.IsActive = false;
			} else {
				MyImage.ImageOpened += ImageHolderForSubscriptionPage_ImageOpened;
				ProcedureLoading(post);
				//MyImage.Source = new BitmapImage(new Uri(post.sample.url ?? post.preview.url));
			}
			TypeHint.PostRef = post;
			TypeHint.Visibility = Visibility.Visible;
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
				if(parent.ImagesSelecting) {
					IsSelected = !IsSelected;
				} else {
					App.PostsList.UpdatePostsList(parent.PostsList);
					App.PostsList.Current = post;
					MainPage.NavigateToPicturePage(
						this.PostRef,
						new string[] { SubscriptionPage.CurrentTag }
					);
				}
			};
			this.RightTapped += ImageHolderForSubscriptionPage_RightTappedForFollowing;
		}

		//used in favorite layout
		public async void LoadFromPostID(MixPost mix, CancellationToken? token = null, Action<E621Post> onPostLoaded = null) {
			LoadingRing.IsActive = true;
			if(App.PostsPool.TryGetValue(mix.ID, out E621Post post)) {
				this.PostRef = post;
			}
			if(this.PostRef == null) {
				this.PostRef = await E621Post.GetPostByIDAsync(mix.ID, token);
				if(this.PostRef != null) {
					App.PostsPool[mix.ID] = this.PostRef;
				}
			}

			mix.PostRef = this.PostRef;
			if(this.PostRef == null) {
				progress.Value = null;
				return;
			}
			onPostLoaded?.Invoke(this.PostRef);
			string url = this.PostRef.sample.url ?? this.PostRef.preview.url;
			if(string.IsNullOrWhiteSpace(url)) {
				HintText.Visibility = Visibility.Visible;
				LoadingRing.IsActive = false;
			} else {
				MyImage.ImageOpened += ImageHolderForSubscriptionPage_ImageOpened;
				ProcedureLoading(mix.PostRef);
				//MyImage.Source = new BitmapImage(new Uri(url));
			}
			TypeHint.PostRef = this.PostRef;
			TypeHint.Visibility = Visibility.Visible;
			BottomInfo.PostRef = this.PostRef;
			type = PathType.PostID;
			path = mix.ID;
			ToolTipService.SetToolTip(this, new ToolTipContentForPost(this.PostRef));
			this.Tapped += (s, e) => {
				if(PostRef == null) {
					return;
				}
				if(parent.ImagesSelecting) {
					IsSelected = !IsSelected;
				} else {
					App.PostsList.UpdatePostsList(parent.PostsList);
					App.PostsList.Current = mix;
					MainPage.NavigateToPicturePage(
						this.PostRef,
						new string[] { SubscriptionPage.CurrentTag }
					);
				}
			};
			this.RightTapped += ImageHolderForSubscriptionPage_RightTappedForPostID;
		}

		//used in favorite layout
		public async void LoadFromLocal(MixPost mix, CancellationToken? token = null, Action<E621Post> onPostLoaded = null) {
			LocalBorder.Visibility = Visibility.Visible;
			(StorageFile file, MetaFile meta) = await Local.GetDownloadFile(mix.LocalPath);
			if(file == null || meta == null) {
				LoadingRing.IsActive = false;
				HintText.Visibility = Visibility.Visible;
				HintText.Text = "Error Loading Local".Language();
				ToolTipService.SetToolTip(HintText, mix.LocalPath);
				return;
			}
			mix.ImageFile = file;
			mix.MetaFile = meta;
			onPostLoaded?.Invoke(meta.MyPost);
			BitmapImage bitmap = new();
			ThumbnailMode mode = ThumbnailMode.SingleItem;
			if(new string[] { ".webm" }.Contains(file?.FileType)) {
				mode = ThumbnailMode.SingleItem;
			} else if(new string[] { ".jpg", ".png" }.Contains(file?.FileType)) {
				mode = ThumbnailMode.PicturesView;
			}
			bitmap.ImageOpened += ImageHolderForSubscriptionPage_ImageOpened;

			using StorageItemThumbnail thumbnail = await file?.GetThumbnailAsync(mode);
			if(thumbnail != null) {
				using Stream stream = thumbnail.AsStreamForRead();
				await bitmap.SetSourceAsync(stream.AsRandomAccessStream());
				IsImageLoaded = true;
				OnLoaded?.Invoke();
			}

			MyImage.Visibility = Visibility.Visible;
			MyImage.Source = bitmap;
			this.PostRef = meta?.MyPost;
			TypeHint.PostRef = this.PostRef;
			TypeHint.Visibility = Visibility.Visible;
			BottomInfo.PostRef = this.PostRef;
			type = PathType.Local;
			path = file?.Path;
			ToolTipService.SetToolTip(this, new ToolTipContentForPost(this.PostRef, true));
			this.Tapped += (s, e) => {
				if(PostRef == null) {
					return;
				}
				if(parent.ImagesSelecting) {
					IsSelected = !IsSelected;
				} else {
					App.PostsList.UpdatePostsList(parent.PostsList);
					App.PostsList.Current = mix;
					MainPage.NavigateToPicturePage(
						new SubscriptionImageParameter(this.PostRef, file),
						new string[] { SubscriptionPage.CurrentTag }
					);
				}
			};
			this.RightTapped += ImageHolderForSubscriptionPage_RightTappedForLocal;
		}

		private void ProcedureLoading(E621Post post) {
			Methods.ProdedureLoading(PreviewImage, MyImage, post, new LoadPoolItemActions() {
				OnUrlsEmpty = () => {
					progress.Value = null;
					HintText.Text = "Error".Language();
					this.Visibility = Visibility.Visible;
				},
				OnSampleUrlEmpty = () => {
					progress.Value = null;
					HintText.Text = "Empty URL".Language();
				},
				OnPreviewStart = () => {
					HintText.Text = "";
				},
				OnPreviewProgress = p => {
					progress.Value = p;
				},
				OnPreviewExists = () => {
					progress.Value = null;
					IsImageLoaded = true;
					OnLoaded?.Invoke();
				},
				OnPreviewOpened = b => {
					progress.Value = null;
					IsImageLoaded = true;
					OnLoaded?.Invoke();
				},
				OnPreviewFailed = () => {
					progress.Value = 0;
				},
				OnSampleStart = b => {

				},
				OnSampleProgress = p => {
					progress.Value = p;
				},
				OnSampleExists = () => {
					progress.Value = null;
				},
				OnSampleOpened = b => {
					progress.Value = null;
				},
				OnSampleFailed = () => {
					progress.Value = null;
					HintText.Text = "Error".Language();
				},
			});
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
			flyout.Items.Add(Item_Download);
			flyout.Items.Add(Item_OpenInBrowser);
			flyout.Items.Add(Item_RemoveFromThis);
			flyout.ShowAt(sender as UIElement, e.GetPosition(this));
		}

		private void ImageHolderForSubscriptionPage_RightTappedForFollowing(object sender, RightTappedRoutedEventArgs e) {
			if(parent.ImagesSelecting) {
				return;
			}
			MenuFlyout flyout = new();
			flyout.Items.Add(Item_ManageFavorites);
			flyout.Items.Add(Item_Download);
			flyout.Items.Add(Item_OpenInBrowser);
			flyout.ShowAt(sender as UIElement, e.GetPosition(this));
		}

		private MenuFlyoutItem Item_RemoveFromThis {
			get {
				MenuFlyoutItem item = new() {
					Icon = new FontIcon() { Glyph = "\uE107" },
					Text = "Remove From This".Language(),
				};
				item.Click += async (sender, args) => {
					if(string.IsNullOrWhiteSpace(belongingListName)) {
						return;
					}
					if(await new ContentDialog() {
						Title = "Confirm".Language(),
						Content = "Are you sure to delete Post".Language() + $" ({PostRef.id})",
						PrimaryButtonText = "Yes".Language(),
						CloseButtonText = "No".Language(),
					}.ShowAsync() == ContentDialogResult.Primary) {
						FavoritesList foundList = FavoritesList.Table.Find(l => l.Name == belongingListName);
						if(foundList != null) {
							foundList.Items.RemoveAll(i => i.Type == type && i.Path == path);
							FavoritesList.Save();
							parent.Refresh();
							parent.UpdateFavoritesTable();
						} else {
							await MainPage.CreatePopupDialog("Error".Language(), "List ({{0}}) not found".Language(belongingListName));
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
					Text = "Manage Favorites".Language(),
				};
				item.Click += async (sender, args) => {
					var dialog = new ContentDialog() {
						Title = "Favorites".Language(),
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
					Text = "Open In Browser".Language(),
				};
				item.Click += async (sender, args) => {
					await Methods.OpenBrowser($"https://{Data.GetHost()}/posts/{PostRef.id}");
				};
				return item;
			}
		}

		private MenuFlyoutItem Item_Download {
			get {
				MenuFlyoutItem item = new() {
					Icon = new FontIcon() { Glyph = "\uE118" },
					Text = "Download".Language(),
				};
				item.Click += async (sender, args) => {
					if(PostRef == null) {
						return;
					}
					if(await DownloadsManager.CheckDownloadAvailableWithDialog()) {
						await DownloadsManager.RegisterDownload(PostRef, SubscriptionPage.DefaultDownloadGroupName);
						MainPage.CreateTip_SuccessDownload(parent);
					}
				};
				return item;
			}
		}

	}

	public class SubscriptionImageParameter: ILocalImage {
		private readonly E621Post imagePost;
		private readonly StorageFile imageFile;
		public E621Post ImagePost => imagePost;
		public StorageFile ImageFile => imageFile;
		public SubscriptionImageParameter(E621Post imagePost, StorageFile imageFile) {
			this.imagePost = imagePost;
			this.imageFile = imageFile;
		}
	}
}
