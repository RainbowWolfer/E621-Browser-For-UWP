using System.Windows.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace YiffBrowser.Views.Controls.PictureViews {
	public sealed partial class ScoreButton : UserControl {


		public bool UpOrDown {
			get => (bool)GetValue(UpOrDownProperty);
			set => SetValue(UpOrDownProperty, value);
		}

		public static readonly DependencyProperty UpOrDownProperty = DependencyProperty.Register(
			nameof(UpOrDown),
			typeof(bool),
			typeof(ScoreButton),
			new PropertyMetadata(false)
		);

		public bool IsLoading {
			get => (bool)GetValue(IsLoadingProperty);
			set => SetValue(IsLoadingProperty, value);
		}

		public static readonly DependencyProperty IsLoadingProperty = DependencyProperty.Register(
			nameof(IsLoading),
			typeof(bool),
			typeof(ScoreButton),
			new PropertyMetadata(false)
		);

		public bool IsChecked {
			get => (bool)GetValue(IsCheckedProperty);
			set => SetValue(IsCheckedProperty, value);
		}

		public static readonly DependencyProperty IsCheckedProperty = DependencyProperty.Register(
			nameof(IsChecked),
			typeof(bool),
			typeof(ScoreButton),
			new PropertyMetadata(false)
		);





		public ICommand Command {
			get => (ICommand)GetValue(CommandProperty);
			set => SetValue(CommandProperty, value);
		}

		public static readonly DependencyProperty CommandProperty = DependencyProperty.Register(
			nameof(Command),
			typeof(ICommand),
			typeof(ScoreButton),
			new PropertyMetadata(null)
		);







		public ScoreButton() {
			this.InitializeComponent();
		}
	}
}
