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
	public sealed partial class CurrentPoolInformation: UserControl {
		public E621Pool Pool { get; private set; }
		public string ID => $"#{Pool.id}";
		public string Description => string.IsNullOrWhiteSpace(Pool.description) ? "None" : Pool.description;
		private bool initializing = true;
		public CurrentPoolInformation(E621Pool pool) {
			this.InitializeComponent();
			Pool = pool;
			if(Local.CheckFollowList(pool.Tag)) {
				FollowButton.IsChecked = true;
			}
			initializing = false;
		}

		private void FollowButton_Click(object sender, RoutedEventArgs e) {
			if(initializing) {
				return;
			}
			bool isOn = (sender as ToggleButton).IsChecked.Value;
			FollowText.Text = isOn ? "Following" : "Follow";
			if(isOn) {
				if(!Local.CheckFollowList(Pool.Tag)) {
					Local.AddFollowList(Pool.Tag);
				}
				if(Local.CheckBlackList(Pool.Tag)) {
					Local.RemoveBlackList(Pool.Tag);
				}
			} else {
				if(Local.CheckFollowList(Pool.Tag)) {
					Local.RemoveFollowList(Pool.Tag);
				}
			}
		}
	}
}
