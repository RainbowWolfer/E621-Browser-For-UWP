using Windows.ApplicationModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using YiffBrowser.Database;
using YiffBrowser.Models.E621;

namespace YiffBrowser {
	/// <summary>
	/// YiffBrowser
	/// </summary>
	public static class YiffApp {
		public static Application Current => Application.Current;

		#region Style

		public static Style DialogStyle => Current.Resources["DefaultContentDialogStyle"] as Style;
		public static Brush TextBoxDefaultBorderBrush => Current.Resources["TextControlBorderBrush"] as Brush;

		#endregion


		#region User

		public static E621User User { get; set; }
		public static E621Post AvatarPost { get; set; }

		#endregion

		public static void Initialize() {
			DataAccess.InitializeDatabase();
		}

		//todo
		public static bool IsDarkTheme() {
			return false;
		}

		public static string GetResourcesString(string uri) {
			return $"ms-appx:///YiffBrowser/Resources/{uri.TrimStart('/')}";
		}

		public static string GetAppVersion() {
			Package package = Package.Current;
			PackageId packageId = package.Id;
			PackageVersion version = packageId.Version;

			return $"{version.Major}.{version.Minor}.{version.Build}.{version.Revision}";
		}
	}
}
