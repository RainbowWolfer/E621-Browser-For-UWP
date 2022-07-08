using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml;

namespace E621Downloader.Views {
	public class UniformBorderGrid: Grid{
		public double BorderWidth {
			get { return (double)GetValue(BorderWidthProperty); }
			set { SetValue(BorderWidthProperty, value); }
		}

		public static readonly DependencyProperty BorderWidthProperty =
			DependencyProperty.Register("BorderWidth", typeof(double), typeof(UniformBorderGrid), new PropertyMetadata(0.0));

		public UniformBorderGrid() {
			this.SetBinding(Control.BorderThicknessProperty, new Binding() {
				Source = this,
				Path = new PropertyPath("BorderWidth"),
				Converter = new UniformThicknessConverter(),
				Mode = BindingMode.OneWay
			});
		}

		class UniformThicknessConverter: IValueConverter {
			public object Convert(object value, Type targetType, object parameter, string language) {
				return new Thickness((double)value);
			}

			public object ConvertBack(object value, Type targetType, object parameter, string language) {
				throw new NotImplementedException();
			}
		}
	}
}
