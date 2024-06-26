﻿using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.AccessCache;
using YiffBrowser.Database;
using YiffBrowser.Helpers;

namespace YiffBrowser.Services.Locals {
	public static class Local {
		public static LocalSettings Settings { get; private set; }
		public static Listing Listing { get; private set; }


		public static StorageFolder LocalFolder => ApplicationData.Current.LocalFolder;
		public static StorageFolder YiffFolder { get; private set; }


		public static StorageFile ListingFile { get; private set; }
		public static StorageFile SettingsFile { get; private set; }

		public static StorageFolder DownloadFolder { get; set; }

		public static StorageFile DatabaseFile { get; private set; }

		public static async Task Initialize() {
			Debug.WriteLine(LocalFolder.Path);

			await Task.Run(async () => {
				YiffFolder = await LocalFolder.CreateFolderAsync("Yiff", CreationCollisionOption.OpenIfExists);

				ListingFile = await YiffFolder.CreateFileAsync("Listings.json", CreationCollisionOption.OpenIfExists);
				SettingsFile = await YiffFolder.CreateFileAsync("Settings.json", CreationCollisionOption.OpenIfExists);

				DatabaseFile = await YiffFolder.CreateFileAsync(E621DownloadDataAccess.DatabaseFileName, CreationCollisionOption.OpenIfExists);

				Listing = await Listing.Read();
				Settings = await LocalSettings.Read();

				try {
					string token = Settings.DownloadFolderToken;
					if (token.IsNotBlank()) {
						DownloadFolder = await StorageApplicationPermissions.FutureAccessList.GetFolderAsync(token);
					}
				} catch { }
			});

			await E621DownloadDataAccess.UpdateConnection();
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
