using E621Downloader.Models.Networks;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E621Downloader.Models.Posts {
	public class E621Comment {
		//https://e621.net/comments.json?group_by=comment&search[post_id]=2671193
		public static async Task<E621Comment[]> GetAsync(int post_id) {
			string url = $"https://e621.net/comments.json?group_by=comment&search[post_id]={post_id}";
			string data = await Data.ReadURLAsync(url);
			if(string.IsNullOrEmpty(data)) {
				return Array.Empty<E621Comment>();
			}
			try {
				return JsonConvert.DeserializeObject<E621Comment[]>(data);
			} catch(Exception) {
				return JsonConvert.DeserializeObject<CommentRoot>(data).comments;
			}
		}

		public async Task<string> LoadAvatar() {
			User = await E621User.GetAsync(creator_id);
			if(User != null) {
				return await E621User.GetAvatorURL(User);
			}
			return null;
		}

		public E621User User { get; private set; }

		public int id;
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
