using Microsoft.UI.Xaml.Controls;
using Windows.UI.Xaml;

namespace E621Downloader.Models {
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
					} else if(0 < value && value <= 100) {
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
					} else if(0 < value && value <= 100) {
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
			this.ring = null;
		}
		public ProgressLoader(ProgressRing ring) {
			this.bar = null;
			this.ring = ring;
		}

	}
}
