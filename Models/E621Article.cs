using System;

namespace E621Downloader.Models {
	public class E621Article {
		private readonly string source;
		public readonly int id;
		public readonly string date;
		public readonly int score;
		public readonly Rating rating;
		public readonly string[] tags;
		public readonly string url_source;//data-file-url
		public readonly string url_preview;//data-preview-file-url

		public bool isLoaded;

		public E621Article(string source) {
			this.isLoaded = false;
			this.source = source;
			this.id = int.Parse(GetProperty("data-id=\""));
			this.tags = GetProperty("data-tags=\"").Split(' ');
			this.url_source = GetProperty("data-file-url=\"");
			//this.url_preview = GetProperty("data-preview-file-url=\"");
			this.url_preview = GetProperty("data-large-file-url=\"");

			string detail = GetProperty("title=\"");
			switch(detail[detail.IndexOf("Rating:") + 8]) {
				case 'e':
					this.rating = Rating.explict;
					break;
				case 's':
					this.rating = Rating.safe;
					break;
				case 'q':
					this.rating = Rating.suggestive;
					break;
				default:
					throw new Exception("rating error");
			}
			this.date = detail.Substring(detail.IndexOf("Date:") + 6, 19);
			int indexScore = detail.IndexOf("Score:") + 7;
			int indexEndScore = detail.IndexOf('\r', indexScore);
			if(indexEndScore == -1) {
				indexEndScore = detail.IndexOf('\n', indexScore);
			}
			this.score = int.Parse(detail.Substring(indexScore, indexEndScore - indexScore));
		}
		private string GetProperty(string target) {
			int index_id = source.IndexOf(target) + target.Length - 1;
			int indexEnd_id = source.IndexOf('\"', index_id + 1);
			return source.Substring(index_id + 1, indexEnd_id - index_id - 1);
		}

	}
	public enum Rating {
		safe, suggestive, explict
	}
}
