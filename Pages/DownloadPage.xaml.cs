using E621Downloader.Models;
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
using Windows.UI.Xaml.Navigation;

namespace E621Downloader.Pages {
	public sealed partial class DownloadPage: Page {

		public readonly ObservableCollection<DownloadInstance> instances;

		public DownloadPage() {
			this.InitializeComponent();
			this.instances = new ObservableCollection<DownloadInstance>();
			this.DataContextChanged += (s, e) => {
				Bindings.Update();
			};
			Load();
		}

		private async void Load() {
			foreach(DownloadInstance item in DownloadsManager.downloads) {
				instances.Add(item);
			}
			foreach(var item in instances) {
				item.AddDownloadingCallBack(e => {
					Debug.WriteLine(item.PostRef.id + " : " + item.DownloadProgress);

					ulong received = item.Operation.Progress.BytesReceived;
					ulong total = item.Operation.Progress.TotalBytesToReceive;
					if(total == 0) {
						item.DownloadProgress = 0;
					} else {
						item.DownloadProgress = received / (double)total;
					}
					if(item.DownloadProgress == 1) {
						Debug.WriteLine("Finish");
					}
				});
			}

			IReadOnlyList<DownloadOperation> list = await BackgroundDownloader.GetCurrentDownloadsAsync();
			//list[0].Progress.BytesReceived
		}
	}
}
