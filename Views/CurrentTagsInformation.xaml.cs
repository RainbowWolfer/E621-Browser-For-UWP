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
			FollowButton.IsEnabled = filtered.Length != 0;
			BlockButton.IsEnabled = filtered.Length != 0;
		}

		private void FollowButton_Click(object sender, RoutedEventArgs e) {
			bool isOn = (sender as ToggleButton).IsChecked.Value;
			FollowText.Text = isOn ? "Following" : "Follow";
			string tag = string.Join(", ", filtered);
			if(isOn) {
				if(!Local.CheckFollowList(tag)) {
					Local.AddFollowList(tag);
				}
				if(Local.CheckBlackList(tag)) {
					Local.RemoveBlackList(tag);
				}
			} else {
				if(Local.CheckFollowList(tag)) {
					Local.RemoveFollowList(tag);
				}
			}
			BlockButton.IsChecked = false;
			BlockText.Text = "Block";
		}

		private void BlockButton_Click(object sender, RoutedEventArgs e) {
			bool isOn = (sender as ToggleButton).IsChecked.Value;
			BlockText.Text = isOn ? "Blocking" : "Block";
			string tag = string.Join(", ", filtered);
			if(isOn) {
				if(!Local.CheckBlackList(tag)) {
					Local.AddBlackList(tag);
				}
				if(Local.CheckFollowList(tag)) {
					Local.RemoveFollowList(tag);
				}
			} else {
				if(Local.CheckBlackList(tag)) {
					Local.AddBlackList(tag);
				}
			}
			FollowButton.IsChecked = false;
			FollowText.Text = "Follow";
		}
	}
}
