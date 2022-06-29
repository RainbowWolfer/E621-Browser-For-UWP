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
						BottomInfo.Visibility = Visibility.Collapsed;
						TypeBorder.Visibility = Visibility.Collapsed;
						ItemIcon.Visibility = Visibility.Visible;
						ItemImage.Visibility = Visibility.Collapsed;
						break;
					case ItemType.Image:
						BottomInfo.Visibility = Visibility.Visible;
						TypeBorder.Visibility = Visibility.Collapsed;
						ItemIcon.Visibility = Visibility.Collapsed;
						ItemImage.Visibility = Visibility.Visible;
						SetImageSource();
						break;
					case ItemType.Gif:
						BottomInfo.Visibility = Visibility.Visible;
						TypeBorder.Visibility = Visibility.Visible;
						TypeTextBlock.Text = "GIF";
						ItemIcon.Visibility = Visibility.Collapsed;
						ItemImage.Visibility = Visibility.Visible;
						SetImageSource();
						break;
					case ItemType.Webm:
						BottomInfo.Visibility = Visibility.Visible;
						TypeBorder.Visibility = Visibility.Visible;
						TypeTextBlock.Text = "WEBM";
						ItemIcon.Visibility = Visibility.Collapsed;
						ItemImage.Visibility = Visibility.Visible;
						SetImageSource();
						break;
					case ItemType.None:
					default:
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
				}
			}
		}

		public LibraryGridViewItem() {
			this.InitializeComponent();
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
