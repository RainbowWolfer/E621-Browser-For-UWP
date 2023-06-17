using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace YiffBrowser.Views.Controls.LoadingViews {
	public sealed partial class LoadingRingWithTextBelow : UserControl {

		public bool IsIndeterminate {
			get => (bool)GetValue(IsIndeterminateProperty);
			set => SetValue(IsIndeterminateProperty, value);
		}

		public static readonly DependencyProperty IsIndeterminateProperty = DependencyProperty.Register(
			nameof(IsIndeterminate),
			typeof(bool),
			typeof(LoadingRingWithTextBelow),
			new PropertyMetadata(true)
		);



		public int Progress {
			get => (int)GetValue(ProgressProperty);
			set => SetValue(ProgressProperty, value);
		}

		public static readonly DependencyProperty ProgressProperty = DependencyProperty.Register(
			nameof(Progress),
			typeof(int),
			typeof(LoadingRingWithTextBelow),
			new PropertyMetadata(0, OnProgressChanged)
		);

		private static void OnProgressChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
			if (d is LoadingRingWithTextBelow view) {
				view.IsIndeterminate = false;
			}
		}



		public string Text {
			get => (string)GetValue(TextProperty);
			set => SetValue(TextProperty, value);
		}

		public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
			nameof(Text),
			typeof(string),
			typeof(LoadingRingWithTextBelow),
			new PropertyMetadata(string.Empty)
		);



		public LoadingRingWithTextBelow() {
			this.InitializeComponent();
		}

		public LoadingRingWithTextBelow(string text) : this() {
			Text = text;
		}
	}
}
