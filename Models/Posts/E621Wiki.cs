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
	public class E621Wiki {
		public static async Task<E621Wiki[]> GetAsync(string tag, CancellationToken? token = null) {
			tag = tag.ToLower().Trim();
			if(wikiDictionary.ContainsKey(tag)) {
				E621Wiki found = GetDefault(tag);
				found.body = wikiDictionary[tag];
				return new E621Wiki[] { found };
			}
			if(tag.StartsWith("fav:")) {
				E621Wiki found = GetDefault(tag);
				found.body = $"Favorites of \"{tag.Substring(4)}\"";
				return new E621Wiki[] { found };
			}
			string url = $"https://e621.net/wiki_pages.json?search[title]={tag}";
			HttpResult<string> result = await Data.ReadURLAsync(url, token);
			if(result.Result == HttpResultType.Success) {
				if(result.Content == "[]") {
					return new E621Wiki[] { GetDefault(tag) };
				}
				return JsonConvert.DeserializeObject<E621Wiki[]>(result.Content);
			} else {
				return new E621Wiki[] { GetDefault(tag) };
			}
		}

		public static E621Wiki GetDefault(string tag) => new E621Wiki() {
			id = 0,
			created_at = DateTime.Now,
			updated_at = DateTime.Now,
			title = tag,
			body = "No Wiki Found",
			creator_id = 0,
			is_locked = false,
			updater_id = 0,
			is_deleted = false,
			other_names = new List<object>(),
			creator_name = "",
			category_name = 0,
		};

		public int id;
		public DateTime created_at;
		public DateTime updated_at;
		public string title;
		public string body;
		public int creator_id;
		public bool is_locked;
		public object updater_id;
		public bool is_deleted;
		public List<object> other_names;
		public string creator_name;
		public int category_name;


		public static readonly Dictionary<string, string> wikiDictionary = new Dictionary<string, string>() {
			{ "order:score", "Highest score first" },
			{ "order:id", "Oldest to newest" },
			{ "order:favcount", "Most favorites first" },
			{ "order:tagcount", "Most tags first" },
			{ "order:comment_count", "Most comments first" },
			{ "order:mpixels", "Largest resolution first" },
			{ "order:filesize", "Largest file size first" },
			{ "order:landscape", "Wide and short to tall and thin" },
			{ "order:change", "Sorts by last update sequence" },
			{ "order:duration", "Video duration longest to shortest" },
			{ "order:random", "Orders posts randomly" },
			{ "order:score_asc", "Lowest score first" },
			{ "order:favcount_asc", "order:favcount_asc" },
			{ "order:tagcount_asc", "Least tags first" },
			{ "order:comment_count_asc", "Least comments first" },
			{ "order:mpixels_asc", "Smallest resolution first" },
			{ "order:filesize_asc", "Smallest file size first" },
			{ "order:portrait", "Tall and thin to wide and short" },
			{ "order:duration_asc", "Video duration shortest to longest" },
			{ "order:rank", "Hot Posts" },
			{ "votedup:anything", "The Voted Post" },
		}.Where(p => !string.IsNullOrWhiteSpace(p.Key)).ToDictionary(x => x.Key, y => y.Value);

	}
}
