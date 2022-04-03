using E621Downloader.Models;
using E621Downloader.Models.Networks;
using E621Downloader.Models.Posts;
using E621Downloader.Pages;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace E621Downloader.Views {
	public sealed partial class PersonalFavoritesListForMultiple: UserControl {
		private ContentDialog dialog;
		private readonly PathType type;
		public ObservableCollection<CheckBoxClass> Paths = new();
		public PersonalFavoritesListForMultiple(ContentDialog dialog, PathType type, IEnumerable<Post> posts) {
			this.InitializeComponent();
			this.dialog = dialog;
			this.type = type;
			foreach(Post post in posts) {
				Paths.Add(new CheckBoxClass(post.id));
			}
		}

		private void BackButton_Tapped(object sender, TappedRoutedEventArgs e) {
			dialog.Hide();
		}

		private void AcceptButton_Tapped(object sender, TappedRoutedEventArgs e) {

			dialog.Hide();
		}

		private void E621FavoriteButton_Tapped(object sender, TappedRoutedEventArgs e) {
			e.Handled = true;
			E621FavoriteButton.IsEnabled = false;
			FavoriteText.Text = "Pending";
			FavoriteIcon.Glyph = "\uE10C";

			//if(E621FavoriteButton.IsChecked.Value) {
			//	HttpResult<string> result = await Favorites.PostAsync(path);
			//	if(result.Result == HttpResultType.Success) {
			//		FavoriteText.Text = "E621 Favorited";
			//		FavoriteIcon.Glyph = "\uEB52";
			//	} else {
			//		FavoriteText.Text = "E621 Favorite";
			//		FavoriteIcon.Glyph = "\uEB51";
			//		MainPage.CreateTip(MainGrid, result.StatusCode.ToString(), result.Helper, Symbol.Important, "OK");
			//		E621FavoriteButton.IsChecked = false;
			//	}
			//} else {
			//	HttpResult<string> result = await Favorites.DeleteAsync(path);
			//	if(result.Result == HttpResultType.Success) {
			//		FavoriteText.Text = "E621 Favorite";
			//		FavoriteIcon.Glyph = "\uEB51";
			//	} else {
			//		FavoriteText.Text = "E621 Favorited";
			//		FavoriteIcon.Glyph = "\uEB52";
			//		MainPage.CreateTip(MainGrid, result.StatusCode.ToString(), result.Helper, Symbol.Important, "OK");
			//		E621FavoriteButton.IsChecked = true;
			//	}
			//}
			E621FavoriteButton.IsEnabled = true;
		}
	}
	public class CheckBoxClass {
		public string Content { get; set; }
		public bool IsChecked { get; set; } = false;

		public CheckBoxClass(string content) {
			Content = content;
		}
	}
}
