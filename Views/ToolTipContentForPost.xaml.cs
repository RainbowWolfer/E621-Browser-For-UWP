﻿using E621Downloader.Models.E621;
using E621Downloader.Models.Utilities;
using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace E621Downloader.Views {
	public sealed partial class ToolTipContentForPost: UserControl {
		private string[] relativeTags;

		public string[] RelativeTags {
			get => relativeTags;
			set {
				relativeTags = value;
				if(value == null || value.Length == 0) {
					RelativeTagsText.Visibility = Visibility.Collapsed;
				} else {
					RelativeTagsText.Visibility = Visibility.Visible;
					RelativeTagsText.Text = " · " + "Relative".Language() + $": ({string.Join(", ", value)})";
				}
			}
		}

		public ToolTipContentForPost(E621Post post, bool local = false) {
			this.InitializeComponent();
			ID.Text = $"#{post.id}" + (local ? " - " + "Local".Language() : "");
			UP.Text = $" · " + "Up".Language() + $" - {Math.Abs(post.score.up)}";
			DOWN.Text = $" · " + "Down".Language() + $" - {Math.Abs(post.score.down)}";
			TYPE.Text = $" · " + "Type".Language() + $" - {post.file.ext.ToUpper()}";
			string parent = post.relationships.parent_id == null ? "None".Language() : $"#{post.relationships.parent_id}";
			PARENT.Text = $" · " + "Parent".Language() + $" - {parent}";
			CHILDREN.Text = $" · " + "Children".Language() + $" - {post.relationships.children.Count}";
			CREATE.Text = $" · " + "Created At".Language() + $" - {post.created_at}";
			TAGS.Text = $" · " + "Tags".Language() + $" - {post.tags.GetAllTags().Count}";
			SIZE.Text = $" · " + "Size".Language() + $" - {Methods.ConvertSizeToKB(post.file.size)}";
			WIDTH.Text = $" · " + "Width".Language() + $" - {post.file.width}";
			HEIGHT.Text = $" · " + "Height".Language() + $" - {post.file.height}";
		}
	}
}
