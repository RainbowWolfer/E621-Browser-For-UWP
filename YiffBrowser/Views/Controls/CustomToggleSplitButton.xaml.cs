using System.Windows.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;

namespace YiffBrowser.Views.Controls {
	public sealed partial class CustomToggleSplitButton : UserControl {

		public bool IsLoading {
			get => (bool)GetValue(IsLoadingProperty);
			set => SetValue(IsLoadingProperty, value);
		}

		public static readonly DependencyProperty IsLoadingProperty = DependencyProperty.Register(
			nameof(IsLoading),
			typeof(bool),
			typeof(CustomToggleSplitButton),
			new PropertyMetadata(false)
		);

		public string OnTitle {
			get => (string)GetValue(OnTitleProperty);
			set => SetValue(OnTitleProperty, value);
		}

		public static readonly DependencyProperty OnTitleProperty = DependencyProperty.Register(
			nameof(OnTitle),
			typeof(string),
			typeof(CustomToggleSplitButton),
			new PropertyMetadata(string.Empty)
		);

		public string OffTitle {
			get => (string)GetValue(OffTitleProperty);
			set => SetValue(OffTitleProperty, value);
		}

		public static readonly DependencyProperty OffTitleProperty = DependencyProperty.Register(
			nameof(OffTitle),
			typeof(string),
			typeof(CustomToggleSplitButton),
			new PropertyMetadata(string.Empty)
		);

		public string OnIcon {
			get => (string)GetValue(OnIconProperty);
			set => SetValue(OnIconProperty, value);
		}

		public static readonly DependencyProperty OnIconProperty = DependencyProperty.Register(
			nameof(OnIcon),
			typeof(string),
			typeof(CustomToggleSplitButton),
			new PropertyMetadata(string.Empty)
		);

		public string OffIcon {
			get => (string)GetValue(OffIconProperty);
			set => SetValue(OffIconProperty, value);
		}

		public static readonly DependencyProperty OffIconProperty = DependencyProperty.Register(
			nameof(OffIcon),
			typeof(string),
			typeof(CustomToggleSplitButton),
			new PropertyMetadata(string.Empty)
		);

		public bool IsOn {
			get => (bool)GetValue(IsOnProperty);
			set => SetValue(IsOnProperty, value);
		}

		public static readonly DependencyProperty IsOnProperty = DependencyProperty.Register(
			nameof(IsOn),
			typeof(bool),
			typeof(CustomToggleSplitButton),
			new PropertyMetadata(false)
		);

		public object SideToggleContent {
			get => GetValue(SideToggleContentProperty);
			set => SetValue(SideToggleContentProperty, value);
		}

		public static readonly DependencyProperty SideToggleContentProperty = DependencyProperty.Register(
			nameof(SideToggleContent),
			typeof(object),
			typeof(CustomToggleSplitButton),
			new PropertyMetadata(null, OnSideToggleContentChanged)
		);

		private static void OnSideToggleContentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
			if (d is not CustomToggleSplitButton view) {
				return;
			}

			view.SideFlyout.Content = new ContentPresenter() {
				Content = e.NewValue,
			};
		}

		public ICommand SideToggleClosingCommand {
			get => (ICommand)GetValue(SideToggleClosingCommandProperty);
			set => SetValue(SideToggleClosingCommandProperty, value);
		}

		public static readonly DependencyProperty SideToggleClosingCommandProperty = DependencyProperty.Register(
			nameof(SideToggleClosingCommand),
			typeof(ICommand),
			typeof(CustomToggleSplitButton),
			new PropertyMetadata(null)
		);

		public ICommand SideToggleOpeningCommand {
			get => (ICommand)GetValue(SideToggleOpeningCommandProperty);
			set => SetValue(SideToggleOpeningCommandProperty, value);
		}

		public static readonly DependencyProperty SideToggleOpeningCommandProperty = DependencyProperty.Register(
			nameof(SideToggleOpeningCommand),
			typeof(ICommand),
			typeof(CustomToggleSplitButton),
			new PropertyMetadata(null)
		);

		public Flyout SideFlyout { get; }
		public CustomToggleSplitButton() {
			this.InitializeComponent();

			SideFlyout = new Flyout() {
				Placement = FlyoutPlacementMode.Bottom,
			};

			SideFlyout.Closing += SideFlyout_Closing;
			SideFlyout.Opening += SideFlyout_Opening;
		}

		private void SideFlyout_Opening(object sender, object args) {
			SideToggleOpeningCommand?.Execute(args);
		}

		private void SideFlyout_Closing(FlyoutBase sender, FlyoutBaseClosingEventArgs args) {
			SideToggleClosingCommand?.Execute(args);
		}

		private void SideToggle_Click(object sender, RoutedEventArgs e) {
			SideFlyout.ShowAt((FrameworkElement)sender);
		}
	}
}
