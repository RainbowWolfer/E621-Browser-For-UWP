using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls.Primitives;

namespace YiffBrowser.Views.Controls.CustomControls {
	public class LockableToggleButton : ToggleButton {
		protected override void OnToggle() {
			if (!LockToggle) {
				base.OnToggle();
			}
		}

		public bool LockToggle {
			get => (bool)GetValue(LockToggleProperty);
			set => SetValue(LockToggleProperty, value);
		}

		public static readonly DependencyProperty LockToggleProperty = DependencyProperty.Register(
			nameof(LockToggle),
			typeof(bool),
			typeof(LockableToggleButton),
			new PropertyMetadata(false)
		);
	}
}
