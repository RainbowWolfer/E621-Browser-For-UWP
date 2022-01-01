using E621Downloader.Models.Networks;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E621Downloader.Models.Posts {
	public class E621Tag {
		public static async Task<E621Tag[]> GetAsync(string tag) {
			string url = $"https://e621.net/tags.json?search[name_matches]={tag}";
			string content = await Data.ReadURLAsync(url);
			if(content == "{\"tags\":[]}") {
				return null;
			}
			try {
				return JsonConvert.DeserializeObject<E621Tag[]>(content);
			} catch {
				Debug.WriteLine("Tags Error");
				return null;
			}
		}
		public static async Task<E621Tag> GetFirstAsync(string tag) {
			return (await GetAsync(tag))?.FirstOrDefault();
		}
		public static E621Tag[] Get(string tag) {
			string url = $"https://e621.net/tags.json?search[name_matches]={tag}";
			string content = Data.ReadURL(url);
			if(content == "{\"tags\":[]}") {
				return null;
			}
			try {
				return JsonConvert.DeserializeObject<E621Tag[]>(content);
			} catch {
				Debug.WriteLine("Tags Error");
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

		public delegate void OnWikiLoadedEventHandler();
		public event OnWikiLoadedEventHandler OnWikiLoaded;
		public bool IsWikiLoaded { get; private set; } = false;
		public E621Wiki[] Wikis { get; private set; } = Array.Empty<E621Wiki>();
		public E621Wiki Wiki => Wikis?.FirstOrDefault();
		public async void LoadWiki() {
			this.Wikis = await E621Wiki.GetAsync(name);
			IsWikiLoaded = true;
			OnWikiLoaded?.Invoke();
		}
		public async Task<E621Wiki> LoadWikiAsync() {
			this.Wikis = await E621Wiki.GetAsync(name);
			IsWikiLoaded = true;
			OnWikiLoaded?.Invoke();
			return this.Wiki;
		}


		public override string ToString() {
			return $"E621Tags:({id})({name})({related_tags})({post_count})({category})";
		}
	}
}
