using E621Downloader.Models.Locals;
using E621Downloader.Models.Posts;
using E621Downloader.Pages;
using E621Downloader.Pages.LibrarySection;
using System;
using System.Collections.Generic;
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

namespace E621Downloader.Views.LibrarySection {
	public sealed partial class LibraryItemsGroupView: UserControl {
		public LibraryPage Library { get; set; }
		private ItemsGroupViewType viewType = ItemsGroupViewType.GridView;

		public List<LibraryItem> LibraryItems { get; } = new List<LibraryItem>();

		public ItemsGroupViewType ViewType {
			get => viewType;
			set {
				viewType = value;
				switch(viewType) {
					case ItemsGroupViewType.ListView:
						GridViewGrid.Visibility = Visibility.Collapsed;
						ListViewGrid.Visibility = Visibility.Visible;
						break;
					case ItemsGroupViewType.GridView:
						GridViewGrid.Visibility = Visibility.Visible;
						ListViewGrid.Visibility = Visibility.Collapsed;
						break;
					default:
						throw new Exception($"View Type ({viewType}) Not Found");
				}
			}
		}

		public LibraryItemsGroupView() {
			this.InitializeComponent();
		}

		public void ClearNoDataGrid() {
			ListViewNoDataPanel.Visibility = Visibility.Collapsed;
			GridViewNoDataPanel.Visibility = Visibility.Collapsed;
		}

		public void UpdateSize(int size) {
			switch(viewType) {
				case ItemsGroupViewType.ListView: {
					double height = LibraryListViewItem.BASE_HEIGHT * (size / 100d);
					foreach(LibraryListViewItem item in MyListView.Items) {
						item.Height = height;
					}
					break;
				}
				case ItemsGroupViewType.GridView: {
					double height = LibraryGridViewItem.BASE_HEIGHT * (size / 100d);
					foreach(LibraryGridViewItem item in MyGridView.Items) {
						item.Height = height;
						item.Width = height + 30;
					}
					break;
				}
				default:
					throw new Exception($"View Type ({viewType}) Not Found");
			}
		}

		private void UpdateGridView() {
			MyGridView.Items.Clear();
			if(LibraryItems.Count != 0) {
				ListViewNoDataPanel.Visibility = Visibility.Collapsed;
				foreach(LibraryItem item in LibraryItems) {
					var viewItem = new LibraryGridViewItem() {
						Item = item,
						ItemName = item.Text,
					};
					if(item is LibraryFolder folder) {
						viewItem.ItemType = ItemType.Folder;
						ToolTipService.SetToolTip(viewItem, new LibraryFolderDetailTooltip(folder.Folder));
					} else if(item is LibraryImage image) {
						viewItem.ItemType = image.ItemType;
					}
					MyGridView.Items.Add(viewItem);
					//await Task.Delay(5);
				}
			} else {
				ListViewNoDataPanel.Visibility = Visibility.Visible;
			}
			UpdateSize(Library.Size);
		}

		private void UpdateListView() {
			MyListView.Items.Clear();
			if(LibraryItems.Count != 0) {
				ListViewNoDataPanel.Visibility = Visibility.Collapsed;
				foreach(LibraryItem item in LibraryItems) {
					var viewItem = new LibraryListViewItem() {
						Item = item,
						ItemName = item.Text,
					};
					if(item is LibraryFolder folder) {
						viewItem.ItemType = ItemType.Folder;
						ToolTipService.SetToolTip(viewItem, new LibraryFolderDetailTooltip(folder.Folder));
					} else if(item is LibraryImage image) {
						viewItem.ItemType = image.ItemType;
					}
					MyListView.Items.Add(viewItem);
					//await Task.Delay(5);
				}
			} else {
				ListViewNoDataPanel.Visibility = Visibility.Visible;
			}
			UpdateSize(Library.Size);
		}

		public void Update() {
			switch(ViewType) {
				case ItemsGroupViewType.ListView:
					UpdateListView();
					break;
				case ItemsGroupViewType.GridView:
					UpdateGridView();
					break;
				default:
					throw new Exception($"View Type ({viewType}) Not Found");
			}
		}

		public void ClearItems() {
			LibraryItems.Clear();
			MyListView.Items.Clear();
			MyGridView.Items.Clear();
		}

		public void SetFolders(List<StorageFolder> folders) {
			LibraryItems.Clear();
			foreach(StorageFolder item in folders) {
				FontIcon icon = new FontIcon() {
					Glyph = "\uE838",
				};
				LibraryItems.Add(new LibraryFolder() {
					Folder = item,
					Text = item.DisplayName,
					Icon = icon,
				});
			}
			Update();
		}

		public void SetImages(List<(MetaFile, BitmapImage, StorageFile)> list) {
			LibraryItems.Clear();
			foreach((MetaFile meta, BitmapImage bitmap, StorageFile file) in list) {
				LibraryItems.Add(new LibraryImage() {
					Bitmap = bitmap,
					Meta = meta,
					File = file,
					Text = file.Name,
					Icon = new Image() {
						Source = bitmap,
					},
				});
			}
			Update();
		}

		private void MyListView_ItemClick(object sender, ItemClickEventArgs e) {
			if(e.ClickedItem is LibraryListViewItem item) {
				if(item.Item is LibraryFolder folder) {
					Library.ToTab(folder.Folder);
				} else if(item.Item is LibraryImage image) {
					App.PostsList.UpdatePostsList(LibraryItems.Where(i => i is LibraryImage).Cast<LibraryImage>().ToList());
					App.PostsList.Current = image;
					MainPage.NavigateToPicturePage(image);
				}
			}
		}

		private void MyGridView_ItemClick(object sender, ItemClickEventArgs e) {
			if(e.ClickedItem is LibraryGridViewItem item) {
				if(item.Item is LibraryFolder folder) {
					Library.ToTab(folder.Folder);
				} else if(item.Item is LibraryImage image) {
					App.PostsList.UpdatePostsList(LibraryItems.Where(i => i is LibraryImage).Cast<LibraryImage>().ToList());
					App.PostsList.Current = image;
					MainPage.NavigateToPicturePage(image);
				}
			}
		}
	}

	public enum ItemType {
		Folder, Image, Gif, Webm, None
	}


	public enum ItemsGroupViewType {
		ListView, GridView
	}

	public abstract class LibraryItem {
		public FrameworkElement Icon { get; set; }
		public string Text { get; set; }

	}

	public class LibraryImage: LibraryItem, ILocalImage {
		Post ILocalImage.ImagePost => Meta.MyPost;
		StorageFile ILocalImage.ImageFile => File;

		public StorageFile File { get; set; }
		public MetaFile Meta { get; set; }
		public BitmapImage Bitmap { get; set; }

		public ItemType ItemType {
			get {
				if(File.FileType == ".png" || File.FileType == ".jpg") {
					return ItemType.Image;
				} else if(File.FileType == ".gif") {
					return ItemType.Gif;
				} else if(File.FileType == ".webm") {
					return ItemType.Webm;
				} else {
					return ItemType.None;
				}
			}
		}

	}

	public class LibraryFolder: LibraryItem {
		public StorageFolder Folder { get; set; }
	}
}
