using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using YiffBrowser.Models.E621;

namespace YiffBrowser {
	/// <summary>
	/// YiffBrowser
	/// </summary>
	public static class App {
		public static Application Current => Application.Current;

		#region Style

		public static Style DialogStyle => Current.Resources["DefaultContentDialogStyle"] as Style;
		public static Brush TextBoxDefaultBorderBrush => Current.Resources["TextControlBorderBrush"] as Brush;

		#endregion


		#region User

		public static E621User User { get; set; }
		public static E621Post AvatarPost { get; set; }

		#endregion

		//todo
		public static bool IsDarkTheme() {
			return false;
		}

		public static string GetResourcesString(string uri) {
			return $"ms-appx:///YiffBrowser/Resources/{uri.TrimStart('/')}";
		}

	}
}
