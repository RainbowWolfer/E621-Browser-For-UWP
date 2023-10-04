using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using YiffBrowser.Helpers;
using YiffBrowser.Services.Locals;

namespace YiffBrowser.Services.Networks {
	public static class NetCode {
		public const string USERAGENT = "E621BrowserUWP/2.0.0.0-PreAlpha by (RainbowWolfer)";

		public static async Task<HttpResult<string>> ReadURLAsync(string url, CancellationToken? token = null, string username = "", string api = "") {
			Debug.WriteLine("Reading: " + url);

			DateTime startDateTime = DateTime.Now;
			Stopwatch stopwatch = Stopwatch.StartNew();

			using HttpClient client = new();
			AddDefaultRequestHeaders(client, username, api);

			HttpResponseMessage message = null;
			HttpResultType result;
			HttpStatusCode code;

			string content = null;
			string helper = "";

			try {
				if (token != null) {
					message = await client.GetAsync(url, token.Value);
				} else {
					message = await client.GetAsync(url);
				}
				message.EnsureSuccessStatusCode();
				code = message.StatusCode;
				content = await message.Content.ReadAsStringAsync();

				result = HttpResultType.Success;
			} catch (OperationCanceledException) {
				code = message?.StatusCode ?? HttpStatusCode.NotFound;
				content = null;

				result = HttpResultType.Canceled;
			} catch (HttpRequestException e) {
				code = message?.StatusCode ?? HttpStatusCode.NotFound;
				content = e.Message;
				helper = e.Message;

				result = HttpResultType.Error;
			} finally {
				message?.Dispose();
			}

			stopwatch.Stop();

			HttpResult<string> hr = new(result, code, content, stopwatch.ElapsedMilliseconds, startDateTime, helper);
			//HttpRequestHistories.AddNewItem(startDateTime, url, hr, "Get");

			return hr;
		}

		public static async Task<HttpResult<string>> PutRequestAsync(string url, KeyValuePair<string, string> pair, CancellationToken? token = null, string username = "", string api = "") {
			DateTime startDateTime = DateTime.Now;
			Stopwatch stopwatch = Stopwatch.StartNew();

			using HttpClient client = new();
			AddDefaultRequestHeaders(client, username, api);

			HttpResponseMessage message = null;
			HttpResultType result;
			HttpStatusCode code;

			string content = null;
			string helper = "";

			try {
				List<KeyValuePair<string, string>> pairs = new() { pair };
				FormUrlEncodedContent data = new(pairs);
				if (token != null) {
					message = await client.PutAsync(url, data, token.Value);
				} else {
					message = await client.PutAsync(url, data);
				}
				message.EnsureSuccessStatusCode();
				code = message.StatusCode;
				content = await message.Content.ReadAsStringAsync();
				result = HttpResultType.Success;
			} catch (OperationCanceledException) {
				code = message?.StatusCode ?? HttpStatusCode.NotFound;
				content = null;
				result = HttpResultType.Canceled;
			} catch (HttpRequestException e) {
				code = message?.StatusCode ?? HttpStatusCode.NotFound;
				content = e.Message;
				helper = e.Message;
				result = HttpResultType.Error;
			} finally {
				message?.Dispose();
			}
			stopwatch.Stop();
			HttpResult<string> hr = new(result, code, content, stopwatch.ElapsedMilliseconds, startDateTime, helper);
			//HttpRequestHistories.AddNewItem(startDateTime, url, hr, "Put");
			return hr;
		}

