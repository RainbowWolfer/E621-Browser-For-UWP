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
	public sealed partial class TagExpander: UserControl {
		private bool _expanded;
		public bool IsExpanded {
			get => _expanded;
			set {
				_expanded = value;
				ExpandPanel.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
				TitleIcon.Glyph = value ? "\uE010" : "\uE011";
			}
		}
		private E621Tag tag;
		public TagExpander(string tag) {
			this.InitializeComponent();
			Load(tag);
		}

		private async void Load(string tag) {
			TagText.Text = $"{tag} (...)";
			LoadingBar.Visibility = Visibility.Visible;
			ConentText.Visibility = Visibility.Collapsed;
			this.tag = await E621Tag.GetFirstAsync(tag);
			TagText.Text = $"{tag ?? "Not Found"} ({this.tag?.post_count ?? 0})";
			if(this.tag != null && !this.tag.IsWikiLoaded) {
				await this.tag.LoadWikiAsync();
			}
			ConentText.Text = this.tag?.Wiki?.body ?? "No Wiki Found";
			LoadingBar.Visibility = Visibility.Collapsed;
			ConentText.Visibility = Visibility.Visible;
		}

		private void Button_Tapped(object sender, TappedRoutedEventArgs e) {
			IsExpanded = !IsExpanded;
		}

	}
}
