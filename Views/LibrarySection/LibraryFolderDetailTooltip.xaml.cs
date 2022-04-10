using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace E621Downloader.Views.LibrarySection {
	public sealed partial class LibraryFolderDetailTooltip: UserControl {
		private readonly StorageFolder folder;
		public LibraryFolderDetailTooltip(StorageFolder folder) {
			this.InitializeComponent();
			this.folder = folder;
			FolderNameText.Text = folder.Name;	
			var date = folder.DateCreated;
			CreatedDateText.Text = $"{date.Year}-{date.Month}-{date.Day} {date.Hour}:{date.Minute}:{date.Second}";
		}
	}
}
