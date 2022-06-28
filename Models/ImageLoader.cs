using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media.Imaging;

namespace E621Downloader.Models {
	public class ImageLoader {
		public List<ImageItem> Images { get; private set; }
		public ImageLoader() {
			Images = new List<ImageItem>();
		}

		//public BitmapImage Get
	}

	public class ImageItem {
		public string PostID { get; private set; }
		public BitmapImage Preview { get; set; } = null;
		public BitmapImage Sample { get; set; } = null;
		public BitmapImage File { get; set; } = null;

		public ImageItem(string postID) {
			PostID = postID;
		}
	}
}
