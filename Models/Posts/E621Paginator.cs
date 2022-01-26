using E621Downloader.Models.Networks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace E621Downloader.Models.Posts {
	public class E621Paginator {
		public async static Task<E621Paginator> GetAsync(string[] tags, int page = 1, CancellationToken? token = null) {
			string tag = "";
			foreach(var item in tags) {
				tag += item + " ";
			}
			string url = $"https://{Data.GetHost()}/posts?tags={tag}&page={page}";
			HttpResult<string> result = await Data.ReadURLAsync(url, token);
			if(result.Result != HttpResultType.Success) {
				return null;
			}
			string data = result.Content;
			int startIndex = data.IndexOf("paginator");

			int currentPageIndex = data.IndexOf("current-page", startIndex);

			string currentPageString = "";
			for(int j = currentPageIndex + 1; j < data.IndexOf("</li>", currentPageIndex); j++) {
				if(char.IsDigit(data[j])) {
					currentPageString += data[j];
					for(int k = 1; k <= 4; k++) {
						if(char.IsDigit(data[j + k])) {
							currentPageString += data[j + k];
						} else {
							break;
						}
					}
					break;
				}
			}

			var pagesString = new List<string>();
			int i = data.IndexOf("numbered-page", startIndex);
			while(i != -1) {
				string pageString = "";
				for(int j = i + 1; j < data.IndexOf("</li>", i); j++) {
					if(char.IsDigit(data[j])) {
						pageString += data[j];
						for(int k = 1; k <= 4; k++) {
							if(char.IsDigit(data[j + k])) {
								pageString += data[j + k];
							} else {
								break;
							}
						}
						pagesString.Add(pageString);
						break;
					}
				}
				i = data.IndexOf("numbered-page", i + 10);
			}

			int currentPage = int.Parse(currentPageString);
			var pages = new List<int>() { currentPage };
			foreach(string s in pagesString) {
				pages.Add(int.Parse(s));
			}
			return new E621Paginator() {
				currentPage = currentPage,
				pages = pages.ToArray(),
			};
		}

		public int currentPage;
		public int[] pages;
		public int GetMaxPage() {
			int max = -1;
			foreach(int item in pages) {
				if(item > max) {
					max = item;
				}
			}
			return max;
		}
	}
}
