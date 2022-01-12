using E621Downloader.Models.Networks;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace E621Downloader.Models.Posts {
	public class E621AutoComplete {
		public static async Task<E621AutoComplete[]> GetAsync(string tag) {
			string data = await Data.ReadURLAsync($"https://e621.net/tags/autocomplete.json?search[name_matches]={tag}");
			if(string.IsNullOrWhiteSpace(data) || data == "{\"error\":\"bad request\"}") {
				return new E621AutoComplete[0];
			} else {
				return JsonConvert.DeserializeObject<E621AutoComplete[]>(data);
			}
		}
		public int id;
		public string name;
		public int post_count;
		public int category;
		public string antecedent_name;
	}
}
