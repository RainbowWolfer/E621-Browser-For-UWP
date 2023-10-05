using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace YiffBrowser.Views.Controls.Common {
	public sealed partial class HeaderIconTextButton : UserControl {



		public string Glyph {
			get => (string)GetValue(GlyphProperty);
			set => SetValue(GlyphProperty, value);
		}

		public static readonly DependencyProperty GlyphProperty = DependencyProperty.Register(
			nameof(Glyph),
			typeof(string),
			typeof(HeaderIconTextButton),
			new PropertyMetadata(null)
		);



		public string SVGUri {
			get => (string)GetValue(SVGUriProperty);
			set => SetValue(SVGUriProperty, value);
		}

		public static readonly DependencyProperty SVGUriProperty = DependencyProperty.Register(
			nameof(SVGUri),
			typeof(string),
			typeof(HeaderIconTextButton),
			new PropertyMetadata(null)
		);




		public string Title {
			get => (string)GetValue(TitleProperty);
			set => SetValue(TitleProperty, value);
		}

		public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(
			nameof(Title),
			typeof(string),
			typeof(HeaderIconTextButton),
			new PropertyMetadata(null)
		);




		public string Text {
			get => (string)GetValue(TextProperty);
			set => SetValue(TextProperty, value);
		}

		public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
			nameof(Text),
			typeof(string),
			typeof(HeaderIconTextButton),
			new PropertyMetadata(null)
		);



		public Uri NavigateURL {
			get => (Uri)GetValue(NavigateURLProperty);
			set => SetValue(NavigateURLProperty, value);
		}

		public static readonly DependencyProperty NavigateURLProperty = DependencyProperty.Register(
			nameof(NavigateURL),
			typeof(Uri),
			typeof(HeaderIconTextButton),
			new PropertyMetadata(null)
		);




		public double TitleWidth {
			get => (double)GetValue(TitleWidthProperty);
			set => SetValue(TitleWidthProperty, value);
		}

		public static readonly DependencyProperty TitleWidthProperty = DependencyProperty.Register(
			nameof(TitleWidth),
			typeof(double),
			typeof(HeaderIconTextButton),
			new PropertyMetadata(120d)
		);


		public HeaderIconTextButton() {
			this.InitializeComponent();
		}
	}
}
