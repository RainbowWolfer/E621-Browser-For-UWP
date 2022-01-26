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
		public ToolTipContentForPost(Post post, bool local = false) {
			this.InitializeComponent();
			ID.Text = $"#{post.id}" + (local ? " - Local" : "");
			UP.Text = $" · Up - {Math.Abs(post.score.up)}";
			DOWN.Text = $" · Down - {Math.Abs(post.score.down)}";
			TYPE.Text = $" · Type - {post.file.ext.ToUpper()}";
			string parent = post.relationships.parent_id == null ? "None" : $"#{post.relationships.parent_id}";
			PARENT.Text = $" · Parent - {parent}";
			CHILDREN.Text = $" · Children - {post.relationships.children.Count}";
			CREATE.Text = $" · Created At - {post.created_at}";
			TAGS.Text = $" · Tags - {post.tags.GetAllTags().Count}";
			SIZE.Text = $" · Size - {Methods.ConvertSizeToKB(post.file.size)}";
			WIDTH.Text = $" · Width - {post.file.width}";
			HEIGHT.Text = $" · Height - {post.file.height}";
		}
	}
}
