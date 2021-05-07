using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace E621Downloader.Pages {
	public sealed partial class SearchPopup: Page {
		private ContentDialog contentDialog;
		public SearchPopup(ContentDialog contentDialog) {
			this.contentDialog = contentDialog;
			this.InitializeComponent();

			contentDialog.IsPrimaryButtonEnabled = false;
		}

		private void SearchBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args) {
			contentDialog.IsPrimaryButtonEnabled = true;
		}
		public string GetSearchText() {
			return SearchBox.Text;
		}
	}
}
