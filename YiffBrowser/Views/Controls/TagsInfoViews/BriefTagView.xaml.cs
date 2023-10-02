using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using YiffBrowser.Helpers;
using YiffBrowser.Models.E621;

namespace YiffBrowser.Views.Controls.TagsInfoViews {
	public sealed partial class BriefTagView : UserControl {
		public E621Tag E621Tag {
			get => (E621Tag)GetValue(MyPropertyProperty);
			set => SetValue(MyPropertyProperty, value);
		}

		public static readonly DependencyProperty MyPropertyProperty = DependencyProperty.Register(
			nameof(E621Tag),
			typeof(E621Tag),
			typeof(BriefTagView),
			new PropertyMetadata(null, OnE621TagChanged)
		);

		private static void OnE621TagChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
			if (d is BriefTagView view && e.NewValue is E621Tag value) {
				view.TitleText.Text = value.Name;
				view.PostCountText.Text = value.PostCount.NumberToK();
				view.CategoryText.Text = E621Tag.GetCategory(value.Category);
				view.CategoryRectangle.Fill = new SolidColorBrush(E621Tag.GetCategoryColor(value.Category));
			}
		}

		public BriefTagView() {
			this.InitializeComponent();
		}
	}
}
