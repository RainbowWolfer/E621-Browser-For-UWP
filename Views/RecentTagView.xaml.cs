using System;
using Windows.ApplicationModel.DataTransfer;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace E621Downloader.Views {
	public sealed partial class RecentTagView: UserControl {
		private string value;
		private string dateTime;

		public Action<string> OnDelete;
		public Action<string> OnClick;

		public string Value {
			get => value;
			set {
				this.value = value;
				ValueText.Text = value;
			}
		}

		public string DateTime {
			get => dateTime;
			set {
				dateTime = value;
				TimeText.Text = value;
			}
		}

		public RecentTagView() {
			this.InitializeComponent();
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
