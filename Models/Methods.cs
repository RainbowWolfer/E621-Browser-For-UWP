using E621Downloader.Models.Inerfaces;
using E621Downloader.Pages;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.System;
using Windows.UI;
using Windows.UI.Core;

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
	}
}
