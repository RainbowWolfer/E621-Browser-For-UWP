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
	public sealed partial class DownloadOverview: Page {
		public static DownloadOverview Instance;

		public DownloadOverview() {
			Instance = this;
			this.InitializeComponent();
			this.NavigationCacheMode = NavigationCacheMode.Enabled;

			Refresh();
		}

		protected override void OnNavigatedTo(NavigationEventArgs e) {
			base.OnNavigatedTo(e);
			//List<DownloadInstance> list = await Local.GetDownloadsInfo();
			Refresh();
		}

		public void Refresh() {
			MainGridView.Items.Clear();
			foreach(DownloadsGroup group in DownloadsManager.groups) {
				MainGridView.Items.Add(new DownloadBlock(group) {
					Height = 300,
					Width = 300,
					Margin = new Thickness(5),
					Padding = new Thickness(20),
				});
			}
			TitleTextBlock.Visibility = MainGridView.Items.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
		}

		private void MainGridView_ItemClick(object sender, ItemClickEventArgs e) {
			DownloadBlock block = e.ClickedItem as DownloadBlock;
			DownloadPage.Instance.NavigateTo(typeof(DownloadDetailsPage), block.Group);
			DownloadPage.Instance.EnableTitleButton(true);
			DownloadPage.Instance.SelectTitle(block.Group.Title);

		}
	}
}
