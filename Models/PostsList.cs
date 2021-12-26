using E621Downloader.Models.Locals;
using E621Downloader.Models.Posts;
using E621Downloader.Pages.LibrarySection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E621Downloader.Models {
	public class PostsList {
		private readonly List<object> items;//post,filepath
		public object Current { private get; set; }
		public int Count => items.Count;

		public int GetCurrentIndex() => items.IndexOf(Current);

		public object GoLeft() {
			//Debug.WriteLine(Count);
			int index = GetCurrentIndex();
			if(index == -1) {
				return Current;
			}
			if(index - 1 < 0 && LocalSettings.Current.cycleList) {
				Current = items.LastOrDefault();
			} else {
				Current = items[Math.Clamp(index - 1, 0, Count - 1)];
			}
			return Current;
		}
		public object GoRight() {
			//Debug.WriteLine(Count);
			int index = GetCurrentIndex();
			if(index == -1) {
				return Current;
			}
			if(index + 1 > Count - 1 && LocalSettings.Current.cycleList) {
				Current = items.FirstOrDefault();
			} else {
				Current = items[Math.Clamp(index + 1, 0, Count - 1)];
			}
			return Current;
		}

		public void UpdatePostsList(List<Post> posts) {
			items.Clear();
			items.AddRange(posts);
		}

		public void UpdatePostsList(List<ItemBlock> block) {
			items.Clear();
			items.AddRange(block);
		}

		public PostsList() {
			items = new List<object>();
		}
	}

}
