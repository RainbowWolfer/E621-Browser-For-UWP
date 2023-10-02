using System;
using Windows.UI.Xaml.Data;

namespace YiffBrowser.Converters {
	internal class ObjectToVisibilityConverter : IValueConverter {
		public object Convert(object value, Type targetType, object parameter, string language) {
			return value is not null;
		}

		public object ConvertBack(object value, Type targetType, object parameter, string language) {
			throw new NotSupportedException();
		}
	}
}
