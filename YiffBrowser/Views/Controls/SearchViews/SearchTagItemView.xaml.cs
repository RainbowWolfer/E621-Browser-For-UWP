using Prism.Mvvm;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using YiffBrowser.Helpers;
using YiffBrowser.Models.E621;

namespace YiffBrowser.Views.Controls.SearchViews {
	public sealed partial class SearchTagItemView : UserControl {

		public E621AutoComplete AutoComplete {
			get => (E621AutoComplete)GetValue(MyPropertyProperty);
			set => SetValue(MyPropertyProperty, value);
		}

		public static readonly DependencyProperty MyPropertyProperty = DependencyProperty.Register(
			nameof(AutoComplete),
			typeof(E621AutoComplete),
			typeof(SearchTagItemView),
			new PropertyMetadata(null, OnAutoCompleteChanged)
		);

		private static void OnAutoCompleteChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
			if (d is SearchTagItemView view) {
				view.ViewModel.AutoComplete = (E621AutoComplete)e.NewValue;
			}
		}

		public bool IsSelected {
			get => (bool)GetValue(IsSelectedProperty);
			set => SetValue(IsSelectedProperty, value);
		}

		public static readonly DependencyProperty IsSelectedProperty = DependencyProperty.Register(
			nameof(IsSelected),
			typeof(bool),
			typeof(SearchTagItemView),
			new PropertyMetadata(false, OnIsSelectedChanged)
		);

		private static void OnIsSelectedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
			if (d is SearchTagItemView view) {
				view.ViewModel.IsSelected = (bool)e.NewValue;

				view.RectangleWidthAnimation.From = view.CategoryRectangle.Width;
				view.RectangleWidthAnimation.To = view.ViewModel.IsSelected ? 8 : 4;
				view.SelectionStoryboard.Begin();
			}
		}

		public SearchTagItemView() {
			this.InitializeComponent();
		}
	}

	public class SearchTagItemViewModel : BindableBase {
		private E621AutoComplete autoComplete;
		private Color mainColor;

		private string fromName;
		private string toName;
		private string count;

		private bool isSelected;

		public E621AutoComplete AutoComplete {
			get => autoComplete;
			set => SetProperty(ref autoComplete, value, OnAutoCompleteChanged);
		}

		public Color MainColor {
			get => mainColor;
			set => SetProperty(ref mainColor, value);
		}

		public string FromName {
			get => fromName;
			set => SetProperty(ref fromName, value);
		}
		public string ToName {
			get => toName;
			set => SetProperty(ref toName, value);
		}
		public string Count {
			get => count;
			set => SetProperty(ref count, value);
		}

		public bool IsSelected {
			get => isSelected;
			set => SetProperty(ref isSelected, value);
		}

		private void OnAutoCompleteChanged() {
			if (AutoComplete == null) {
				return;
			}

			Count = AutoComplete.post_count.NumberToK();
			if (AutoComplete.antecedent_name.IsBlank()) {
				FromName = AutoComplete.name;
				ToName = string.Empty;
			} else {
				FromName = AutoComplete.antecedent_name;
				ToName = AutoComplete.name;
			}

			MainColor = E621Tag.GetCategoryColor(AutoComplete.category);
		}

	}
}
