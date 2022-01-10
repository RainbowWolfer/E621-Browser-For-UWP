using E621Downloader.Models.Posts;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

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
	}
}
