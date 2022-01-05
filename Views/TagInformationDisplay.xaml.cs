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
			string description = "No Wiki Found";
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
				description = e621tag.Wiki?.body ?? "No Wiki Found";
			}
			ContentText.Text = $"Count: {count}\nDescription: {description}";
			LoadingRing.IsActive = false;
		}
	}
}
