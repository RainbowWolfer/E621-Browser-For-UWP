using E621Downloader.Models.Posts;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace E621Downloader.Views {
	public sealed partial class PostMoreInfoDialog: UserControl {
		public PostMoreInfoDialog(Post post) {
			this.InitializeComponent();
			if(post == null) {
				return;
			}
			CreatedDateText.Text = $"{Check(post.created_at)}";
			UpdatedDateText.Text = $"{Check(post.updated_at)}";
			SizeText.Text = $"{post.file?.width} x {post.file?.height} ( {post.file?.size / 1000}KB )";
			ApproverText.Text = $"{Check(post.approver_id)}";
			UploaderText.Text = $"{Check(post.uploader_id)}";
			ArtistTag.Text = $"{post.tags?.artist?.Count}";
			CopyrightTag.Text = $"{post.tags?.copyright?.Count}";
			SpeciesTag.Text = $"{post.tags?.species?.Count}";
			CharacterTag.Text = $"{post.tags?.character?.Count}";
			GeneralTag.Text = $"{post.tags?.general?.Count}";
			MetaTag.Text = $"{post.tags?.meta?.Count}";
			InvalidTag.Text = $"{post.tags?.invalid?.Count}";
			LoreTag.Text = $"{post.tags?.lore?.Count}";
		}

		private string Check(object content) {
			return string.IsNullOrWhiteSpace($"{content}") ? "None" : $"{content}";
		}
	}
}
