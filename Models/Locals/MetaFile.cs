using E621Downloader.Models.E621;
using Newtonsoft.Json;

namespace E621Downloader.Models.Locals {
	public class MetaFile {
		public string FilePath { get; set; }
		public string Group { get; set; }
		public bool FinishedDownloading { get; set; }
		public E621Post MyPost { get; set; }
		public MetaFile(string filePath, string group, E621Post post) {
			FilePath = filePath;
			Group = group;
			FinishedDownloading = false;
			MyPost = post;
		}

		[JsonConstructor]
		public MetaFile(string filePath, string group, bool finishedDownloading, E621Post myPost) {
			FilePath = filePath;
			Group = group;
			FinishedDownloading = finishedDownloading;
			MyPost = myPost;
		}

		public string ConvertJson() {
			return JsonConvert.SerializeObject(this, Formatting.Indented);
		}

		public static MetaFile ReadFromJson(string content) {
			return JsonConvert.DeserializeObject<MetaFile>(content);
		}

	}
}
