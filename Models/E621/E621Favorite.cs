using E621Downloader.Models.Networks;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace E621Downloader.Models.E621 {
	public static class E621Favorite {
		public static async Task<HttpResult<string>> PostAsync(string post_id, CancellationToken? token = null) {
			string url = $"https://{Data.GetHost()}/favorites.json";
			return await Data.PostRequestAsync(url, new List<KeyValuePair<string, string>>() { new KeyValuePair<string, string>("post_id", post_id) }, token);
		}

		public static async Task<HttpResult<string>> DeleteAsync(string post_id, CancellationToken? token = null) {
			string url = $"https://{Data.GetHost()}/favorites/{post_id}.json";
			return await Data.DeleteRequestAsync(url, token);
		}
	}
}
