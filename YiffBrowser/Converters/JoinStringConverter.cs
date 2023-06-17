using Windows.UI.Xaml.Data;
using System;
using System.Collections.Generic;

namespace YiffBrowser.Converters {
	public class JoinStringConverter : IValueConverter {
		public string Separator { get; set; } = " ";

		public object Convert(object value, Type targetType, object parameter, string language) {
			if (value is IEnumerable<string> strs) {
				return string.Join(Separator, strs).Trim();
			}
			return value;
		}

		public object ConvertBack(object value, Type targetType, object parameter, string language) {
			throw new NotSupportedException();
		}
	}
}
