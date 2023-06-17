﻿using Windows.UI.Xaml.Data;
using System;

namespace YiffBrowser.Converters {
	public class NullToBoolConverter : IValueConverter {
		public object Convert(object value, Type targetType, object parameter, string language) {
			return value is null;
		}

		public object ConvertBack(object value, Type targetType, object parameter, string language) {
			throw new NotSupportedException();
		}
	}
}
