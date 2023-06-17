using Microsoft.UI.Xaml;
using Windows.UI.Xaml.Data;
using System;
using System.Collections;
using YiffBrowser.Helpers;
using Windows.UI.Xaml;

namespace YiffBrowser.Converters {
	public class ArrayNotEmptyToBoolConverter : IValueConverter {
		public object Convert(object value, Type targetType, object parameter, string language) {
			if (value == null) {
				return Visibility.Collapsed;
			}

			if (value is IEnumerable ie) {
				return ie.IsNotEmpty();
			} else if (value is int count) {
				return count != 0;
			}
			return value;
		}

		public object ConvertBack(object value, Type targetType, object parameter, string language) {
			throw new NotSupportedException();
		}
	}
}
