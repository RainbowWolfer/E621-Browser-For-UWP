using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Windows.Input;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using YiffBrowser.Helpers;
using YiffBrowser.Models.E621;
using YiffBrowser.Services.Networks;

namespace YiffBrowser.Views.Controls.Common {
	public sealed partial class PostHeaderSimpleInfoView : UserControl {

		public E621Post Post {
			get => (E621Post)GetValue(PostProperty);
			set => SetValue(PostProperty, value);
		}

		public static readonly DependencyProperty PostProperty = DependencyProperty.Register(
			nameof(Post),
			typeof(E621Post),
			typeof(PostHeaderSimpleInfoView),
			new PropertyMetadata(null)
		);




		public PostHeaderSimpleInfoView() {
			this.InitializeComponent();
		}
	}

	internal class PostHeaderSimpleInfoViewModel : BindableBase {
		private E621Post post;
		private string fileTypeIcon;
		private string fileTypeToolTip;
		private string ratingToolTip;
		private string idTitle;
		private Color ratingColor;
		private string duration;
		private bool showSoundWarning;
		private Color soundWarningColor;
		private string soundWarningToolTip;
		private long fileSize;

		public E621Post Post {
			get => post;
			set => SetProperty(ref post, value, OnPostChanged);
		}

		public long FileSize {
			get => fileSize;
			set => SetProperty(ref fileSize, value);
		}

		public string FileTypeIcon {
			get => fileTypeIcon;
			set => SetProperty(ref fileTypeIcon, value);
		}

		public string Duration {
			get => duration;
			set => SetProperty(ref duration, value);
		}

		public string FileTypeToolTip {
			get => fileTypeToolTip;
			set => SetProperty(ref fileTypeToolTip, value);
		}

		public string RatingToolTip {
			get => ratingToolTip;
			set => SetProperty(ref ratingToolTip, value);
		}

		public string IDTitle {
			get => idTitle;
			set => SetProperty(ref idTitle, value);
		}
		public Color RatingColor {
			get => ratingColor;
			set => SetProperty(ref ratingColor, value);
		}

		public bool ShowSoundWarning {
			get => showSoundWarning;
			set => SetProperty(ref showSoundWarning, value);
		}
		public Color SoundWarningColor {
			get => soundWarningColor;
			set => SetProperty(ref soundWarningColor, value);
		}
		public string SoundWarningToolTip {
			get => soundWarningToolTip;
			set => SetProperty(ref soundWarningToolTip, value);
		}

		private void OnPostChanged() {
			if (Post == null) {
				return;
			}

			IDTitle = $"{Post.ID} ({Post.Rating.ToString().Substring(0, 1)})";
			RatingColor = Post.Rating.GetRatingColor();


			FileSize = Post.File.Size;
			Duration = Post.Duration;

			FileType type = Post.GetFileType();
			FileTypeIcon = type switch {
				FileType.Png or FileType.Jpg => "\uEB9F",
				FileType.Gif => "\uF4A9",
				FileType.Webm => "\uE714",
				_ => "\uE9CE",
			};
			FileTypeToolTip = $"Type: {type}";

			RatingToolTip = $"Rating: {Post.Rating}";

			List<string> tags = Post.Tags.GetAllTags();
			if (tags.Contains("sound_warning")) {
				ShowSoundWarning = true;
				SoundWarningColor = E621Rating.Explicit.GetRatingColor();
				SoundWarningToolTip = "This Video Has 'sound_warning' Tag";
			} else if (tags.Contains("sound")) {
				ShowSoundWarning = true;
				SoundWarningColor = E621Rating.Questionable.GetRatingColor();
				SoundWarningToolTip = "This Video Has 'sound' Tag";
			} else {
				ShowSoundWarning = false;
			}

		}

		public PostHeaderSimpleInfoViewModel() {

		}


		public ICommand CopyURLCommand => new DelegateCommand(CopyURL);
		public ICommand OpenInBrowserCommand => new DelegateCommand(OpenInBrowser);

		private void CopyURL() {
			@$"https:/{E621API.GetHost()}/posts/{Post.ID}".CopyToClipboard();
		}

		private void OpenInBrowser() {
			@$"https:/{E621API.GetHost()}/posts/{Post.ID}".OpenInBrowser();
		}

	}
}
