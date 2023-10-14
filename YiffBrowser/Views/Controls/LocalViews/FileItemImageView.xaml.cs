using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using YiffBrowser.Views.Pages.E621;


namespace YiffBrowser.Views.Controls.LocalViews {
	public sealed partial class FileItemImageView : UserControl {

		public FileItem FileItem {
			get => (FileItem)GetValue(FileItemProperty);
			set => SetValue(FileItemProperty, value);
		}

		public static readonly DependencyProperty FileItemProperty = DependencyProperty.Register(
			nameof(FileItem),
			typeof(FileItem),
			typeof(FileItemImageView),
			new PropertyMetadata(null)
		);

		public FileItemImageView() {
			this.InitializeComponent();
		}
	}
}
