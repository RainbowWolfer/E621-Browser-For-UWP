﻿using E621Downloader.Models.Debugging;
using E621Downloader.Models.Locals;
using E621Downloader.Models.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage.Streams;

namespace E621Downloader.Models.Networks {
	public static class Data {
		public const string USERAGENT = "E621BrowserForUWP_RainbowWolfer";

		public static string GetHost() {
			if((LocalSettings.Current?.customHostEnable ?? false) && !string.IsNullOrWhiteSpace(LocalSettings.Current.customHost)) {
				return LocalSettings.Current.customHost;
			} else {
				return "e926.net";
			}
		}

		public static string GetSimpleHost() {
			string hostName = GetHost().ToUpper();
			int index = hostName.IndexOf(".");
			if(index != -1) {
				hostName = hostName.Substring(0, index);
			}
			return hostName;
		}

		public static async Task<HttpResult<string>> PatchAsync(string url, string
		 stringContent, CancellationToken token = default, string username = "", string api = "") {
			Debug.WriteLine("Reading: " + url);
			DateTime startDateTime = DateTime.Now;
			Stopwatch stopwatch = new();
			stopwatch.Start();
			var client = new HttpClient();
			AddDefaultRequestHeaders(client, username, api);
			HttpResponseMessage message = null;
			HttpResultType result;
			HttpStatusCode code;
			string content = null;
			string helper = "";
			try {
				HttpRequestMessage request = new(new HttpMethod("PATCH"), url) {
					Content = new StringContent(/*"user[avatar_id]:{imageID}"*/stringContent),
				};
				message = await client.SendAsync(request, token);
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
			HttpResult<string> hr = new(result, code, content, stopwatch.ElapsedMilliseconds, helper);
			HttpRequestHistories.AddNewItem(startDateTime, url, hr, "Patch");
			return hr;
		}

		public static async Task<HttpResult<string>> ReadURLAsync(string url, CancellationToken? token = null, string username = "", string api = "") {
			Debug.WriteLine("Reading: " + url);
			DateTime startDateTime = DateTime.Now;
			Stopwatch stopwatch = new();
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
			HttpResult<string> hr = new(result, code, content, stopwatch.ElapsedMilliseconds, helper);
			HttpRequestHistories.AddNewItem(startDateTime, url, hr, "Get");
			return hr;
		}

		public static async Task<HttpResult<InMemoryRandomAccessStream>> ReadImageStreamAsync(string url, CancellationToken? token = null, string username = "", string api = "") {
			DateTime startDateTime = DateTime.Now;
			Stopwatch stopwatch = new();
			stopwatch.Start();
			HttpClient client = new();
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
				DataWriter writer = new(stream.GetOutputStreamAt(0));
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

			HttpResult<InMemoryRandomAccessStream> hr = new(result, code, stream, stopwatch.ElapsedMilliseconds, helper);
			HttpRequestHistories.AddNewItem(startDateTime, url, hr, "Get");
			return hr;
		}

		public static async Task<HttpResult<string>> PostRequestAsync(string url, List<KeyValuePair<string, string>> pairs, CancellationToken? token = null, string username = "", string api = "") {
			DateTime startDateTime = DateTime.Now;
			Stopwatch stopwatch = new();
			stopwatch.Start();
			HttpClient client = new();
			AddDefaultRequestHeaders(client, username, api);
			HttpResponseMessage message = null;
			HttpResultType result;
			HttpStatusCode code;
			string helper = "";
			string content = "";
			try {
				var data = new FormUrlEncodedContent(pairs);
				if(token != null) {
					message = await client.PostAsync(url, data, token.Value);
				} else {
					message = await client.PostAsync(url, data);
				}
				message.EnsureSuccessStatusCode();
				content = await message.Content.ReadAsStringAsync();
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

			HttpResult<string> hr = new(result, code, content, stopwatch.ElapsedMilliseconds, helper);
			HttpRequestHistories.AddNewItem(startDateTime, url, hr, "Post");
			return hr;
		}

		public static async Task<HttpResult<string>> DeleteRequestAsync(string url, CancellationToken? token = null, string username = "", string api = "") {
			DateTime startDateTime = DateTime.Now;
			Stopwatch stopwatch = new();
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

			HttpResult<string> hr = new(result, code, content, stopwatch.ElapsedMilliseconds, helper);
			HttpRequestHistories.AddNewItem(startDateTime, url, hr, "Delete");
			return hr;
		}

		public static async Task<HttpResult<string>> PutRequestAsync(string url, KeyValuePair<string, string> pair, CancellationToken? token = null, string username = "", string api = "") {
			DateTime startDateTime = DateTime.Now;
			Stopwatch stopwatch = new();
			stopwatch.Start();
			var client = new HttpClient();
			AddDefaultRequestHeaders(client, username, api);
			HttpResponseMessage message = null;
			HttpResultType result;
			HttpStatusCode code;
			string content = null;
			string helper = "";
			try {
				var pairs = new List<KeyValuePair<string, string>>() { pair };
				var data = new FormUrlEncodedContent(pairs);
				if(token != null) {
					message = await client.PutAsync(url, data, token.Value);
				} else {
					message = await client.PutAsync(url, data);
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
			HttpResult<string> hr = new(result, code, content, stopwatch.ElapsedMilliseconds, helper);
			HttpRequestHistories.AddNewItem(startDateTime, url, hr, "Put");
			return hr;
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
