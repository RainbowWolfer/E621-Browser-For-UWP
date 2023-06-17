﻿using Windows.UI.Xaml.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YiffBrowser.Helpers;

namespace YiffBrowser.Converters {
	public class AbsConverter : IValueConverter {
		public object Convert(object value, Type targetType, object parameter, string language) {
			return CommonHelpers.Abs(value);
		}

		public object ConvertBack(object value, Type targetType, object parameter, string language) {
			return value;
		}


	}
}
