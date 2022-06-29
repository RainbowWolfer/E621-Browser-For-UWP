using E621Downloader.Models;
using E621Downloader.Models.Locals;
using E621Downloader.Models.Posts;
using E621Downloader.Pages;
using E621Downloader.Pages.LibrarySection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Media.Imaging;

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
						foreach(LibraryListViewItem item in MyListView.Items.Cast<LibraryListViewItem>()) {
							item.Height = height;
						}
						break;
					}
				case ItemsGroupViewType.GridView: {
						double height = LibraryGridViewItem.BASE_HEIGHT * (size / 100d);
						foreach(LibraryGridViewItem item in MyGridView.Items.Cast<LibraryGridViewItem>()) {
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
						viewItem.ContextFlyout = GetContextMenu(item);
					} else if(item is LibraryImage image) {
						viewItem.ItemType = image.ItemType;
						viewItem.ContextFlyout = GetContextMenu(item);
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
						viewItem.ContextFlyout = GetContextMenu(item);
					} else if(item is LibraryImage image) {
						viewItem.ItemType = image.ItemType;
						viewItem.ContextFlyout = GetContextMenu(item);
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
				FontIcon icon = new() {
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

		private void Open(LibraryItem item) {
			if(item is LibraryFolder folder) {
				Library.ToTab(folder.Folder);
			} else if(item is LibraryImage image) {
				App.PostsList.UpdatePostsList(LibraryItems.Where(i => i is LibraryImage).Cast<LibraryImage>().ToList());
				App.PostsList.Current = image;
				MainPage.NavigateToPicturePage(image);
			}
		}

		private async void ConfirmDelete(LibraryItem item) {
			if(await new ContentDialog() {
				Title = "Confirm".Language(),
				Content = "Are you sure to delete this item".Language(),
				PrimaryButtonText = "Yes".Language(),
				CloseButtonText = "No".Language(),
				DefaultButton = ContentDialogButton.Close,
			}.ShowAsync() == ContentDialogResult.Primary) {
				try {
					if(item is LibraryFolder folder) {
						await folder.Folder.DeleteAsync();
					} else if(item is LibraryImage image) {
						await image.File.DeleteAsync();
					}
				} catch(Exception ex) {
					Debug.WriteLine(ex.Message);
				}

				for(int i = 0; i < MyGridView.Items.Count; i++) {
					if(MyGridView.Items[i] is LibraryGridViewItem viewItem && viewItem.Item == item) {
						MyGridView.Items.RemoveAt(i);
						break;
					}
				}

				for(int i = 0; i < MyListView.Items.Count; i++) {
					if(MyListView.Items[i] is LibraryListViewItem viewItem && viewItem.Item == item) {
						MyListView.Items.RemoveAt(i);
						break;
					}
				}

			}
		}

		private async void Rename(LibraryItem item) {
			string originName = "";
			if(item is LibraryFolder folder) {
				originName = folder.Folder.Name;
			} else if(item is LibraryImage image) {
				originName = image.File.Name;
			}

			await LibraryPage.Instance.ShowRenameDialog(originName, async newName => {
				try {
					if(item is LibraryFolder folder) {
						await folder.Folder.RenameAsync(newName);
					} else if(item is LibraryImage image) {
						await image.File.RenameAsync(newName);
					}
				} catch(Exception ex) {
					Debug.WriteLine(ex.Message);
				}

				for(int i = 0; i < MyGridView.Items.Count; i++) {
					if(MyGridView.Items[i] is LibraryGridViewItem viewItem && viewItem.Item == item) {
						viewItem.ItemName = newName;
						break;
					}
				}

				for(int i = 0; i < MyListView.Items.Count; i++) {
					if(MyListView.Items[i] is LibraryListViewItem viewItem && viewItem.Item == item) {
						viewItem.ItemName = newName;
						break;
					}
				}
			});
		}

		private void MyListView_ItemClick(object sender, ItemClickEventArgs e) {
			if(e.ClickedItem is LibraryListViewItem item) {
				Open(item.Item);
			}
		}

		private void MyGridView_ItemClick(object sender, ItemClickEventArgs e) {
			if(e.ClickedItem is LibraryGridViewItem item) {
				Open(item.Item);
			}
		}

		public MenuFlyout GetContextMenu(LibraryItem item) {
			MenuFlyout flyout = new() {
				Placement = FlyoutPlacementMode.RightEdgeAlignedTop,
			};
			MenuFlyoutItem item_open = new() {
				Text = "Open".Language(),
				Icon = new FontIcon { Glyph = "\uE197" },
			};
			MenuFlyoutItem item_delete = new() {
				Text = "Delete".Language(),
				Icon = new FontIcon { Glyph = "\uE107" },
			};
			MenuFlyoutItem item_rename = new() {
				Text = "Rename".Language(),
				Icon = new FontIcon { Glyph = "\uE13E" },
			};
			item_open.Click += (s, e) => {
				Open(item);
			};
			item_delete.Click += (s, e) => {
				ConfirmDelete(item);
			};
			item_rename.Click += (s, e) => {
				Rename(item);
			};
			flyout.Items.Add(item_open);
			flyout.Items.Add(item_delete);
			flyout.Items.Add(item_rename);
			return flyout;
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
				if(File.FileType is ".png" or ".jpg") {
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
