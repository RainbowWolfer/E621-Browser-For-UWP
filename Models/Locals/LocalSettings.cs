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
		public int subscriptionSizeView;

		public bool adaptiveGrid;
		public double fixedHeight;
		public double adaptiveSizeMultiplier;

		public string customHost;
		public bool customHostEnable;
		public bool cycleList;
		public bool concatTags;
		public bool mediaBackgroundPlay;
		public bool mediaAutoPlay;
		public bool mediaAutoMute;
		public bool enableHotKeys;
		public bool enableGifAutoPlay;
		public int gifLimit;

		public SpotFilterType spot_FilterType;
		public FileType spot_fileType;
		public bool spot_includeSafe;
		public bool spot_includeQuestoinable;
		public bool spot_includeExplicit;
		public int spot_amount;

		public SlideshowConfiguration slideshowConfiguration;

		public ItemsGroupViewType library_viewType_images;
		public ItemsGroupViewType library_viewType_folders;

		public int librarSizeView_images;
		public int librarSizeView_folders;

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
				mediaAutoMute = false,
				customHost = "",
				spot_amount = 1,
				enableHotKeys = true,
				adaptiveGrid = true,
				adaptiveSizeMultiplier = 1,
				fixedHeight = 280,
				randomTagMaxCount = 10000,
				tabsOpenLength = 300,
				library_viewType_images = ItemsGroupViewType.ListView,
				enableGifAutoPlay = true,
				gifLimit = 5,
				slideshowConfiguration = new(),
				subscriptionSizeView = 200,
				librarSizeView_folders = 100,
				librarSizeView_images = 100,
				library_viewType_folders = ItemsGroupViewType.ListView,
			};
		}
	}
}
