using Microsoft.Toolkit.Uwp.Helpers;
using Windows.UI;
using YiffBrowser.Models.E621;

namespace YiffBrowser.Helpers {
	public static class E621Helpers {

		public static Color GetRatingColor(this E621Rating rating) {
			bool isDark = YiffApp.IsDarkTheme();
			return rating switch {
				E621Rating.Safe => (isDark ? "#008000" : "#36973E").ToColor(),
				E621Rating.Questionable => (isDark ? "#FFFF00" : "#EFC50C").ToColor(),
				E621Rating.Explicit => (isDark ? "#FF0000" : "#C92A2D").ToColor(),
				_ => (isDark ? "#FFF" : "#000").ToColor(),
			};
		}

		public static string GetRatingIcon(this E621Rating rating) {
			return rating switch {
				E621Rating.Safe => "\uF78C",
				E621Rating.Questionable => "\uE897",
				E621Rating.Explicit => "\uE814",
				_ => "\uE8BB",
			};
		}

	}
}
