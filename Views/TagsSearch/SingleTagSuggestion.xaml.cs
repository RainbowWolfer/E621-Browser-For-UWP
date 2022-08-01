using E621Downloader.Models.E621;
using E621Downloader.Models.Utilities;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace E621Downloader.Views.TagsSearch {
	public sealed partial class SingleTagSuggestion: UserControl {
		private readonly E621AutoComplete autoComplete;
		private bool isSelected = false;

		public int Category => autoComplete.category;
		public string CompleteName => autoComplete.name;
		public string AntecedentName => autoComplete.antecedent_name;
		public string Count => Methods.NumberToK(autoComplete.post_count);

		public Color MainColor { get; private set; }
		public SolidColorBrush MainBrush => new(MainColor);

		public bool IsSelected {
			get => isSelected;
			set {
				isSelected = value;
				RectangleWidthAnimation.From = CategoryRectangle.Width;
				RectangleWidthAnimation.To = value ? 8 : 4;
				SelectionStoryboard.Begin();

				if(isSelected) {
					RootPanel.BorderBrush = MainBrush;
					RootPanel.BorderThickness = new Thickness(1.5);
					RootPanel.CornerRadius = new CornerRadius(5);
					CategoryRectangle.RadiusX = 0;
					CategoryRectangle.RadiusY = 0;
				} else {
					RootPanel.BorderBrush = new SolidColorBrush(Colors.Transparent);
					RootPanel.BorderThickness = new Thickness(0);
					RootPanel.CornerRadius = new CornerRadius(0);
					CategoryRectangle.RadiusX = 2;
					CategoryRectangle.RadiusY = 2;
				}
			}
		}

		public SingleTagSuggestion(E621AutoComplete autoComplete) {
			this.InitializeComponent();
			this.autoComplete = autoComplete;
			if(string.IsNullOrWhiteSpace(AntecedentName)) {
				Arrow.Visibility = Visibility.Collapsed;
				ToText.Visibility = Visibility.Collapsed;
				FromText.Text = CompleteName;
			} else {
				FromText.Text = AntecedentName;
				ToText.Text = CompleteName;
			}
			var catrgory = E621Tag.GetTagCategory(Category);
			bool isDark = App.GetApplicationTheme() == ApplicationTheme.Dark;
			MainColor = E621Tag.GetCatrgoryColor(catrgory, isDark);
		}

		public void SetSelected(bool value) {
			IsSelected = value;
		}

	}
}
