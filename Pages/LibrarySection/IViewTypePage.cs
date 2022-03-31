using E621Downloader.Views.LibrarySection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E621Downloader.Pages.LibrarySection {
	public interface ILibraryGridPage {
		void UpdateSize(int size);
		LibraryItemsGroupView GetGroupView();
		void Search(string content);
		void RefreshRequest();
	}
}
