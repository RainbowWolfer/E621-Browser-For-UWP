using E621Downloader.Models.Networks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E621Downloader.Models.Posts {
	public class E621Paginator {
		public static E621Paginator Get(string tag, int page = 1) {
			string url = $"https://e621.net/posts?tags={tag}&page={page}";
			string data = Data.ReadURL(url);
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
	}
}
