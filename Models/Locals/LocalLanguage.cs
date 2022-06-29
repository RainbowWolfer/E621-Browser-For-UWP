using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;

namespace E621Downloader.Models.Locals {
	public class LocalLanguage {
		public static LocalLanguage Current { get; private set; }
		public static StorageFile LanguageFile { get; private set; }
		public const string LANGUAGUE_FILE_NAME = "Language.settings";


		public Language? language;

		public string GetSystemLanguage() {
			return language switch {
				Language.English => "en-US",
				Language.Chinese => "zh-Hans",
				null => "",
				_ => "",
			};
		}

		public static async Task Initialize() {
			LanguageFile = await Local.LocalFolder.CreateFileAsync(LANGUAGUE_FILE_NAME, CreationCollisionOption.OpenIfExists);
			await ReadLocalSettings();
		}

		public static async void Save() {
			await WriteLocalSettings();
		}

		public static async Task ReadLocalSettings() {
			using(Stream stream = await LanguageFile.OpenStreamForReadAsync()) {
				using StreamReader reader = new(stream);
				Current = JsonConvert.DeserializeObject<LocalLanguage>(await reader.ReadToEndAsync());
			}
			if(Current == null) {
				Current = GetDefault();
				await WriteLocalSettings();
			}
		}

		public static async Task WriteLocalSettings() {
			await FileIO.WriteTextAsync(LanguageFile, JsonConvert.SerializeObject(Current, Formatting.Indented));
		}

		public static LocalLanguage GetDefault() {
			return new LocalLanguage() {
				language = null,
			};
		}

	}


	public enum Language {
		English, Chinese
	}
}
