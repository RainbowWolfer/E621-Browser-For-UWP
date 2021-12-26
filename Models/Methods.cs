using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E621Downloader.Models {
	public static class Methods {
		public static void PrintIEnumerable(IEnumerable<object> i) {
			string line = "";
			foreach(object item in i) {
				line += item.ToString() + " ";
			}
			Debug.WriteLine(line);
		}
	}
}
