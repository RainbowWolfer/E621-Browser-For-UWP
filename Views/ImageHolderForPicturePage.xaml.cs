using E621Downloader.Models.Posts;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
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
	public sealed partial class ImageHolderForPicturePage: UserControl {
		private string post_ID;
		private bool isLoading;

		public string Post_ID {
			get => post_ID;
			set {
				if(IsLoading) {
					return;
				}
				post_ID = value;
				Load(post_ID);
			}
		}

		public bool IsLoading {
			get => isLoading;
			set {
				isLoading = value;
				LoadingRing.IsActive = value;
			}
		}


		private async void Load(string post_id) {
			if(string.IsNullOrWhiteSpace(post_id)) {
				IsLoading = false;
				return;
			}
			IsLoading = true;
			Post post = await Post.GetPostByIDAsync(post_id);
			UpdateInfo(post);
			if(post.flags.deleted) {
				HintText.Visibility = Visibility.Visible;
				IsLoading = false;
			} else {
				BitmapImage image = new BitmapImage(new Uri(post.sample.url ?? post.preview.url ?? "ms-appx:///Assets/e621.png"));
				MyImage.Source = image;
				image.ImageOpened += (s, e) => {
					IsLoading = false;
				};
			}
		}

		private void UpdateInfo(Post post) {
			if(post == null) {
				return;
			}
			ParentUpvoteText.Text = $"{post.score.up}";
			ParentFavoriteText.Text = $"{post.fav_count}";
			ParentCommentText.Text = $"{post.comment_count}";
			ParentRatingText.Text = $"{post.rating.ToUpper()}";
			switch(post.rating) {
				case "s":
					RatingIcon.Glyph = "\uF78C";
					RatingIcon.Foreground = new SolidColorBrush(Colors.Green);
					break;
				case "q":
					RatingIcon.Glyph = "\uF142";
					RatingIcon.Foreground = new SolidColorBrush(Colors.Yellow);
					break;
				case "e":
					RatingIcon.Glyph = "\uE814";
					RatingIcon.Foreground = new SolidColorBrush(Colors.Red);
					break;
				default:
					RatingIcon.Foreground = new SolidColorBrush(Colors.White);
					break;
			}
			if(post.file.ext == "webm") {
				TypeBorder.Visibility = Visibility.Visible;
				TypeTextBlock.Text = "WEBM";
			} else if(post.file.ext == "anim") {
				TypeBorder.Visibility = Visibility.Visible;
				TypeTextBlock.Text = "ANIM";
			} else {
				TypeBorder.Visibility = Visibility.Collapsed;
				TypeTextBlock.Text = "";
			}
		}

		public ImageHolderForPicturePage() {
			this.InitializeComponent();
		}
	}
}
