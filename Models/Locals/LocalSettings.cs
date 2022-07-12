using E621Downloader.Pages;
using E621Downloader.Views.LibrarySection;

namespace E621Downloader.Models.Locals {
	public class LocalSettings {
		public static LocalSettings Current { get; set; }
		public static async void Save() {
			await Local.WriteLocalSettings();
		}

		public int randomTagMaxCount = 10000;

		public int tabsOpenLength = 300;
		public int subscriptionSizeView = 200;

		public bool adaptiveGrid = true;
		public double fixedHeight = 280;
		public double adaptiveSizeMultiplier = 1;

		public string customHost = "";
		public bool customHostEnable = false;
		public bool cycleList = true;
		public bool concatTags = false;
		public bool mediaBackgroundPlay = false;
		public bool mediaAutoPlay = true;
		public bool mediaAutoMute = false;
		public bool enableHotKeys = true;
		public bool enableGifAutoPlay = true;
		public int gifLimit = 5;

		public SpotFilterType spot_FilterType = SpotFilterType.All;
		public FileType spot_fileType = FileType.Jpg;
		public bool spot_includeSafe = false;
		public bool spot_includeQuestoinable = false;
		public bool spot_includeExplicit = true;
		public int spot_amount = 1;

		public SlideshowConfiguration slideshowConfiguration = new();

		public ItemsGroupViewType library_viewType_images = ItemsGroupViewType.ListView;
		public ItemsGroupViewType library_viewType_folders = ItemsGroupViewType.ListView;

		public int librarSizeView_images = 100;
		public int librarSizeView_folders = 100;

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
