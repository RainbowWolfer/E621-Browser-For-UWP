using E621Downloader.Models.Posts;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
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

namespace E621Downloader.Views.TagsManagementSection {
	public sealed partial class TagsSelectionView: Page {
		private ContentDialog dialog;
		public bool handleSearch = false;
		private List<SingleTagDisplay> TagDisplays => TagsListView.Items.Cast<SingleTagDisplay>().ToList();
		private readonly Dictionary<string, E621Tag> tags = new Dictionary<string, E621Tag>();

		public TagsSelectionView(ContentDialog dialog, string[] tags) {
			this.InitializeComponent();
			this.dialog = dialog;
			foreach(string item in tags) {
				MySuggestBox.Text += item + " ";
			}
			MySuggestBox.Text = MySuggestBox.Text.Trim();
		}

		private void MySuggestBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args) {
			foreach(SingleTagDisplay item in TagsListView.Items) {
				item.CancelLoadingTag();
			}
			TagsListView.Items.Clear();
			foreach(string item in sender.Text.Trim().Split(" ").Where(s => !string.IsNullOrEmpty(s))) {
				TagsListView.Items.Add(new SingleTagDisplay(this, item));
			}
		}

		public E621Tag GetE621Tag(string tag) => tags.ContainsKey(tag) ? tags[tag] : null;
		public void RegisterE621Tag(string tag, E621Tag e621tag) {
			if(tags.ContainsKey(tag)) {
				tags[tag] = e621tag;
			} else {
				tags.Add(tag, e621tag);
			}
		}


		public void RemoveTag(string tag) {
			MySuggestBox.Text = MySuggestBox.Text.Replace(tag, "").Trim();
		}
		public string[] GetTags() {
			return tags.Keys.ToArray();
		}

		private void SearchButton_Tapped(object sender, TappedRoutedEventArgs e) {
			handleSearch = true;
			dialog.Hide();
		}

		private void DialogBackButton_Tapped(object sender, TappedRoutedEventArgs e) {
			dialog.Hide();
		}
	}
}
