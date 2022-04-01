using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace E621Downloader.Views {
	public sealed partial class AddButtonInput: UserControl {
		public event Action<string> OnTextSubmit;

		private readonly Brush origin_box_border_brush;

		public List<string> Existing { get; set; } = new List<string>();

		public string PlaceholderText { get; set; }

		private bool isInitializing = true;
		public AddButtonInput() {
			this.InitializeComponent();
			origin_box_border_brush = Box.BorderBrush;
			Initialize();
		}

		public void Initialize() {
			isInitializing = true;
			Box.Text = "";
			Box.BorderBrush = origin_box_border_brush;
			isInitializing = false;
		}

		private void Box_TextChanging(TextBox sender, TextBoxTextChangingEventArgs args) {
			if(isInitializing) {
				return;
			}
			string text = Box.Text.Trim();
			if(string.IsNullOrWhiteSpace(text) || Existing.Contains(text)) {
				Box.BorderBrush = new SolidColorBrush(Colors.Red);
			} else {
				Box.BorderBrush = origin_box_border_brush;
			}
		}

		private void BoxAddButton_Click(object sender, RoutedEventArgs e) {
			Submit();
		}

		private void Box_KeyDown(object sender, KeyRoutedEventArgs e) {
			if(e.Key == VirtualKey.Enter) {
				Submit();
			}
		}

		private void Submit() {
			if(Warn()) {
				return;
			}
			OnTextSubmit?.Invoke(Box.Text.Trim());
			Initialize();
		}

		private bool Warn() {
			string text = Box.Text;
			if(string.IsNullOrWhiteSpace(text)) {
				WarningTip.Subtitle = "Cannot be empty";
				WarningTip.IsOpen = true;
				return true;
			} else if(Existing.Contains(text)) {
				WarningTip.Subtitle = "Already existed";
				WarningTip.IsOpen = true;
				return true;
			}
			return false;
		}

	}
}
