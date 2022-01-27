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
using Windows.Storage.Streams;
using Windows.UI.Popups;
using Windows.UI.Xaml.Controls;

namespace E621Downloader.Models.Networks {
	public static class Data {
		public const string USERAGENT = "RainbowWolferE621TestApp";

		public static string GetHost() {
			if((LocalSettings.Current?.customHostEnable ?? false) && !string.IsNullOrWhiteSpace(LocalSettings.Current.customHost)) {
				return LocalSettings.Current.customHost;
			} else {
				return "e926.net";
			}
		}

		public static async Task<HttpResult<string>> ReadURLAsync(string url, CancellationToken? token = null, string username = "", string api = "") {
			Debug.WriteLine("Reading: " + url);
			Stopwatch stopwatch = new Stopwatch();
			stopwatch.Start();
			var client = new HttpClient();
			AddDefaultRequestHeaders(client, username, api);
			HttpResponseMessage message = null;
			HttpResultType result;
			HttpStatusCode code;
			string content = null;
			string helper = "";
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
				helper = e.Message;
				result = HttpResultType.Error;
			} finally {
				client.Dispose();
				message?.Dispose();
			}
			stopwatch.Stop();
			return new HttpResult<string>(result, code, content, stopwatch.ElapsedMilliseconds, helper);
		}

		public async static Task<HttpResult<InMemoryRandomAccessStream>> ReadImageStreamAsync(string url, CancellationToken? token = null, string username = "", string api = "") {
			Stopwatch stopwatch = new Stopwatch();
			stopwatch.Start();
			HttpClient client = new HttpClient();
			AddDefaultRequestHeaders(client, username, api);
			HttpResponseMessage message = null;
			InMemoryRandomAccessStream stream = null;
			HttpResultType result;
			HttpStatusCode code;
			string helper = "";
			try {
				if(token != null) {
					message = await client.GetAsync(url, token.Value);
				} else {
					message = await client.GetAsync(url);
				}
				message.EnsureSuccessStatusCode();
				code = message.StatusCode;
				stream = new InMemoryRandomAccessStream();
				DataWriter writer = new DataWriter(stream.GetOutputStreamAt(0));
				writer.WriteBytes(await message.Content.ReadAsByteArrayAsync());
				await writer.StoreAsync();
				result = HttpResultType.Success;
			} catch(OperationCanceledException) {
				code = message?.StatusCode ?? HttpStatusCode.NotFound;
				stream = null;
				result = HttpResultType.Canceled;
			} catch(HttpRequestException e) {
				code = message?.StatusCode ?? HttpStatusCode.NotFound;
				stream = null;
				result = HttpResultType.Error;
				helper = e.Message;
			} finally {
				client.Dispose();
				message?.Dispose();
			}
			stopwatch.Stop();
			return new HttpResult<InMemoryRandomAccessStream>(result, code, stream, stopwatch.ElapsedMilliseconds, helper);
		}

		public async static Task<HttpResult<string>> PostRequestAsync(string url, KeyValuePair<string, string> pair, CancellationToken? token = null, string username = "", string api = "") {
			Stopwatch stopwatch = new Stopwatch();
			stopwatch.Start();
			HttpClient client = new HttpClient();
			AddDefaultRequestHeaders(client, username, api);
			HttpResponseMessage message = null;
			HttpResultType result;
			HttpStatusCode code;
			string helper = "";
			try {
				var pairs = new List<KeyValuePair<string, string>>() { pair };
				var data = new FormUrlEncodedContent(pairs);
				if(token != null) {
					message = await client.PostAsync(url, data, token.Value);
				} else {
					message = await client.PostAsync(url, data);
				}
				message.EnsureSuccessStatusCode();
				string r = await message.Content.ReadAsStringAsync();
				result = HttpResultType.Success;
				code = message.StatusCode;
			} catch(OperationCanceledException) {
				result = HttpResultType.Canceled;
				code = message?.StatusCode ?? HttpStatusCode.BadRequest;
			} catch(HttpRequestException e) {
				result = HttpResultType.Error;
				code = message?.StatusCode ?? HttpStatusCode.BadRequest;
				helper = e.Message;
			} finally {
				client.Dispose();
				message?.Dispose();
			}

			stopwatch.Stop();
			return new HttpResult<string>(result, code, "", stopwatch.ElapsedMilliseconds, helper);
		}

		public async static Task<HttpResult<string>> DeleteRequestAsync(string url, CancellationToken? token = null, string username = "", string api = "") {
			Stopwatch stopwatch = new Stopwatch();
			stopwatch.Start();
			var client = new HttpClient();
			AddDefaultRequestHeaders(client, username, api);
			HttpResponseMessage message = null;
			HttpResultType result;
			HttpStatusCode code;
			string content = null;
			string helper = "";
			try {
				if(token != null) {
					message = await client.DeleteAsync(url, token.Value);
				} else {
					message = await client.DeleteAsync(url);
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
				helper = e.Message;
				result = HttpResultType.Error;
			} finally {
				client.Dispose();
				message?.Dispose();
			}
			stopwatch.Stop();
			return new HttpResult<string>(result, code, content, stopwatch.ElapsedMilliseconds, helper);
		}

		private static void AddDefaultRequestHeaders(HttpClient client, string username, string api) {
			client.DefaultRequestHeaders.Add("User-Agent", "RainbowWolferE621TestApp");
			AddAuthorizationHeader(client, username, api);
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

	public class HttpResult<T> {
		public HttpResultType Result { get; set; }
		public HttpStatusCode StatusCode { get; private set; }
		public T Content { get; private set; }
		public string Helper { get; private set; }
		public long Time { get; private set; }
		public HttpResult(HttpResultType result, HttpStatusCode statusCode, T content, long time, string helper = null) {
			Result = result;
			StatusCode = statusCode;
			Content = content;
			Time = time;
			Helper = helper;
		}
	}

	public class HttpResultTypeNotFoundException: Exception {
		public HttpResultTypeNotFoundException() : base("") { }
	}

	public enum HttpResultType {
		Success, Error, Canceled
	}
}
