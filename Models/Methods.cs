using E621Downloader.Models.Inerfaces;
using E621Downloader.Models.Posts;
using E621Downloader.Pages;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.Storage;
using Windows.System;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;

namespace E621Downloader.Models {
	public static class Methods {
		public static void PrintIEnumerable(IEnumerable<object> i) {
			string line = "";
			foreach(object item in i) {
				line += item.ToString() + " ";
			}
			Debug.WriteLine(line);
		}

		public static string ConvertSizeToKB(int size) {
			return string.Format("{0:#,###,###}", size / 1000) + "KB";
		}

		public static string ToCamelCase(this string s) {
			List<char> list = s.ToList();
			for(int i = 0; i < list.Count; i++) {
				char c = list[i];
				if(i == 0) {
					list[i] = char.ToUpper(list[i]);
				}
				if(c == ' ' && i < list.Count - 1) {
					list[i + 1] = char.ToUpper(list[i + 1]);
				}
			}
			return new string(list.ToArray());
		}

		public static List<GroupTag> ToGroupTag(this List<string> tags, Color color) {
			List<GroupTag> result = new();
			foreach(string tag in tags) {
				result.Add(new GroupTag(tag, color));
			}
			return result;
		}

		public static string NumberToK(int number) {
			if(number > 1000) {
				int a = number / 1000;
				int length = $"{number}".Length;
				int pow = (int)Math.Pow(10, length - 1);
				int head = int.Parse($"{number}".First().ToString());
				int b = (number - pow * head) / (pow / 10);
				if(b == 0) {
					return $"{a}K";
				} else {
					return $"{a}.{b}K";
				}
			} else {
				return $"{number}";
			}
		}

		public static string ToDuo(this int number) {
			if(number <= 0) {
				return number.ToString();
			} else if(number < 10) {
				return $"0{number}";
			} else {
				return number.ToString();
			}
		}

		public static void FocusModeUpdate(this IPage page) {
			page.FocusMode(MainPage.Instance.ScreenMode == ScreenMode.Focus);
		}

		public static string GetDate(DateTime dateTime) {
			return $"{dateTime.Year}:{dateTime.Month.ToDuo()}:{dateTime.Day.ToDuo()}";
		}
		public static string GetDate() => GetDate(DateTime.Now);

		public static string GetTime(DateTime dateTime) {
			return $"{dateTime.Hour.ToDuo()}:{dateTime.Minute.ToDuo()}:{dateTime.Second.ToDuo()}";
		}
		public static string GetTime() => GetTime(DateTime.Now);

		public static string GetDateTime(DateTime dateTime) {
			return $"{GetDate(dateTime)} {GetTime(dateTime)}";
		}
		public static string GetDateTime() => GetDateTime(DateTime.Now);

		public static void Print<T, W>(this Dictionary<T, W> dic, Func<T, object> key = null, Func<W, object> value = null) {
			Debug.WriteLine($"Print Dictionary: {dic}");
			if(key == null) {
				key = i => i;
			}
			if(value == null) {
				value = i => i;
			}
			int index = 0;
			foreach(KeyValuePair<T, W> item in dic) {
				Debug.WriteLine($"{index++} -> {key.Invoke(item.Key)} : {value.Invoke(item.Value)}");
			}
		}


		public static string Language(this string str, params object[] args) {
			if(string.IsNullOrWhiteSpace(str)) {
				return str;
			}
			if(CoreWindow.GetForCurrentThread() != null) {
				try {
					var result = ResourceLoader.GetForCurrentView().GetString(str);
					try {
						for(int i = 0; i < args.Length; i++) {
							result = result.Replace("{{" + i + "}}", args[i].ToString());
						}
					} catch { }
					return result;
				} catch(Exception ex) {
					Debug.WriteLine(ex.Message);
					return "String Not Found";
				}
			} else {
				return "Error";
			}
		}

		public static async Task OpenBrowser(string uri) {
			if(!await Launcher.LaunchUriAsync(new Uri(uri))) {
				await MainPage.CreatePopupDialog("Error".Language(), "Could not Open Default Browser".Language());
			}
		}

