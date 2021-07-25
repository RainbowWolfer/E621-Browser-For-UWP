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
using E621Downloader.Models.Posts;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Storage.Streams;
using Windows.Storage.FileProperties;

namespace E621Downloader.Models.Locals {
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

		public static StorageFolder DownloadFolder { get; private set; }

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


		public async static Task WriteTokenToFile(string token) {
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
			try {
				DownloadFolder = await StorageApplicationPermissions.FutureAccessList.GetFolderAsync(token);
			} catch(ArgumentException e) {
				Debug.WriteLine(e);
			}
		}

		public async static void ClearToken(string token) {
			StorageApplicationPermissions.FutureAccessList.Remove(token);
			Local.token = null;
			DownloadFolder = null;
			await FileIO.WriteTextAsync(FutureAccessTokenFile, "");
		}

		private async static Task<string> GetTokenFromFile() {
			Stream stream = await FutureAccessTokenFile.OpenStreamForReadAsync();
			StreamReader reader = new StreamReader(stream);
			return reader.ReadToEnd();
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

		public static MetaFile CreateMetaFile(StorageFile file, Post post, string groupName) {
			MetaFile meta = new MetaFile(file.Path, groupName, post);
			WriteMetaFile(meta, file, post);
			return meta;
		}
		private async static void WriteMetaFile(MetaFile meta, StorageFile file, Post post) {
			StorageFolder folder = await file.GetParentAsync();
			StorageFile target = await folder.CreateFileAsync($"{post.id}.meta", CreationCollisionOption.ReplaceExisting);
			try {
				await FileIO.WriteTextAsync(target, meta.ConvertJson());
			} catch(Exception e) {
				Debug.WriteLine(e.Message);
			}
		}
		public async static void WriteMetaFile(MetaFile meta, Post post, string groupName) {
			(MetaFile, StorageFile) file = await GetMetaFile(post.id.ToString(), groupName);
			WriteMetaFile(meta, file.Item2, post);
		}

		public async static Task<StorageFolder[]> GetDownloadsFolders() {
			if(DownloadFolder == null) {
				return Array.Empty<StorageFolder>();
			}
			return (await DownloadFolder.GetFoldersAsync()).ToArray();
		}
		private class Pair {
			public MetaFile meta;
			public BitmapImage source;
			public StorageFile file;
			public string SourceID => file.DisplayName;

			public bool IsValid => meta != null /*&& source != null */&& SourceID != null;

			public static void Add(List<Pair> list, MetaFile meta) {
				foreach(var item in list) {
					if(item.SourceID == meta.MyPost.id.ToString()) {
						item.meta = meta;
						return;
					}
				}
				list.Add(new Pair() { meta = meta });
			}
			public static void Add(List<Pair> list, BitmapImage source, StorageFile file) {
				foreach(var item in list) {
					if(item.meta != null && item.meta.MyPost.id.ToString() == file.DisplayName) {
						item.file = file;
						item.source = source;
						return;
					}
				}
				list.Add(new Pair() { file = file, source = source });
			}

			public static (MetaFile, BitmapImage, StorageFile) Convert(Pair pair) {
				return (pair.meta, pair.source, pair.file);
			}

			public static List<(MetaFile, BitmapImage, StorageFile)> Convert(List<Pair> list, Func<Pair, bool> check) {
				var result = new List<(MetaFile, BitmapImage, StorageFile)>();
				foreach(Pair item in list) {
					if((check?.Invoke(item)).Value) {
						result.Add((item.meta, item.source, item.file));
					}
				}
				return result;
			}
		}

		public async static Task<List<(MetaFile, BitmapImage, StorageFile)>> GetMetaFiles(string folderName) {
			var result = new List<(MetaFile, BitmapImage)>();
			StorageFolder folder = await DownloadFolder.GetFolderAsync(folderName);
			var pairs = new List<Pair>();
			foreach(StorageFile file in await folder.GetFilesAsync()) {
				if(file.FileType == ".meta") {
					MetaFile meta;
					using(Stream stream = await file.OpenStreamForReadAsync()) {
						using(StreamReader reader = new StreamReader(stream)) {
							meta = JsonConvert.DeserializeObject<MetaFile>(reader.ReadToEnd());
						}
					}
					if(meta != null) {
						Pair.Add(pairs, meta);
					}
				} else {
					BitmapImage bitmap = new BitmapImage();
					ThumbnailMode mode = ThumbnailMode.SingleItem;
					if(new string[] { ".webm" }.Contains(file.FileType)) {
						mode = ThumbnailMode.SingleItem;
					} else if(new string[] { ".jpg", ".png" }.Contains(file.FileType)) {
						mode = ThumbnailMode.PicturesView;
					}
					//Debug.WriteLine(mode);
					using(StorageItemThumbnail thumbnail = await file.GetThumbnailAsync(mode)) {
						if(thumbnail != null) {
							using(Stream stream = thumbnail.AsStreamForRead()) {
								bitmap.SetSource(stream.AsRandomAccessStream());
							}
						}
					}
					Pair.Add(pairs, bitmap, file);
				}
			}
			return Pair.Convert(pairs, p => p.IsValid);
		}

		public async static Task<(MetaFile, StorageFile)> GetMetaFile(string postID, string groupName) {
			StorageFolder folder = await DownloadFolder.GetFolderAsync(groupName);
			StorageFile file = await folder.GetFileAsync($"{postID}.meta");
			using(Stream stream = await file.OpenStreamForReadAsync()) {
				using(StreamReader reader = new StreamReader(stream)) {
					return (JsonConvert.DeserializeObject<MetaFile>(await reader.ReadToEndAsync()), file);
				}
			}
		}

		public async static Task<List<MetaFile>> GetAllMetaFiles() {
			var metas = new List<MetaFile>();
			foreach(StorageFolder folder in await DownloadFolder.GetFoldersAsync()) {
				foreach(StorageFile file in await folder.GetFilesAsync()) {
					if(file.FileType != ".meta") {
						continue;
					}
					using(Stream stream = await file.OpenStreamForReadAsync()) {
						using(StreamReader reader = new StreamReader(stream)) {
							string content = reader.ReadToEnd();
							MetaFile meta = JsonConvert.DeserializeObject<MetaFile>(content);
							if(meta == null) {
								continue;
							}
							metas.Add(meta);
						}
					}
				}
			};
			return metas;
		}


		public async static void UpdateMetaFile(StorageFile file, MetaFile meta) {
			await FileIO.WriteTextAsync(file, meta.ConvertJson());
		}

		public async static Task<List<MetaFile>> FindAllMetaFiles() {
			var result = new List<MetaFile>();

			foreach(StorageFolder folder in await DownloadFolder.GetFoldersAsync()) {
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
