using E621Downloader.Models.Posts;
using E621Downloader.Pages;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace E621Downloader.Views {
	public sealed partial class BottomInformationForImageHolder: UserControl {
		private Post post;
		public BottomInformationForImageHolder() {
			this.InitializeComponent();
		}

		public Post PostRef {
			get => post;
			set {
				post = value;
				UpdateInfo(value);
			}
		}

		private void UpdateInfo(Post post) {
			if(post == null) {
				return;
			}
			ParentUpvoteText.Text = $"{post.score.up}";
			ParentFavoriteText.Text = $"{post.fav_count}";
			if(post.is_favorited) {
				FavoriteIcon.Foreground = new SolidColorBrush((Color)Application.Current.Resources["SystemAccentColor"]);
			}
			ParentCommentText.Text = $"{post.comment_count}";
			ParentRatingText.Text = $"{post.rating.ToUpper()}";
			Color color;
			switch(post.rating) {
				case "s":
					RatingIcon.Glyph = "\uF78C";
					color = PicturePage.GetColor(Rating.safe);
					break;
				case "q":
					RatingIcon.Glyph = "\uE897";
					color = PicturePage.GetColor(Rating.suggestive);
					break;
				case "e":
					RatingIcon.Glyph = "\uE814";
					color = PicturePage.GetColor(Rating.explict);
					break;
				default:
					color = PicturePage.GetColor(null);
					break;
			}
			RatingIcon.Foreground = new SolidColorBrush(color);
		}
	}
}
