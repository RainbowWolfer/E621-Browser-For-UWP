using E621Downloader.Models;
using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;

namespace E621Downloader.Views {
	public sealed partial class ResizeBar: UserControl {
		private int size;
		private int minimum;
		private int maximum;
		private int step;

		public int GetSize() => size;

		public void SetSize(int value, bool invokeSave = true) {
			size = value;
			OnSizeChanged?.Invoke(value, invokeSave);
		}

		public int Minimum {
			get => minimum;
			set {
				minimum = value;
				internalChange = true;
				SizeSlider.Minimum = minimum;
				internalChange = false;
			}
		}

		public int Maximum {
			get => maximum;
			set {
				maximum = value;
				internalChange = true;
				SizeSlider.Maximum = maximum;
				internalChange = false;
			}
		}

		public int Step {
			get => step;
			set {
				step = value;
				internalChange = true;
				SizeSlider.StepFrequency = step;
				internalChange = false;
			}
		}

		public event Action<int, bool> OnSizeChanged;

		private bool initialized = false;
		private bool internalChange = false;
		public ResizeBar() {
			this.InitializeComponent();
			initialized = true;
		}

		private void SizeButton_Click(object sender, RoutedEventArgs e) {
			if(size + Step > Maximum) {
				SetSize(Minimum);
			} else {
				SetSize(size + Step);
			}
			SizeSlider.Value = size;
		}

		private void SizeButton_RightTapped(object sender, Windows.UI.Xaml.Input.RightTappedRoutedEventArgs e) {
			if(size - Step < MinHeight) {
				SetSize(Maximum);
			} else {
				SetSize(size - Step);
			}
			SizeSlider.Value = size;
		}

		private void SizeSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e) {
			if(!initialized) {
				return;
			}
			SetSize((int)e.NewValue, !internalChange);
			ToolTipService.SetToolTip(SizeButton, "Current Size".Language() + " : " + size);
		}

		public void UpdateValue() {
			internalChange = true;
			SizeSlider.Value = size;
			internalChange = false;
		}
	}
}
