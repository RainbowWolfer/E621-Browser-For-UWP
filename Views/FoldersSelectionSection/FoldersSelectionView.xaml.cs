using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

namespace E621Downloader.Views.FoldersSelectionSection {
	public sealed partial class FoldersSelectionView: UserControl {
		private readonly ObservableCollection<StorageFolder> folders;
		public FoldersSelectionView(List<StorageFolder> folders) {
			this.InitializeComponent();
			this.folders = new ObservableCollection<StorageFolder>();
			foreach(StorageFolder item in folders) {
				this.folders.Add(item);
			}

		}

		private void ReverseSelectionButton_Tapped(object sender, TappedRoutedEventArgs e) {
			for(int i = 0; i < MyListView.Items.Count; i++) {
				var item = MyListView.ContainerFromIndex(i) as ListViewItem;
				item.IsSelected = !item.IsSelected;
			}
		}
	}
}
