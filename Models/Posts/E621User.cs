using E621Downloader.Models.Networks;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media.Imaging;

namespace E621Downloader.Models.Posts {
	public class E621User {
		//https://e621.net/users/306106.json
		public static async Task<E621User> GetAsync(int id) {
			string url = $"https://e621.net/users/{id}.json";
			string data = await Data.ReadURLAsync(url);
			if(string.IsNullOrEmpty(data)) {
				Debug.WriteLine("?????");
				return null;
			}
			return JsonConvert.DeserializeObject<E621User>(data);
		}

		public static async Task<string> GetAvatorURL(E621User user) {
			string url = $"https://e621.net/posts/{user.avatar_id}.json";
			string data = await Data.ReadURLAsync(url);
			if(string.IsNullOrEmpty(data)) {
				return "ms - appx:///Assets/esix2.jpg";
			}
			Post post = JsonConvert.DeserializeObject<PostRoot>(data).post;
			string preview = post.preview.url;
			return preview;
		}

		public int wiki_page_version_count;
		public int artist_version_count;
		public int pool_version_count;
		public int forum_post_count;
		public int comment_count;
		public int appeal_count;
		public int flag_count;
		public int positive_feedback_count;
		public int neutral_feedback_count;
		public int negative_feedback_count;
		public int upload_limit;
		public int id;
		public DateTime created_at;
		public object name;
		public int level;
		public int base_upload_limit;
		public int post_upload_count;
		public int post_update_count;
		public int note_update_count;
		public bool is_banned;
		public bool can_approve_posts;
		public bool can_upload_free;
		public string level_string;
		public int? avatar_id;
	}
}
