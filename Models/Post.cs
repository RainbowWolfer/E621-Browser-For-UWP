using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E621Downloader.Models {
	public partial class Post {
		public static List<Post> GetPostsByTags(int page, params string[] tags) {
			var posts = new List<Post>();
			if(page <= 0) {
				throw new Exception("Page not valid");
			}
			string url = string.Format("https://e621.net/posts.json?page={0}&tags=", page);
			tags.ToList().ForEach((t) => url += t + "+");

			string data = Data.ReadURL(url);

			return JsonConvert.DeserializeObject<PostRoot>(data).posts;
		}
	}
	public partial class Post {
		public int id;
		public DateTime created_at;
		public DateTime updated_at;
		public ArticleFile file;
		public Preview preview;
		public Sample sample;
		public Score score;
		public Tags tags;
		public List<object> locked_tags;
		public int change_seq;
		public Flags flags;
		public string rating;
		public int fav_count;
		public List<string> sources;
		public List<int> pools;
		public Relationships relationships;
		public int? approver_id;
		public int uploader_id;
		public string description;
		public int comment_count;
		public bool is_favorited;
		public bool has_notes;
		public object duration;
	}

	public class ArticleFile {
		public int width;
		public int height;
		public string ext;
		public int size;
		public string md5;
		public string url;
	}

	public class Preview {
		public int width;
		public int height;
		public string url;
	}

	public class Alternates {
	}

	public class Sample {
		public bool has;
		public int height;
		public int width;
		public string url;
		public Alternates alternates;
	}

	public class Score {
		public int up;
		public int down;
		public int total;
	}

	public class Tags {
		public List<string> general;
		public List<string> species;
		public List<string> character;
		public List<string> copyright;
		public List<string> artist;
		public List<object> invalid;
		public List<object> lore;
		public List<string> meta;
	}

	public class Flags {
		public bool pending;
		public bool flagged;
		public bool note_locked;
		public bool status_locked;
		public bool rating_locked;
		public bool deleted;
	}

	public class Relationships {
		public object parent_id;
		public bool has_children;
		public bool has_active_children;
		public List<int> children;
	}


	public class PostRoot {
		public List<Post> posts;
	}


	public enum Rating {
		safe, suggestive, explict
	}
}
