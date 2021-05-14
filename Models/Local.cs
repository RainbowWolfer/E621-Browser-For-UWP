using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage;
using Windows.UI.Popups;

namespace E621Downloader.Models {
	public static class Local {
		private static bool initialized = false;
		private const string FOLLOWLISTNAME = "FollowList.txt";
		private const string BLACKLISTNAME = "BlackList.txt";
		private static StorageFolder LocalFolder => ApplicationData.Current.LocalFolder;

		private static StorageFile followListFile;
		private static StorageFile blackListFile;

		public static string[] FollowList { get; private set; }
		public static string[] BlackList { get; private set; }

		public async static void Initialize() {
			if(initialized) {
				throw new Exception("Local has been initialized more than one time!");
			}
			initialized = true;
			followListFile = await LocalFolder.CreateFileAsync(FOLLOWLISTNAME, CreationCollisionOption.OpenIfExists);
			blackListFile = await LocalFolder.CreateFileAsync(BLACKLISTNAME, CreationCollisionOption.OpenIfExists);

			Debug.WriteLine(followListFile.Path);
			Debug.WriteLine(blackListFile.Path);

			await Reload();
		}

		public async static Task Reload() {
			FollowList = await GetList(followListFile);
			BlackList = await GetList(blackListFile);
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
