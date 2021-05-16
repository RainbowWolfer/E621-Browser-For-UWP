using E621Downloader.Models;
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
	public sealed partial class PostDebugView: UserControl {
		private const int FONTSIZE = 24;
		public Post post;
		public PostDebugView(Post post) {
			this.post = post;
			this.InitializeComponent();

			Add("id", post.id);
			Add("created_at", post.created_at);
			Add("updated_at", post.updated_at);

			AddGap(20);

			Add("file.width", post.file.width);
			Add("file.height", post.file.height);
			Add("file.ext", post.file.ext);
			Add("file.size", post.file.size);
			Add("file.md5", post.file.md5);
			Add("file.url", post.file.url);

			AddGap(20);

			Add("preview.width", post.preview.width);
			Add("preview.height", post.preview.height);
			Add("preview.url", post.preview.url);

			AddGap(20);

			Add("sample.has", post.sample.has);
			Add("sample.height", post.sample.height);
			Add("sample.width", post.sample.width);
			Add("sample.url", post.sample.url);
			Add("sample.alternates", post.sample.alternates);

			AddGap(20);

			Add("score.up", post.score.up);
			Add("score.down", post.score.down);
			Add("score.total", post.score.total);

			AddGap(20);

			Add("tags.general", post.tags.general.Count);
			Add("tags.species", post.tags.species.Count);
			Add("tags.character", post.tags.character.Count);
			Add("tags.copyright", post.tags.copyright.Count);
			Add("tags.artist", post.tags.artist.Count);
			Add("tags.invalid", post.tags.invalid.Count);
			Add("tags.lore", post.tags.lore.Count);
			Add("tags.meta", post.tags.meta.Count);

			AddGap(20);

			Add("locked_tags", post.locked_tags.Count);
			Add("change_seq", post.change_seq);

			AddGap(20);

			Add("flags.pending", post.flags.pending);
			Add("flags.flagged", post.flags.flagged);
			Add("flags.note_locked", post.flags.note_locked);
			Add("flags.status_locked", post.flags.status_locked);
			Add("flags.rating_locked", post.flags.rating_locked);
			Add("flags.deleted", post.flags.deleted);

			AddGap(20);

			Add("rating", post.rating);
			Add("fav_count", post.fav_count);
			Add("sources", post.sources.Count);
			Add("pools", post.pools.Count);

			AddGap(20);

			Add("relationships.parent_id", post.relationships.parent_id);
			Add("relationships.has_children", post.relationships.has_children);
			Add("relationships.has_active_children", post.relationships.has_active_children);
			Add("relationships.children", post.relationships.children.Count);

			AddGap(20);

			Add("approver_id", post.approver_id);
			Add("uploader_id", post.uploader_id);
			Add("description", post.description);
			Add("comment_count", post.comment_count);
			Add("is_favorited", post.is_favorited);
			Add("has_notes", post.has_notes);
			Add("duration", post.duration);
		}

		private void AddGap(int height) {
			MainStackPanel.Children.Add(new TextBlock() { Height = height });
		}

		private void Add(string title, object content) {
			var panel = new StackPanel() {
				Orientation = Orientation.Horizontal,
				Margin = new Thickness(2),
			};
			var titleTB = new TextBlock() {
				Text = title + " : ",
				FontSize = FONTSIZE,
				Margin = new Thickness(2, 0, 2, 0)
			};
			var contentTB = new TextBlock() {
				Text = content == null ? "NULL" : content.ToString(),
				FontSize = FONTSIZE,
				Margin = new Thickness(2, 0, 2, 0),
			};
			panel.Children.Add(titleTB);
			panel.Children.Add(contentTB);
			MainStackPanel.Children.Add(panel);
		}
	}
}
