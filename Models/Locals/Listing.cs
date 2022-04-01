using E621Downloader.Models.Networks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E621Downloader.Models.Locals {
	public class Listing {
		public SingleListing CloudBlackList { get; set; } = new SingleListing("Cloud") { IsCloud = true, };
		public List<SingleListing> LocalBlackLists { get; set; } = new List<SingleListing>();
		public List<SingleListing> LocalFollowingLists { get; set; } = new List<SingleListing>();

		public Listing() {

		}

		public static Listing GetDefaultListing() {
			string hostName = Data.GetHost().ToUpper();
			int index = hostName.IndexOf(".");
			if(index != -1) {
				hostName = hostName.Substring(0, index);
			}
			return new Listing() {
				CloudBlackList = new SingleListing($"{hostName} Cloud") {
					IsCloud = true,
				},
				LocalBlackLists = new List<SingleListing>() {
					new SingleListing("Default Black List") {
						IsActive = true,
						IsDefault = true,
					},
				},
				LocalFollowingLists = new List<SingleListing>() {
					new SingleListing("Default Following List") {
						IsActive = true,
						IsDefault = true,
					},
				},
			};
		}


		//public bool CheckFollowList(string tag) => FollowList.Contains(tag);
		//public bool CheckFollowList(string[] tags) => FollowList.Contains(E621Tag.JoinTags(tags));
		//public bool CheckBlackList(string tag) => BlackList.Contains(tag);
		//public bool CheckBlackList(string[] tags) => BlackList.Contains(E621Tag.JoinTags(tags));
	}


	public class SingleListing {
		public string Name { get; set; }
		public bool IsDefault { get; set; }
		public bool IsActive { get; set; }
		public bool IsCloud { get; set; } = false;
		public List<string> Tags { get; set; }

		public SingleListing(string name) {
			Name = name;
			Tags = new List<string>();
		}
	}
}
