using E621Downloader.Models;
using E621Downloader.Models.Download;
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
	public sealed partial class DownloadBlock: UserControl {
		public DownloadsGroup Group { get; private set; }
		public DownloadBlock(DownloadsGroup group) {
			this.InitializeComponent();
			this.DataContextChanged += (s, e) => Bindings.Update();
			Group = group;
			foreach(DownloadInstance item in Group.downloads) {
				item.DownloadingAction += () => {
					Bar1.SetBarValue(item.Percentage);
				};
			}
		}
	}
}
