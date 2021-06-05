using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.UI.Popups;
using Windows.UI.Xaml.Controls;

namespace E621Downloader.Models.Networks {
	public static class Data {
		public const string USERAGENT = "RainbowWolferE621TestApp";
		/*
		public static E621Article[] GetPostsByTags(int page, params string[] tags) {
			if(page <= 0) {
				throw new Exception("Page not valid");
			}
			string url = string.Format("https://e621.net/posts?page={0}&tags=", page);
			tags.ToList().ForEach((t) => url += t + "+");

			var articles = new List<E621Article>();

			string result = ReadURL(url);
			Debug.WriteLine(result);
			if(result == null) {
				return articles.ToArray();
			}
			try {
				int start = result.IndexOf("<article") + 8;
				for(int i = start; i < result.Length; i++) {
					string endTest = result.Substring(i, 10);
					if(endTest == "</article>") {
						string allInfo = result.Substring(start, i - start);
						articles.Add(new E621Article(allInfo));
						if(result.IndexOf("<article", i) != -1) {
							start = i + 10;
							i = start;
						} else {
							break;
						}
					}
				}
			} catch(Exception e) {
				return articles.ToArray();
			}
			return articles.ToArray();
		}
		*/
		public static string ReadURL(string url) {
			var request = (HttpWebRequest)WebRequest.Create(url);
			request.UserAgent = USERAGENT;
			request.Headers.Add("login", "rainbowwolfer");
			request.Headers.Add("api_key", "WUwPNbGDrfXnQoHfvU1nR3TD");
			HttpWebResponse response;
			try {
				response = (HttpWebResponse)request.GetResponse();
			} catch(Exception e) {
				Debug.WriteLine(e.Message);
				return null;
			}
			Stream dataStream = response.GetResponseStream();
			StreamReader reader = new StreamReader(dataStream);
			string data = reader.ReadToEnd();
			return data;
		}
	}
}
