using E621Downloader.Models.Locals;
using E621Downloader.Models.Networks;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace E621Downloader.Models.Posts {
	public class Post {
		public static async Task<List<Post>> GetPostsByTagsAsync(CancellationToken? token, int page, params string[] tags) {
			if(page <= 0) {
				throw new Exception("Page not valid");
			}
			string url = $"https://{Data.GetHost()}/posts.json?page={page}&tags=";
			tags.ToList().ForEach((t) => url += t + "+");
			//CheckSafe(ref url);

			HttpResult<string> result = await Data.ReadURLAsync(url, token);
			if(result.Result == HttpResultType.Success) {
				return JsonConvert.DeserializeObject<PostsRoot>(result.Content).posts ?? new List<Post>();
			} else if(result.Result == HttpResultType.Canceled) {
				return null;
			} else {
				return new List<Post>();
			}
		}

		public static async Task<List<Post>> GetPostsByTagsAsync(CancellationToken? token, bool combine, int page, params string[] tags) {
			if(page <= 0) {
				throw new Exception("Page not valid");
			}
			string url = $"https://{Data.GetHost()}/posts.json?page={page}&tags=";
			tags.ToList().ForEach(t => url += $"{(combine ? "~" : "")}{t}+");
			//CheckSafe(ref url);

			HttpResult<string> result = await Data.ReadURLAsync(url, token);
			if(result.Result == HttpResultType.Success) {
				return JsonConvert.DeserializeObject<PostsRoot>(result.Content).posts;
			} else {
				return null;
			}
		}

		public static async Task<List<Post>> GetPostsByRandomAsync(CancellationToken? token, int amount, params string[] tags) {
			string url = $"https://{Data.GetHost()}/posts.json?tags=order:random+limit:{amount}+";
			tags.ToList().ForEach(t => url += $"{t}+");
			Debug.WriteLine($"THIS: {url}");
			//CheckSafe(ref url);
			HttpResult<string> result = await Data.ReadURLAsync(url, token);
			if(result.Result == HttpResultType.Success) {
				return JsonConvert.DeserializeObject<PostsRoot>(result.Content).posts;
			} else {
				return null;
			}
		}

		public static async Task<Post> GetPostByIDAsync(CancellationToken? token, string id) {
			string url = $"https://{Data.GetHost()}/posts/{id}.json";
			HttpResult<string> result = await Data.ReadURLAsync(url, token);
			if(result.Result == HttpResultType.Success) {
				return JsonConvert.DeserializeObject<PostRoot>(result.Content).post;
			} else {
				return null;
			}
		}

		public static async Task<List<Post>> GetPostsByIDsAsync(CancellationToken? token, IEnumerable<string> ids) {
			List<Post> posts = new List<Post>();
			foreach(string id in ids) {
				posts.Add(await GetPostByIDAsync(token, id));
			}
			return posts;
		}

		//private static void CheckSafe(ref string url) {
		//	if(LocalSettings.Current.safeMode) {
		//		url += "rating:s";
		//	}
		//}

		public string id;
		public DateTime? created_at;
		public DateTime? updated_at;
		public ArticleFile file;
		public Preview preview;
		public Sample sample;
		public Score score;
		public Tags tags;
		public List<string> locked_tags;
		public int change_seq;
		public Flags flags;
		public string rating;
		public int fav_count;
		public List<string> sources;
		public List<string> pools;
		public Relationships relationships;
		public int? approver_id;
		public int uploader_id;
		public string description;
		public int comment_count;
		public bool is_favorited;
		public bool has_notes;
		public string duration;

		public override string ToString() {
			return $"E621Post ({id}.{file.ext})";
		}
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
		public List<string> invalid;
		public List<string> lore;
		public List<string> meta;

		public List<string> GetAllTags() {
			var result = new List<string>();
			general.ForEach(s => result.Add(s));
			species.ForEach(s => result.Add(s));
			character.ForEach(s => result.Add(s));
			copyright.ForEach(s => result.Add(s));
			artist.ForEach(s => result.Add(s));
			invalid.ForEach(s => result.Add(s));
			lore.ForEach(s => result.Add(s));
			meta.ForEach(s => result.Add(s));
			return result;
		}

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
		public string parent_id;
		public bool has_children;
		public bool has_active_children;
		public List<string> children;
	}


	public class PostsRoot {
		public List<Post> posts;
	}

	public class PostRoot {
		public Post post;
	}

	public enum Rating {
		safe, suggestive, explict
	}
}
