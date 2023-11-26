using System;
using System.Collections;
using Windows.UI.Xaml.Data;
using YiffBrowser.Helpers;

namespace YiffBrowser.Converters {
	public class ArrayToCountConverter : IValueConverter {
		public object Convert(object value, Type targetType, object parameter, string language) {
			if (value is IEnumerable e) {
				return e.Count().ToString();
			}
			return (-1).ToString();
		}

		public object ConvertBack(object value, Type targetType, object parameter, string language) {
			throw new NotSupportedException();
		}
	}
}
