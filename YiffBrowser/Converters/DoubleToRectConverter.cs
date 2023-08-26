using System;
using Windows.Foundation;
using Windows.UI.Xaml.Data;

namespace YiffBrowser.Converters {
	internal class DoubleToRectConverter : IValueConverter {
		public object Convert(object value, Type targetType, object parameter, string language) {
			// Check if the value is a double
			if (value is double width) {
				// Return a new Rect with the given width and height
				return new Rect(0, 0, width, width);
			} else {
				// Return an empty Rect
				return Rect.Empty;
			}
		}

		public object ConvertBack(object value, Type targetType, object parameter, string language) {
			throw new NotSupportedException();
		}
	}
}
