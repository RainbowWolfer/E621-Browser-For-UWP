using System.Diagnostics;
using System.Threading.Tasks;
using System;
using Windows.Storage;
using Newtonsoft.Json;

namespace BaseFramework {
	public static class NewVersionService {
		public static StorageFolder LocalFolder => ApplicationData.Current.LocalFolder;

		public static StorageFile NewVersionFile { get; private set; }

		private static NewVersionModel Model { get; set; }

		public static bool IsCurrentInNewVersion() {
			return Model.UseNewVersion;
		}

		public static async Task UseNewVersion(bool on) {
			Model.UseNewVersion = on;
			await WriteNewVersionModel();
		}

		public static bool IsFirstTime() {
			return Model.FirstTime;
		}

		public static async Task DisableFirstTime() {
			Model.FirstTime = false;
			await WriteNewVersionModel();
		}

		public static async Task ReadNewVersionModel() {
			try {
				NewVersionFile = await LocalFolder.CreateFileAsync("NewVersion", CreationCollisionOption.OpenIfExists);
				string json = await FileIO.ReadTextAsync(NewVersionFile);
				if (string.IsNullOrWhiteSpace(json)) {
					throw new Exception();
				}
				Model = JsonConvert.DeserializeObject<NewVersionModel>(json);
				if (Model == null) {
					throw new Exception();
				}
			} catch (Exception ex) {
				Debug.WriteLine(ex);
				Model = new NewVersionModel();
				await WriteNewVersionModel();
			}
		}

		public static async Task WriteNewVersionModel() {
			try {
				string model = JsonConvert.SerializeObject(Model);
				await FileIO.WriteTextAsync(NewVersionFile, model);
			} catch (Exception ex) {
				Debug.WriteLine(ex);
			}
		}



	}

	internal class NewVersionModel {
		public bool UseNewVersion { get; set; } = true;
		public bool FirstTime { get; set; } = true;

		public NewVersionModel() {

		}

		[JsonConstructor]
		public NewVersionModel(bool useNewVersion, bool firstTime) {
			UseNewVersion = useNewVersion;
			FirstTime = firstTime;
		}
	}
}
