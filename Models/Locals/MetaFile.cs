using E621Downloader.Models.Posts;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E621Downloader.Models.Locals {
	public class MetaFile {
		public string FilePath { get; set; }
		public bool FinishedDownloading { get; set; }
		public SimplePost MyPost { get; set; }
		public MetaFile(string filePath, Post post) {
			FilePath = filePath;
			FinishedDownloading = false;
			MyPost = new SimplePost(post);
		}

		public string ConvertJson() {
			return JsonConvert.SerializeObject(this);
		}

		public static MetaFile ReadFromJson(string content) {
			return JsonConvert.DeserializeObject<MetaFile>(content);
		}

	}
}
