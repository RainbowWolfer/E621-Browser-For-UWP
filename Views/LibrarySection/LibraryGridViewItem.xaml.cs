using System.Numerics;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace E621Downloader.Views.LibrarySection {
	public sealed partial class LibraryGridViewItem: UserControl, ILibraryItemView {
		public const double BASE_HEIGHT = 200;

		private ItemType itemType;
		private string itemName;
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
						BottomInfo.Visibility = Visibility.Collapsed;
						TypeBorder.Visibility = Visibility.Collapsed;
						ItemIcon.Visibility = Visibility.Visible;
						ItemImage.Visibility = Visibility.Collapsed;
						break;
					case ItemType.Image:
						FolderChildrenCountText.Visibility = Visibility.Collapsed;
						BottomInfo.Visibility = Visibility.Visible;
						TypeBorder.Visibility = Visibility.Collapsed;
						ItemIcon.Visibility = Visibility.Collapsed;
						ItemImage.Visibility = Visibility.Visible;
						SetImageSource();
						break;
					case ItemType.Gif:
						FolderChildrenCountText.Visibility = Visibility.Collapsed;
						BottomInfo.Visibility = Visibility.Visible;
						TypeBorder.Visibility = Visibility.Visible;
						TypeTextBlock.Text = "GIF";
						ItemIcon.Visibility = Visibility.Collapsed;
						ItemImage.Visibility = Visibility.Visible;
						SetImageSource();
						break;
					case ItemType.Webm:
						FolderChildrenCountText.Visibility = Visibility.Collapsed;
						BottomInfo.Visibility = Visibility.Visible;
						TypeBorder.Visibility = Visibility.Visible;
						TypeTextBlock.Text = "WEBM";
						ItemIcon.Visibility = Visibility.Collapsed;
						ItemImage.Visibility = Visibility.Visible;
						SetImageSource();
						break;
					case ItemType.None:
					default:
						FolderChildrenCountText.Visibility = Visibility.Collapsed;
						BottomInfo.Visibility = Visibility.Visible;
						TypeBorder.Visibility = Visibility.Visible;
						TypeTextBlock.Text = "UNKNOWN";
						ItemIcon.Visibility = Visibility.Collapsed;
						ItemImage.Visibility = Visibility.Visible;
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
					LibraryListViewItem.CalculateFolderChildren(folder, count => {
						FolderChildrenCountText.Text = $"( {count} )";
					});
				}
			}
		}

		public LibraryGridViewItem() {
			this.InitializeComponent();
			CountTextBorder.Translation += new Vector3(0, 0, 8);
		}

		public void SetImageSource() {
			if(Item is LibraryImage image) {
				ItemImage.Source = image.Bitmap;
			}
		}

		private void UpdateItem() {

		}

		private void Grid_Loaded(object sender, RoutedEventArgs e) {

		}
	}
}
