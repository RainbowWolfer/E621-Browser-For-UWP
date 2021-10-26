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
			E621Tag[] e621tags = await E621Tag.GetAsync(tag);
			string count = "0";
			string description = "not found";
			if(e621tags != null && e621tags.Length > 0) {
				count = e621tags[0].post_count.ToString();
				E621Wiki[] e621wikies = await E621Wiki.GetAsync(tag);
				if(e621wikies != null && e621wikies.Length > 0) {
					description = e621wikies[0].body;
				}
			}
			LoadingRing.IsActive = false;
			ContentText.Text = $"Count: {count}\nDescription: {description}";

		}
	}
}
