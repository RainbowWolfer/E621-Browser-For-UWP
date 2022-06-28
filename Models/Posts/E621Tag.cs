using E621Downloader.Models.Networks;
using Microsoft.Toolkit.Uwp.Helpers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.UI;

namespace E621Downloader.Models.Posts {
	public class E621Tag {
		public static async Task<E621Tag[]> GetAsync(string tag, CancellationToken? token = null) {
			tag = tag.ToLower().Trim();
			string url = $"https://{Data.GetHost()}/tags.json?search[name_matches]={tag}";
			HttpResult<string> result = await Data.ReadURLAsync(url, token);
			if(result.Result == HttpResultType.Success && result.Content != "{\"tags\":[]}") {
				return JsonConvert.DeserializeObject<E621Tag[]>(result.Content);
			} else {
				return new E621Tag[] { GetDefault(tag) };
			}
		}

		public static async Task<E621Tag> GetFirstAsync(string tag, CancellationToken? token = null) {
			return (await GetAsync(tag, token))?.FirstOrDefault();
		}

		public static E621Tag GetDefault(string tag) => new() {
			id = 0,
			name = tag,
			post_count = 0,
			related_tags = "",
			related_tags_updated_at = DateTime.Now,
			category = 0,
			is_locked = false,
			created_at = DateTime.Now,
			updated_at = DateTime.Now,
		};

		public static bool CheckMetatag(string tag) => tag.Contains(":");
		public static string[] FilterMetatags(string[] tags) {
			return tags.Where(t => !CheckMetatag(t)).ToArray();
		}

		public static string[] SortOutMetatags(string[] tags) {
			List<string> result = new();
			foreach(string item in tags) {
				if(!CheckMetatag(item)) {
					result.Add(item);
				}
			}
			foreach(string item in tags) {
				if(CheckMetatag(item)) {
					result.Add(item);
				}
			}
			return result.ToArray();
		}

		public static string JoinTags(params string[] tags) {
			if(tags == null) {
				return "";
			}
			return string.Join(", ", tags).Trim().ToLower();
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

		public static TagCategory GetTagCategory(int category) {
			return category switch {
				0 => TagCategory.General,
				1 => TagCategory.Artists,
				2 => TagCategory.NotFound,
				3 => TagCategory.Copyrights,
				4 => TagCategory.Characters,
				5 => TagCategory.Species,
				6 => TagCategory.Invalid,
				7 => TagCategory.Meta,
				8 => TagCategory.Lore,
				_ => TagCategory.UnKnown,
			};
		}

		public static Color GetCatrgoryColor(TagCategory category, bool isDarkTheme = true) {
			return category switch {
				TagCategory.Artists => (isDarkTheme ? "#F2AC08" : "#E39B00").ToColor(),
				TagCategory.Copyrights => (isDarkTheme ? "#DD00DD" : "#DD00DD").ToColor(),
				TagCategory.Species => (isDarkTheme ? "#ED5D1F" : "#ED5D1F").ToColor(),
				TagCategory.Characters => (isDarkTheme ? "#00AA00" : "#00AA00").ToColor(),
				TagCategory.General => (isDarkTheme ? "#B4C7D9" : "#0B7EE2").ToColor(),
				TagCategory.Meta => (isDarkTheme ? "#FFFFFF" : "#000000").ToColor(),
				TagCategory.Invalid => (isDarkTheme ? "#FF3D3D" : "#FF3D3D").ToColor(),
				TagCategory.Lore => (isDarkTheme ? "#228822" : "#228822").ToColor(),
				TagCategory.NotFound => (isDarkTheme ? "#B85277" : "#B40249").ToColor(),
				TagCategory.UnKnown => (isDarkTheme ? "#CCCBF9" : "#050507").ToColor(),
				_ => (isDarkTheme ? "#FFFFFF" : "#000000").ToColor(),
			};
		}

		public int id;
		public string name;
		public int post_count;
		public string related_tags;
		public DateTime related_tags_updated_at;
		public int category;
		public bool is_locked;
		public DateTime created_at;
		public DateTime updated_at;

		public delegate void OnWikiLoadedEventHandler();
		public event OnWikiLoadedEventHandler OnWikiLoaded;
		public bool IsWikiLoaded { get; private set; } = false;
		public E621Wiki[] Wikis { get; private set; } = Array.Empty<E621Wiki>();
		public E621Wiki Wiki => Wikis?.FirstOrDefault();
		public async Task<E621Wiki> LoadWikiAsync(CancellationToken token = default) {
			this.Wikis = await E621Wiki.GetAsync(name, token);
			if(Wikis != null && Wiki != null) {
				OnWikiLoaded?.Invoke();
				IsWikiLoaded = true;
			}
			return this.Wiki;
		}

		public override string ToString() {
			return $"E621Tags:({id})({name})({related_tags})({post_count})({category})";
		}
	}

	public enum TagCategory {
		General,
		Artists,
		NotFound,
		Characters,
		Copyrights,
		Species,
		Invalid,
		Meta,
		Lore,
		UnKnown,
	}
}
