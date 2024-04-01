using Newtonsoft.Json;
using Prism.Mvvm;
using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage;
using Windows.Storage.AccessCache;
using YiffBrowser.Attributes;
using YiffBrowser.Helpers;

namespace YiffBrowser.Services.Locals {
	//General
	public partial class LocalSettings : BindableBase {
		public static HostType StartHostType { get; private set; } = HostType.E926;//write after initial read. don't change it during app running

		private HostType hostType = HostType.E621;
		private int e621PageLimitCount = 75;
		private bool enableStartupTags = true;
		private string[] startupTags = ["order:rank"];
		private bool enableTransitionAnimation = true;
		private int postsPerPage = 75;
		private AppTheme appTheme = AppTheme.System;
		private StartupTagsType startupTagsType = StartupTagsType.StartupTags;
		private bool showDebugPanel = false;
		private int imageAdaptWidth = 380;
		private int imageAdaptHeight = 50;

		public HostType HostType {
			get => hostType;
			set => SetProperty(ref hostType, value);
		}

		public int E621PageLimitCount {
			get => e621PageLimitCount;
			set => SetProperty(ref e621PageLimitCount, value);
		}

		public bool EnableStartupTags {
			get => enableStartupTags;
			set => SetProperty(ref enableStartupTags, value);
		}

		//"brazhnik"
		public string[] StartupTags {
			get => startupTags;
			set => SetProperty(ref startupTags, value);
		}

		public int PostsPerPage {
			get => postsPerPage;
			set => SetProperty(ref postsPerPage, value);
		}

		public AppTheme AppTheme {
			get => appTheme;
			set => SetProperty(ref appTheme, value);
		}

		public bool ShowDebugPanel {
			get => showDebugPanel;
			set => SetProperty(ref showDebugPanel, value);
		}

		public StartupTagsType StartupTagsType {
			get => startupTagsType;
			set => SetProperty(ref startupTagsType, value);
		}

		public bool EnableTransitionAnimation {
			get => enableTransitionAnimation;
			set => SetProperty(ref enableTransitionAnimation, value);
		}

		public int ImageAdaptWidth {
			get => imageAdaptWidth;
			set => SetProperty(ref imageAdaptWidth, value);
		}

		public int ImageAdaptHeight {
			get => imageAdaptHeight;
			set => SetProperty(ref imageAdaptHeight, value);
		}

	}

	//Media
	public partial class LocalSettings {
		public event TypedEventHandler<LocalSettings, MediaControlType> MediaControlTypeChanged;

		private MediaControlType mediaControlType = MediaControlType.Full;
		private bool autoLooping = true;

		public MediaControlType MediaControlType {
			get => mediaControlType;
			set {
				SetProperty(ref mediaControlType, value);
				MediaControlTypeChanged?.Invoke(this, value);
			}
		}

		public bool AutoLooping {
			get => autoLooping;
			set => SetProperty(ref autoLooping, value);
		}

	}

	public enum MediaControlType {
		Full, Compact, Simple
	}

	//Download
	public partial class LocalSettings {
		private string downloadFolderToken = null;

		public string DownloadFolderToken {
			get => downloadFolderToken;
			set => SetProperty(ref downloadFolderToken, value);
		}

		public bool HasDownloadFolderToken() => DownloadFolderToken.IsNotBlank() && Local.DownloadFolder != null;

		public void SetDownloadFolder(StorageFolder folder) {
			if (folder == null) {
				return;
			}
			Local.DownloadFolder = folder;
			string token = StorageApplicationPermissions.FutureAccessList.Add(folder);
			DownloadFolderToken = token;
		}

		public void ClearDownloadFolder() {
			Local.DownloadFolder = null;
			DownloadFolderToken = null;
		}

	}

	//User
	public partial class LocalSettings {
		private UserModel user = default;
		private UserModel userE6AI;

		public UserModel User {
			get => user;
			set => SetProperty(ref user, value);
		}

		public UserModel UserE6AI {
			get => userE6AI;
			set => SetProperty(ref userE6AI, value);
		}

		public bool CheckLocalUser() {
			UserModel user = GetCurrentUser();
			return user.Username.IsNotBlank() && user.UserAPI.IsNotBlank();
		}

		public void SetLocalUser(string username, string api) {
			UserModel model = new(username, api);
			switch (StartHostType) {
				case HostType.E926:
					User = model;
					break;
				case HostType.E621:
					User = model;
					break;
				case HostType.E6AI:
					UserE6AI = model;
					break;
				default:
					throw new NotImplementedException();
			}
		}

		public void ClearLocalUser() {
			switch (StartHostType) {
				case HostType.E926:
					User = default;
					break;
				case HostType.E621:
					User = default;
					break;
				case HostType.E6AI:
					UserE6AI = default;
					break;
				default:
					throw new NotImplementedException();
			}
		}

		public UserModel GetCurrentUser() {
			return StartHostType switch {
				HostType.E926 => User,
				HostType.E621 => User,
				HostType.E6AI => UserE6AI,
				_ => throw new NotImplementedException(),
			};
		}

	}

	public readonly record struct UserModel(string Username, string UserAPI);

	//Local
	public partial class LocalSettings {

		private static bool InitializingLock { get; set; }

		public static async Task<LocalSettings> Read() {
			InitializingLock = true;
			string json = await Local.ReadFile(Local.SettingsFile);
			LocalSettings settings = JsonConvert.DeserializeObject<LocalSettings>(json) ?? new LocalSettings();
			StartHostType = settings.HostType;
			InitializingLock = false;
			return settings;
		}

		public static async void Write() {
			string json = JsonConvert.SerializeObject(Local.Settings, Formatting.Indented);
			await Local.WriteFile(Local.SettingsFile, json);
		}

		protected override void OnPropertyChanged(PropertyChangedEventArgs args) {
			base.OnPropertyChanged(args);
			if (InitializingLock) {
				return;
			}
			Write();
		}

	}

	public enum RequestDownloadAction {
		[Description("Manually Select Folder")]
		[ToolTip("Tool Tip Here")]
		Select = 0,
		[Description("Auto Download To Tags Related Folder")]
		TagsRelated = 1,
		[Description("Auto Download To Specified Sub Folder")]
		Specify = 2,
	}

	public enum DownloadFileConflictAction {
		[Description("Generate New Name")]
		[ToolTip("Tool Tip Here")]
		GenerateNewName = 0,
		[Description("Skip Files With Same Name")]
		Skip = 1,
		[Description("Replace Files With Same Name")]
		Replace,
	}

	public enum HostType {
		E926 = 0,
		E621 = 1,
		E6AI = 2,
	}

	public enum AppTheme {
		Light = 0,
		Dark = 1,
		System = 2,
	}

	public enum StartupTagsType {
		StartupTags = 0,
		RestoreTags = 1,
	}
}
