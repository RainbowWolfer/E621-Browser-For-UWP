using E621Downloader.Models;
using E621Downloader.Models.Posts;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

namespace E621Downloader.Views {
	public sealed partial class TagExpander: UserControl {
		private bool _expanded;
		public bool IsExpanded {
			get => _expanded;
			set {
				_expanded = value;
				ExpandPanel.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
				TitleIcon.Glyph = value ? "\uE010" : "\uE011";
			}
		}
		private E621Tag tag;
		public TagExpander(string tag) {
			this.InitializeComponent();
			if(string.IsNullOrEmpty(tag)) {
				TagText.Text = "Default".Language();
				ConentText.Text = "Order by newest";
				LoadingBar.Visibility = Visibility.Collapsed;
				ConentText.Visibility = Visibility.Visible;
			} else {
				Load(tag);
			}
		}

		private async void Load(string tag) {
			TagText.Text = $"{tag} (...)";
			CategoryRect.Visibility = Visibility.Collapsed;
			LoadingBar.Visibility = Visibility.Visible;
			ConentText.Visibility = Visibility.Collapsed;
			this.tag = await E621Tag.GetFirstAsync(tag);
			CategoryRect.Visibility = Visibility.Visible;
			if(this.tag != null) {
				TagCategory categoory = E621Tag.GetTagCategory(this.tag.category);
				Color color = E621Tag.GetCatrgoryColor(categoory, App.GetApplicationTheme() == ApplicationTheme.Dark);
				CategoryRect.Fill = new SolidColorBrush(color);
				ToolTipService.SetToolTip(CategoryRect, $"{E621Tag.GetCategory(this.tag.category)} - {this.tag.post_count}");
			}
			TagText.Text = $"{tag ?? "Not Found".Language()} ({this.tag?.post_count ?? 0})";
			if(this.tag != null && !this.tag.IsWikiLoaded) {
				await this.tag.LoadWikiAsync();
			}
			ConentText.Text = this.tag?.Wiki?.body ?? "No Wiki Found".Language();
			LoadingBar.Visibility = Visibility.Collapsed;
			ConentText.Visibility = Visibility.Visible;
		}

		private void Button_Tapped(object sender, TappedRoutedEventArgs e) {
			IsExpanded = !IsExpanded;
		}

	}
}
