using System;
using Windows.UI.Xaml.Controls;

namespace E621Downloader.Views {
	public sealed partial class LoadingDialog: UserControl {
		private string dialogContent;

		public string DialogContent {
			get => dialogContent;
			set {
				dialogContent = value;
				ContentDialog.Text = value;
			}
		}

		public Action OnCancel { get; set; }

		public LoadingDialog() {
			this.InitializeComponent();
		}

		private void DialogRoot_CloseButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args) {
			OnCancel?.Invoke();
		}
	}
}
