using E621Downloader.Models.Locals;
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
	public sealed partial class CurrentTagsInformation: UserControl {
		private readonly string[] tags;
		private readonly string[] filtered;
		private bool initializing = true;
		public CurrentTagsInformation(string[] tags) {
			this.InitializeComponent();
			this.tags = E621Tag.SortOutMetatags(tags);
			for(int i = 0; i < this.tags.Length; i++) {
				ExpanderPanel.Children.Add(new TagExpander(this.tags[i]) {
					IsExpanded = true,
					Margin = new Thickness(5),
				});
			}
			filtered = E621Tag.FilterMetatags(this.tags);
			string tag = E621Tag.JoinTags(filtered);
			if(filtered.Length == 0) {
				FollowingToggle.IsEnabled = false;
				BlackToggle.IsEnabled = false;
			} else {
				var follow_list = Local.Listing.LocalFollowingLists.ToList();
				FollowingToggle.Initialize(tag, follow_list);
				FollowingToggle.OnComboBoxChecked += dic => {
					//(list name, isChecked)
					foreach(KeyValuePair<string, bool> item in dic) {
						SingleListing found = follow_list.Find(l => l.Name == item.Key);
						if(found == null) {
							continue;
						}
						if(item.Value) {
							if(!found.Tags.Contains(tag)) {
								found.Tags.Add(tag);
							}
							if(found.IsDefault) {
								BlackToggle.UncheckDefault();
							}
						} else {
							if(found.Tags.Contains(tag)) {
								found.Tags.Remove(tag);
							}
						}
					}
				};
				FollowingToggle.OnToggled += () => {
					BlackToggle.UncheckDefault();
				};

				var black_list = Local.Listing.LocalBlackLists.Append(Local.Listing.CloudBlackList).ToList();
				BlackToggle.Initialize(tag, black_list);
				BlackToggle.OnComboBoxChecked += dic => {
					foreach(KeyValuePair<string, bool> item in dic) {
						SingleListing found = black_list.Find(l => l.Name == item.Key);
						if(found == null) {
							continue;
						}
						if(item.Value) {
							if(!found.Tags.Contains(tag)) {
								found.Tags.Add(tag);
							}
							if(found.IsDefault) {
								FollowingToggle.UncheckDefault();
							}
						} else {
							if(found.Tags.Contains(tag)) {
								found.Tags.Remove(tag);
							}
						}
					}
				};
				BlackToggle.OnToggled += () => {
					FollowingToggle.UncheckDefault();
				};

			}
			initializing = false;
		}

	}
}
