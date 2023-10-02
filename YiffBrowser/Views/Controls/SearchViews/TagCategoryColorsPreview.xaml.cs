using System.Collections.Generic;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;
using YiffBrowser.Models.E621;

namespace YiffBrowser.Views.Controls.SearchViews {
	public sealed partial class TagCategoryColorsPreview : UserControl {
		public TagCategoryColorsPreview() {
			this.InitializeComponent();
			foreach (var item in new Dictionary<Rectangle, E621TagCategory>() {
				{ Artists, E621TagCategory.Artists },
				{ Copyrights, E621TagCategory.Copyrights },
				{ Species, E621TagCategory.Species },
				{ Characters, E621TagCategory.Characters },
				{ General, E621TagCategory.General },
				{ Meta, E621TagCategory.Meta },
				{ Invalid, E621TagCategory.Invalid },
				{ Lore, E621TagCategory.Lore },
			}) {
				item.Key.Fill = new SolidColorBrush(E621Tag.GetCategoryColor(item.Value));
			}
		}
	}
}
