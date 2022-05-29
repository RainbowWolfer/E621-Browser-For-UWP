using E621Downloader.Models.Download;
using E621Downloader.Models.Locals;
using E621Downloader.Models.Posts;
using E621Downloader.Pages;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.Storage.FileProperties;
using Windows.Storage.Streams;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace E621Downloader.Models.Locals {
	public static class Local {
		private static bool initialized = false;

		private const string LISTING_NAME = "Listing.json";
		private const string TOKEN_NAME = "Token.txt";
		private const string DOWNLOADS_INFO_NAME = "DownloadsInfo.json";
		private const string FAVORITES_LIST_NAME = "FavorutesList.json";
		private const string LOCAL_SETTINGS_NAME = "LocalSettings.settings";
		private const string HISTORY_NAME = "History.json";
		private const string WALLPAPERS_FOLDER_NAME = "WallpapersCache";

		public static StorageFolder LocalFolder => ApplicationData.Current.LocalFolder;

		public static StorageFolder WallpapersFolder { get; private set; }

		public static StorageFile ListingFile { get; private set; }
		//public static StorageFile BlackListFile { get; private set; }

		public static StorageFile FutureAccessTokenFile { get; private set; }
		public static StorageFile DownloadsInfoFile { get; private set; }
		public static StorageFile FavoritesListFile { get; private set; }
		public static StorageFile LocalSettingsFile { get; private set; }

		public static StorageFile HistoryFile { get; private set; }

		//public static string[] FollowList { get; private set; }
		//public static string[] BlackList { get; private set; }

		public static Listing Listing { get; private set; }

		public static History History { get; private set; }

		private static string token;

		public static StorageFolder DownloadFolder { get; private set; }

		public static async Task Initialize() {
			Debug.WriteLine("Initializing Local");
			Debug.WriteLine(LocalFolder.Path);

			if(initialized) {
				throw new Exception("Local has been initialized more than one time!");
			}
			initialized = true;
			ListingFile = await LocalFolder.CreateFileAsync(LISTING_NAME, CreationCollisionOption.OpenIfExists);

			FutureAccessTokenFile = await LocalFolder.CreateFileAsync(TOKEN_NAME, CreationCollisionOption.OpenIfExists);

			DownloadsInfoFile = await LocalFolder.CreateFileAsync(DOWNLOADS_INFO_NAME, CreationCollisionOption.OpenIfExists);

			FavoritesListFile = await LocalFolder.CreateFileAsync(FAVORITES_LIST_NAME, CreationCollisionOption.OpenIfExists);

			LocalSettingsFile = await LocalFolder.CreateFileAsync(LOCAL_SETTINGS_NAME, CreationCollisionOption.OpenIfExists);

			HistoryFile = await LocalFolder.CreateFileAsync(HISTORY_NAME, CreationCollisionOption.OpenIfExists);

			WallpapersFolder = await LocalFolder.CreateFolderAsync(WALLPAPERS_FOLDER_NAME, CreationCollisionOption.OpenIfExists);

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


		public static async Task WriteTokenToFile(string token) {
			await FileIO.WriteTextAsync(FutureAccessTokenFile, token);
			await SetToken(token);
		}

		public static string GetToken() => token;
		public static async Task SetToken(string token) {
			Local.token = token;
			if(string.IsNullOrWhiteSpace(token)) {
				//set to download library
				return;
			}
			try {
				DownloadFolder = await StorageApplicationPermissions.FutureAccessList.GetFolderAsync(token);
			} catch(ArgumentException e) {
				Debug.WriteLine(e);
			} catch(FileNotFoundException e) {
				Debug.WriteLine(e);
			}
		}

		public static async void ClearToken(string token) {
			StorageApplicationPermissions.FutureAccessList.Remove(token);
			Local.token = null;
			DownloadFolder = null;
			await FileIO.WriteTextAsync(FutureAccessTokenFile, "");
		}

		private static async Task<string> GetTokenFromFile() {
			return await FileIO.ReadTextAsync(FutureAccessTokenFile);
		}

		public static async Task Reload() {
			await ReadListing();
			await ReadHistory();
			await ReadLocalSettings();
			string token = await GetTokenFromFile();
			await SetToken(token);
			await ReadFavoritesLists();
		}

		public static async Task WriteHistory() {
			string json = JsonConvert.SerializeObject(History, Formatting.Indented);
			await FileIO.WriteTextAsync(HistoryFile, json);
		}

		public static async Task ReadHistory() {
			string json = await FileIO.ReadTextAsync(HistoryFile);
			History = JsonConvert.DeserializeObject<History>(json) ?? new();
		}

		public static async Task WriteListing() {
			string json = JsonConvert.SerializeObject(Listing, Formatting.Indented);
			await FileIO.WriteTextAsync(ListingFile, json);
		}

		public static async Task ReadListing() {
			string json = await FileIO.ReadTextAsync(ListingFile);
			Listing = JsonConvert.DeserializeObject<Listing>(json);
			if(Listing == null) {
				Listing = Listing.GetDefaultListing();
				await WriteListing();
			}
		}

		public static MetaFile CreateMetaFile(StorageFile file, Post post, string groupName) {
			MetaFile meta = new(file.Path, groupName, post);
			WriteMetaFile(meta, file, post);
			return meta;
		}
		private static async void WriteMetaFile(MetaFile meta, StorageFile file, Post post) {
			try {
				StorageFolder folder = await file.GetParentAsync();
				//An exception of type 'System.IO.FileLoadException' occurred in System.Private.CoreLib.dll but was not handled in user code
				//The process cannot access the file because it is being used by another process. (Exception from HRESULT: 0x80070020)
				StorageFile target = await folder.CreateFileAsync($"{post.id}.meta", CreationCollisionOption.ReplaceExisting);
				await FileIO.WriteTextAsync(target, meta.ConvertJson());
			} catch(Exception e) {
				Debug.WriteLine(e.Message);
			}
		}
		public static async void WriteMetaFile(MetaFile meta, Post post, string groupName) {
			//System.IO.FileLoadException: 'The process cannot access the file because it is being used by another process. (Exception from HRESULT: 0x80070020)'
			try {
				(MetaFile, StorageFile) file = await GetMetaFile(post.id.ToString(), groupName);
				WriteMetaFile(meta, file.Item2, post);
			} catch(Exception e) {
				Debug.WriteLine(e.Message);
			}
		}

		public static async Task<StorageFolder[]> GetDownloadsFolders() {
			return DownloadFolder == null ? null : (await DownloadFolder.GetFoldersAsync()).ToArray();
		}
		private class Pair {
			public MetaFile meta;
			public BitmapImage source;
			public StorageFile file;
			public string SourceID => file?.DisplayName;

			public bool IsValid => meta != null /*&& source != null */&& SourceID != null;

			public static void Add(List<Pair> list, MetaFile meta) {
				foreach(var item in list) {
					if(item.file == null) {//not sure
						continue;
					}
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

		public static async Task<List<(MetaFile meta, BitmapImage bitmap, StorageFile file)>> GetMetaFiles(StorageFolder folder, Action<StorageFile, int, int> onNextFileLoad = null) {
			var result = new List<(MetaFile, BitmapImage)>();
			//StorageFolder folder = await DownloadFolder.GetFolderAsync(folderName);
			if(folder == null) {
				return new List<(MetaFile, BitmapImage, StorageFile)>();
			}
			var pairs = new List<Pair>();
			IReadOnlyList<StorageFile> files = await folder.GetFilesAsync();
			for(int i = 0; i < files.Count; i++) {
				StorageFile file = files[i];
				onNextFileLoad?.Invoke(file, i + 1, files.Count);
				if(file.FileType == ".meta") {
					MetaFile meta;
					using(Stream stream = await file.OpenStreamForReadAsync()) {
						using StreamReader reader = new(stream);
						meta = JsonConvert.DeserializeObject<MetaFile>(await reader.ReadToEndAsync());
					}
					if(meta != null) {
						Pair.Add(pairs, meta);
					}
				} else {
					BitmapImage bitmap = new();
					ThumbnailMode mode = ThumbnailMode.SingleItem;
					if(new string[] { ".webm" }.Contains(file.FileType)) {
						mode = ThumbnailMode.SingleItem;
					} else if(new string[] { ".jpg", ".png" }.Contains(file.FileType)) {
						mode = ThumbnailMode.PicturesView;
					}
					//Debug.WriteLine(mode);
					using(StorageItemThumbnail thumbnail = await file.GetThumbnailAsync(mode)) {
						if(thumbnail != null) {
							using Stream stream = thumbnail.AsStreamForRead();
							await bitmap.SetSourceAsync(stream.AsRandomAccessStream());
						}
					}
					Pair.Add(pairs, bitmap, file);
				}
			}
			return Pair.Convert(pairs, p => p.IsValid);
		}

		public static async Task<(MetaFile, StorageFile)> GetMetaFile(string postID, string groupName) {
			StorageFolder folder = await DownloadFolder.GetFolderAsync(groupName);
			StorageFile file = await folder.GetFileAsync($"{postID}.meta");
			using Stream stream = await file.OpenStreamForReadAsync();
			using StreamReader reader = new(stream);
			return (JsonConvert.DeserializeObject<MetaFile>(await reader.ReadToEndAsync()), file);
		}

		public static async Task<List<MetaFile>> GetAllMetaFiles() {
			var metas = new List<MetaFile>();
			foreach(StorageFolder folder in await DownloadFolder.GetFoldersAsync()) {
				foreach(StorageFile file in await folder.GetFilesAsync()) {
					if(file.FileType != ".meta") {
						continue;
					}
					using Stream stream = await file.OpenStreamForReadAsync();
					using StreamReader reader = new(stream);
					string content = await reader.ReadToEndAsync();
					MetaFile meta = JsonConvert.DeserializeObject<MetaFile>(content);
					if(meta == null) {
						continue;
					}
					metas.Add(meta);
				}
			};
			return metas;
		}


		public static async void UpdateMetaFile(StorageFile file, MetaFile meta) {
			await FileIO.WriteTextAsync(file, meta.ConvertJson());
		}

		public static async Task<List<MetaFile>> FindAllMetaFiles() {
			var result = new List<MetaFile>();

			foreach(StorageFolder folder in await DownloadFolder.GetFoldersAsync()) {
				foreach(StorageFile item in await folder.GetFilesAsync()) {
					using Stream stream = await item.OpenStreamForReadAsync();
					using StreamReader reader = new(stream);
					result.Add(JsonConvert.DeserializeObject(await reader.ReadToEndAsync()) as MetaFile);
				}
			}
			return result;
		}

		public static async Task WriteLocalSettings() {
			await FileIO.WriteTextAsync(LocalSettingsFile, JsonConvert.SerializeObject(LocalSettings.Current, Formatting.Indented));
		}

		public static async Task ReadLocalSettings() {
			using(Stream stream = await LocalSettingsFile.OpenStreamForReadAsync()) {
				using StreamReader reader = new(stream);
				LocalSettings.Current = JsonConvert.DeserializeObject<LocalSettings>(await reader.ReadToEndAsync());
			}
			if(LocalSettings.Current == null) {
				LocalSettings.Current = new LocalSettings() {
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
					customHost = "",
					spot_amount = 1,
					enableHotKeys = true,
					adaptiveGrid = true,
					adaptiveSizeMultiplier = 1,
					fixedHeight = 280,
					randomTagMaxCount = 10000,
				};
				await WriteLocalSettings();
			}
		}

		public static async Task ReadFavoritesLists() {
			using Stream stream = await FavoritesListFile.OpenStreamForReadAsync();
			using StreamReader reader = new(stream);
			FavoritesList.Table = JsonConvert.DeserializeObject<List<FavoritesList>>(await reader.ReadToEndAsync()) ?? new List<FavoritesList>();
		}

		public static async Task WriteFavoritesLists() {
			await FileIO.WriteTextAsync(FavoritesListFile, JsonConvert.SerializeObject(FavoritesList.Table, Formatting.Indented));
		}

		//F:\E621\creepypasta -momo_(creepypasta) rating;e\1820721.png
		public static async Task<(StorageFile, MetaFile)> GetDownloadFile(string path) {
			StorageFile file;
			try {
				file = await StorageFile.GetFileFromPathAsync(path);
			} catch(FileNotFoundException) {
				return (null, null);
			}
			string metaPath = path.Substring(0, path.LastIndexOf('.')) + ".meta";
			StorageFile metaFile;
			try {
				metaFile = await StorageFile.GetFileFromPathAsync(metaPath);
			} catch(FileNotFoundException) {
				return (file, null);
			}
			MetaFile meta;
			using(Stream stream = await metaFile.OpenStreamForReadAsync()) {
				using StreamReader reader = new(stream);
				meta = JsonConvert.DeserializeObject<MetaFile>(await reader.ReadToEndAsync());
			}
			return (file, meta);
		}
	}
}
