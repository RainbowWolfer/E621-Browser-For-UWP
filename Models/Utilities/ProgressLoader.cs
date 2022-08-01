using Microsoft.UI.Xaml.Controls;
using Windows.UI.Xaml;

namespace E621Downloader.Models.Utilities {
	public class ProgressLoader {
		private readonly ProgressBar bar;
		private readonly ProgressRing ring;

		private int? value;
		public int? Value {
			get => value;
			set {
				this.value = value;
				if(bar != null) {
					if(value == null) {
						bar.Visibility = Visibility.Collapsed;
					} else if(value is > 0 and <= 100) {
						bar.Visibility = Visibility.Visible;
						bar.IsIndeterminate = false;
						bar.Value = (double)value;
					} else {
						bar.Visibility = Visibility.Visible;
						bar.IsIndeterminate = true;
					}
				} else if(ring != null) {
					if(value == null) {
						ring.Visibility = Visibility.Collapsed;
					} else if(value is > 0 and <= 100) {
						ring.Visibility = Visibility.Visible;
						ring.IsIndeterminate = false;
						ring.Value = (double)value;
					} else {
						ring.Visibility = Visibility.Visible;
						ring.IsIndeterminate = true;
					}
				}
			}
		}

		public ProgressLoader(ProgressBar bar) {
			this.bar = bar;
			ring = null;
		}
		public ProgressLoader(ProgressRing ring) {
			bar = null;
			this.ring = ring;
		}

	}
}
