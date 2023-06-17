using Windows.UI.Xaml.Data;
using System;

namespace YiffBrowser.Converters {
	public class NotNullToBoolConverter : IValueConverter {
		public object Convert(object value, Type targetType, object parameter, string language) {
			return value is not null;
		}

		public object ConvertBack(object value, Type targetType, object parameter, string language) {
			throw new NotSupportedException();
		}
	}
}
