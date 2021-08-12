using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E621Downloader.Models.Posts {
	public class E621User {
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
		public int avatar_id;
	}
}
