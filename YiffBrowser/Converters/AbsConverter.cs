using System;
using Windows.UI.Xaml.Data;
using YiffBrowser.Helpers;

namespace YiffBrowser.Converters {
	public class AbsConverter : IValueConverter {
		public object Convert(object value, Type targetType, object parameter, string language) {
			return CommonHelpers.Abs(value);
		}

		public object ConvertBack(object value, Type targetType, object parameter, string language) {
			return value;
		}


	}
}
