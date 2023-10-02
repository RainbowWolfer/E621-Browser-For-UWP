using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml.Controls;
using YiffBrowser.Models.E621;

namespace YiffBrowser.Views.Controls.TagsInfoViews {
	public sealed partial class CompareTagsView : UserControl {

		public Tags Tags { get; }

		public string[] PreviewURLs { get; set; }

		private readonly E621Post[] posts;
		public CompareTagsView(params E621Post[] posts) {
			InitializeComponent();
			this.posts = posts;

			Tags = new Tags() {
				General = GetCommonTags(x => x.General),
				Species = GetCommonTags(x => x.Species),
				Character = GetCommonTags(x => x.Character),
				Copyright = GetCommonTags(x => x.Copyright),
				Artist = GetCommonTags(x => x.Artist),
				Invalid = GetCommonTags(x => x.Invalid),
				Lore = GetCommonTags(x => x.Lore),
				Meta = GetCommonTags(x => x.Meta),
			};

			PreviewURLs = posts.Select(x => x.Preview?.URL).ToArray();
		}

		private List<string> GetCommonTags(Func<Tags, IList<string>> getTags) {
			Dictionary<string, int> all = new();
			IList<string>[] listPerPost = posts.Select(x => getTags(x.Tags)).ToArray();
			string[] array = listPerPost.SelectMany(x => x).ToArray();
			foreach (string item in array) {
				if (all.ContainsKey(item)) {
					all[item]++;
				} else {
					all.Add(item, 1);
				}
			}

			List<string> tags = new();

			foreach (KeyValuePair<string, int> item in all) {
				if (item.Value == posts.Length) {
					tags.Add(item.Key);
				}
			}

			return tags;
		}
	}
}
