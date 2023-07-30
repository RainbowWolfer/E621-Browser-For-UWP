using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using YiffBrowser.Models.E621;

namespace YiffBrowser.Views.Controls.CustomControls {
	public class FileTypeFontIcon : FontIcon {

		public FileType FileType {
			get => (FileType)GetValue(FileTypeProperty);
			set => SetValue(FileTypeProperty, value);
		}

		public static readonly DependencyProperty FileTypeProperty = DependencyProperty.Register(
			nameof(FileType),
			typeof(FileType),
			typeof(FileTypeFontIcon),
			new PropertyMetadata(FileType.Anim, OnFileTypeChanged)
		);

		private static void OnFileTypeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
			if (d is not FileTypeFontIcon icon) {
				return;
			}

			icon.Glyph = (FileType)e.NewValue switch {
				FileType.Png => "\uE91B",
				FileType.Jpg => "\uEB9F",
				FileType.Gif => "\uF4A9",
				FileType.Webm => "\uE116",
				FileType.Anim => "\uE13B",
				_ => "",
			};
		}
	}
}
