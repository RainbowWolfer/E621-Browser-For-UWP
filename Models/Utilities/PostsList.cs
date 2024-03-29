﻿using E621Downloader.Models.E621;
using E621Downloader.Models.Locals;
using E621Downloader.Pages;
using E621Downloader.Views.LibrarySection;
using E621Downloader.Views.PictureSection;
using System;
using System.Collections.Generic;
using System.Linq;

namespace E621Downloader.Models.Utilities {
	public class PostsList {
		private readonly List<object> items;//post, file path
		public object Current { get; set; }
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

		public void UpdatePostsList(List<E621Post> posts) {
			items.Clear();
			items.AddRange(posts);
		}

		public void UpdatePostsList(List<LibraryImage> block) {
			items.Clear();
			items.AddRange(block);
		}

		public void UpdatePostsList(List<MixPost> ids) {
			items.Clear();
			items.AddRange(ids);
		}

		public void UpdatePostsList(List<object> objs) {
			items.Clear();
			items.AddRange(objs);
		}

		public IEnumerable<object> GetItems() {
			return items;
		}

		public PostsList() {
			items = new List<object>();
		}

		public List<object> GetPhotosList() {
			List<object> result = new();
			foreach(var item in items) {
				if(PhotosListItem.CheckTypeValid(item)) {
					result.Add(item);
				}
			}
			return result;
		}
	}

}
