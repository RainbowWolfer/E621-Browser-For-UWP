using E621Downloader.Models;
using E621Downloader.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Networking.BackgroundTransfer;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

namespace E621Downloader.Pages.DownloadSection {
	public sealed partial class DownloadPage: Page {
		public static DownloadPage Instance;

		//public readonly ObservableCollection<DownloadProgressBar> bars;

		public DownloadPage() {
			Instance = this;
			this.InitializeComponent();
			this.NavigationCacheMode = NavigationCacheMode.Enabled;
			//this.bars = new ObservableCollection<DownloadProgressBar>();
			//this.DataContextChanged += (s, e) => Bindings.Update();
			//Load();
		}

		protected override void OnNavigatedTo(NavigationEventArgs e) {
			base.OnNavigatedTo(e);
			//check bars

			//foreach(DownloadInstance item in DownloadsManager.downloads) {
			//	bool contian = false;
			//	foreach(DownloadProgressBar bar in bars) {
			//		if(bar.Instance == item) {
			//			contian = true;
			//			break;
			//		}
			//	}
			//	if(!contian) {
			//		var bar = new DownloadProgressBar(item);
			//		bars.Insert(0, bar);
			//		MyListView.Items.Insert(0, bar);
			//	}
			//}
		}

		//private void Load() {
		//	foreach(DownloadInstance item in DownloadsManager.downloads) {
		//		var bar = new DownloadProgressBar(item);
		//		bars.Insert(0, bar);
		//		//MyListView.Items.Insert(0, bar);
		//	}
		//}

		private void Pivot_SelectionChanged(object sender, SelectionChangedEventArgs e) {
			string to = (e.AddedItems[0] as PivotItem).Header as string;
			if(to == "Downloading") {
				MainFrame.Navigate(typeof(DownloadingSection), null, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromLeft });
			} else if(to == "Downloaded") {
				MainFrame.Navigate(typeof(DownloadedSection), null, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromRight });
			}
		}

		private void AutoSuggestBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args) {

		}
	}
}
