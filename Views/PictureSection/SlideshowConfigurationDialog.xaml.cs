using E621Downloader.Models.Locals;
using E621Downloader.Pages;
using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;

namespace E621Downloader.Views.PictureSection {
	public sealed partial class SlideshowConfigurationDialog: UserControl {
		public SlideshowConfiguration Configuration { get; private set; }
		public SlideshowConfigurationDialog() {
			this.InitializeComponent();
			Configuration = LocalSettings.Current.slideshowConfiguration;
			InitializeSetings();
		}

		private void InitializeSetings() {
			SecondsSlider.Value = Configuration.SecondsInterval;
			SecondsText.Text = Configuration.SecondsInterval.ToString();
			RandomCheckBox.IsChecked = Configuration.IsRandom;
		}

		private void SecondsSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e) {
			if(Configuration == null) {
				return;
			}
			Configuration.SecondsInterval = Math.Round(e.NewValue, 1);
			SecondsText.Text = Configuration.SecondsInterval.ToString();
		}

		private void RandomCheckBox_Click(object sender, RoutedEventArgs e) {
			if(Configuration == null) {
				return;
			}
			Configuration.IsRandom = RandomCheckBox.IsChecked == true;
		}
	}
}
