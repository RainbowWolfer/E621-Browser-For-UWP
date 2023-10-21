using Microsoft.UI.Xaml.Controls;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.ComponentModel;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.AccessCache;
using YiffBrowser.Attributes;
using YiffBrowser.Helpers;

namespace YiffBrowser.Services.Locals {
	public class LocalSettings {
		public HostType HostType { get; set; } = HostType.E621;
		public int E621PageLimitCount { get; set; } = 75;


		public bool EnableStartupTags { get; set; } = true;
		//"brazhnik"
		public string[] StartupTags { get; set; } = { "order:rank" };

		public bool EnableTransitionAnimation { get; set; } = true;

		public int ImageAdaptWidth { get; set; } = 380;
		public int ImageAdaptHeight { get; set; } = 50;

		public string Username { get; set; } = "";
		public string UserAPI { get; set; } = "";


		public string DownloadFolderToken { get; set; }

		#region User

		public bool CheckLocalUser() => Username.IsNotBlank() && UserAPI.IsNotBlank();

		public void SetLocalUser(string username, string api) {
			Username = username;
			UserAPI = api;
			Write();
		}

		public void ClearLocalUser() {
			Username = string.Empty;
			UserAPI = string.Empty;
			Write();
		}

		#endregion

		#region FolderToken

		public bool HasDownloadFolderToken() => DownloadFolderToken.IsNotBlank() && Local.DownloadFolder != null;

		public void SetDownloadFolder(StorageFolder folder) {
			if (folder == null) {
				return;
			}
			Local.DownloadFolder = folder;
			string token = StorageApplicationPermissions.FutureAccessList.Add(folder);
			DownloadFolderToken = token;
			Write();
		}

		public void ClearDownloadFolder() {
			Local.DownloadFolder = null;
			DownloadFolderToken = null;
			Write();
		}

		#endregion

		#region Local

		public static async Task Read() {
			string json = await Local.ReadFile(Local.SettingsFile);
			Local.Settings = JsonConvert.DeserializeObject<LocalSettings>(json) ?? new LocalSettings();
		}

		public static async void Write() {
			string json = JsonConvert.SerializeObject(Local.Settings, Formatting.Indented);
			await Local.WriteFile(Local.SettingsFile, json);
		}

		#endregion

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
