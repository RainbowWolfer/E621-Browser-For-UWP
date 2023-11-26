using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace YiffBrowser.Views.Controls.CustomControls {
	public class SettingCardControl : ContentControl {


		public string Title {
			get => (string)GetValue(TitleProperty);
			set => SetValue(TitleProperty, value.ToUpper());
		}

		public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(
			nameof(Title),
			typeof(string),
			typeof(SettingCardControl),
			new PropertyMetadata(string.Empty)
		);


	}
}
