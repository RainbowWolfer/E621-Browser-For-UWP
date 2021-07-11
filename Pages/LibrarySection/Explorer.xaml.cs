using E621Downloader.Models.Locals;
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
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

namespace E621Downloader.Pages.LibrarySection {
	public sealed partial class Explorer: Page {
		private LibraryPage libraryPage;
		private readonly List<StorageFolder> folders;
		public ObservableCollection<ItemBlock> items;
		private GridView test => MyGridView;
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
					if(objs[0] is LibraryTab tab) {
						if(tab.title == "Home") {
							foreach(StorageFolder folder in await Local.GetDownloadsFolders()) {
								folders.Add(folder);
								items.Add(new ItemBlock() {
									parentFolder = folder,
									parent = null,
								});
							}
						} else {
							List<(MetaFile, BitmapImage, StorageFile)> v = await Local.GetMetaFiles(tab.folder.DisplayName);
							foreach(var item in v) {
								items.Add(new ItemBlock() {
									meta = item.Item1,
									thumbnail = item.Item2,
									imageFile = item.Item3,
								});
							}
						}
					} else if(objs[0] is ItemBlock parent) {
						List<(MetaFile, BitmapImage, StorageFile)> v = await Local.GetMetaFiles(parent.Name);
						foreach(var item in v) {
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
