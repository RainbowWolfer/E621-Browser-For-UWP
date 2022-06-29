namespace E621Downloader.Models.Inerfaces {
	public interface IPage {
		void UpdateNavigationItem();
		void FocusMode(bool enabled);//need to be checked in every OnNavigatedTo();
	}
}