		public static void ProdedureLoading(Image previewImage, Image sampleImage, Post post, LoadPoolItemActions actions = null) {
			string previewURL = post.preview.url;
			string sampleURL = post.sample.url;
			LoadPoolItem loader = LoadPool.SetNew(post);
			if(string.IsNullOrWhiteSpace(previewURL) && string.IsNullOrWhiteSpace(sampleURL)) {
				actions?.OnUrlsEmpty?.Invoke();
			} else {
				bool previewLoaded = false;
				if(!string.IsNullOrWhiteSpace(previewURL)) {
					LoadPreview();
				} else {
					LoadSample();
				}

				void LoadPreview() {
					if(string.IsNullOrWhiteSpace(previewURL)) {
						return;
					}
					actions?.OnPreviewStart?.Invoke();
					previewImage.ImageFailed += (s, e) => {
						actions?.OnPreviewFailed?.Invoke();
						LoadSample();
					};
					previewImage.ImageOpened += (s, e) => {
						var bitmap = previewImage.Source as BitmapImage;
						previewLoaded = true;
						actions?.OnPreviewOpened?.Invoke(bitmap);
						loader.Preview = bitmap;
						LoadSample();
					};
					if(loader.Preview != null) {
						previewImage.Source = loader.Preview;
						previewLoaded = true;
						actions?.OnPreviewExists?.Invoke();
						LoadSample();
					} else {
						previewImage.Source = new BitmapImage(new Uri(previewURL));
					}
					(previewImage.Source as BitmapImage).DownloadProgress += (s, e) => {
						actions?.OnPreviewProgress?.Invoke(e.Progress);
					};
				}
				void LoadSample() {
					if(string.IsNullOrWhiteSpace(sampleURL)) {
						actions?.OnSampleUrlEmpty?.Invoke();
						return;
					}
					actions?.OnSampleStart?.Invoke(previewLoaded);
					if(loader.Sample != null) {
						sampleImage.Source = loader.Sample;
						sampleImage.Visibility = Visibility.Visible;
						previewImage.Visibility = Visibility.Collapsed;
						actions?.OnSampleExists?.Invoke();
					} else {
						sampleImage.Source = new BitmapImage(new Uri(sampleURL));
					}
					sampleImage.ImageFailed += (s, e) => {
						actions?.OnSampleFailed?.Invoke();
					};
					sampleImage.ImageOpened += (s, e) => {
						var bitmap = sampleImage.Source as BitmapImage;
						actions?.OnSampleOpened?.Invoke(bitmap);
						loader.Sample = bitmap;
						sampleImage.Visibility = Visibility.Visible;
						previewImage.Visibility = Visibility.Collapsed;
					};
					(sampleImage.Source as BitmapImage).DownloadProgress += (s, e) => {
						actions?.OnSampleProgress?.Invoke(previewLoaded ? null : e.Progress);
					};
				}
			}
		}

		public static async Task LaunchFile(StorageFile file) {
			try {
				await Launcher.LaunchFileAsync(file);
			} catch(Exception ex) {
				await new ContentDialog() {
					Title = "Error".Language(),
					Content = $"{"An error occurred while opening this file".Language()}\n {"File".Language()}:{file.Path}\n{ex.Message}",
					CloseButtonText = "Back".Language(),
					DefaultButton = ContentDialogButton.Close,
				}.ShowAsync();
			}
		}

		public static bool IsInRange(this double number, double min, double max) {
			return min <= number && number <= max;
		}

		public static bool CompareItemsEqual(this IEnumerable<object> a, IEnumerable<object> b) {
			if(a.Count() != b.Count()) {
				return false;
			}
			var la = a.ToArray();
			var lb = b.ToArray();
			for(int i = 0; i < la.Length; i++) {
				if(la[i] != lb[i]) {
					return false;
				}
			}
			return true;
		}

		public static T GetRandomItem<T>(this IEnumerable<T> values) {
			return values.ElementAtOrDefault(new Random().Next(0, values.Count()));
		}
	}
}
