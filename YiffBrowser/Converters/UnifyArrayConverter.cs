using Windows.UI.Xaml.Data;
using System;
using System.Collections;
using System.Linq;
using YiffBrowser.Models.E621;

namespace YiffBrowser.Converters {
	public abstract class UnifyArrayConverter<T> : IValueConverter {
		public object Convert(object value, Type targetType, object parameter, string language) {
			if (value is IList list) {
				return Enumerable.Range(0, list.Count).Select(i => list[i]).Cast<T>().ToArray();
			}
			return value;
		}

		public object ConvertBack(object value, Type targetType, object parameter, string language) {
			throw new NotSupportedException();
		}
	}

	public class UnifyE621PostsConverter : UnifyArrayConverter<E621Post> { }
}
