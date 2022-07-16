using E621Downloader.Models.Locals;
using E621Downloader.Models.Posts;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace E621Downloader.Views {
	public sealed partial class CurrentTagsInformation: UserControl {
		private readonly string[] tags;
		private readonly string[] filtered;

		private readonly ContentDialog dialog;

		public CurrentTagsInformation(string[] tags, ContentDialog dialog) {
			this.InitializeComponent();
			this.dialog = dialog;
			if(tags.Length == 0) {
				this.tags = tags;
				ExpanderPanel.Children.Add(new TagExpander("") {
					IsExpanded = true,
					Margin = new Thickness(5),
				});
			} else {
				this.tags = E621Tag.SortOutMetatags(tags);
				for(int i = 0; i < this.tags.Length; i++) {
					ExpanderPanel.Children.Add(new TagExpander(this.tags[i]) {
						IsExpanded = true,
						Margin = new Thickness(5),
					});
				}
			}
			filtered = E621Tag.FilterMetatags(this.tags);
			string tag = E621Tag.JoinTags(filtered);
			if(filtered.Length == 0) {
				FollowingToggle.IsEnabled = false;
				BlackToggle.IsEnabled = false;
				FollowingToggle.IsChecked = false;
				BlackToggle.IsChecked = false;
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
				FollowingToggle.OnToggled += BlackToggle.UncheckDefault;

				var black_list = Local.Listing.LocalBlackLists.ToList();
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
				BlackToggle.OnToggled += FollowingToggle.UncheckDefault;
			}
		}

		private void FollowingToggle_OnSettingsClick() {
			dialog.Hide();
		}

		private void BlackToggle_OnSettingsClick() {
			dialog.Hide();
		}
	}
}
