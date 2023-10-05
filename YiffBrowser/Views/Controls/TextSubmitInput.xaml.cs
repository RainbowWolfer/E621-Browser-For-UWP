using System;
using System.Linq;
using System.Windows.Input;
using Windows.System;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using YiffBrowser.Helpers;

namespace YiffBrowser.Views.Controls {
	public sealed partial class TextSubmitInput : UserControl {
		public string Text {
			get => (string)GetValue(TextProperty);
			set => SetValue(TextProperty, value);
		}

		public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
			nameof(Text),
			typeof(string),
			typeof(TextSubmitInput),
			new PropertyMetadata(string.Empty, OnTextChanged)
		);

		private static void OnTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
			if (d is TextSubmitInput input) {
				input.Update();
			}
		}

		private string HintText {
			get => (string)GetValue(HintTextProperty);
			set => SetValue(HintTextProperty, value);
		}

		private static readonly DependencyProperty HintTextProperty = DependencyProperty.Register(
			nameof(HintText),
			typeof(string),
			typeof(TextSubmitInput),
			new PropertyMetadata(string.Empty)
		);



		public string[] Exists {
			get => (string[])GetValue(ExistsProperty);
			set => SetValue(ExistsProperty, value);
		}

		public static readonly DependencyProperty ExistsProperty = DependencyProperty.Register(
			nameof(Exists),
			typeof(string[]),
			typeof(TextSubmitInput),
			new PropertyMetadata(Array.Empty<string>())
		);

		private void Update() {
			if (Text.IsBlank()) {
				HintText = "Cannot be empty";
			} else if (Exists?.Contains(Text) ?? false) {
				HintText = "Already exists";
			} else {
				HintText = string.Empty;
			}

			TextBoxBorderBrush = HintText.IsBlank() ? YiffApp.TextBoxDefaultBorderBrush : new SolidColorBrush(Colors.Red);
		}


		public Brush TextBoxBorderBrush {
			get => (Brush)GetValue(TextBoxBorderBrushProperty);
			set => SetValue(TextBoxBorderBrushProperty, value);
		}

		public static readonly DependencyProperty TextBoxBorderBrushProperty = DependencyProperty.Register(
			nameof(TextBoxBorderBrush),
			typeof(Brush),
			typeof(TextSubmitInput),
			new PropertyMetadata(YiffApp.TextBoxDefaultBorderBrush)
		);



		public ICommand SubmitCommand {
			get => (ICommand)GetValue(SubmitCommandProperty);
			set => SetValue(SubmitCommandProperty, value);
		}

		public static readonly DependencyProperty SubmitCommandProperty = DependencyProperty.Register(
			nameof(SubmitCommand),
			typeof(ICommand),
			typeof(TextSubmitInput),
			new PropertyMetadata(null)
		);




		public TextSubmitInput() {
			this.InitializeComponent();
		}

		private void TextBox_PreviewKeyDown(object sender, KeyRoutedEventArgs e) {
			if (e.Key == VirtualKey.Enter && HintText.IsBlank()) {
				SubmitCommand.Execute(Text);
			}
		}
	}
}
