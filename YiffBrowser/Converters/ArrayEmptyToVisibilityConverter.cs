﻿using System;
using System.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using YiffBrowser.Helpers;

namespace YiffBrowser.Converters {
	public class ArrayEmptyToVisibilityConverter : IValueConverter {
		public object Convert(object value, Type targetType, object parameter, string language) {
			if (value == null) {
				return Visibility.Visible;
			}
			if (value is IEnumerable ie) {
				return ie.IsEmpty().ToVisibility();
			} else if (value is int count) {
				return (count == 0).ToVisibility();
			}
			return value;
		}

		public object ConvertBack(object value, Type targetType, object parameter, string language) {
			throw new NotSupportedException();
		}
	}
}
