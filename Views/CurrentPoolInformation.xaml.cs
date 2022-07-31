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
			if(Local.Listing.CheckFollowPool(pool.id)) {
				FollowButton.IsChecked = true;
				UpdateFollowButton(true);
			}
			initializing = false;
		}

		private void UpdateFollowButton(bool on){
			FollowText.Text = on ? "Following".Language() : "Follow".Language();
			FollowIcon.Glyph = on ? "\uEB52" : "\uEB51";
		}

		private async void FollowButton_Click(object sender, RoutedEventArgs e) {
			if(initializing) {
				return;
			}
			bool isOn = (sender as ToggleButton).IsChecked.Value;
			UpdateFollowButton(isOn);
			if(isOn) {
				if(!Local.Listing.CheckFollowPool(Pool.id)) {
					await Local.Listing.AddFollowPool(Pool.id);
				}
			} else {
				if(Local.Listing.CheckFollowPool(Pool.id)) {
					await Local.Listing.AddFollowPool(Pool.id);
				}
			}
		}
	}
}
