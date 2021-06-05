using E621Downloader.Models;
using E621Downloader.Models.Download;
using E621Downloader.Models.Locals;
using E621Downloader.Views.DownloadSection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Networking.BackgroundTransfer;
using Windows.System;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace E621Downloader.Pages.DownloadSection {
	public sealed partial class DownloadDetailsPage: Page {
		public DownloadsGroup group;
		public List<DownloadInstance> list;
		public DownloadDetailsPage() {
			this.InitializeComponent();
			list = new List<DownloadInstance>();
			//this.NavigationCacheMode = NavigationCacheMode.Enabled;
		}
		protected override void OnNavigatedTo(NavigationEventArgs e) {
			base.OnNavigatedTo(e);
			if(e.Parameter == null) {
				return;
			}
			group = e.Parameter as DownloadsGroup;
			list = group.downloads;
			Refresh();
		}
		public void Refresh(Predicate<DownloadInstance> p = null) {
			MyListView.Items.Clear();
			foreach(DownloadInstance item in list) {
				if(p == null || p.Invoke(item)) {
					MyListView.Items.Add(new DownloadProgressBar(this, item));
				}
			}
			UpdateDownloadsInfo();
		}

		private MyTag currentTag = MyTag.All;

		private void RadioButton_Checked(object sender, RoutedEventArgs e) {
			if((sender as RadioButton).Tag == null) {
				return;
			}
			int index = int.Parse((sender as RadioButton).Tag as string);
			currentTag = (MyTag)index;
			switch(currentTag) {
				case MyTag.All:
					Refresh(p => true);
					break;
				case MyTag.Downloading:
					Refresh(p => p.Status != BackgroundTransferStatus.Completed);
					break;
				case MyTag.Downloaded:
					Refresh(p => p.Status == BackgroundTransferStatus.Completed);
					break;
				default:
					throw new Exception();
			}
		}

		public void MoveToDownloaded(DownloadInstance instance) {
			if(currentTag == MyTag.Downloading) {
				for(int i = 0; i < MyListView.Items.Count; i++) {
					var bar = MyListView.Items[i] as DownloadProgressBar;
					if(bar.Instance == instance) {
						MyListView.Items.RemoveAt(i);
						break;
					}
				}
			} else if(currentTag == MyTag.Downloaded) {
				MyListView.Items.Add(new DownloadProgressBar(this, instance));
			}
		}

		public void UpdateDownloadsInfo() {
			DownloadsInfoBlock.Text = $"Downloads {list.Count(d => d.Status == BackgroundTransferStatus.Completed)}/{list.Count}";
		}

		private async void OpenFolderButton_Tapped(object sender, TappedRoutedEventArgs e) {
			await Launcher.LaunchFolderAsync(await Local.DownloadFolder.GetFolderAsync(group.Title), new FolderLauncherOptions() {
				DesiredRemainingView = ViewSizePreference.UseMore
			});
		}

		private enum MyTag {
			All = 0, Downloading = 1, Downloaded = 2,
		}
	}
}
