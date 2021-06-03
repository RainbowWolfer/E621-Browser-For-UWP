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
using Newtonsoft.Json;
using E621Downloader.Models.Download;
using E621Downloader.Models.Locals;

namespace E621Downloader.Models {
	public static class Local {
		private static bool initialized = false;
		private const string FOLLOWLISTNAME = "FollowList.txt";
		private const string BLACKLISTNAME = "BlackList.txt";
		private const string TOKENNAME = "Token.txt";
		private const string DOWNLOADSINFONAME = "DownloadsInfo.json";

		private static StorageFolder LocalFolder => ApplicationData.Current.LocalFolder;

		public static StorageFile FollowListFile { get; private set; }
		public static StorageFile BlackListFile { get; private set; }

		public static StorageFile FutureAccessTokenFile { get; private set; }
		public static StorageFile DownloadsInfoFile { get; private set; }

		public static string[] FollowList { get; private set; }
		public static string[] BlackList { get; private set; }

		private static string token;

		public static StorageFolder downloadFolder;

		public async static void Initialize() {
			Debug.WriteLine(LocalFolder.Path);
			if(initialized) {
				throw new Exception("Local has been initialized more than one time!");
			}
			initialized = true;
			FollowListFile = await LocalFolder.CreateFileAsync(FOLLOWLISTNAME, CreationCollisionOption.OpenIfExists);
			BlackListFile = await LocalFolder.CreateFileAsync(BLACKLISTNAME, CreationCollisionOption.OpenIfExists);

			FutureAccessTokenFile = await LocalFolder.CreateFileAsync(TOKENNAME, CreationCollisionOption.OpenIfExists);

			DownloadsInfoFile = await LocalFolder.CreateFileAsync(DOWNLOADSINFONAME, CreationCollisionOption.OpenIfExists);

			await Reload();
		}

		//public async static Task<List<DownloadInstanceLocalManager.DownloadInstanceLocal>> GetDownloadsInfo() {
		//	Stream stream = await DownloadsInfoFile.OpenStreamForReadAsync();
		//	StreamReader reader = new StreamReader(stream);
		//	var ReList = JsonConvert.DeserializeObject<List<DownloadInstanceLocalManager.DownloadInstanceLocal>>(reader.ReadToEnd());
		//	return ReList;
		//}

		//public async static void WriteDownloadsInfo() {
		//	string json = JsonConvert.SerializeObject(DownloadsManager.downloads);
		//	await FileIO.WriteTextAsync(DownloadsInfoFile, json);
		//}


		public async static Task WriteToken(string token) {
			await FileIO.WriteTextAsync(FutureAccessTokenFile, token);
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
			await FileIO.WriteLinesAsync(FollowListFile, list);
		}
		public async static void WriteBlackList(string[] list) {
			await FileIO.WriteLinesAsync(BlackListFile, list);
		}

		public async static Task Reload() {
			FollowList = await GetFollowList();
			BlackList = await GetBlackList();
			await SetToken(await GetTokenFromFile());
		}

		private async static Task<string> GetTokenFromFile() {
			Stream stream = await FutureAccessTokenFile.OpenStreamForReadAsync();
			StreamReader reader = new StreamReader(stream);
			return reader.ReadToEnd();
		}

		private async static Task<string[]> GetFollowList() => await GetListFromFile(FollowListFile);
		private async static Task<string[]> GetBlackList() => await GetListFromFile(BlackListFile);
		private async static Task<string[]> GetListFromFile(StorageFile file) {
			var list = new List<string>();
			using(Stream stream = await file.OpenStreamForReadAsync()) {
				using(StreamReader reader = new StreamReader(stream)) {
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
				}
			}
			return list.ToArray();
		}

		public async static void CreateMetaFile(StorageFile file, Post post, string groupName) {
			StorageFolder folder = await file.GetParentAsync();
			StorageFile target = await folder.CreateFileAsync($"{post.id}.meta", CreationCollisionOption.ReplaceExisting);

			MetaFile meta = new MetaFile(file.Path, post.file.ext, post.id.ToString(), groupName, post.tags.GetAllTags().ToArray());

			await FileIO.WriteTextAsync(target, meta.ConvertJson());
		}

		public async static Task<MetaFile> GetMetaFile(string postID, string groupName) {
			StorageFolder folder = await downloadFolder.GetFolderAsync(groupName);
			StorageFile file = await folder.GetFileAsync($"{postID}.meta");
			using(Stream stream = await file.OpenStreamForReadAsync()) {
				using(StreamReader reader = new StreamReader(stream)) {
					return JsonConvert.DeserializeObject(reader.ReadToEnd()) as MetaFile;
				}
			}
		}

		public async static Task<List<MetaFile>> FindAllMetaFiles() {
			var result = new List<MetaFile>();

			foreach(StorageFolder folder in await downloadFolder.GetFoldersAsync()) {
				foreach(StorageFile item in await folder.GetFilesAsync()) {
					using(Stream stream = await item.OpenStreamForReadAsync()) {
						using(StreamReader reader = new StreamReader(stream)) {
							result.Add(JsonConvert.DeserializeObject(reader.ReadToEnd()) as MetaFile);
						}
					}
				}
			}
			return result;
		}
	}
}
