﻿using E621Downloader.Models.Debugging;
using E621Downloader.Models.Networks;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace E621Downloader.Models.E621 {
	public class E621Paginator {
		public static async Task<DataResult<E621Paginator>> GetAsync(string[] tags, int page = 1, CancellationToken? token = null) {
			string tag = string.Join("+", tags).Trim().ToLower();
			//foreach(var item in tags) {
			//	tag += item + " ";
			//}
			string url = $"https://{Data.GetHost()}/posts?tags={tag}&page={page}";
			HttpResult<string> result = await Data.ReadURLAsync(url, token);
			if(result.Result != HttpResultType.Success) {
				return new DataResult<E621Paginator>(result.Result, null);
			}

			try {
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
				return new DataResult<E621Paginator>(HttpResultType.Success, new E621Paginator() {
					currentPage = currentPage,
					pages = pages.ToArray(),
				});
			} catch(Exception ex) {
				ErrorHistories.Add(ex);
				Debug.WriteLine(ex);
				//return 0 length paginator
				return new DataResult<E621Paginator>(HttpResultType.Success, new E621Paginator() {
					currentPage = 1,
					pages = new int[] { 0 }
				});
			}
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
