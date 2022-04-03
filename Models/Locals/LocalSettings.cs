using E621Downloader.Pages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E621Downloader.Models.Locals {
	public class LocalSettings {
		public static LocalSettings Current { get; set; }
		public static void Save() {
			Local.WriteLocalSettings();
		}

		public bool adaptiveGrid;
		public double fixedHeight;
		public double adaptiveSizeMultiplier;

		public string customHost;
		public bool customHostEnable;
		public bool cycleList;
		public bool concatTags;
		public bool mediaBackgroundPlay;
		public bool mediaAutoPlay;
		public bool enableHotKeys;

		public SpotFilterType spot_FilterType;
		public FileType spot_fileType;
		public bool spot_includeSafe;
		public bool spot_includeQuestoinable;
		public bool spot_includeExplicit;
		public int spot_amount;

		public string user_username = "";
		public string user_api = "";

		public bool CheckLocalUser() {
			return !string.IsNullOrWhiteSpace(user_username) && !string.IsNullOrWhiteSpace(user_api);
		}
		public void SetLocalUser(string username, string api) {
			user_username = username;
			user_api = api;
		}
	}
}
