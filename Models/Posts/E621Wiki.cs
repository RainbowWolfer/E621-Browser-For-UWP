using E621Downloader.Models.Networks;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E621Downloader.Models.Posts {
	public class E621Wiki {
		public static E621Wiki[] Get(string tag) {
			string url = $"https://e621.net/wiki_pages.json?search[title]={tag}";
			string data = Data.ReadURL(url);
			if(data == "[]") {
				return null;
			}
			try {
				return JsonConvert.DeserializeObject<E621Wiki[]>(data);
			} catch {
				Debug.WriteLine("Wiki Error");
				return null;
			}
		}

		public static async Task<E621Wiki[]> GetAsync(string tag) {
			string url = $"https://e621.net/wiki_pages.json?search[title]={tag}";
			string data = await Data.ReadURLAsync(url);
			if(data == "[]") {
				return null;
			}
			try {
				return JsonConvert.DeserializeObject<E621Wiki[]>(data);
			} catch {
				Debug.WriteLine("Wiki Error");
				return null;
			}
		}

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
	}
}
