namespace E621Downloader.Views.LibrarySection {
	public interface ILibraryItemView {
		string ItemName { get; set; }
		ItemType ItemType { get; set; }
	}
}
