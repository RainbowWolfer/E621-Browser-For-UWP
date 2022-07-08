using E621Downloader.Models.Posts;
using System;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;

namespace E621Downloader.Views {
	public sealed partial class RecentViewView: UserControl {
		private string value;

		public string Value {
			get => value;
			set {
				this.value = value;
				PostIDTextShadow.Text = $"#{value}";
				PostIDText.Text = $"#{value}";
			}
		}

		public string DateTime { get; set; }
		public Action<string> OnDelete { get; set; }
		public Action<string> OnClick { get; set; }

		public bool HasLoaded { get; private set; } = false;

		public RecentViewView(string value) {
			this.InitializeComponent();
			this.Value = value;
			TextBorder.Translation += new Vector3(0, 0, 32);
		}

		public async Task StartLoading(CancellationToken token) {
			LoadingBar.Visibility = Visibility.Visible;
			Post post = await Post.GetPostByIDAsync(Value, token);
			if(post == null) {
				LoadingBar.ShowError = true;
				return;
			}
			string url = post.sample.url ?? post.preview.url ?? post.file.url;
			if(url == null) {
				LoadingBar.ShowError = true;
				return;
			}
			ImageView.Source = new BitmapImage(new Uri(url));
			ImageView.ImageOpened += (s, e) => {
				LoadingBar.Visibility = Visibility.Collapsed;
				HasLoaded = true;
			};
			ImageView.ImageFailed += (s, e) => {
				LoadingBar.Visibility = Visibility.Collapsed;
			};
		}

		private void MenuItemCopy_Click(object sender, RoutedEventArgs e) {
			var dataPackage = new DataPackage() {
				RequestedOperation = DataPackageOperation.Copy
			};
			dataPackage.SetText(Value);
			Clipboard.SetContent(dataPackage);
		}

		private void MenuItemDelete_Click(object sender, RoutedEventArgs e) {
			OnDelete?.Invoke(Value);
		}

		private void Button_Click(object sender, RoutedEventArgs e) {
			OnClick?.Invoke(Value);
		}
	}
}
