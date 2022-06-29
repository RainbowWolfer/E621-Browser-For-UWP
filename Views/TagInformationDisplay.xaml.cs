using E621Downloader.Models;
using E621Downloader.Models.Posts;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml.Controls;

namespace E621Downloader.Views {
	public sealed partial class TagInformationDisplay: UserControl {
		private readonly string tag;
		private readonly Dictionary<string, E621Tag> pool;
		public TagInformationDisplay(Dictionary<string, E621Tag> pool, string tag) {
			this.InitializeComponent();
			this.tag = tag;
			this.pool = pool ?? new Dictionary<string, E621Tag>();
			Load();
		}

		private async void Load() {
			LoadingRing.IsActive = true;
			E621Tag e621tag;
			int count = 0;
			string description = "No Wiki Found".Language();
			if(pool.ContainsKey(tag)) {
				e621tag = pool[tag];
			} else {
				e621tag = (await E621Tag.GetAsync(tag))?.FirstOrDefault();
				pool.Add(tag, e621tag);
			}
			if(e621tag != null) {
				count = e621tag.post_count;
				if(!e621tag.IsWikiLoaded) {
					await e621tag.LoadWikiAsync();
				}
				description = e621tag.Wiki?.body ?? "No Wiki Found".Language();
			}
			ContentText.Text = "Count".Language() + $": {count}\n" + "Description".Language() + $": {description}";
			LoadingRing.IsActive = false;
		}
	}
}
