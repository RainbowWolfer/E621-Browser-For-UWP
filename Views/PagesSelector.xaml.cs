﻿using System;
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
				FromSlider.Maximum = maximum;
				ToSlider.Maximum = maximum;
			}
		}

		public int CurrentPage {
			get => currentPage;
			set {
				currentPage = value;
				FromSlider.Value = currentPage;
				ToSlider.Value = Gap - currentPage;
				UpdateScoreLimit();
			}
		}

		private int Gap => Maximum - Minimum + 2;

		public PagesSelector() {
			this.InitializeComponent();
			initializing = false;
		}

		private void CurrentPageButton_Tapped(object sender, TappedRoutedEventArgs e) {
			FromSlider.Value = CurrentPage;
			ToSlider.Value = Gap - CurrentPage;
		}

		private void FromSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e) {
			if(initializing) {
				return;
			}
			if(Gap - (int)FromSlider.Value < (int)ToSlider.Value) {
				ToSlider.Value = Gap - (int)FromSlider.Value;
			}
			UpdateScoreLimit();
		}

		private void ToSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e) {
			if(initializing) {
				return;
			}
			if((int)FromSlider.Value > Gap - (int)ToSlider.Value) {
				FromSlider.Value = Gap - (int)ToSlider.Value;
			}
			UpdateScoreLimit();
		}


		public (int from, int to) GetRange() {
			int from = (int)FromSlider.Value;
			int to = (int)(Gap - ToSlider.Value);
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
