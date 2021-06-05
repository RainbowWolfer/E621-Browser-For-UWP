using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E621Downloader.Models.Posts {
	public class SimplePost {
		public int id;
		public DateTime? created_at;
		public DateTime? updated_at;
		public ArticleFile file;
		public Score score;
		public Tags tags;
		public string rating;
		public int fav_count;
		public List<string> sources;
		public List<int> pools;
		public Relationships relationships;
		public string description;

		public SimplePost(Post post) {
			this.id = post.id;
			this.created_at = post.created_at;
			this.updated_at = post.updated_at;
			this.file = post.file;
			this.score = post.score;
			this.tags = post.tags;
			this.rating = post.rating;
			this.fav_count = post.fav_count;
			this.sources = post.sources;
			this.pools = post.pools;
			this.relationships = post.relationships;
			this.description = post.description;
		}
	}
}
