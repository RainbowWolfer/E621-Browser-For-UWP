using System;
using Windows.UI;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;

namespace YiffBrowser.Converters {
	public class ColorToBrushConverter : IValueConverter {
		public object Convert(object value, Type targetType, object parameter, string language) {
			if (value is Color color) {
				return new SolidColorBrush(color);
			}
			return value;
		}

		public object ConvertBack(object value, Type targetType, object parameter, string language) {
			throw new NotSupportedException();
		}
	}
}
