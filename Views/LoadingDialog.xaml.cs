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
