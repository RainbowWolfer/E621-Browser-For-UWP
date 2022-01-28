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
	public sealed partial class ResizeBar: UserControl {
		private int size;
		public int Size {
			get => size;
			set {
				size = value;
				OnSizeChanged?.Invoke(value);
			}
		}

		public event Action<int> OnSizeChanged;

		public ResizeBar() {
			this.InitializeComponent();
		}

		private void SizeButton_Click(object sender, RoutedEventArgs e) {
			if(Size + 25 > 500) {
				Size = 200;
			} else {
				Size += 25;
			}
			SizeSlider.Value = Size;
		}

		private void SizeSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e) {
			Size = (int)e.NewValue;
			ToolTipService.SetToolTip(SizeButton, "Current Size : " + Size);
		}
	}
}
