using E621Downloader.Models.Networks;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace E621Downloader.Models.Posts {
	public class E621Pool {
		public static async Task<E621Pool> GetAsync(string id, CancellationToken? token = null) {
			HttpResult<string> result = await Data.ReadURLAsync($"https://{Data.GetHost()}/pools/{id}.json", token);
			if(result.Result == HttpResultType.Success) {
				return JsonConvert.DeserializeObject<E621Pool>(result.Content);
			} else {
				return null;
			}
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
		public string TagName => $"pool:{name}";
	}
}
