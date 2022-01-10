using E621Downloader.Models.Networks;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E621Downloader.Models.Posts {
	public class E621Pool {
		public async static Task<E621Pool> GetAsync(string id) {
			string data = await Data.ReadURLAsync($"https://e621.net/pools/{id}.json");
			return JsonConvert.DeserializeObject<E621Pool>(data);
		}

		public int id;
		public string name;
		public DateTime created_at;
		public DateTime updated_at;
		public int creator_id;
		public string description;
		public bool is_active;
		public string category;
		public bool is_deleted;
		public List<string> post_ids;
		public string creator_name;
		public int post_count;

		public string Tag => $"pool:{id}";
	}
}
