using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E621Downloader.Models.Locals {
	public class MetaFile {
		public string FilePath { get; set; }
		public string FileType { get; set; }
		public string PostID { get; set; }
		public string GroupName { get; set; }
		public string[] PostTags { get; set; }

		public MetaFile(string filePath, string fileType, string postID, string groupName, string[] postTags) {
			FilePath = filePath;
			FileType = fileType;
			PostID = postID;
			GroupName = groupName;
			PostTags = postTags;
		}

		public string ConvertJson() {
			return JsonConvert.SerializeObject(this);
		}

		public static MetaFile ReadFromJson(string content) {
			return JsonConvert.DeserializeObject<MetaFile>(content);
		}

	}
}
