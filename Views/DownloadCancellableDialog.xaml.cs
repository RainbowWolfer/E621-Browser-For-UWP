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
	public sealed partial class DownloadCancellableDialog: UserControl {
		private string text;

		public string Text {
			get => text;
			set {
				text = value;
				MainText.Text = text;
			}
		}

		public Action OnCancel { get; set; }

		public DownloadCancellableDialog() {
			this.InitializeComponent();
		}

		private void CancelButton_Click(object sender, RoutedEventArgs e) {
			OnCancel?.Invoke();
			CancelButton.IsEnabled = false;
		}
	}
}
