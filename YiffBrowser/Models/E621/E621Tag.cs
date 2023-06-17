using Microsoft.Toolkit.Uwp.Helpers;
using System;
using System.Collections.Generic;
using Windows.UI;

namespace YiffBrowser.Models.E621 {
	public class E621Tag {
		public int id;
		public string name;
		public int post_count;
		public string related_tags;
		public DateTime related_tags_updated_at;
		public int category;
		public bool is_locked;
		public DateTime created_at;
		public DateTime updated_at;

		public static Dictionary<string, E621Tag> Pool { get; } = new();

		public override string ToString() {
			return $"E621Tags:({id})({name})({related_tags})({post_count})({category})";
		}

		public static string GetCategory(int category) {
			return category switch {
				0 => "General",
				1 => "Artists",
				2 => "Not Found",
				3 => "Copyrights",
				4 => "Characters",
				5 => "Species",
				6 => "Invalid",
				7 => "Meta",
				8 => "Lore",
				_ => "UnKnown",
			};
		}

		public static Color GetCatrgoryColor(E621TagCategory category) {
			bool isDarkTheme = App.IsDarkTheme();
			return category switch {
				E621TagCategory.Artists => (isDarkTheme ? "#F2AC08" : "#E39B00").ToColor(),
				E621TagCategory.Copyrights => (isDarkTheme ? "#DD00DD" : "#DD00DD").ToColor(),
				E621TagCategory.Species => (isDarkTheme ? "#ED5D1F" : "#ED5D1F").ToColor(),
				E621TagCategory.Characters => (isDarkTheme ? "#00AA00" : "#00AA00").ToColor(),
				E621TagCategory.General => (isDarkTheme ? "#B4C7D9" : "#0B7EE2").ToColor(),
				E621TagCategory.Meta => (isDarkTheme ? "#FFFFFF" : "#000000").ToColor(),
				E621TagCategory.Invalid => (isDarkTheme ? "#FF3D3D" : "#FF3D3D").ToColor(),
				E621TagCategory.Lore => (isDarkTheme ? "#228822" : "#228822").ToColor(),
				E621TagCategory.NotFound => (isDarkTheme ? "#B85277" : "#B40249").ToColor(),
				E621TagCategory.UnKnown => (isDarkTheme ? "#CCCBF9" : "#050507").ToColor(),
				_ => (isDarkTheme ? "#FFFFFF" : "#000000").ToColor(),
			};
		}

		public static Color GetCatrgoryColor(int category) {
			return GetCatrgoryColor((E621TagCategory)category);
		}
	}

	public enum E621TagCategory {
		General = 0,
		Artists = 1,
		NotFound = 2,
		Characters = 3,
		Copyrights = 4,
		Species = 5,
		Invalid = 6,
		Meta = 7,
		Lore = 8,
		UnKnown = 9,
	}
}