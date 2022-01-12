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
		public static async Task<E621User> GetAsync(string username) {
			string url = $"https://e621.net/users/{username}.json";
			string data = await Data.ReadURLAsync(url);
			if(string.IsNullOrEmpty(data)) {
				Debug.WriteLine("?????");
				return null;
			}
			return JsonConvert.DeserializeObject<E621User>(data);
		}

		public static async Task<E621User> GetAsync(int id) {
			return await GetAsync($"{id}");
		}

		public const string DEFAULT_AVATAR = "ms-appx:///Assets/esix2.jpg";
		public static async Task<string> GetAvatarURLAsync(E621User user) {
			string url = $"https://e621.net/posts/{user.avatar_id}.json";
			string data = await Data.ReadURLAsync(url);
			if(string.IsNullOrWhiteSpace(data)) {
				return DEFAULT_AVATAR;
			}
			Post post = JsonConvert.DeserializeObject<PostRoot>(data).post;
			string result = post.preview.url ?? post.sample.url;
			if(string.IsNullOrWhiteSpace(result)) {
				return DEFAULT_AVATAR;
			}
			return result;
		}

		public static E621User Current { get; set; }

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
		public string name;
		public int level;
		public int base_upload_limit;
		public int post_upload_count;
		public int post_update_count;
		public int note_update_count;
		public bool is_banned;
		public bool can_approve_posts;
		public bool can_upload_free;
		public string level_string;
		public string avatar_id;
		public bool show_avatars;
		public bool blacklist_avatars;
		public bool blacklist_users;
		public bool description_collapsed_initially;
		public bool hide_comments;
		public bool show_hidden_comments;
		public bool show_post_statistics;
		public bool has_mail;
		public bool receive_email_notifications;
		public bool enable_keyboard_navigation;
		public bool enable_privacy_mode;
		public bool style_usernames;
		public bool enable_auto_complete;
		public bool has_saved_searches;
		public bool disable_cropped_thumbnails;
		public bool disable_mobile_gestures;
		public bool enable_safe_mode;
		public bool disable_responsive_mode;
		public bool disable_post_tooltips;
		public bool no_flagging;
		public bool no_feedback;
		public bool disable_user_dmails;
		public bool enable_compact_uploader;
		public DateTime updated_at;
		public string email;
		public DateTime last_logged_in_at;
		public DateTime last_forum_read_at;
		public string recent_tags;
		public int comment_threshold;
		public string default_image_size;
		public string favorite_tags;
		public string blacklisted_tags;
		public string time_zone;
		public int per_page;
		public string custom_style;
		public int favorite_count;
		public int api_regen_multiplier;
		public int api_burst_limit;
		public int remaining_api_limit;
		public int statement_timeout;
		public int favorite_limit;
		public int tag_query_limit;
	}
}
