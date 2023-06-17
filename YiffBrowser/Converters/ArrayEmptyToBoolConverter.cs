using Microsoft.UI.Xaml;
using Windows.UI.Xaml.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YiffBrowser.Helpers;
using Windows.UI.Xaml;

namespace YiffBrowser.Converters {
	public class ArrayEmptyToBoolConverter : IValueConverter {
		public object Convert(object value, Type targetType, object parameter, string language) {
			if (value == null) {
				return Visibility.Visible;
			}
			if (value is IEnumerable ie) {
				return ie.IsEmpty();
			} else if (value is int count) {
				return count == 0;
			}
			return value;
		}

		public object ConvertBack(object value, Type targetType, object parameter, string language) {
			throw new NotSupportedException();
		}
	}
}
