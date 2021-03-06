using E621Downloader.Models;
using E621Downloader.Models.Locals;
using E621Downloader.Models.Posts;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;

namespace E621Downloader.Views {
	public sealed partial class CurrentPoolInformation: UserControl {
		public E621Pool Pool { get; private set; }
		public string ID => $"#{Pool.id}";
		public string Description => string.IsNullOrWhiteSpace(Pool.description) ? "None".Language() : Pool.description;
		private bool initializing = true;
		public CurrentPoolInformation(E621Pool pool) {
			this.InitializeComponent();
			Pool = pool;
			if(Local.Listing.CheckFollowingList(pool.Tag)) {
				FollowButton.IsChecked = true;
			}
			initializing = false;
		}

		private async void FollowButton_Click(object sender, RoutedEventArgs e) {
			if(initializing) {
				return;
			}
			bool isOn = (sender as ToggleButton).IsChecked.Value;
			FollowText.Text = isOn ? "Following".Language() : "Follow".Language();
			if(isOn) {
				if(!Local.Listing.CheckFollowingList(Pool.Tag)) {
					await Local.Listing.AddFollowingList(Pool.Tag);
				}
				if(Local.Listing.CheckBlackList(Pool.Tag)) {
					await Local.Listing.RemoveBlackList(Pool.Tag);
				}
			} else {
				if(Local.Listing.CheckFollowingList(Pool.Tag)) {
					await Local.Listing.RemoveFollowingList(Pool.Tag);
				}
			}
		}
	}
}
