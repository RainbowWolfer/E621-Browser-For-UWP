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
		public ObservableCollection<ItemBlock> items;
		public Explorer() {
			this.InitializeComponent();
			items = new ObservableCollection<ItemBlock>();
		}

		protected async override void OnNavigatedTo(NavigationEventArgs e) {
			base.OnNavigatedTo(e);
			if(e.Parameter is object[] objs) {
				if(objs.Length >= 2 && objs[1] is LibraryPage libraryPage) {
					this.libraryPage = libraryPage;
					if(objs[0] is LibraryTab tab) {
						if(tab.title == "Home") {
							foreach(StorageFolder folder in await Local.GetDownloadsFolders()) {
								items.Add(new ItemBlock() {
									isFolder = true,
									name = folder.Name,
									parent = null,
								});
							}
						} else {

						}
					} else if(objs[0] is ItemBlock parent) {
						List<(MetaFile, BitmapImage)> v = await Local.GetMetaFiles(parent.name);
						foreach(var item in v) {
							items.Add(new ItemBlock() {
								isFolder = false,
								name = item.Item1.MyPost.id.ToString(),
								image = item.Item2,
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
			if(target.isFolder) {
				libraryPage.Navigate(typeof(Explorer), new object[] { target, libraryPage });
				libraryPage.ToTab(target.name);
			}
		}
	}

	public class ItemBlock {
		public bool isFolder;
		public string name;
		public ItemBlock parent;
		public ImageSource image;

		public Visibility FolderIconVisibility { get => isFolder ? Visibility.Visible : Visibility.Collapsed; }
		public Visibility ImagePreviewVisibility { get => isFolder ? Visibility.Collapsed : Visibility.Visible; }

	}
}
