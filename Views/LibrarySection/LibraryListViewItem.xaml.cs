using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Storage;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace E621Downloader.Views.LibrarySection {
	public sealed partial class LibraryListViewItem: UserControl, ILibraryItemView {
		public const int BASE_HEIGHT = 55;
		private string itemName;
		private ItemType itemType;
		private LibraryItem item;

		public string ItemName {
			get => itemName;
			set {
				itemName = value;
				ItemNameText.Text = itemName;
			}
		}

		public ItemType ItemType {
			get => itemType;
			set {
				itemType = value;
				switch(itemType) {
					case ItemType.Folder:
						FolderChildrenCountText.Visibility = Visibility.Visible;
						ItemIcon.Glyph = "\uE838";
						Color folderColor = App.GetApplicationTheme() switch {
							ApplicationTheme.Light => Colors.Orange,
							ApplicationTheme.Dark => Colors.Yellow,
							_ => Colors.Yellow,
						};
						ItemIcon.Foreground = new SolidColorBrush(folderColor);
						ItemImage.Visibility = Visibility.Collapsed;
						BottomInfo.Visibility = Visibility.Collapsed;
						break;
					case ItemType.Image:
						FolderChildrenCountText.Visibility = Visibility.Collapsed;
						ItemIcon.Glyph = "\uEB9F";
						ItemImage.Visibility = Visibility.Visible;
						BottomInfo.Visibility = Visibility.Visible;
						SetImageSource();
						break;
					case ItemType.Gif:
						FolderChildrenCountText.Visibility = Visibility.Collapsed;
						ItemIcon.Glyph = "\uF4A9";
						ItemImage.Visibility = Visibility.Visible;
						BottomInfo.Visibility = Visibility.Visible;
						SetImageSource();
						break;
					case ItemType.Webm:
						FolderChildrenCountText.Visibility = Visibility.Collapsed;
						ItemIcon.Glyph = "\uE8B2";
						ItemImage.Visibility = Visibility.Visible;
						BottomInfo.Visibility = Visibility.Visible;
						SetImageSource();
						break;
					case ItemType.None:
					default:
						FolderChildrenCountText.Visibility = Visibility.Collapsed;
						ItemIcon.Glyph = "\uE8A5";
						ItemImage.Visibility = Visibility.Visible;
						BottomInfo.Visibility = Visibility.Visible;
						SetImageSource();
						break;
				}
			}
		}

		public LibraryItem Item {
			get => item;
			set {
				item = value;
				if(item is LibraryImage image) {
					BottomInfo.PostRef = image.Meta.MyPost;
				} else if(item is LibraryFolder folder) {
					CalculateFolderChildren(folder.Folder);
				}
			}
		}

		private async void CalculateFolderChildren(StorageFolder folder) {
			//normally the .meta file and the image file are shown in pairs.
			List<StorageFile> files = (await folder.GetFilesAsync()).ToList();
			//replaced it with Supported File Types
			files = files.Where(f => {
				return new string[] {
					".meta", ".png", ".webm", ".jpg", ".gif"
				}.Contains(f.FileType.ToLower());
			}).ToList();
			FolderChildrenCountText.Text = $"( {files.Count / 2} )";
		}

		public LibraryListViewItem() {
			this.InitializeComponent();
		}

		public void SetImageSource() {
			if(Item is LibraryImage image) {
				ItemImage.Source = image.Bitmap;
			}
		}
	}
}
