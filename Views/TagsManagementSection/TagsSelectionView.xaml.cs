using E621Downloader.Models.Posts;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
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

		private bool itemClick = false;
		private void AutoCompletesListView_ItemClick(object sender, ItemClickEventArgs e) {
			var item = (SingleTagSuggestion)e.ClickedItem;
			var tag = item.CompleteName;
			//change last
			//var last = GetLast(MySuggestBox.Text);
			int lastSpace = MySuggestBox.Text.LastIndexOf(' ');
			if(lastSpace == -1) {
				MySuggestBox.Text = tag;
			} else {
				//MySuggestBox.Text = MySuggestBox.Text.Trim() + " " + tag;
				string cut = MySuggestBox.Text.Substring(0, lastSpace).Trim();
				MySuggestBox.Text = cut + " " + tag;
			}
			AutoCompletesListView.Items.Clear();
			itemClick = true;
		}

		private void MySuggestBox_TextChanged(object sender, TextChangedEventArgs e) {
			if(itemClick) {
				itemClick = false;
				return;
			}
			var box = (TextBox)sender;
			if(box.Text.LastOrDefault() == ' ') {
				AutoCompletesListView.Items.Clear();
				return;
			}
			currentTags.Clear();
			foreach(string item in box.Text.Trim().Split(" ").Where(s => !string.IsNullOrEmpty(s)).ToList()) {
				currentTags.Add(item);
			}
			string last = currentTags.LastOrDefault();
			//if(args.Reason == AutoSuggestionBoxTextChangeReason.UserInput) {
			if(string.IsNullOrWhiteSpace(last)) {
				AutoCompletesListView.Items.Clear();
			} else {
				LoadAutoSuggestion(last);
			}
			//}
		}
		private CancellationTokenSource cts;
		private async void LoadAutoSuggestion(string tag) {
			if(cts != null) {
				cts.Cancel();
				cts.Dispose();
			}
			cts = new CancellationTokenSource();
			SetLoadingbar(true);
			AutoCompletesListView.Items.Clear();
			E621AutoComplete[] acs = await E621AutoComplete.GetAsync(tag, cts.Token);
			AutoCompletesListView.Items.Clear();
			//if(acs == null || acs.Length == 0) {
			//	SetLoadingbar(false);
			//	return;
			//}
			foreach(E621AutoComplete item in acs) {
				AutoCompletesListView.Items.Add(new SingleTagSuggestion(item));
			}
			var last = AutoCompletesListView.Items.Cast<SingleTagSuggestion>().LastOrDefault();
			if(last != null) {
				last.Loaded += (s, e) => {
					SetLoadingbar(false);
				};
			}
		}

		private void SetLoadingbar(bool active) {
			LoadingBar.IsIndeterminate = active;
			LoadingBar.Visibility = active ? Visibility.Visible : Visibility.Collapsed;
		}

		public E621Tag GetE621Tag(string tag) {
			return tags_pool.ContainsKey(tag) ? tags_pool[tag] : null;
		}

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

		private string GetLast(string value) {
			int lastSpace = value.LastIndexOf(' ');
			if(lastSpace != -1) {
				return value.Substring(lastSpace, value.Length - lastSpace).Trim();
			} else {
				return value;
			}
		}

		public enum ResultType {
			None, Search, Hot, Random
		}
	}
}
