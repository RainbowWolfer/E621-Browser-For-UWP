using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace E621Downloader.Views {
	public sealed partial class PagesSelector: UserControl {
		private bool initializing = true;

		private int minimum = 1;
		private int maximum = 750;
		private int currentPage;

		public int Minimum {
			get => minimum;
			set {
				minimum = value;
				FromSlider.Minimum = minimum;
				ToSlider.Minimum = minimum;
			}
		}

		public int Maximum {
			get => maximum;
			set {
				maximum = value;
				FromSlider.Maximum = maximum - 2;
				ToSlider.Maximum = maximum - 2;
			}
		}

		public int CurrentPage {
			get => currentPage;
			set {
				currentPage = value;
				int gap = Maximum - Minimum;
				FromSlider.Value = currentPage;
				ToSlider.Value = gap - currentPage;
			}
		}

		public PagesSelector() {
			this.InitializeComponent();
			initializing = false;
		}

		private void CurrentPageButton_Tapped(object sender, TappedRoutedEventArgs e) {
			int gap = Maximum - Minimum;
			FromSlider.Value = CurrentPage;
			ToSlider.Value = gap - CurrentPage;
		}

		private void FromSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e) {
			if(initializing) {
				return;
			}
			int gap = Maximum - Minimum;
			if(gap - (int)FromSlider.Value < (int)ToSlider.Value) {
				ToSlider.Value = gap - (int)FromSlider.Value;
			}
			UpdateScoreLimit();
		}

		private void ToSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e) {
			if(initializing) {
				return;
			}
			int gap = Maximum - Minimum;
			if((int)FromSlider.Value > gap - (int)ToSlider.Value) {
				FromSlider.Value = gap - (int)ToSlider.Value;
			}
			UpdateScoreLimit();
		}


		public (int from, int to) GetRange() {
			int gap = Maximum - Minimum;
			int from = (int)FromSlider.Value;
			int to = (int)(gap - ToSlider.Value);
			return (from, to);
		}

		private void UpdateScoreLimit() {
			var (from, to) = GetRange();
			if(from == to) {
				RangeText.Text = $"( Page: {from} )";
			} else {
				RangeText.Text = $"( Page: {from} - {to} )";
			}
		}

	}
}
