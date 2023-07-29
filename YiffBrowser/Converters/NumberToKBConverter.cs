using Windows.UI.Xaml.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YiffBrowser.Helpers;

namespace YiffBrowser.Converters {
	public class NumberToKBConverter : IValueConverter {
		public object Convert(object value, Type targetType, object parameter, string language) {
			int number = (int)CommonHelpers.Abs(value);
			if (parameter != null) {
				return number.FileSizeToKB(true);
			} else {
				return number.FileSizeToKB();
			}
		}

		public object ConvertBack(object value, Type targetType, object parameter, string language) {
			return value;
		}
	}
}
