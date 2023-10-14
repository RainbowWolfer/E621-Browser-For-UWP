using System;
using Windows.Storage.FileProperties;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media.Imaging;

namespace YiffBrowser.Converters {
	internal class ThumbnailToImageConverter : IValueConverter {
		public object Convert(object value, Type targetType, object parameter, string language) {
			BitmapImage image = new();

			if (value is StorageItemThumbnail thumbnail) {
				thumbnail.Seek(0);
				image.SetSource(thumbnail);
			}

			return image;
		}

		public object ConvertBack(object value, Type targetType, object parameter, string language) {
			throw new NotSupportedException();
		}
	}
}
