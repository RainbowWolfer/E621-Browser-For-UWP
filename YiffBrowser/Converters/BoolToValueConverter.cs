using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;

namespace YiffBrowser.Converters {
	public abstract class BoolToValueConverter<T> : IValueConverter {
		public T TrueValue { get; set; }
		public T FalseValue { get; set; }

		public virtual object Convert(object value, Type targetType, object parameter, string language) {
			if (value is bool b) {
				return b ? TrueValue : FalseValue;
			}
			return value;
		}

		public object ConvertBack(object value, Type targetType, object parameter, string language) {
			throw new NotSupportedException();
		}
	}


	public class BoolToStringConverter : BoolToValueConverter<string> { }
	public class BoolToNumberConverter : BoolToValueConverter<double> { }
	public class BoolToBrushConverter : BoolToValueConverter<Brush> { }
	public class BoolToThicknessConverter : BoolToValueConverter<Thickness> { }
	public class BoolToCornerRadiusConverter : BoolToValueConverter<CornerRadius> { }
	public class BoolToSplitViewDisplayModeConverter : BoolToValueConverter<SplitViewDisplayMode> { }


}
