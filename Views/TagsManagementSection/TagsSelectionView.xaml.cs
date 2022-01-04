using E621Downloader.Models.Posts;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
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
		public ResultType Result { get; private set; } = ResultType.None;
		private readonly Dictionary<string, E621Tag> tags_pool = new Dictionary<string, E621Tag>();
		private readonly List<string> currentTags = new List<string>();

		public TagsSelectionView(ContentDialog dialog, string[] tags) {
			this.InitializeComponent();
			this.dialog = dialog;
			foreach(string item in tags) {
				MySuggestBox.Text += item + " ";
			}
			MySuggestBox.Text = MySuggestBox.Text.Trim();
		}

		private void MySuggestBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args) {
			foreach(SingleTagDisplay item in TagsStackPanel.Children) {
				item.CancelLoadingTag();
			}
			TagsStackPanel.Children.Clear();
			currentTags.Clear();
			foreach(string item in sender.Text.Trim().Split(" ").Where(s => !string.IsNullOrEmpty(s)).ToList()) {
				currentTags.Add(item);
				TagsStackPanel.Children.Add(new SingleTagDisplay(this, item));
			}
		}

		public E621Tag GetE621Tag(string tag) => tags_pool.ContainsKey(tag) ? tags_pool[tag] : null;
		public void RegisterE621Tag(string tag, E621Tag e621tag) {
			if(tags_pool.ContainsKey(tag)) {
				tags_pool[tag] = e621tag;
			} else {
				tags_pool.Add(tag, e621tag);
			}
		}


		public void RemoveTag(string tag) {
			MySuggestBox.Text = MySuggestBox.Text.Replace(tag, "").Trim();
		}
		public string[] GetTags() => currentTags.ToArray();

		private void SearchButton_Tapped(object sender, TappedRoutedEventArgs e) {
			Result = ResultType.Search;
			dialog.Hide();
		}

		private void DialogBackButton_Tapped(object sender, TappedRoutedEventArgs e) {
			Result = ResultType.None;
			dialog.Hide();
		}

		private void HotButton_Tapped(object sender, TappedRoutedEventArgs e) {
			Result = ResultType.Hot;
			dialog.Hide();
		}

		private void RandomButton_Tapped(object sender, TappedRoutedEventArgs e) {
			Result = ResultType.Random;
			dialog.Hide();
		}

		public enum ResultType {
			None, Search, Hot, Random
		}
	}
}
