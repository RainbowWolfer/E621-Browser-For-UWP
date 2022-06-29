using E621Downloader.Models.Networks;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace E621Downloader.Models.Posts {
	public class E621AutoComplete {
		public static async Task<(E621AutoComplete[], HttpResultType)> GetAsync(string tag, CancellationToken? token = null) {
			HttpResult<string> result = await Data.ReadURLAsync($"https://{Data.GetHost()}/tags/autocomplete.json?search[name_matches]={tag}", token);
			Debug.WriteLine(result.Time);
			if(result.Result == HttpResultType.Success) {
				return (JsonConvert.DeserializeObject<E621AutoComplete[]>(result.Content), HttpResultType.Success);
			} else {
				return (new E621AutoComplete[0], result.Result);
			}
		}
		public string id;
		public string name;
		public int post_count;
		public int category;
		public string antecedent_name;
	}
}
