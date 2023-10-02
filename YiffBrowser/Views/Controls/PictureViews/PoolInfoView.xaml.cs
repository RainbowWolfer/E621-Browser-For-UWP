using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using YiffBrowser.Helpers;
using YiffBrowser.Models.E621;
using YiffBrowser.Services.Locals;

namespace YiffBrowser.Views.Controls.PictureViews {
	public sealed partial class PoolInfoView : UserControl {



		public E621Pool E621Pool {
			get => (E621Pool)GetValue(E621PoolProperty);
			set => SetValue(E621PoolProperty, value);
		}

		public static readonly DependencyProperty E621PoolProperty = DependencyProperty.Register(
			nameof(E621Pool),
			typeof(E621Pool),
			typeof(PoolInfoView),
			new PropertyMetadata(null, OnE621PoolChanged)
		);

		private static void OnE621PoolChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
			if (d is not PoolInfoView view) {
				return;
			}

			view.IsFollowing = Local.Listing.ContainPool((E621Pool)e.NewValue);
		}

		public bool IsFollowing {
			get => (bool)GetValue(IsFollowingProperty);
			set => SetValue(IsFollowingProperty, value);
		}

		public static readonly DependencyProperty IsFollowingProperty = DependencyProperty.Register(
			nameof(IsFollowing),
			typeof(bool),
			typeof(PoolInfoView),
			new PropertyMetadata(false, OnIsFollowingChanged)
		);

		private static void OnIsFollowingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
			if (d is not PoolInfoView view) {
				return;
			}

		}

		public PoolInfoView() {
			this.InitializeComponent();
		}

		public static async Task ShowAsDialog(E621Pool pool) {
			await new PoolInfoView() {
				E621Pool = pool,
			}.CreateContentDialog(new ContentDialogParameters() {
				CloseText = "Back",
				Title = $"Pool Info",
			}).ShowAsyncSafe();
		}

		private void FollowToggleButton_Click(object sender, RoutedEventArgs e) {
			if (IsFollowing) {
				Local.Listing.AddToPool(E621Pool);
			} else {
				Local.Listing.RemoveFromPool(E621Pool);
			}
			Listing.Write();
		}
	}
}
