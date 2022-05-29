using E621Downloader.Models;
using E621Downloader.Models.Download;
using E621Downloader.Pages.DownloadSection;
using System;
using System.Collections.Generic;
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
using Windows.UI.Xaml.Navigation;

namespace E621Downloader.Views.DownloadSection {
	public sealed partial class DownloadBlock: UserControl {
		private const int MAXCOLS = 7;
		private readonly DownloadOverview parent;
		public DownloadsGroup Group { get; private set; }

		public List<SimpleDownloadProgressBar> Bars => new() {
			Bar1, Bar2, Bar3, Bar4, Bar5, Bar6, Bar7
		};

		public DownloadBlock(DownloadOverview parent, DownloadsGroup group) {
			this.InitializeComponent();
			this.DataContextChanged += (s, e) => Bindings.Update();
			this.parent = parent;
			this.Group = group;
			UpdateCount();

			Bars.ForEach(b => b.SetParent(this));
			foreach(DownloadInstance item in Group.downloads) {
				item.DedicatedDownloadingAction = (p) => {
					UpdateCount();
					if(IsAllCompleted()) {
						UpdateDisplay();
					}
				};
			}
			UpdateDisplay();
		}

		public void UpdateDisplay() {
			if(Group.downloads.Count <= MAXCOLS) {
				int i;
				for(i = 0; i < Group.downloads.Count; i++) {
					Bars[i].SetTarget(Group.downloads[i]);
				}
				for(int j = i; j < MAXCOLS; j++) {//rest
					Bars[j].SetVisible(false);
				}
			} else {
				var filtered = Group.downloads.Where(d => d.Status != BackgroundTransferStatus.Completed).ToList();
				if(filtered.Count <= MAXCOLS) {
					filtered.AddRange(Group.downloads.Where(d => d.Status == BackgroundTransferStatus.Completed));
				}
				for(int i = 0; i < MAXCOLS; i++) {
					Bars[i].SetTarget(filtered[i]);
				}
			}
		}

		public bool IsAllCompleted() {
			foreach(SimpleDownloadProgressBar item in Bars) {
				if(item.Instance.Status != BackgroundTransferStatus.Completed) {
					return false;
				}
			}
			return true;
		}

		public void UpdateCount() {
			int l = Group.downloads.Count(d => d.metaFile.FinishedDownloading);
			int r = Group.downloads.Count();
			CountOverview.Text = $"Downloading... {l}/{r}";
		}

		private void GoToLibraryItem_Click(object sender, RoutedEventArgs e) {
			MainPage.NavigateToLibrary(Group.Title);
		}

		private void DeleteItem_Click(object sender, RoutedEventArgs e) {
			DownloadsManager.RemoveDownloads(Group);
			parent.Remove(this);
		}
	}
}
