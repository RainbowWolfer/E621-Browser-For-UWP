using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;

namespace YiffBrowser.Views.Controls.CustomControls {
	public sealed class IconTextHorizontal : Control {

		public string Glyph {
			get => (string)GetValue(GlyphProperty);
			set => SetValue(GlyphProperty, value);
		}

		public static readonly DependencyProperty GlyphProperty = DependencyProperty.Register(
			nameof(Glyph),
			typeof(string),
			typeof(IconTextHorizontal),
			new PropertyMetadata(string.Empty)
		);

		public string Text {
			get => (string)GetValue(TextProperty);
			set => SetValue(TextProperty, value);
		}

		public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
			nameof(Text),
			typeof(string),
			typeof(IconTextHorizontal),
			new PropertyMetadata(string.Empty)
		);

		public double Spacing {
			get => (double)GetValue(SpacingProperty);
			set => SetValue(SpacingProperty, value);
		}

		public static readonly DependencyProperty SpacingProperty = DependencyProperty.Register(
			nameof(Spacing),
			typeof(double),
			typeof(IconTextHorizontal),
			new PropertyMetadata(10d)
		);

		public double TextWidth {
			get => (double)GetValue(TextWidthProperty);
			set => SetValue(TextWidthProperty, value);
		}

		public static readonly DependencyProperty TextWidthProperty = DependencyProperty.Register(
			nameof(TextWidth),
			typeof(double),
			typeof(IconTextHorizontal),
			new PropertyMetadata(double.NaN)
		);

		public TextAlignment TextAlignment {
			get => (TextAlignment)GetValue(TextAlignmentProperty);
			set => SetValue(TextAlignmentProperty, value);
		}

		public static readonly DependencyProperty TextAlignmentProperty = DependencyProperty.Register(
			nameof(TextAlignment),
			typeof(TextAlignment),
			typeof(IconTextHorizontal),
			new PropertyMetadata(TextAlignment.Center)
		);

		public IconTextHorizontal() {
			DefaultStyleKey = typeof(IconTextHorizontal);
		}

	}

	internal class DoubleToThicknessConverter : IValueConverter {
		public object Convert(object value, Type targetType, object parameter, string language) {
			if (value is double d) {
				return new Thickness(d, 0, 0, 0);
			}
			return new Thickness();
		}

		public object ConvertBack(object value, Type targetType, object parameter, string language) {
			if (value is Thickness thickness) {
				return thickness.Left;
			}
			return 0;
		}
	}

}
