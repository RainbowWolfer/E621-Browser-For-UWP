using E621Downloader.Models.Locals;
using System;
using System.Threading;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;

namespace E621Downloader.Views {
	public sealed partial class ImagesSizeDialog: UserControl {
		private bool _loaded = false;
		public event Action<bool, double> UpdateImagesLayout;
		public ImagesSizeDialog() {
			this.InitializeComponent();
			if(LocalSettings.Current.adaptiveGrid) {
				AdaptiveHeightRadioButton.IsChecked = true;
				AdaptiveHeightSlider.IsEnabled = true;
				FixedHeightSlider.IsEnabled = false;
			} else {
				FixedHeightRadioButton.IsChecked = true;
				AdaptiveHeightSlider.IsEnabled = false;
				FixedHeightSlider.IsEnabled = true;
			}
			AdaptiveHeightSlider.Value = LocalSettings.Current.adaptiveSizeMultiplier;
			FixedHeightSlider.Value = LocalSettings.Current.fixedHeight;
			_loaded = true;
		}

		private void FixedHeightRadioButton_Checked(object sender, RoutedEventArgs e) {
			if(!_loaded) {
				return;
			}
			RadioButton button = (RadioButton)sender;
			if(button.IsChecked.Value) {
				FixedHeightSlider.IsEnabled = true;
				AdaptiveHeightSlider.IsEnabled = false;
				LocalSettings.Current.adaptiveGrid = false;
				LocalSettings.Save();
				UpdateImagesLayout?.Invoke(false, FixedHeightSlider.Value);
			}
		}

		private void AdaptiveHeightRadioButton_Checked(object sender, RoutedEventArgs e) {
			if(!_loaded) {
				return;
			}
			RadioButton button = (RadioButton)sender;
			if(button.IsChecked.Value) {
				FixedHeightSlider.IsEnabled = false;
				AdaptiveHeightSlider.IsEnabled = true;
				LocalSettings.Current.adaptiveGrid = true;
				LocalSettings.Save();
				UpdateImagesLayout?.Invoke(true, AdaptiveHeightSlider.Value);
			}
		}

		private void FixedHeightSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e) {
			if(!_loaded) {
				return;
			}
			UpdateImagesLayout?.Invoke(false, FixedHeightSlider.Value);
			SaveValue(e.NewValue, false);
		}

		private void AdaptiveHeightSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e) {
			if(!_loaded) {
				return;
			}
			UpdateImagesLayout?.Invoke(true, AdaptiveHeightSlider.Value);
			SaveValue(e.NewValue, true);
		}

		private CancellationTokenSource cts;
		private async void SaveValue(double value, bool isAdaptive) {
			if(cts != null) {
				cts.Cancel();
				cts.Dispose();
			}
			cts = new CancellationTokenSource();
			try {
				await Task.Delay(1000, cts.Token);
				value = Math.Round(value, 2);
				if(isAdaptive) {
					LocalSettings.Current.adaptiveSizeMultiplier = value;
				} else {
					LocalSettings.Current.fixedHeight = value;
				}
				LocalSettings.Save();
			} catch(TaskCanceledException) { }
		}
	}
}
