using E621Downloader.Models.Locals;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Windows.UI.Popups;
using Windows.UI.Xaml.Controls;

namespace E621Downloader.Models.Networks {
	public static class Data {
		public const string USERAGENT = "RainbowWolferE621TestApp";

		public static async Task<string> ReadURLAsync(string url, CancellationToken token = default, string username = "", string api = "") {
			Debug.WriteLine("Async : " + url);
			var request = (HttpWebRequest)WebRequest.Create(url);
			request.UserAgent = USERAGENT;
			if(string.IsNullOrWhiteSpace(username) && string.IsNullOrWhiteSpace(api)) {
				if(LocalSettings.Current?.CheckLocalUser() ?? false) {
					string encoded = Convert.ToBase64String(Encoding.GetEncoding("ISO-8859-1").GetBytes(LocalSettings.Current.user_username + ":" + LocalSettings.Current.user_api));
					request.Headers.Add("Authorization", "Basic " + encoded);
				}
			} else {
				string encoded = Convert.ToBase64String(Encoding.GetEncoding("ISO-8859-1").GetBytes(username + ":" + api));
				request.Headers.Add("Authorization", "Basic " + encoded);
			}
			HttpWebResponse response;
			try {
				response = await request.GetResponseAsync() as HttpWebResponse;
			} catch(WebException e) {
				Debug.WriteLine(e.Message);
				if(e.Response != null) {
					using(var stream = e.Response.GetResponseStream()) {
						using(var reader = new StreamReader(stream)) {
							Debug.WriteLine(reader.ReadToEnd());
						}
					}
				}
				return null;
			}
			using(Stream dataStream = response.GetResponseStream()) {
				using(StreamReader reader = new StreamReader(dataStream)) {
					string data = await reader.ReadToEndAsync();
					return data;
				}
			}
		}
	}
}
