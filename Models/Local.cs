using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.UI.Popups;

namespace E621Downloader.Models {
	public static class Local {
		private static bool initialized = false;
		private const string FOLLOWLISTNAME = "FollowList.txt";
		private const string BLACKLISTNAME = "BlackList.txt";
		private const string TOKENNAME = "Token.txt";
		private static StorageFolder LocalFolder => ApplicationData.Current.LocalFolder;

		private static StorageFile followListFile;
		private static StorageFile blackListFile;

		private static StorageFile futureAccessTokenFile;

		public static string[] FollowList { get; private set; }
		public static string[] BlackList { get; private set; }
		private static string token;

		public static StorageFolder downloadFolder;

		public async static void Initialize() {
			if(initialized) {
				throw new Exception("Local has been initialized more than one time!");
			}
			initialized = true;
			followListFile = await LocalFolder.CreateFileAsync(FOLLOWLISTNAME, CreationCollisionOption.OpenIfExists);
			blackListFile = await LocalFolder.CreateFileAsync(BLACKLISTNAME, CreationCollisionOption.OpenIfExists);

			futureAccessTokenFile = await LocalFolder.CreateFileAsync(TOKENNAME, CreationCollisionOption.OpenIfExists);

			Debug.WriteLine(followListFile.Path);
			Debug.WriteLine(blackListFile.Path);

			await Reload();
		}

		public async static void WriteToken(string token) {
			await FileIO.WriteTextAsync(futureAccessTokenFile, token);
			await SetToken(token);
		}

		public static string GetToken() => token;
		public async static Task SetToken(string token) {
			Local.token = token;
			if(string.IsNullOrEmpty(token)) {
				//set to download library
				return;
			}
			downloadFolder = await StorageApplicationPermissions.FutureAccessList.GetFolderAsync(token);
		}

		public async static void WriteFollowList(string[] list) {
			await FileIO.WriteLinesAsync(followListFile, list);
		}
		public async static void WriteBlackList(string[] list) {
			await FileIO.WriteLinesAsync(blackListFile, list);
		}

		public async static Task Reload() {
			FollowList = await GetFollowList();
			BlackList = await GetBlackList();
			await SetToken(await GetTokenFromFile());
		}

		private async static Task<string> GetTokenFromFile() {
			Stream stream = await futureAccessTokenFile.OpenStreamForReadAsync();
			StreamReader reader = new StreamReader(stream);
			return reader.ReadToEnd();
		}

		private async static Task<string[]> GetFollowList() => await GetList(followListFile);
		private async static Task<string[]> GetBlackList() => await GetList(blackListFile);
		private async static Task<string[]> GetList(StorageFile file) {
			Stream stream = await file.OpenStreamForReadAsync();
			StreamReader reader = new StreamReader(stream);

			var list = new List<string>();
			string line = "";
			foreach(char c in reader.ReadToEnd()) {
				if(c == '\r' || c == '\n') {
					if(line.Length > 0) {
						list.Add(line);
					}
					line = "";
					continue;
				}
				line += c;
			}
			if(line.Length > 0) {
				list.Add(line);
			}

			return list.ToArray();
		}
	}
}