		public static async Task<HttpResult<string>> PostRequestAsync(string url, List<KeyValuePair<string, string>> pairs, CancellationToken? token = null, string username = "", string api = "") {
			DateTime startDateTime = DateTime.Now;
			Stopwatch stopwatch = Stopwatch.StartNew();

			using HttpClient client = new();
			AddDefaultRequestHeaders(client, username, api);

			HttpResponseMessage message = null;
			HttpResultType result;
			HttpStatusCode code;

			string helper = "";
			string content = "";
			try {
				var data = new FormUrlEncodedContent(pairs);
				if (token != null) {
					message = await client.PostAsync(url, data, token.Value);
				} else {
					message = await client.PostAsync(url, data);
				}
				message.EnsureSuccessStatusCode();
				content = await message.Content.ReadAsStringAsync();
				result = HttpResultType.Success;
				code = message.StatusCode;
			} catch (OperationCanceledException) {
				result = HttpResultType.Canceled;
				code = message?.StatusCode ?? HttpStatusCode.BadRequest;
			} catch (HttpRequestException e) {
				result = HttpResultType.Error;
				code = message?.StatusCode ?? HttpStatusCode.BadRequest;
				helper = e.Message;
			} finally {
				client.Dispose();
				message?.Dispose();
			}
			stopwatch.Stop();

			HttpResult<string> hr = new(result, code, content, stopwatch.ElapsedMilliseconds, startDateTime, helper);
			//HttpRequestHistories.AddNewItem(startDateTime, url, hr, "Post");
			return hr;
		}

		public static async Task<HttpResult<string>> DeleteRequestAsync(string url, CancellationToken? token = null, string username = "", string api = "") {
			DateTime startDateTime = DateTime.Now;
			Stopwatch stopwatch = Stopwatch.StartNew();

			using HttpClient client = new();
			AddDefaultRequestHeaders(client, username, api);

			HttpResponseMessage message = null;
			HttpResultType result;
			HttpStatusCode code;

			string content = null;
			string helper = "";

			try {
				if (token != null) {
					message = await client.DeleteAsync(url, token.Value);
				} else {
					message = await client.DeleteAsync(url);
				}
				message.EnsureSuccessStatusCode();
				code = message.StatusCode;
				content = await message.Content.ReadAsStringAsync();
				result = HttpResultType.Success;
			} catch (OperationCanceledException) {
				code = message?.StatusCode ?? HttpStatusCode.NotFound;
				content = null;
				result = HttpResultType.Canceled;
			} catch (HttpRequestException e) {
				code = message?.StatusCode ?? HttpStatusCode.NotFound;
				content = e.Message;
				helper = e.Message;
				result = HttpResultType.Error;
			} finally {
				message?.Dispose();
			}
			stopwatch.Stop();

			HttpResult<string> hr = new(result, code, content, stopwatch.ElapsedMilliseconds, startDateTime, helper);
			//HttpRequestHistories.AddNewItem(startDateTime, url, hr, "Delete");
			return hr;
		}

		private static void AddDefaultRequestHeaders(HttpClient client, string username, string api) {
			client.DefaultRequestHeaders.Add("User-Agent", USERAGENT);
			AddAuthorizationHeader(client, username, api);
			//AddAuthorizationHeader(client, "RainbowWolfer", "WUwPNbGDrfXnQoHfvU1nR3TD");
		}

		private static void AddAuthorizationHeader(HttpClient client, string username, string api) {
			if (username.IsBlank() && api.IsBlank()) {
				if (Local.Settings.CheckLocalUser()) {
					username = Local.Settings.Username;
					api = Local.Settings.UserAPI;
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
		public DateTime StartTime { get; private set; }
		public HttpResult(HttpResultType result, HttpStatusCode statusCode, T content, long time, DateTime startTime, string helper = null) {
			Result = result;
			StatusCode = statusCode;
			Content = content;
			Time = time;
			Helper = helper;
			StartTime = startTime;
		}
	}

	public class DataResult<T> {
		public HttpResultType ResultType { get; set; }

		public T Data { get; set; }

		public DataResult(HttpResultType resultType, T data) {
			ResultType = resultType;
			Data = data;

		}
	}

	public class HttpResultTypeNotFoundException : Exception {
		public HttpResultTypeNotFoundException() : base("") { }
	}

	public enum HttpResultType {
		Success, Error, Canceled
	}
}
