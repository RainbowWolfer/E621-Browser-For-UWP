using E621Downloader.Models.Posts;
using System.Collections.Generic;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;

namespace E621Downloader.Views {
	public sealed partial class TagCategoryColorsPreview: UserControl {
		public TagCategoryColorsPreview() {
			this.InitializeComponent();
			bool isDark = App.GetApplicationTheme() == ApplicationTheme.Dark;
			foreach(var item in new Dictionary<Rectangle, TagCategory>() {
				{ Artists, TagCategory.Artists },
				{ Copyrights, TagCategory.Copyrights },
				{ Species, TagCategory.Species },
				{ Characters, TagCategory.Characters },
				{ General, TagCategory.General },
				{ Meta, TagCategory.Meta },
				{ Invalid, TagCategory.Invalid },
				{ Lore, TagCategory.Lore },
			}) {
				item.Key.Fill = new SolidColorBrush(E621Tag.GetCatrgoryColor(item.Value, isDark));
			}
		}
	}
}
