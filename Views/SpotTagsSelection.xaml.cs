using E621Downloader.Models.Locals;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace E621Downloader.Views {
	public sealed partial class SpotTagsSelection: UserControl {
		private string[] Followings { get; set; }
		private ObservableCollection<string> InputTags { get; set; } = new ObservableCollection<string>();
		private string[] initial_followings;
		public SpotTagsSelection() {
			this.InitializeComponent();
			Followings = Local.Listing.GetGetDefaultFollowList().Tags.ToArray();
		}

		public void Initialize(string[] input, string[] followings) {
			if(input != null && input.Length != 0) {
				input.ToList().ForEach(i => InputTags.Add(i));
			}
			initial_followings = followings ?? new string[0];
		}

		private void ReverseButton_Tapped(object sender, TappedRoutedEventArgs e) {
			for(int i = 0; i < FollowingTagsListView.Items.Count; i++) {
				var item = FollowingTagsListView.ContainerFromIndex(i) as ListViewItem;
				item.IsSelected = !item.IsSelected;
			}
		}

		private void Preselect(string[] selected) {
			for(int i = 0; i < FollowingTagsListView.Items.Count; i++) {
				var item = FollowingTagsListView.ContainerFromIndex(i) as ListViewItem;
				if(selected.Contains(item.Content as string)) {
					item.IsSelected = true;
				}
			}
		}

		public string[] GetSelectedTags() {
			return FollowingTagsListView.SelectedItems.Where(i => i is string).Select(i => i).Cast<string>().ToArray();
		}

		public string[] GetInputTags() {
			return InputTags.ToArray();
		}

		private void TagsInput_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args) {
			string text = args.QueryText.Trim().ToLower();
			if(!string.IsNullOrWhiteSpace(text) && !InputTags.Contains(text)) {
				InputTags.Insert(0, text);
				sender.Text = "";
			}
		}

		private void DeleteButton_Tapped(object sender, TappedRoutedEventArgs e) {
			string tag = (sender as Button).Tag as string;
			if(InputTags.Contains(tag)) {
				InputTags.Remove(tag);
			}
		}

		private void InputTagsListView_Loaded(object sender, RoutedEventArgs e) {
			if(initial_followings.Length != 0) {
				Preselect(initial_followings);
			}
		}
	}
}
