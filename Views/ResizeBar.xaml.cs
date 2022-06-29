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

		public int Size {
			get => size;
			set {
				size = value;
				OnSizeChanged?.Invoke(value);
			}
		}

		public int Minimum {
			get => minimum;
			set {
				minimum = value;
				SizeSlider.Minimum = minimum;
			}
		}

		public int Maximum {
			get => maximum;
			set {
				maximum = value;
				SizeSlider.Maximum = maximum;
			}
		}

		public int Step {
			get => step;
			set {
				step = value;
				SizeSlider.StepFrequency = step;
			}
		}

		public event Action<int> OnSizeChanged;

		public ResizeBar() {
			this.InitializeComponent();
		}

		private void SizeButton_Click(object sender, RoutedEventArgs e) {
			if(Size + Step > Maximum) {
				Size = Minimum;
			} else {
				Size += Step;
			}
			SizeSlider.Value = Size;
		}

		private void SizeSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e) {
			Size = (int)e.NewValue;
			ToolTipService.SetToolTip(SizeButton, "Current Size".Language() + " : " + Size);
		}
	}
}
