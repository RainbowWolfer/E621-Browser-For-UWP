using E621Downloader.Models.Locals;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Windows.UI.Popups;
using Windows.UI.Xaml.Controls;

namespace E621Downloader.Models.Networks {
	public static class Data {
		public const string USERAGENT = "RainbowWolferE621TestApp";
		[Obsolete("Use E621Downloader.Models.Networks.Data.ReadURLAsync() Instead", true)]
		public static async Task<string> ReadURLAsync_Old(string url, CancellationToken token = default, string username = "", string api = "") {
			Debug.WriteLine("Async : " + url);
			var request = (HttpWebRequest)WebRequest.Create(url);
			request.UserAgent = USERAGENT;
			if(string.IsNullOrWhiteSpace(username) && string.IsNullOrWhiteSpace(api)) {
				if(LocalSettings.Current?.CheckLocalUser() ?? false) {
					AddAuthorizationHeader(request,
						LocalSettings.Current.user_username,
						LocalSettings.Current.user_api
					);
				}
			} else {
				AddAuthorizationHeader(request, username, api);
			}
			HttpWebResponse response;
			try {
				response = await request.GetResponseAsync() as HttpWebResponse;
				request.Abort();
			} catch(WebException e) {
				Debug.WriteLine(e.Message);
				if(e.Response != null) {
					using(Stream stream = e.Response.GetResponseStream()) {
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

		[Obsolete]
		private static void AddAuthorizationHeader(HttpWebRequest request, string username, string api) {
			string encoded = Convert.ToBase64String(Encoding.GetEncoding("ISO-8859-1").GetBytes(username + ":" + api));
			request.Headers.Add("Authorization", "Basic " + encoded);
		}


		public static async Task<HttpResult> ReadURLAsync(string url, CancellationToken? token = null, string username = "", string api = "") {
			Stopwatch stopwatch = new Stopwatch();
			stopwatch.Start();
			var client = new HttpClient();
			client.DefaultRequestHeaders.Add("User-Agent", "RainbowWolferE621TestApp");
			AddAuthorizationHeader(client, username, api);
			HttpResponseMessage message = null;
			HttpResultType result;
			HttpStatusCode code;
			string content = null;
			try {
				if(token != null) {
					message = await client.GetAsync(url, token.Value);
				} else {
					message = await client.GetAsync(url);
				}
				message.EnsureSuccessStatusCode();
				code = message.StatusCode;
				content = await message.Content.ReadAsStringAsync();
				result = HttpResultType.Success;
			} catch(OperationCanceledException) {
				code = message?.StatusCode ?? HttpStatusCode.NotFound;
				content = null;
				result = HttpResultType.Canceled;
			} catch(HttpRequestException e) {
				code = message?.StatusCode ?? HttpStatusCode.NotFound;
				content = e.Message;
				result = HttpResultType.Error;
			} finally {
				client.Dispose();
				message.Dispose();
			}
			stopwatch.Stop();
			return new HttpResult(result, code, content, stopwatch.ElapsedMilliseconds);
		}

		private static void AddAuthorizationHeader(HttpClient client, string username, string api) {
			if(string.IsNullOrWhiteSpace(username) && string.IsNullOrWhiteSpace(api)) {
				if(LocalSettings.Current?.CheckLocalUser() ?? false) {
					username = LocalSettings.Current.user_username;
					api = LocalSettings.Current.user_api;
				} else {
					return;
				}
			}
			string encoded = Convert.ToBase64String(Encoding.GetEncoding("ISO-8859-1").GetBytes(username + ":" + api));
			client.DefaultRequestHeaders.Add("Authorization", "Basic " + encoded);
		}
	}

	public class HttpResult {
		public HttpResultType Result { get; set; }
		public HttpStatusCode StatusCode { get; private set; }
		public string Content { get; private set; }
		public long Time { get; private set; }
		public HttpResult(HttpResultType result, HttpStatusCode statusCode, string content, long time) {
			Result = result;
			StatusCode = statusCode;
			Content = content;
			Time = time;
		}
	}

	public class HttpResultTypeNotFoundException: Exception {
		public HttpResultTypeNotFoundException() : base("") {

		}
	}

	public enum HttpResultType {
		Success, Error, Canceled
	}
}
