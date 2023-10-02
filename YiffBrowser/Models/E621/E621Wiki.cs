using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace YiffBrowser.Models.E621 {
	public class E621Wiki {
		[JsonProperty("id")]
		public int ID { get; set; }
		[JsonProperty("created_at")]
		public DateTime CreatedAt { get; set; }
		[JsonProperty("updated_at")]
		public DateTime UpdatedAt { get; set; }
		[JsonProperty("title")]
		public string Title { get; set; }
		[JsonProperty("body")]
		public string Body { get; set; }
		[JsonProperty("creator_id")]
		public int CreatorID { get; set; }
		[JsonProperty("is_locked")]
		public bool IsLocked { get; set; }
		[JsonProperty("updater_id")]
		public object UpdaterID { get; set; }
		[JsonProperty("is_deleted")]
		public bool IsDeleted { get; set; }
		[JsonProperty("other_names")]
		public List<object> OtherNames { get; set; }
		[JsonProperty("creator_name")]
		public string CreatorName { get; set; }
		[JsonProperty("category_name")]
		public int CategoryName { get; set; }

		public static Dictionary<string, E621Wiki> Pool { get; } = new();


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
