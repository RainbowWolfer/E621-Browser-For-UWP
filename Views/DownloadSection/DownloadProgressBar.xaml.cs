using E621Downloader.Models;
using E621Downloader.Models.Download;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
	public sealed partial class DownloadProgressBar: UserControl {
		public DownloadInstance Instance { get; private set; }
		public DownloadProgressBar(DownloadInstance instance) {
			this.InitializeComponent();
			Instance = instance;

			NameTextBlock.Text = "Posts: " + instance.PostRef.id;

			instance.StartDownload(e => {
				Debug.WriteLine(Instance.PostRef.id + " : " + Instance.Progress);

				InfoTextBlock.Text = instance.PostRef.file.url;

				PercentageTextBlok.Text = string.Format("{0}%   {1} KB / {2} KB", Instance.Percentage, instance.ReceivedKB, instance.TotalKB);
				MyProgressBar.Value = instance.Progress;
			});
		}

	}
}
