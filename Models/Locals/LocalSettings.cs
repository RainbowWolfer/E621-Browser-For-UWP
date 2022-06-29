using E621Downloader.Pages;
using E621Downloader.Views.LibrarySection;

namespace E621Downloader.Models.Locals {
	public class LocalSettings {
		public static LocalSettings Current { get; set; }
		public static async void Save() {
			await Local.WriteLocalSettings();
		}

		public int randomTagMaxCount;

		public int tabsOpenLength;

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
		public bool enableGifAutoPlay;
		public int gifLimit;

		public SpotFilterType spot_FilterType;
		public FileType spot_fileType;
		public bool spot_includeSafe;
		public bool spot_includeQuestoinable;
		public bool spot_includeExplicit;
		public int spot_amount;


		public ItemsGroupViewType library_viewType;

		public string user_username = "";
		public string user_api = "";

		public bool CheckLocalUser() {
			return !string.IsNullOrWhiteSpace(user_username) && !string.IsNullOrWhiteSpace(user_api);
		}

		public void SetLocalUser(string username, string api) {
			user_username = username;
			user_api = api;
		}

		public static LocalSettings GetDefault() {
			return new LocalSettings() {
				customHostEnable = false,
				spot_fileType = FileType.Jpg,
				spot_FilterType = SpotFilterType.All,
				spot_includeExplicit = true,
				spot_includeQuestoinable = false,
				spot_includeSafe = false,
				cycleList = true,
				concatTags = false,
				mediaBackgroundPlay = false,
				mediaAutoPlay = true,
				customHost = "",
				spot_amount = 1,
				enableHotKeys = true,
				adaptiveGrid = true,
				adaptiveSizeMultiplier = 1,
				fixedHeight = 280,
				randomTagMaxCount = 10000,
				tabsOpenLength = 300,
				library_viewType = ItemsGroupViewType.ListView,
				enableGifAutoPlay = true,
				gifLimit = 5,
			};
		}
	}
}
