using E621Downloader.Models.Locals;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

namespace E621Downloader.Pages.LibrarySection {
	public sealed partial class Explorer: Page {
		private LibraryPage libraryPage;
		private readonly List<StorageFolder> folders;
		public ObservableCollection<ItemBlock> items;

		private LibraryTab libraryTab;
		private ItemBlock itemBlock;

		public Explorer() {
			this.InitializeComponent();
			items = new ObservableCollection<ItemBlock>();
			folders = new List<StorageFolder>();
		}

		protected async override void OnNavigatedTo(NavigationEventArgs e) {
			base.OnNavigatedTo(e);
			if(e.Parameter is object[] objs) {
				if(objs.Length >= 2 && objs[1] is LibraryPage libraryPage) {
					this.libraryPage = libraryPage;
					libraryPage.current = this;
					//await LoadAsync(objs[0] as LibraryTab, objs[0] as ItemBlock);
					LibraryTab tab = objs[0] as LibraryTab;
					ItemBlock parent = objs[0] as ItemBlock;
					if(tab != null) {
						libraryTab = tab;
						if(tab.title == "Home") {
							foreach(StorageFolder folder in await Local.GetDownloadsFolders()) {
								if(folder == null) {
									Debug.WriteLine("1");
								}
								folders.Add(folder);
								items.Add(new ItemBlock() {
									parentFolder = folder,
									parent = null,
								});
							}
							if(folders.Count == 0) {
								HintText.Visibility = Visibility.Visible;
							}
						} else if(tab.title == "Filter") {
							Debug.WriteLine("Filter VIEW");
						} else {
							List<(MetaFile, BitmapImage, StorageFile)> v = await Local.GetMetaFiles(tab.folder.DisplayName);
							Debug.WriteLine("111Count" + v.Count);
							foreach((MetaFile, BitmapImage, StorageFile) item in v) {
								Debug.WriteLine($"First_ {item.Item1},{item.Item2},{item.Item3}");
								items.Add(new ItemBlock() {
									meta = item.Item1,
									thumbnail = item.Item2,
									imageFile = item.Item3,
								});
							}
						}
					} else if(parent != null) {
						itemBlock = parent;
						List<(MetaFile, BitmapImage, StorageFile)> v = await Local.GetMetaFiles(parent.Name);

						Debug.WriteLine("222Count" + v.Count);
						foreach((MetaFile, BitmapImage, StorageFile) item in v) {
							Debug.WriteLine($"Second_ {item.Item1},{item.Item2},{item.Item3}");
							items.Add(new ItemBlock() {
								meta = item.Item1,
								thumbnail = item.Item2,
								imageFile = item.Item3,
								parent = parent,
							});
						}
					} else {
						throw new Exception("?");
					}
				}
			}
			MyProgressRing.IsActive = false;
		}
		private async Task LoadAsync(LibraryTab tab, ItemBlock parent) {

		}

		private void MyGridView_ItemClick(object sender, ItemClickEventArgs e) {
			var target = e.ClickedItem as ItemBlock;
			if(target.IsFolder) {
				libraryPage.Navigate(typeof(Explorer), new object[] { target, libraryPage });
				libraryPage.ToTab(folders.Find(f => f.DisplayName == target.Name), target.Name);
			} else {
				MainPage.Instance.parameter_picture = target;
				MainPage.SelectNavigationItem(PageTag.Picture);
			}
		}

		//public async void Refresh() {
		//	MyProgressRing.IsActive = true;
		//	items.Clear();
		//	await LoadAsync(libraryTab, itemBlock);
		//}

		public void RefreshRequest() {

		}
	}

	public class ItemBlock {
		public StorageFolder parentFolder;
		public StorageFile imageFile;
		public MetaFile meta;
		public ItemBlock parent;
		public ImageSource thumbnail;

		public string Name => meta == null ? parentFolder.DisplayName : meta.MyPost.id.ToString();
		public bool IsFolder => parentFolder != null;

		public Visibility FolderIconVisibility => IsFolder ? Visibility.Visible : Visibility.Collapsed;
		public Visibility ImagePreviewVisibility => IsFolder ? Visibility.Collapsed : Visibility.Visible;

	}
}
