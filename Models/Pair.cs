namespace E621Downloader.Models {
	public class Pair<T, W> {
		public T Key { get; set; }
		public W Value { get; set; }
		public Pair(T key, W value) {
			Key = key;
			Value = value;
		}

		public override string ToString() {
			return $"({Key} : {Value})";
		}
	}
}
