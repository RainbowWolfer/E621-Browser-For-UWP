﻿using E621Downloader.Models.E621;
using E621Downloader.Models.Utilities;
using Windows.UI.Xaml.Controls;

namespace E621Downloader.Views.PictureSection {
	public sealed partial class PostMoreInfoDialog: UserControl {
		public PostMoreInfoDialog(E621Post post) {
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
			return string.IsNullOrWhiteSpace($"{content}") ? "None".Language() : $"{content}";
		}
	}
}
