using E621Downloader.Models.Networks;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// evolve_(copyright)
/// </summary>
namespace E621Downloader.Models.Posts {
	public class E621Tag {
		public static E621Tag[] Get(string tag) {
			string url = $"https://e621.net/tags.json?search[name_matches]={tag}";
			string content = Data.ReadURL(url);
			if(content == "{\"tags\":[]}") {
				return null;
			}
			try {
				return JsonConvert.DeserializeObject<E621Tag[]>(content);
			} catch {
				Debug.WriteLine("!");
				return null;
			}
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
	}
}
