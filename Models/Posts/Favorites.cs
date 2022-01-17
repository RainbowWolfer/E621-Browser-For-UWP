using E621Downloader.Models.Networks;
using E621Downloader.Models.Posts;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.UI.Xaml;

namespace E621Downloader.Models.Posts {
	public static class Favorites {
		public static async Task<HttpResult<string>> PostAsync(Post post, CancellationToken? token = null) {
			string url = "https://e621.net/favorites.json";
			return await Data.PostRequestAsync(url, new KeyValuePair<string, string>("post_id", post.id), token);
		}

		public static async Task<HttpResult<string>> DeleteAsync(string post_id, CancellationToken? token = null) {
			string url = $"https://e621.net/favorites/{post_id}.json";
			return await Data.DeleteRequestAsync(url, token);
		}
	}
}
