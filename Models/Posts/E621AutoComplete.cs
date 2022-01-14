﻿using E621Downloader.Models.Networks;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace E621Downloader.Models.Posts {
	public class E621AutoComplete {
		public static async Task<E621AutoComplete[]> GetAsync(string tag, CancellationToken? token = null) {
			HttpResult result = await Data.ReadURLAsync($"https://e621.net/tags/autocomplete.json?search[name_matches]={tag}", token);
			if(result.Result == HttpResultType.Success) {
				return JsonConvert.DeserializeObject<E621AutoComplete[]>(result.Content);
			} else {
				return new E621AutoComplete[0];
			}
		}
		public string id;
		public string name;
		public int post_count;
		public int category;
		public string antecedent_name;
	}
}