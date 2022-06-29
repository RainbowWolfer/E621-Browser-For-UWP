using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

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
