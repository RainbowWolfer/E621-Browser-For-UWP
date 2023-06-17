using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using YiffBrowser.Models.E621;

namespace YiffBrowser {
	public static class App {
		public static Application Current => Application.Current;

		#region Style

		public static Style DialogStyle => Current.Resources["DefaultContentDialogStyle"] as Style;
		public static Brush TextBoxDefaultBorderBrush => (Brush)Current.Resources["TextControlBorderBrush"];

		#endregion


		#region User

		public static E621User User { get; set; }
		public static E621Post AvatarPost { get; set; }

		#endregion

		//todo
		internal static bool IsDarkTheme() {
			return false;
		}

	}
}
