using Windows.UI.Xaml.Data;
using System;

namespace YiffBrowser.Converters {
	public class BoolToBoolReConverter : IValueConverter {
		public object Convert(object value, Type targetType, object parameter, string language) {
			if (value is bool boolValue) {
				return !boolValue;
			}
			return value;
		}

		public object ConvertBack(object value, Type targetType, object parameter, string language) {
			if (value is bool boolValue) {
				return !boolValue;
			}
			return value;
		}
	}
}
