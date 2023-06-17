using Microsoft.UI.Xaml;
using Windows.UI.Xaml.Data;
using System;
using Windows.UI.Xaml;

namespace YiffBrowser.Converters {
	public class BoolToVisibilityConverter : IValueConverter {
		public object Convert(object value, Type targetType, object parameter, string language) {
			return value != null && (bool)value ? Visibility.Visible : Visibility.Collapsed;
		}

		public object ConvertBack(object value, Type targetType, object parameter, string language) {
			throw new NotSupportedException();
		}
	}
}
