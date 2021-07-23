using E621Downloader.Models.Locals;
using E621Downloader.Views.FoldersSelectionSection;
using E621Downloader.Views.TagsManagementSection;
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
		private readonly List<ItemBlock> original;

		public LibraryTab CurrentLibraryTab { get; private set; }
		public ItemBlock CurrentItemBlock { get; private set; }

		//private bool refreshNeeded;

		public Explorer() {
			this.InitializeComponent();
			items = new ObservableCollection<ItemBlock>();
			folders = new List<StorageFolder>();
			original = new List<ItemBlock>();
		}

		protected async override void OnNavigatedTo(NavigationEventArgs e) {
			base.OnNavigatedTo(e);
			FilterGrid.Visibility = Visibility.Collapsed;
			if(e.Parameter is object[] objs) {
				if(objs.Length >= 2 && objs[1] is LibraryPage libraryPage) {
					this.libraryPage = libraryPage;
					libraryPage.current = this;
					if(objs[0] is LibraryTab tab) {//side tab
						CurrentLibraryTab = tab;
						if(tab.title == "Home") {//click home
							foreach(StorageFolder folder in await Local.GetDownloadsFolders()) {
								folders.Add(folder);
								var myitem = new ItemBlock() {
									parentFolder = folder,
									parent = null,
								};
								items.Add(myitem);
								original.Add(myitem);
							}
							if(folders.Count == 0) {
								HintText.Visibility = Visibility.Visible;
							}
						} else if(tab.title == "Filter") {//click filter page
							Debug.WriteLine("Filter VIEW");
							FilterGrid.Visibility = Visibility.Visible;
							foreach(StorageFolder folder in await Local.GetDownloadsFolders()) {
								folders.Add(folder);
							}


						} else {//click folder
							List<(MetaFile, BitmapImage, StorageFile)> v = await Local.GetMetaFiles(tab.folder.DisplayName);
							foreach((MetaFile, BitmapImage, StorageFile) item in v) {
								var myitem = new ItemBlock() {
									meta = item.Item1,
									thumbnail = item.Item2,
									imageFile = item.Item3,
								};
								items.Add(myitem);
								original.Add(myitem);
							}
						}
					} else if(objs[0] is ItemBlock parent) {//click folder in page
						CurrentItemBlock = parent;
						List<(MetaFile, BitmapImage, StorageFile)> v = await Local.GetMetaFiles(parent.Name);
						foreach((MetaFile, BitmapImage, StorageFile) item in v) {
							var myitem = new ItemBlock() {
								meta = item.Item1,
								thumbnail = item.Item2,
								imageFile = item.Item3,
								parent = parent,
							};
							items.Add(myitem);
							original.Add(myitem);
						}
					} else {
						throw new Exception("?");
					}
				}
			}
			//refreshNeeded = false;
			MyProgressRing.IsActive = false;
			libraryPage.EnableRefreshButton(false);
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

		public void Search(string content) {
			foreach(ItemBlock o in original) {
				if(o.Name.Contains(content)) {
					if(!items.Contains(o)) {
						items.Add(o);
					}
				} else {
					if(items.Contains(o)) {
						items.Remove(o);
					}
				}
			}

		}
		public void ClearSearch() {

		}

		public void RefreshRequest() {
			//refreshNeeded = true;
			libraryPage.EnableRefreshButton(true);
		}

		private async void TagsButton_Tapped(object sender, TappedRoutedEventArgs e) {
			List<LocalTagsInfo> list = (await GetLocalTagsInfo()).OrderByDescending(s => s.count).ToList();
			foreach(var item in list) {
				Debug.WriteLine($"{item.name}_{item.count}");
			}
			var dialog = new ContentDialog() {
				Title = "Manage Your Search Tags",
			};

			var frame = new Frame();
			dialog.Content = frame;
			frame.Navigate(typeof(TagsSelectionView), new object[] { dialog, PostsBrowser.Instance.tags });
			await dialog.ShowAsync();

			var content = (dialog.Content as Frame).Content as TagsSelectionView;
			if(content.handleSearch) {
				Debug.WriteLine(content.tags.Count);
			}
		}

		private async void FoldersButton_Tapped(object sender, TappedRoutedEventArgs e) {
			if(folders == null | folders.Count == 0) {
				return;
			}
			var foldersView = new FoldersSelectionView(folders);
			if(await new ContentDialog() {
				Title = "Manage Your Search Tags",
				Content = foldersView,
				PrimaryButtonText = "Confirm",
				CloseButtonText = "Back",
			}.ShowAsync() == ContentDialogResult.Primary) {

			}
		}

		private async Task<List<LocalTagsInfo>> GetLocalTagsInfo() {
			var tags = new List<LocalTagsInfo>();
			foreach(MetaFile item in await Local.GetAllMetaFiles()) {
				foreach(string t in item.MyPost.tags.GetAllTags()) {
					LocalTagsInfo.AddToList(tags, new LocalTagsInfo(t));
				}
			}
			return tags;
		}

	}
	public class LocalTagsInfo {
		public string name;
		public int count = 1;

		public LocalTagsInfo(string name) {
			this.name = name;
		}

		public static void AddToList(List<LocalTagsInfo> list, LocalTagsInfo newOne) {
			foreach(LocalTagsInfo item in list) {
				if(item.name == newOne.name) {
					item.count++;
					return;
				}
			}
			list.Add(newOne);
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

		public Visibility TypeBorderVisibility => meta != null && new string[] { "webm", "gif", "anim" }.Contains(meta.MyPost?.file?.ext?.ToLower()) ? Visibility.Visible : Visibility.Collapsed;
		public string TypeText => meta != null && meta.MyPost != null && meta.MyPost.file != null ? meta.MyPost.file.ext : "UNDEFINED";
	}
}
