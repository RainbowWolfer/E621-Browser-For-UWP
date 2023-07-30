using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using YiffBrowser.Helpers;

namespace YiffBrowser.Converters {
	internal class CountToVisibilityConverter : IValueConverter {
		public object Convert(object value, Type targetType, object parameter, string language) {
			if (value is int count) {
				return (count > 0).ToVisibility();
			}

			return Visibility.Collapsed;
		}

		public object ConvertBack(object value, Type targetType, object parameter, string language) {
			throw new NotSupportedException();
		}
	}
}
