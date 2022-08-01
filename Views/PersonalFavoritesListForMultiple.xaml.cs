using E621Downloader.Models.E621;
using E621Downloader.Models.Locals;
using E621Downloader.Pages;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace E621Downloader.Views {
	public sealed partial class PersonalFavoritesListForMultiple: UserControl {
		private readonly ContentDialog dialog;
		private readonly PathType type;
		private readonly IEnumerable<E621Post> posts;

		private readonly ObservableCollection<FavoriteListCheckBoxClass> FavoriteLists = new();

		public PersonalFavoritesListForMultiple(ContentDialog dialog, PathType type, IEnumerable<E621Post> posts) {
			this.InitializeComponent();
			this.dialog = dialog;
			this.type = type;
			this.posts = posts;

			foreach(FavoritesList item in FavoritesList.Table) {
				FavoriteLists.Add(new FavoriteListCheckBoxClass(item));
			}
		}

		private List<string> GetSelectedList() {
			return FavoriteLists.Where(i => i.IsChecked).Select(i => i.Name).ToList();
		}

		private void BackButton_Tapped(object sender, TappedRoutedEventArgs e) {
			dialog.Hide();
		}

		private void AcceptButton_Tapped(object sender, TappedRoutedEventArgs e) {
			List<string> selected = GetSelectedList();
			foreach(E621Post post in posts) {
				FavoritesList.Modify(new(), selected, post.id, PathType.PostID);
			}
			dialog.Hide();
		}

		private void ListView_ItemClick(object sender, ItemClickEventArgs e) {
			///two way is not working
			//if(e.ClickedItem is FavoriteListCheckBoxClass cc) {
			//	cc.IsChecked = !cc.IsChecked;
			//}
		}
	}

	public class FavoriteListCheckBoxClass {
		public bool IsChecked { get; set; } = false;
		public string Name { get; set; }
		public int Count { get; set; }

		public string Text => $"{Name} - {Count}";

		public FavoriteListCheckBoxClass(FavoritesList list) {
			Name = list.Name;
			Count = list.Items.Count;
		}
	}
}
