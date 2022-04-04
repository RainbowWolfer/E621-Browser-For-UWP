using E621Downloader.Models.Networks;
using E621Downloader.Models.Posts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E621Downloader.Models.Locals {
	public class Listing {
		//public SingleListing CloudBlackList { get; set; } = new SingleListing("Cloud") { IsCloud = true, };
		public List<SingleListing> LocalFollowingLists { get; set; } = new List<SingleListing>();
		public List<SingleListing> LocalBlackLists { get; set; } = new List<SingleListing>();

		public Listing() {

		}

		public SingleListing GetGetDefaultFollowList() {
			return LocalFollowingLists.Find(i => i.IsDefault) ?? new SingleListing("Error");
		}

		public SingleListing GetGetDefaultBlackList() {
			return LocalBlackLists.ToList().Find(i => i.IsDefault) ?? new SingleListing("Error");
		}

		public async Task AddFollowingList(string tag) {
			if(!GetGetDefaultFollowList().Tags.Contains(tag)) {
				GetGetDefaultFollowList().Tags.Add(tag);
			}
			await Local.WriteListing();
		}

		public async Task AddBlackList(string tag) {
			if(!GetGetDefaultBlackList().Tags.Contains(tag)) {
				GetGetDefaultBlackList().Tags.Add(tag);
			}
			await Local.WriteListing();
		}

		public async Task RemoveFollowingList(string tag) {
			if(GetGetDefaultFollowList().Tags.Contains(tag)) {
				GetGetDefaultFollowList().Tags.Remove(tag);
			}
			await Local.WriteListing();
		}

		public async Task RemoveBlackList(string tag) {
			if(GetGetDefaultBlackList().Tags.Contains(tag)) {
				GetGetDefaultBlackList().Tags.Remove(tag);
			}
			await Local.WriteListing();
		}

		public bool CheckFollowingList(params string[] tags) {
			if(tags.Length == 0) {
				return false;
			} else {
				return GetGetDefaultFollowList().Tags.Contains(E621Tag.JoinTags(tags)); ;
			}
		}

		public bool CheckBlackList(params string[] tags) {
			if(tags.Length == 0) {
				return false;
			} else {
				return GetGetDefaultBlackList().Tags.Contains(E621Tag.JoinTags(tags)); ;
			}
		}

		public static Listing GetDefaultListing() {
			return new Listing() {
				LocalBlackLists = new List<SingleListing>() {
					new SingleListing($"{Data.GetSimpleHost()} Cloud") {
						IsCloud = true,
						IsDefault = false,
					},
					new SingleListing("Default Black List") {
						IsDefault = true,
					},
				},
				LocalFollowingLists = new List<SingleListing>() {
					new SingleListing("Default Follow List") {
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
		//public bool IsActive { get; set; }
		public bool IsCloud { get; set; } = false;
		public List<string> Tags { get; set; }

		public SingleListing(string name) {
			Name = name;
			Tags = new List<string>();
		}
	}
}
