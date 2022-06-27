using E621Downloader.Models;
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

		public ToolTipContentForPost(Post post, bool local = false) {
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
