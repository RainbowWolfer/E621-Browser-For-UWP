using System;
using Windows.UI.Xaml.Data;
using YiffBrowser.Helpers;

namespace YiffBrowser.Converters {
	public class NullToVisibilityConverter : IValueConverter {
		public object Convert(object value, Type targetType, object parameter, string language) {
			return (value is null).ToVisibility();
		}

		public object ConvertBack(object value, Type targetType, object parameter, string language) {
			throw new NotSupportedException();
		}
	}
}
