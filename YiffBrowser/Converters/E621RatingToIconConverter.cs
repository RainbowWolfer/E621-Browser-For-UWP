using Windows.UI.Xaml.Data;
using System;
using YiffBrowser.Helpers;
using YiffBrowser.Models.E621;

namespace YiffBrowser.Converters {
	public class E621RatingToIconConverter : IValueConverter {
		public object Convert(object value, Type targetType, object parameter, string language) {
			if (value is E621Rating rating) {
				return E621Helpers.GetRatingIcon(rating);
			}
			return value;
		}

		public object ConvertBack(object value, Type targetType, object parameter, string language) {
			throw new NotSupportedException();
		}
	}
}
