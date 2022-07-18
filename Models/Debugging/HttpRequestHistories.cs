using E621Downloader.Models.Networks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage.Streams;

namespace E621Downloader.Models.Debugging {
	public static class HttpRequestHistories {
		public static List<RequestHistoryItem> Items { get; } = new();

		public static void AddNewItem<T>(DateTime dateTime, string url, HttpResult<T> result, string method, string hintText = null) {
			DataType type;
			ulong size;
			if(result.Content is string str) {
				type = DataType.Text;
				size = (ulong)str.Length;
			} else if(result.Content is InMemoryRandomAccessStream stream) {
				type = DataType.Image;
				size = stream.Size;
			} else {
				type = DataType.Undefined;
				size = 0;
			}
			Items.Insert(0, new RequestHistoryItem(
				dateTime,
				url: url,
				method: method,
				result: result.Result,
				statusCode: result.StatusCode,
				time: result.Time,
				contentType: type,
				contentSize: size,
				hintText: hintText
			));
		}

		public class RequestHistoryItem {
			public DateTime StartDateTime { get; set; }
			public string Url { get; set; }
			public string Method { get; set; }
			public HttpResultType Result { get; set; }
			public HttpStatusCode StatusCode { get; set; }
			public long Time { get; private set; }
			public DataType ContentType { get; set; }
			public ulong ContentSize { get; set; }

			public string HintText { get; set; }

			public RequestHistoryItem(DateTime dateTime, string url, string method, HttpResultType result, HttpStatusCode statusCode, long time, DataType contentType, ulong contentSize, string hintText = null) {
				StartDateTime = dateTime;
				Url = url;
				Method = method;
				Result = result;
				StatusCode = statusCode;
				Time = time;
				ContentType = contentType;
				ContentSize = contentSize;
				HintText = hintText;
			}
		}

		public enum DataType {
			Text, Image, Undefined
		}
	}
}
