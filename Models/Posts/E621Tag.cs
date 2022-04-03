using E621Downloader.Models.Networks;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

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
			return string.Join(", ", tags).Trim().ToLower();
		}

		public static string GetCategory(int category) {
			return category switch {
				0 => "General",
				1 => "Artists",
				2 => "Not Found",
				3 => "Copyrights",
				4 => "Artists",
				5 => "Species",
				6 => "Invalid",
				7 => "Meta",
				8 => "Lore",
				_ => "UnKnown",
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
}
