using E621Downloader.Models;
using E621Downloader.Models.Posts;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml.Controls;

namespace E621Downloader.Views {
	public sealed partial class PostsCommonTagsDialog: UserControl {
		private readonly IEnumerable<Post> posts;

		private readonly Dictionary<string, int> all = new();
		public PostsCommonTagsDialog(IEnumerable<Post> posts) {
			this.InitializeComponent();
			this.posts = posts;
			TitleText.Text = $"{posts.Count()} {"Posts".Language()}";
			TagsPanel.Children.Clear();

			foreach(string item in posts.SelectMany(x => x.tags.GetAllTags())) {
				if(all.ContainsKey(item)) {
					all[item]++;
				} else {
					all.Add(item, 1);
				}
			}

			foreach(KeyValuePair<string, int> item in all) {
				if(item.Value == this.posts.Count()) {
					AddText(item.Key);
				}
			}
		}

		private void AddText(string tag) {
			TagsPanel.Children.Add(new TextBlock() {
				Text = $"- {tag}",
				IsTextSelectionEnabled = true,
				FontSize = 18,
			});
		}
	}
}
