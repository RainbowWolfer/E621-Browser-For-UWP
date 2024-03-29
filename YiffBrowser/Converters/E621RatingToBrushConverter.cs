﻿using System;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;
using YiffBrowser.Helpers;
using YiffBrowser.Models.E621;

namespace YiffBrowser.Converters {
	public class E621RatingToBrushConverter : IValueConverter {
		public object Convert(object value, Type targetType, object parameter, string language) {
			if (value is E621Rating rating) {
				return new SolidColorBrush(E621Helpers.GetRatingColor(rating));
			}
			return value;
		}

		public object ConvertBack(object value, Type targetType, object parameter, string language) {
			throw new NotSupportedException();
		}
	}
}
