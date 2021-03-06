using System;

namespace E621Downloader.Models {
	public class HistoryItem {
		public string Value { get; set; }
		public DateTime Time { get; set; }

		public string GetValueWithHashTag() => $"#{Value}";

		public HistoryItem(string value, DateTime time) {
			Value = value;
			Time = time;
		}

		public override bool Equals(object obj) {
			return obj is HistoryItem i && i.Value == this.Value;
		}

		public override int GetHashCode() {
			return base.GetHashCode();
		}

		public override string ToString() {
			return $"HistoryItem(Value:{Value}, Time:{Time})";
		}
	}
}
