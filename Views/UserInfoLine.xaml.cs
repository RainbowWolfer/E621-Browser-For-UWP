using Windows.UI.Xaml.Controls;

namespace E621Downloader.Views {
	public sealed partial class UserInfoLine: UserControl {
		private string titleText;
		private string contentText;

		public string TitleText {
			get => titleText;
			set {
				titleText = value;
				TitleTextBlock.Text = TitleText;
			}
		}
		public string ContentText {
			get => contentText;
			set {
				contentText = value;
				ContentTextBlock.Text = ContentText;
			}
		}
		public UserInfoLine() {
			this.InitializeComponent();
		}
		public UserInfoLine(string title, string content) {
			this.InitializeComponent();
			TitleText = title;
			ContentText = content;
		}
	}
}
