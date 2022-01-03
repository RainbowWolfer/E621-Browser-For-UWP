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
		private ContentDialog dialog;
		private string tag;
		public TagInformationDisplay(ContentDialog dialog, string tag) {
			this.InitializeComponent();
			this.dialog = dialog;
			this.tag = tag;
			Load();
		}

		private async void Load() {
			LoadingRing.IsActive = true;
			E621Tag e621tag = (await E621Tag.GetAsync(tag))?.FirstOrDefault();
			int count = 0;
			string description = "not found";
			if(e621tag != null) {
				count = e621tag.post_count;
				E621Wiki wiki = null;
				if(!e621tag.IsWikiLoaded) {
					wiki = await e621tag.LoadWikiAsync();
				}
				description = e621tag.Wiki?.body ?? "Error";
			}
			ContentText.Text = $"Count: {count}\nDescription: {description}";
			LoadingRing.IsActive = false;
		}
	}
}
