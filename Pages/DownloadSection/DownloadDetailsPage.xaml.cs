using E621Downloader.Models.Download;
using E621Downloader.Views.DownloadSection;
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

namespace E621Downloader.Pages.DownloadSection {
	public sealed partial class DownloadDetailsPage: Page {
		public DownloadDetailsPage() {
			this.InitializeComponent();
			//this.NavigationCacheMode = NavigationCacheMode.Enabled;
		}
		protected override void OnNavigatedTo(NavigationEventArgs e) {
			base.OnNavigatedTo(e);
			if(e.Parameter == null) {
				return;
			}
			var list = e.Parameter as List<DownloadInstance>;
			//Debug.WriteLine(list.Count);
			foreach(DownloadInstance item in list) {
				MyListView.Items.Add(new DownloadProgressBar(item));
			}
		}
	}
}
