﻿using E621Downloader.Models.Networks;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace E621Downloader.Models.E621 {
	public class E621Comment {
		public static async Task<E621Comment[]> GetAsync(string post_id, CancellationToken? token = null) {
			string url = $"https://{Data.GetHost()}/comments.json?group_by=comment&search[post_id]={post_id}";
			Debug.WriteLine(url);
			HttpResult<string> result = await Data.ReadURLAsync(url, token);
			if(result?.Content == "{\"comments\":[]}") {
				return Array.Empty<E621Comment>();
			}
			if(result.Result == HttpResultType.Success) {
				return JsonConvert.DeserializeObject<E621Comment[]>(result.Content);
			} else {
				return Array.Empty<E621Comment>();
			}
		}

		public E621User User { get; private set; }

		public string id;
		public DateTime created_at;
		public int post_id;
		public int creator_id;
		public string body;
		public int score;
		public DateTime updated_at;
		public int updater_id;
		public bool do_not_bump_post;
		public bool is_hidden;
		public bool is_sticky;
		public object warning_type;
		public object warning_user_id;
		public string creator_name;
		public string updater_name;

		public E621Comment ToSelf() => this;
		public class CommentRoot {
			public E621Comment[] comments;
		}
	}
}
