using System;
using System.Diagnostics;
using Windows.UI.Xaml.Data;

namespace YiffBrowser.Converters {
	internal class NumberDividedBy2Converter : IValueConverter {
		public object Convert(object value, Type targetType, object parameter, string language) {
			try {
				double d = System.Convert.ToDouble(value);
				return d / 2;
			} catch (Exception ex) {
				Debug.WriteLine(ex);
				return 0d;
			}
		}

		public object ConvertBack(object value, Type targetType, object parameter, string language) {
			try {
				double d = System.Convert.ToDouble(value);
				return d * 2;
			} catch (Exception ex) {
				Debug.WriteLine(ex);
				return 0d;
			}
		}
	}
}
