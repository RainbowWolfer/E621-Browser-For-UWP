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
	public sealed partial class DownloadBlock: UserControl {
		public DownloadsGroup Group { get; private set; }

		public SimpleDownloadProgressBar[] Bars => new SimpleDownloadProgressBar[] {
			Bar1, Bar2, Bar3, Bar4, Bar5, Bar6, Bar7
		};

		public DownloadBlock(DownloadsGroup group) {
			this.InitializeComponent();
			this.DataContextChanged += (s, e) => Bindings.Update();
			this.Group = group;

			int i;
			for(i = 0; i < Math.Min(group.downloads.Count, 7); i++) {
				DownloadInstance instance = group.downloads[i];
				SimpleDownloadProgressBar b = Bars[i];

				b.SetBarValue(instance.DownloadProgress);
				b.SetIcon(instance);
				b.SetVisible(true);
				instance.DownloadingAction = (p) => {
					b.SetBarValue(instance.DownloadProgress);
					b.SetIcon(instance);
				};
			}

			for(int j = i; j < 7; j++) {//rest
				Bars[j].SetVisible(false);
			}
		}

		public void Update(){

		}

	}
}
