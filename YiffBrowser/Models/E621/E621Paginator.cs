namespace YiffBrowser.Models.E621 {
	public class E621Paginator {

		public int CurrentPage { get; set; }
		public int[] Pages { get; set; }

		public int GetMaxPage() {
			int max = -1;
			foreach (int item in Pages) {
				if (item > max) {
					max = item;
				}
			}
			return max;
		}
	}
}
