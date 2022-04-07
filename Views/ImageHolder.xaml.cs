using E621Downloader.Models;
using E621Downloader.Models.Download;
using E621Downloader.Models.Locals;
using E621Downloader.Models.Posts;
using E621Downloader.Pages;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage.Streams;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

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
		public string LoadUrl => PostRef.sample.url;

		private bool _isSelected;
		public bool IsSelected {
			get => _isSelected;
			set {
				_isSelected = value;
				BorderGrid.BorderThickness = new Thickness(value ? 4 : 0);
			}
		}

		public BitmapImage Image { get; private set; }
		private readonly PathType type;
		private readonly string path;
		private readonly Page page;

		//private bool isLoaded;

		public ImageHolder(Page page, Post post, int index, PathType type, string path) {
			this.InitializeComponent();
			this.page = page;
			this.PostRef = post;
			this.Index = index;
			this.type = type;
			this.path = path;
			OnImagedLoaded += (b) => this.Image = b;
			if(LoadUrl != null) {
				MyImage.Source = new BitmapImage(new Uri(LoadUrl));
			} else {
				MyProgressRing.IsActive = false;
				MyProgressRing.Visibility = Visibility.Collapsed;
				FailureTextBlock.Text = "Failed";
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
						Text = "Select This",
						Icon = new FontIcon() { Glyph = "\uE152" },
					};
					item_select.Click += (sender, arg) => {
						PostsBrowserPage.SetSelectionMode(true);
						IsSelected = true;
						PostsBrowserPage.SetSelectionFeedback(this);
					};
					flyout.Items.Add(item_select);

					MenuFlyoutItem item_hide = new() {
						Text = "Hide This Image",
						Icon = new FontIcon() { Glyph = "\uE894" },
					};
					item_hide.Click += (sender, arg) => {

					};
					flyout.Items.Add(item_hide);
				}

				MenuFlyoutItem item_download = new() {
					Text = "Download This",
					Icon = new FontIcon() { Glyph = "\uE896" },
				};
				item_download.Click += async (sender, arg) => {
					if(await DownloadsManager.CheckDownloadAvailableWithDialog()) {
						string title = E621Tag.JoinTags(BelongedTags);
						if(await DownloadsManager.RegisterDownload(PostRef, title)) {
							MainPage.CreateTip_SuccessDownload(page);
						} else {
							await MainPage.CreatePopupDialog("Error", "Downloads Failed");
						}
					}
				};
				flyout.Items.Add(item_download);

				MenuFlyoutItem item_favorite = new() {
					Text = "Add Favorites",
					Icon = new FontIcon() { Glyph = "\uE0A5" },
				};
				item_favorite.Click += async (sender, arg) => {
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
				flyout.Items.Add(item_favorite);

				flyout.Placement = FlyoutPlacementMode.Left;
				if(flyout.Items.Count != 0) {
					flyout.ShowAt(s as UIElement, e.GetPosition(s as UIElement));
				}
			};
		}

		private void Grid_Tapped(object sender, TappedRoutedEventArgs e) {
			if(LoadUrl == null) {
				return;
			}
			if(MainPage.Instance.currentTag == PageTag.PostsBrowser) {
				if(PostsBrowserPage.IsInMultipleSelectionMode()) {
					IsSelected = !IsSelected;
					PostsBrowserPage.SetSelectionFeedback(this);
				} else {
					App.PostsList.UpdatePostsList(PostsBrowserPage.GetCurrentPosts());
					App.PostsList.Current = PostRef;

					MainPage.NavigateToPicturePage(PostRef);
				}
			} else if(MainPage.Instance.currentTag == PageTag.Spot) {
				App.PostsList.UpdatePostsList(SpotPage.Instance.Posts);
				App.PostsList.Current = PostRef;

				MainPage.NavigateToPicturePage(PostRef);
			}
		}

		private void MyImage_ImageOpened(object sender, RoutedEventArgs e) {
			LoadingPanel.Visibility = Visibility.Collapsed;
			//Debug.WriteLine("\n" + (DateTime.Now - startTime) + "\n");
			OnImagedLoaded?.Invoke((BitmapImage)MyImage.Source);
		}

		private void MyImage_ImageFailed(object sender, ExceptionRoutedEventArgs e) {
			MyProgressRing.IsActive = false;
			//MyLoadingTextBlock.Text = "FAILED";
			Debug.WriteLine("FAILED");
		}
	}
}
