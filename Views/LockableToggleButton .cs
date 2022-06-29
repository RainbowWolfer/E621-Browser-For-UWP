using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls.Primitives;

namespace E621Downloader.Views {
	public class LockableToggleButton: ToggleButton {
		protected override void OnToggle() {
			if(!LockToggle) {
				base.OnToggle();
			}
		}

		public bool LockToggle {
			get => (bool)GetValue(LockToggleProperty);
			set {
				SetValue(LockToggleProperty, value);
			}
		}

		// Using a DependencyProperty as the backing store for LockToggle.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty LockToggleProperty =
			DependencyProperty.Register("LockToggle", typeof(bool), typeof(LockableToggleButton), new PropertyMetadata(false));
	}
}