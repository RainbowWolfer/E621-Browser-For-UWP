using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.AccessCache;
using YiffBrowser.Helpers;

namespace YiffBrowser.Services.Locals {
	public static class Local {
		public static LocalSettings Settings { get; set; }
		public static Listing Listing { get; set; }


		public static StorageFolder LocalFolder => ApplicationData.Current.LocalFolder;


		public static StorageFile ListingFile { get; private set; }
		public static StorageFile SettingsFile { get; private set; }
		public static StorageFolder DownloadFolder { get; set; }

		public static async Task Initialize() {
			Debug.WriteLine(LocalFolder.Path);

			await Task.Run(async () => {
				ListingFile = await LocalFolder.CreateFileAsync("Listings.json", CreationCollisionOption.OpenIfExists);
				SettingsFile = await LocalFolder.CreateFileAsync("Settings.json", CreationCollisionOption.OpenIfExists);

				await Listing.Read();
				await LocalSettings.Read();

				try {
					string token = Settings.DownloadFolderToken;
					if (token.IsNotBlank()) {
						DownloadFolder = await StorageApplicationPermissions.FutureAccessList.GetFolderAsync(token);
					}
				} catch { }
			});
		}



		public static async Task<string> ReadFile(IStorageFile file) {
			try {
				return await FileIO.ReadTextAsync(file);
			} catch (Exception ex) {
				Debug.WriteLine(ex);
				Debugger.Break();
				return null;
			}
		}

		public static async Task WriteFile(IStorageFile file, string content) {
			try {
				await FileIO.WriteTextAsync(file, content);
			} catch (Exception ex) {
				Debug.WriteLine(ex);
				Debugger.Break();
			}
		}

	}
}
