using E621Downloader.Models.Locals;
using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;

namespace E621Downloader.Views.PictureSection {
	public sealed partial class SlideshowConfigurationDialog: UserControl {
		private bool initialized = false;
		public SlideshowConfigurationDialog() {
			this.InitializeComponent();
			InitializeSetings();
		}

		private void InitializeSetings() {
			SecondsSlider.Value = LocalSettings.Current.slideshowConfiguration.secondsInterval;
			SecondsText.Text = LocalSettings.Current.slideshowConfiguration.secondsInterval.ToString();
			RandomCheckBox.IsChecked = LocalSettings.Current.slideshowConfiguration.isRandom;
			initialized = true;
		}

		private void SecondsSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e) {
			if(!initialized) {
				return;
			}
			LocalSettings.Current.slideshowConfiguration.secondsInterval = Math.Round(e.NewValue, 1);
			SecondsText.Text = LocalSettings.Current.slideshowConfiguration.secondsInterval.ToString();
		}

		private void RandomCheckBox_Click(object sender, RoutedEventArgs e) {
			if(!initialized) {
				return;
			}
			LocalSettings.Current.slideshowConfiguration.isRandom = RandomCheckBox.IsChecked == true;
		}
	}
}
