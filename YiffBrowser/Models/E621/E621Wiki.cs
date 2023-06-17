using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YiffBrowser.Models.E621 {
	public class E621Wiki {
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
