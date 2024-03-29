﻿using E621Downloader.Models.E621;
using E621Downloader.Pages;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace E621Downloader.Views {
	public sealed partial class BottomInformationForImageHolder: UserControl {
		private E621Post post;
		public BottomInformationForImageHolder() {
			this.InitializeComponent();
		}

		public E621Post PostRef {
			get => post;
			set {
				post = value;
				UpdateInfo(value);
			}
		}

		private void UpdateInfo(E621Post post) {
			if(post == null) {
				return;
			}
			ParentUpvoteText.Text = $"{post.score.total}";
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
