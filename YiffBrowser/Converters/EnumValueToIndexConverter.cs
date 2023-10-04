using System;
using Windows.UI.Xaml.Data;

namespace YiffBrowser.Converters {
	public class EnumValueToIndexConverter : IValueConverter {
		public object Convert(object value, Type targetType, object parameter, string language) {
			return (int)value;
		}

		public object ConvertBack(object value, Type targetType, object parameter, string language) {
			return value;
		}
	}
}
