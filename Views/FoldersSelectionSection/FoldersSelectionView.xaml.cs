using System.Collections.Generic;
using System.Collections.ObjectModel;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace E621Downloader.Views.FoldersSelectionSection {
	public sealed partial class FoldersSelectionView: UserControl {
		private readonly ObservableCollection<StorageFolder> folders;
		private readonly List<StorageFolder> defaultSelected;
		public FoldersSelectionView(List<StorageFolder> folders, List<StorageFolder> selected) {
			this.InitializeComponent();
			this.folders = new ObservableCollection<StorageFolder>();
			this.defaultSelected = selected;
			foreach(StorageFolder item in folders) {
				this.folders.Add(item);
			}

		}

		private void ReverseSelectionButton_Click(object sender, RoutedEventArgs e) {
			for(int i = 0; i < MyListView.Items.Count; i++) {
				var item = MyListView.ContainerFromIndex(i) as ListViewItem;
				item.IsSelected = !item.IsSelected;
			}
		}

		public List<StorageFolder> GetSelected() {
			var result = new List<StorageFolder>();
			foreach(object item in MyListView.SelectedItems) {
				if(item is StorageFolder folder) {
					result.Add(folder);
				}
			}
			return result;
		}

		private void MyListView_Loaded(object sender, RoutedEventArgs e) {
			if(defaultSelected == null || defaultSelected.Count == 0) {
				return;
			}
			foreach(StorageFolder item in folders) {
				if(defaultSelected.Contains(item)) {
					MyListView.SelectedItems.Add(item);
				}
			}
		}
	}
}
