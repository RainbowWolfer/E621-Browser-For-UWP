using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.System;
using Windows.UI.Xaml;
using YiffBrowser.Models.E621;

namespace YiffBrowser.Helpers {
	public static class CommonHelpers {
		public static int Count(this IEnumerable ie) {
			if (ie == null) {
				return 0;
			}

			if (ie is IList list) {
				return list.Count;
			}

			int count = 0;
			IEnumerator enumerator = ie.GetEnumerator();
			enumerator.Reset();
			while (enumerator.MoveNext()) {
				count = checked(count + 1);
			}

			return count;
		}

		public static bool IsEmpty(this IEnumerable ie) {
			if (ie == null) {
				return true;
			}

			if (ie is IList list) {
				return list.Count == 0;
			}

			IEnumerator enumerator = ie.GetEnumerator();
			enumerator.Reset();
			return !enumerator.MoveNext();
		}

		public static bool IsNotEmpty(this IEnumerable ie) => !ie.IsEmpty();


		public static bool IsEmpty<T>(this IEnumerable<T> ie) => ie == null || !ie.Any();
		public static bool IsNotEmpty<T>(this IEnumerable<T> ie) => !ie.IsEmpty();

		public static bool IsBlank(this string str) => string.IsNullOrWhiteSpace(str);
		public static bool IsNotBlank(this string str) => !str.IsBlank();

		public static string NotBlankCheck(this string text) => text.IsBlank() ? null : text;

		public static Visibility ToVisibility(this bool b, bool reverse = false) {
			if (reverse) {
				return b ? Visibility.Collapsed : Visibility.Visible;
			} else {
				return b ? Visibility.Visible : Visibility.Collapsed;
			}
		}

		public static string NumberToK(this int number) {
			if (number > 1000) {
				int a = number / 1000;
				int length = $"{number}".Length;
				int pow = (int)Math.Pow(10, length - 1);
				int head = int.Parse($"{number}".First().ToString());
				int b = (number - pow * head) / (pow / 10);
				if (b == 0) {
					return $"{a}K";
				} else {
					return $"{a}.{b}K";
				}
			} else {
				return $"{number}";
			}
		}

		public static bool OnlyContainDigits(this string text) {
			foreach (char item in text) {
				if (!char.IsDigit(item)) {
					return false;
				}
			}
			return true;
		}

		public static string ToFullString(this IEnumerable<string> tags) {
			return string.Join(" ", tags);
		}

		public static void CopyToClipboard(this string text) {
			if (text.IsBlank()) {
				return;
			}
			DataPackage package = new() {
				RequestedOperation = DataPackageOperation.Copy,
			};
			package.SetText(text);
			Clipboard.SetContent(package);
		}

		public static bool IsNumber(this object value) {
			return value is byte || value is sbyte ||
				value is short || value is ushort ||
				value is int || value is uint ||
				value is long || value is ulong ||
				value is float || value is double ||
				value is decimal;
		}

		public static double Abs(object value) {
			if (IsNumber(value)) {
				return Math.Abs(Convert.ToDouble(value));
			} else {
				throw new ArgumentException("Value must be a number");
			}
		}


		public static string FileSizeToKB(this int size, bool gap = false) {
			string kb = $"{size / 1000}";
			string output = Regex.Replace(kb, ".{3}(?!.)", ",$&").Trim(',');
			if (gap) {
				return $"{output} KB";
			} else {
				return $"{output}KB";
			}
		}

		public static FileType GetFileType(this E621Post post) {
			return post.File.Ext.ToLower().Trim() switch {
				"jpg" => FileType.Jpg,
				"png" => FileType.Png,
				"gif" => FileType.Gif,
				"anim" or "swf" => FileType.Anim,
				"webm" => FileType.Webm,
				_ => throw new System.Exception($"New Type({post.File.Ext}) Found"),
			};
		}


		public static async void OpenInBrowser(this string url) {
			if (url.IsBlank()) {
				return;
			}
			await Launcher.LaunchUriAsync(new Uri(url));
		}

		public static double Distance(this Point a, Point b) {
			var x = Math.Pow(a.X - b.X, 2);
			var y = Math.Pow(a.Y - b.Y, 2);
			return Math.Sqrt(x + y);
		}
	}
}
