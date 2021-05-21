using E621Downloader.Models;
using E621Downloader.Models.Download;
using E621Downloader.Views;
using E621Downloader.Views.DownloadSection;
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

namespace E621Downloader.Pages.DownloadSection {
	public sealed partial class DownloadingSection: Page {

		public DownloadingSection() {
			this.InitializeComponent();
			this.NavigationCacheMode = NavigationCacheMode.Enabled;
			foreach(DownloadsGroup group in DownloadsManager.groups) {
				MainGridView.Items.Add(new DownloadBlock(group));
			}
			//for(int i = 0; i < 15; i++) {
			//	MainGridView.Items.Add(new DownloadBlock(null));
			//}
		}

		protected async override void OnNavigatedTo(NavigationEventArgs e) {
			base.OnNavigatedTo(e);
			//List<DownloadInstance> list = await Local.GetDownloadsInfo();

		}

		private void MainGridView_ItemClick(object sender, ItemClickEventArgs e) {
			DownloadBlock block = e.ClickedItem as DownloadBlock;
			DownloadPage.Instance.NavigateTo(typeof(DownloadDetailsPage), block.Group.downloads);
		}
	}
}
