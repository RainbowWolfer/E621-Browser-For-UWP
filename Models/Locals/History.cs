using E621Downloader.Models.Posts;
using E621Downloader.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace E621Downloader.Models.Locals {
	public class History {
		private const int SAVE_DELAY = 500;
		public List<HistoryItem> Tags { get; set; } = new();
		public List<HistoryItem> PostIDs { get; set; } = new();

		public History() {

		}

		private CancellationTokenSource cts;

		public void AddTag(string[] tags) {
			if(tags == null || tags.Length == 0 || tags.Where(t => !string.IsNullOrWhiteSpace(t)).Count() == 0) {
				return;
			}
			AddTag(E621Tag.JoinTags(tags));
		}

		public async void AddTag(string tag) {
			if(string.IsNullOrWhiteSpace(tag)) {
				return;
			}
			var historyItem = new HistoryItem(tag, DateTime.Now);
			if(Tags.Contains(historyItem)) {//put it on the first
				Tags.Remove(historyItem);
				Tags.Insert(0, historyItem);
			} else {
				Tags.Insert(0, historyItem);
			}
			await WaitAndSave();
		}

		public async void RemoveTag(string tag) {
			if(string.IsNullOrWhiteSpace(tag)) {
				return;
			}
			int effect = Tags.RemoveAll(t => t.Value == tag);
			if(effect == 0) {
				return;
			}
			await WaitAndSave();
		}

		public async void AddPostID(string id) {
			if(string.IsNullOrWhiteSpace(id)) {
				return;
			}
			var historyItem = new HistoryItem(id, DateTime.Now);
			if(PostIDs.Contains(historyItem)) {
				PostIDs.Remove(historyItem);
				PostIDs.Insert(0, historyItem);
			} else {
				PostIDs.Insert(0, historyItem);
			}
			await WaitAndSave();
		}

		public async void RemovePostID(string id) {
			if(string.IsNullOrWhiteSpace(id)) {
				return;
			}
			int effect = PostIDs.RemoveAll(t => t.Value == id);
			if(effect == 0) {
				return;
			}
			await WaitAndSave();
		}

		public async Task RemoveBeforeDate(RecentType limitType) {
			int limit = (int)limitType;
			List<HistoryItem> removeTags = new();
			for(int i = 0; i < Tags.Count; i++) {
				int type = (int)HistoryDialogView.CalculateDateTimeType(Tags[i].Time);
				if(type >= limit) {
					removeTags.Add(Tags[i]);
				}
			}
			foreach(HistoryItem i in removeTags) {
				Tags.Remove(i);
			}

			List<HistoryItem> removeItems = new();
			for(int i = 0; i < PostIDs.Count; i++) {
				int type = (int)HistoryDialogView.CalculateDateTimeType(PostIDs[i].Time);
				if(type >= limit) {
					removeItems.Add(PostIDs[i]);
				}
			}
			foreach(HistoryItem i in removeItems) {
				PostIDs.Remove(i);
			}

			await WaitAndSave();
		}

		private async Task WaitAndSave() {
			try {
				if(cts != null) {
					cts.Cancel();
					cts.Dispose();
					cts = null;
				}
				cts = new CancellationTokenSource();
				await Task.Delay(SAVE_DELAY, cts.Token);
				await Local.WriteHistory();
			} catch(OperationCanceledException) { }
		}
	}
}

