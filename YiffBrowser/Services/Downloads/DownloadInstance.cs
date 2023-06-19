using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Networking.BackgroundTransfer;
using YiffBrowser.Models.E621;

namespace YiffBrowser.Services.Downloads {
	internal class DownloadInstance {

		public E621Post Post { get; }
		public DownloadOperation Download { get; }

		public DownloadInstance(E621Post post, DownloadOperation download) {
			Post = post;
			Download = download;
		}



	}
}
