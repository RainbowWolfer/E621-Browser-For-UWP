using System;
using Windows.UI.Xaml.Data;

namespace YiffBrowser.Converters {
	public class SubStringConverter : IValueConverter {
		public int Length { get; set; }

		public object Convert(object value, Type targetType, object parameter, string language) {
			return value.ToString().Substring(0, Length);
		}

		public object ConvertBack(object value, Type targetType, object parameter, string language) {
			throw new NotSupportedException();
		}
	}
}
