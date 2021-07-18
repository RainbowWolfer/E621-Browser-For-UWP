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
		public static async Task<string> ReadURLAsync(string url) {
			var request = (HttpWebRequest)WebRequest.Create(url);
			request.UserAgent = USERAGENT;
			request.Headers.Add("login", "rainbowwolfer");
			request.Headers.Add("api_key", "WUwPNbGDrfXnQoHfvU1nR3TD");
			HttpWebResponse response;
			try {
				response = await request.GetResponseAsync() as HttpWebResponse;
			} catch(Exception e) {
				Debug.WriteLine(e.Message);
				return null;
			}
			Stream dataStream = response.GetResponseStream();
			StreamReader reader = new StreamReader(dataStream);
			string data = await reader.ReadToEndAsync();
			return data;
		}
	}
}
