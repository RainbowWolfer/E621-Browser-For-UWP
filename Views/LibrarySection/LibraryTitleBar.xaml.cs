using E621Downloader.Pages.LibrarySection;
using System;
using System.Linq;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace E621Downloader.Views.LibrarySection {
	public sealed partial class LibraryTitleBar: UserControl {
		private bool showExpanderButton = true;
		private string title = "Tag Name Here";
		private bool showLocalChangedHintText = false;
		private OrderEnum asecDesOrder = OrderEnum.Desc;
		private OrderType orderType = OrderType.Date;
		private bool isExpanded = true;
		private bool showExplorerButton = true;
		private bool enableRefreshButton = true;
		private bool isFolderBar = true;
		private bool enableSortButtons;
		private bool enableClearEmptyFile;

		public event Action<bool> OnExpandedChanged;
		public event Action<string> OnSearchSubmit;
		public event Action<OrderType> OnOrderTypeChanged;
		public event Action<OrderEnum> OnAsecDesOrderChanged;
		public event Action OnRefresh;
		public event Action OnExplorerClick;
		public event Action<VirtualKey> OnSearchInput;
		public event Action OnEmptyFileClear;

		public bool ShowExplorerButton {
			get => showExplorerButton;
			set {
				showExplorerButton = value;
				OpenExplorerButton.Visibility = showExplorerButton ? Visibility.Visible : Visibility.Collapsed;
			}
		}

		public bool ShowExpanderButton {
			get => showExpanderButton;
			set {
				showExpanderButton = value;
				ExpanderButton.Visibility = showExpanderButton ? Visibility.Visible : Visibility.Collapsed;
			}
		}

		public bool ShowLocalChangedHintText {
			get => showLocalChangedHintText;
			set {
				showLocalChangedHintText = value;
				LocalChangedHintText.Visibility = showLocalChangedHintText ? Visibility.Visible : Visibility.Collapsed;
			}
		}

		public bool EnableRefreshButton {
			get => enableRefreshButton;
			set {
				enableRefreshButton = value;
				RefreshButton.IsEnabled = enableRefreshButton;
			}
		}

		public string Title {
			get => title;
			set {
				title = value;
				TitleTextBlock.Text = title;
			}
		}

		public OrderEnum AsecDesOrder {
			get => asecDesOrder;
			set {
				asecDesOrder = value;
				if(asecDesOrder == OrderEnum.Desc) {
					AsecDesIcon.Glyph = "\uE64F";
				} else {
					AsecDesIcon.Glyph = "\uE650";
				}
				OnAsecDesOrderChanged?.Invoke(asecDesOrder);
			}
		}

		public OrderType OrderType {
			get => orderType;
			set {
				orderType = value;
				OnOrderTypeChanged?.Invoke(value);
			}
		}

		public bool IsExpanded {
			get => isExpanded;
			set {
				isExpanded = value;
				ExpanderIcon.Glyph = isExpanded ? "\uE0E4" : "\uE0E5";
				ExpanderButton.IsChecked = isExpanded;
				OnExpandedChanged?.Invoke(isExpanded);
			}
		}

		public bool IsFolderBar {
			get => isFolderBar;
			set {
				isFolderBar = value;
				if(isFolderBar) {
					SizeComboItem.Visibility = Visibility.Collapsed;
					TypeComboItem.Visibility = Visibility.Collapsed;
					ScoreComboItem.Visibility = Visibility.Collapsed;
					if(OrderComboBox.SelectedItem == SizeComboItem ||
						OrderComboBox.SelectedItem == TypeComboItem ||
						OrderComboBox.SelectedItem == ScoreComboItem) {
						OrderComboBox.SelectedItem = DateComboItem;
					}
				} else {
					SizeComboItem.Visibility = Visibility.Visible;
					TypeComboItem.Visibility = Visibility.Visible;
					ScoreComboItem.Visibility = Visibility.Visible;
				}
			}
		}

		public bool EnableSortButtons {
			get => enableSortButtons;
			set {
				enableSortButtons = value;
				OrderComboBox.IsEnabled = enableSortButtons;
				AsecDesButton.IsEnabled = enableSortButtons;
			}
		}

		public bool EnableClearEmptyFile {
			get => enableClearEmptyFile;
			set {
				enableClearEmptyFile = value;
				ClearEmptyFileItem.IsEnabled = enableClearEmptyFile;
			}
		}

		public LibraryTitleBar() {
			this.InitializeComponent();
		}

		public void SetOrderItem(OrderEnum order, OrderType orderType) {
			this.asecDesOrder = order;
			if(order == OrderEnum.Desc) {
				AsecDesIcon.Glyph = "\uE64F";
			} else {
				AsecDesIcon.Glyph = "\uE650";
			}
			this.orderType = orderType;
			OrderComboBox.SelectedItem = orderType switch {
				OrderType.Name => NameComboItem,
				OrderType.Date => DateComboItem,
				OrderType.Size => SizeComboItem,
				OrderType.Type => TypeComboItem,
				OrderType.NumberOfFiles => new NotImplementedException(),
				OrderType.Score => ScoreComboItem,
				_ => throw new Exception($"Type not found {OrderType}"),
			};
		}

		private void AsecDesButton_Click(object sender, RoutedEventArgs e) {
			if(AsecDesOrder == OrderEnum.Desc) {
				AsecDesOrder = OrderEnum.Asc;
			} else {
				AsecDesOrder = OrderEnum.Desc;
			}
		}

		private void OrderComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) {
			if(e.AddedItems == null || e.AddedItems.Count == 0) {
				return;
			}
			object first = e.AddedItems.FirstOrDefault();
			if(first == DateComboItem) {
				OrderType = OrderType.Date;
			} else if(first == NameComboItem) {
				OrderType = OrderType.Name;
			} else if(first == SizeComboItem) {
				OrderType = OrderType.Size;
			} else if(first == TypeComboItem) {
				OrderType = OrderType.Type;
			} else if(first == ScoreComboItem) {
				OrderType = OrderType.Score;
			} else {
				return;
			}
		}

		private void MySearchBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args) {
			OnSearchSubmit?.Invoke(args.QueryText);
		}

		private void RefreshButton_Click(object sender, RoutedEventArgs e) {
			OnRefresh?.Invoke();
		}

		private void OpenExplorerButton_Click(object sender, RoutedEventArgs e) {
			OnExplorerClick?.Invoke();
		}

		private void MySearchBox_PreviewKeyDown(object sender, KeyRoutedEventArgs e) {
			OnSearchInput?.Invoke(e.Key);
		}

		private void ExpanderButton_Click(object sender, RoutedEventArgs e) {
			IsExpanded = !IsExpanded;
		}

		private void ClearEmptyFileItem_Click(object sender, RoutedEventArgs e) {
			OnEmptyFileClear?.Invoke();
		}
	}
}
