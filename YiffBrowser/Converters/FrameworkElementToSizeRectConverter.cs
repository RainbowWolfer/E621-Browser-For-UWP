using System;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace YiffBrowser.Converters {
	internal class FrameworkElementToSizeRectConverter : IValueConverter {
		public object Convert(object value, Type targetType, object parameter, string language) {
			if (value is FrameworkElement element) {
				return new Rect(0, 0, element.ActualWidth, element.ActualHeight);
			}

			return Rect.Empty;
		}

		public object ConvertBack(object value, Type targetType, object parameter, string language) {
			throw new NotSupportedException();
		}
	}
}
