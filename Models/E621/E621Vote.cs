using E621Downloader.Models.Networks;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace E621Downloader.Models.E621 {
	public class E621Vote {
		public static async Task<DataResult<E621Vote>> VotePost(string postID, int score, bool no_unvote, CancellationToken? token = null) {
			HttpResult<string> result = await Data.PostRequestAsync($"https://{Data.GetHost()}/posts/{postID}/votes.json", new List<KeyValuePair<string, string>> {
				new KeyValuePair<string, string>("score", $"{score}"),
				new KeyValuePair<string, string>("no_unvote", $"{no_unvote}"),
			}, token);
			return new DataResult<E621Vote>(result.Result, JsonConvert.DeserializeObject<E621Vote>(result.Content));
		}


		// no up and down
		public static async Task<DataResult<E621Vote>> VoteComment(string commentID, int score, bool no_unvote, CancellationToken? token = null) {
			HttpResult<string> result = await Data.PostRequestAsync($"https://{Data.GetHost()}/comments/{commentID}/votes.json", new List<KeyValuePair<string, string>> {
				new KeyValuePair<string, string>("score", $"{score}"),
				new KeyValuePair<string, string>("no_unvote", $"{no_unvote}"),
			}, token);
			return new DataResult<E621Vote>(result.Result, JsonConvert.DeserializeObject<E621Vote>(result.Content));
		}

		public int score;
		public int up;
		public int down;
		public int our_score;
	}
}
