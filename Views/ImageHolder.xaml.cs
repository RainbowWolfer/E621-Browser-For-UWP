using E621Downloader.Models;
using E621Downloader.Models.Download;
using E621Downloader.Models.Locals;
using E621Downloader.Models.Posts;
using E621Downloader.Pages;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Diagnostics;
using System.Threading;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Imaging;
using static System.Net.Mime.MediaTypeNames;

namespace E621Downloader.Views {
	public delegate void OnImageLoadedEventHandler(BitmapImage bitmap);
	public sealed partial class ImageHolder: UserControl {
		public string[] BelongedTags { get; set; }
		public Post PostRef { get; private set; }
		public int Index { get; private set; }

		public event OnImageLoadedEventHandler OnImagedLoaded;

		private int _spanCol;
		private int _spanRow;
		public int SpanCol {
			get => _spanCol;
			set {
				_spanCol = Math.Clamp(value, 5, 15);
				VariableSizedWrapGrid.SetColumnSpan(this, _spanCol);
			}
		}
		public int SpanRow {
			get => _spanRow;
			set {
				_spanRow = Math.Clamp(value, 4, 15);
				VariableSizedWrapGrid.SetRowSpan(this, _spanRow);
			}
		}
		public string PrevieweUrl => PostRef.preview.url;
		public string SampleUrl => PostRef.sample.url;

		private bool _isSelected;
		public bool IsSelected {
			get => _isSelected;
			set {
				_isSelected = value;
				BorderGrid.BorderThickness = new Thickness(value ? 4 : 0);
			}
		}

		public int? Progress {
			get => progress;
			private set {
				progress = value;
				if(value == null) {
					MyProgressRing.Visibility = Visibility.Collapsed;
				} else if(0 < value && value <= 100) {
					MyProgressRing.Visibility = Visibility.Visible;
					MyProgressRing.IsIndeterminate = false;
					MyProgressRing.Value = (double)value;
				} else {
					MyProgressRing.Visibility = Visibility.Visible;
					MyProgressRing.IsIndeterminate = true;
				}
			}
		}

		public BitmapImage Image { get; private set; }
		private readonly PathType type;
		private readonly string path;
		private readonly Page page;

		//private bool isLoaded;

		private ContentDialog dialog_setAs;
		private LoadingDialog dialog_setAs_content;
		private CancellationTokenSource cts_SetAs;
		private int? progress;
		private readonly LoadPoolItem loader;

