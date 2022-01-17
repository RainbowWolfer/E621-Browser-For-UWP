using E621Downloader.Pages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E621Downloader.Models.Locals {
	public class FavoritesList {
		public static List<FavoritesList> Lists { get; set; } = new List<FavoritesList>();
		public static void AddList(string name) {
			Lists.Add(new FavoritesList(name));
		}
		public static async void Save() {
			await Local.WriteFavoritesLists();
		}

		public string Name { get; private set; } = "Undefined";
		private readonly List<FavoriteItem> content = new List<FavoriteItem>();

		public FavoritesList(string name, IEnumerable<FavoriteItem> content) {
			Name = name;
			this.content = content.ToList();
		}

		public FavoritesList(string name) {
			Name = name;
		}

		public void AddPostID(string post_id) {
			content.Add(new FavoriteItem(PathType.PostID, post_id));
		}

		public void AddLocal(string local) {
			content.Add(new FavoriteItem(PathType.Local, local));
		}

		public FavoriteItem[] GetContent() => content.ToArray();


	}

	public class FavoriteItem {
		public PathType Type { get; private set; }
		public string Path { get; private set; }
		public FavoriteItem(PathType type, string path) {
			Type = type;
			Path = path;
		}
	}
	public class PathTypeException: Exception {
		public PathTypeException() : base("Path Type Error") { }
	}
}
