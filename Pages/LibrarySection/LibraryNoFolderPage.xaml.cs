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

namespace E621Downloader.Pages.LibrarySection {
	public sealed partial class LibraryNoFolderPage: Page {
		public LibraryNoFolderPage() {
			this.InitializeComponent();
		}

		private void HintActionButton_Click(object sender, RoutedEventArgs e) {
			MainPage.NavigateTo(PageTag.Settings);
		}
	}
}
