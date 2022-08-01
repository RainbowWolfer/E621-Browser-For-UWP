using E621Downloader.Models;
using E621Downloader.Models.E621;
using E621Downloader.Models.Networks;
using E621Downloader.Models.Utilities;
using E621Downloader.Pages;
using System;
using System.IO;
using System.Numerics;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.Storage.Streams;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace E621Downloader.Views.PictureSection {
	public sealed partial class PhotosListItem: UserControl {
		private bool isSelected = false;
		private object photo;

		public bool IsSelected {
			get => isSelected;
			set {
				isSelected = value;
				ColorAnimation.From = (MainGrid.BorderBrush as SolidColorBrush).Color;
				if(isSelected) {
					MainGrid.BorderThickness = new Thickness(3);
					ColorAnimation.To = (Color)Application.Current.Resources["SystemAccentColor"];
				} else {
					MainGrid.BorderThickness = new Thickness(1.2);
					ColorAnimation.To = Colors.Beige;
				}
				SelectedStoryboard.Begin();
			}
		}

		public object Photo {
			get => photo;
			private set {
				photo = value;
			}
		}

		private readonly ProgressLoader progress;
		private bool isLoading = false;
		private bool hasLoaded = false;

		public PhotosListItem() {
			this.InitializeComponent();
			progress = new ProgressLoader(LoadingRing);
			PlayIcon.Translation += new Vector3(0, 0, 5);
		}

		public async Task LoadAsync() {
			if(!CheckTypeValid(Photo) || progress.Value != null || isLoading || hasLoaded) {
				return;
			}
			isLoading = true;
			progress.Value = 0;
			MainImage.Source = null;
			if(Photo is E621Post post) {
				await LoadPost(post);
			} else if(Photo is MixPost mix) {
				if(mix.Type == PathType.PostID && mix.PostRef != null) {
					await LoadPost(mix.PostRef);
				} else if(mix.Type == PathType.Local && mix.ImageFile != null) {
					await LoadLocal(mix.ImageFile, mix.MetaFile.MyPost);
				}
			} else if(Photo is ILocalImage local) {
				await LoadLocal(local.ImageFile, local.ImagePost);
			} else if(Photo is string postID) {
				E621Post postRef;
				if(App.PostsPool.ContainsKey(postID)) {
					postRef = App.PostsPool[postID];
				} else {
					postRef = await E621Post.GetPostByIDAsync(postID);
					if(postRef == null) {
						return;
					} else {
						if(App.PostsPool.ContainsKey(postID)) {
							App.PostsPool[postID] = postRef;
						} else {
							App.PostsPool.Add(postID, postRef);
						}
					}
				}
				await LoadPost(postRef);
			}
			hasLoaded = true;
			isLoading = false;
			progress.Value = null;
		}

		private async Task LoadPost(E621Post post) {
			UpdateIcon(post);
			LoadPoolItem loader = LoadPool.SetNew(post);
			if(loader.Sample != null) {
				MainImage.Source = loader.Sample;
			} else if(loader.Preview != null) {
				MainImage.Source = loader.Preview;
			} else {
				if(!string.IsNullOrWhiteSpace(post.preview.url)) {
					HttpResult<InMemoryRandomAccessStream> result = await Data.ReadImageStreamAsync(post.preview.url);
					if(result.Result == HttpResultType.Success) {
						var preview = new BitmapImage();
						await preview.SetSourceAsync(result.Content);
						loader.Preview = preview;
						MainImage.Source = preview;
					}
				}
				if(!string.IsNullOrWhiteSpace(post.sample.url)) {
					HttpResult<InMemoryRandomAccessStream> result = await Data.ReadImageStreamAsync(post.sample.url);
					if(result.Result == HttpResultType.Success) {
						var sample = new BitmapImage();
						await sample.SetSourceAsync(result.Content);
						loader.Sample = sample;
						MainImage.Source = sample;
					}
				}
			}
			CheckGifPlay(post);
		}

		private async Task LoadLocal(StorageFile file, E621Post post) {
			UpdateIcon(post);
			using StorageItemThumbnail thumbnail = await file.GetThumbnailAsync(ThumbnailMode.SingleItem);
			if(thumbnail != null) {
				using Stream stream = thumbnail.AsStreamForRead();
				BitmapImage bitmap = new();
				await bitmap.SetSourceAsync(stream.AsRandomAccessStream());
				MainImage.Source = bitmap;
			}
			CheckGifPlay(post);
		}

		private void UpdateIcon(E621Post post) {
			FileType type = PicturePage.GetFileType(post);
			if(type == FileType.Gif) {
				PlayIcon.Visibility = Visibility.Visible;
				PlayIcon.Glyph = "\uF4A9";
			} else if(type == FileType.Webm) {
				PlayIcon.Visibility = Visibility.Visible;
				PlayIcon.Glyph = "\uE102";
			} else {
				PlayIcon.Visibility = Visibility.Collapsed;
			}
		}

		private bool IsGif(E621Post post) => PicturePage.GetFileType(post) == FileType.Gif;

		private void CheckGifPlay(E621Post post) {
			if(IsGif(post)) {
				if(MainImage.Source is BitmapImage bi) {
					bi.Stop();
				}
			}
		}

		public static bool CheckTypeValid(object obj) {
			return obj is E621Post || obj is MixPost || obj is ILocalImage || (obj is string id && long.TryParse(id, out long _));
		}

		public void SetPhoto(object obj) {
			if(CheckTypeValid(obj)) {
				Photo = obj;
			}
		}

		public void SetPhoto(E621Post post) {
			Photo = post;
		}
		public void SetPhoto(MixPost mix) {
			Photo = mix;
		}
		public void SetPhoto(ILocalImage local) {
			Photo = local;
		}
		public void SetPhoto(string postID) {
			if(long.TryParse(postID, out long _)) {
				Photo = postID;
			}
		}
	}
}
