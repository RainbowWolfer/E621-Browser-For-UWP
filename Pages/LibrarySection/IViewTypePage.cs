using E621Downloader.Views.LibrarySection;

namespace E621Downloader.Pages.LibrarySection {
	public interface ILibraryGridPage {
		bool IsLoading { get; set; }
		void UpdateSize(int size);
		LibraryItemsGroupView GetGroupView();
		void RefreshRequest();
		void DisplayHeader(bool enabled);
	}
}
