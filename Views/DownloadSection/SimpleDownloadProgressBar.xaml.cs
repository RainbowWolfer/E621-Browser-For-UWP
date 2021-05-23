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

namespace E621Downloader.Views.DownloadSection {
	public sealed partial class SimpleDownloadProgressBar: UserControl {
		public const char icon_complete = '\uE10B';
		public const char icon_downloading = '\uE118';
		public const char icon_paused = '\uE103';
		public const char icon_error = '\uE783';

		public SimpleDownloadProgressBar() {
			this.InitializeComponent();
		}

		public void SetBarValue(int percentage) {
			MyProgressBar.Value = percentage;
		}
	}
}