		public ImageHolder(Page page, Post post, int index, PathType type, string path) {
			this.InitializeComponent();
			this.page = page;
			this.PostRef = post;
			this.Index = index;
			this.type = type;
			this.path = path;
			this.loader = LoadPool.SetNew(post);
			PreviewyImage.Source = null;
			SampleImage.Source = null;
			if(!string.IsNullOrWhiteSpace(PrevieweUrl) || !string.IsNullOrWhiteSpace(SampleUrl)) {
				bool previewLoaded = false;
				if(!string.IsNullOrWhiteSpace(PrevieweUrl)) {
					LoadPreview();
				} else {
					LoadSample();
				}

				void LoadPreview() {
					if(string.IsNullOrWhiteSpace(PrevieweUrl)) {
						return;
					}
					LoadingPanel.Visibility = Visibility.Visible;
					FailureTextBlock.Text = "";
					PreviewyImage.ImageFailed += (s, e) => {
						Progress = 0;
						LoadSample();
					};
					PreviewyImage.ImageOpened += (s, e) => {
						var bitmap = PreviewyImage.Source as BitmapImage;
						previewLoaded = true;
						Progress = null;
						loader.Preview = bitmap;
						LoadSample();
						OnImagedLoaded?.Invoke(bitmap);
					};
					if(loader.Preview != null) {
						PreviewyImage.Source = loader.Preview;
						previewLoaded = true;
						Progress = null;
						LoadSample();
					} else {
						PreviewyImage.Source = new BitmapImage(new Uri(PrevieweUrl));
					}
					(PreviewyImage.Source as BitmapImage).DownloadProgress += (s, e) => {
						Progress = e.Progress;
					};
				}
				void LoadSample() {
					if(string.IsNullOrWhiteSpace(SampleUrl)) {
						LoadingPanel.Visibility = Visibility.Visible;
						Progress = null;
						FailureTextBlock.Text = "Empty URL".Language();
						return;
					}
					if(previewLoaded) {
						LoadingPanel.Visibility = Visibility.Collapsed;
					} else {
						LoadingPanel.Visibility = Visibility.Visible;
					}
					if(loader.Sample != null) {
						SampleImage.Source = loader.Sample;
						SampleImage.Visibility = Visibility.Visible;
						PreviewyImage.Visibility = Visibility.Collapsed;
						LoadingPanel.Visibility = Visibility.Collapsed;
						Progress = null;
					} else {
						SampleImage.Source = new BitmapImage(new Uri(SampleUrl));
					}
					SampleImage.ImageFailed += (s, e) => {
						Progress = null;
						LoadingPanel.Visibility = Visibility.Visible;
						FailureTextBlock.Text = "Error".Language();
					};
					SampleImage.ImageOpened += (s, e) => {
						var bitmap = SampleImage.Source as BitmapImage;
						LoadingPanel.Visibility = Visibility.Collapsed;
						Progress = null;
						loader.Sample = bitmap;
						SampleImage.Visibility = Visibility.Visible;
						PreviewyImage.Visibility = Visibility.Collapsed;
					};
					(SampleImage.Source as BitmapImage).DownloadProgress += (s, e) => {
						Progress = previewLoaded ? null : e.Progress;
					};
				}
			} else {
				Progress = null;
				FailureTextBlock.Text = "Error".Language();
				this.Visibility = Visibility.Visible;
				VariableSizedWrapGrid.SetColumnSpan(this, SpanCol);
				VariableSizedWrapGrid.SetRowSpan(this, SpanRow);
			}
			if(post != null) {
				ToolTipService.SetToolTip(this, new ToolTipContentForPost(post));
				ToolTipService.SetPlacement(this, PlacementMode.Bottom);
			}
			SetRightClick();
		}

		public void BeginEntranceAnimation() {
			EntranceAnimation.Begin();
		}

