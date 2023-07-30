using System;
using Windows.UI.Xaml.Data;

namespace YiffBrowser.Converters {
	internal class CountToBoolConverter : IValueConverter {
		public object Convert(object value, Type targetType, object parameter, string language) {
			if (value is int count) {
				return count > 0;
			}


			return false;
		}

		public object ConvertBack(object value, Type targetType, object parameter, string language) {
			throw new NotSupportedException();
		}
	}
}
