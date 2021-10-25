﻿using E621Downloader.Models.Networks;
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
		public async static Task<E621Tag[]> GetAsync(string tag) {
			string url = $"https://e621.net/tags.json?search[name_matches]={tag}";
			string content = await Data.ReadURLAsync(url);
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

		public override string ToString() {
			return $"E621Tags:({id})({name})({related_tags})({post_count})({category})";
		}
	}
}