		private void SetRightClick() {
			this.RightTapped += (s, e) => {
				if(PostsBrowserPage.IsInMultipleSelectionMode()) {
					return;
				}
				MenuFlyout flyout = new();

				if(MainPage.Instance.currentTag == PageTag.PostsBrowser) {
					MenuFlyoutItem item_select = new() {
						Text = "Select This".Language(),
						Icon = new FontIcon() { Glyph = "\uE152" },
					};
					item_select.Click += (sender, arg) => {
						PostsBrowserPage.SetSelectionMode(true);
						IsSelected = true;
						PostsBrowserPage.SetSelectionFeedback();
					};
					flyout.Items.Add(item_select);

					MenuFlyoutItem item_hide = new() {
						Text = "Hide This Image".Language(),
						Icon = new FontIcon() { Glyph = "\uE894" },
					};
					item_hide.Click += (sender, arg) => {

					};
					flyout.Items.Add(item_hide);
				}

				MenuFlyoutItem item_download = new() {
					Text = "Download This".Language(),
					Icon = new FontIcon() { Glyph = "\uE896" },
				};
				item_download.Click += async (sender, arg) => {
					if(await DownloadsManager.CheckDownloadAvailableWithDialog()) {
						string title = E621Tag.JoinTags(BelongedTags);
						if(await DownloadsManager.RegisterDownload(PostRef, title)) {
							MainPage.CreateTip_SuccessDownload(page);
						} else {
							await MainPage.CreatePopupDialog("Error".Language(), "Downloads Failed".Language());
						}
					}
				};
				flyout.Items.Add(item_download);

				MenuFlyoutItem item_favorite = new() {
					Text = "Add Favorites".Language(),
					Icon = new FontIcon() { Glyph = "\uE0A5" },
				};
				item_favorite.Click += async (sender, arg) => {
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
				flyout.Items.Add(item_favorite);

				MenuFlyoutSubItem item_setAs = new() {
					Text = "Set As".Language(),
					Icon = new FontIcon() { Glyph = "\uEE71" },
				};

				MenuFlyoutItem item_setAsWallpaper = new() {
					Text = "Set As Wallpaper".Language(),
					Icon = new FontIcon() { Glyph = "\uE620" },
				};

				item_setAsWallpaper.Click += Item_setAsWallpaper_Click;

				MenuFlyoutItem item_setAsLockscreen = new() {
					Text = "Set As Lock-screen".Language(),
					Icon = new FontIcon() { Glyph = "\uEE3F" },
				};

				item_setAsLockscreen.Click += Item_setAsLockscreen_Click;

				item_setAs.IsEnabled = PicturePage.IsAbleToSetAs(PostRef);

				item_setAs.Items.Add(item_setAsWallpaper);
				item_setAs.Items.Add(item_setAsLockscreen);

				flyout.Items.Add(item_setAs);

				flyout.Placement = FlyoutPlacementMode.Left;
				if(flyout.Items.Count != 0) {
					flyout.ShowAt(s as UIElement, e.GetPosition(s as UIElement));
				}
			};
		}
		private async void ShowLoadingDialog(string title, string content, Action onCancel = null) {
			dialog_setAs_content = new LoadingDialog() {
				DialogContent = content,
				OnCancel = onCancel,
			};
			dialog_setAs = new ContentDialog() {
				Title = title,
				Content = dialog_setAs_content,
				CloseButtonText = "Cancel".Language(),
			};
			await dialog_setAs.ShowAsync();
		}

		private void UpdateDialogContent(string content) {
			if(dialog_setAs_content == null) {
				return;
			}
			dialog_setAs_content.DialogContent = content;
		}

		private void HideLoadingDialog() {
			dialog_setAs?.Hide();
			dialog_setAs = null;
		}

		private void CancelLoading() {
			if(cts_SetAs != null) {
				cts_SetAs.Cancel();
				cts_SetAs.Dispose();
			}
			cts_SetAs = null;
		}

		private async void Item_setAsLockscreen_Click(object sender, RoutedEventArgs e) {
			if(sender is not MenuFlyoutItem item || !item.IsEnabled) {
				return;
			}
			CancelLoading();
			ShowLoadingDialog("Loading".Language(), "Getting Image".Language(), CancelLoading);
			cts_SetAs = new CancellationTokenSource();
			try {
				StorageFile file = await PicturePage.GetImageFile(PathType.PostID, "", PostRef, cts_SetAs.Token);
				UpdateDialogContent("Setting Lock-screen".Language());
				await PicturePage.SetLockScreenAsync(file);
			} catch(OperationCanceledException) { }
			HideLoadingDialog();
			CancelLoading();
		}

		private async void Item_setAsWallpaper_Click(object sender, RoutedEventArgs e) {
			if(sender is not MenuFlyoutItem item || !item.IsEnabled) {
				return;
			}
			CancelLoading();
			ShowLoadingDialog("Loading".Language(), "Getting Image".Language(), CancelLoading);
			cts_SetAs = new CancellationTokenSource();
			try {
				StorageFile file = await PicturePage.GetImageFile(PathType.PostID, "", PostRef, cts_SetAs.Token);
				UpdateDialogContent("Setting Wallpaper".Language());
				await PicturePage.SetWallpaperAsync(file);
			} catch(OperationCanceledException) { }
			HideLoadingDialog();
			CancelLoading();
		}

		private void Grid_Tapped(object sender, TappedRoutedEventArgs e) {
			if(SampleUrl == null) {
				return;
			}
			if(MainPage.Instance.currentTag == PageTag.PostsBrowser) {
				if(PostsBrowserPage.IsInMultipleSelectionMode()) {
					IsSelected = !IsSelected;
					PostsBrowserPage.SetSelectionFeedback();
				} else {
					App.PostsList.UpdatePostsList(PostsBrowserPage.GetCurrentPosts());
					App.PostsList.Current = PostRef;

					MainPage.NavigateToPicturePage(PostRef, PostsBrowserPage.Instance?.CurrentTab?.Tags);
				}
			} else if(MainPage.Instance.currentTag == PageTag.Spot) {
				App.PostsList.UpdatePostsList(SpotPage.Instance.Posts);
				App.PostsList.Current = PostRef;

				MainPage.NavigateToPicturePage(PostRef, new string[] { "Spot", Methods.GetDate() });
			}
		}
	}
}
