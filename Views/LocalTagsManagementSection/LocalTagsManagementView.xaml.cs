using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace E621Downloader.Views.LocalTagsManagementSection {
	public sealed partial class LocalTagsManagementView: UserControl {
		private readonly ContentDialog dialog;
		public bool HandleConfirm { get; private set; }
		private readonly ObservableCollection<string> input = new();

		public LocalTagsManagementView(ContentDialog dialog, string[] initialTags) {
			this.InitializeComponent();
			this.dialog = dialog;
			HandleConfirm = false;

			MyInputBox.Text = string.Join(" ", initialTags);
		}

		public List<string> GetResult() => input.ToList();

		private void ConfirmButton_Tapped(object sender, TappedRoutedEventArgs e) {
			HandleConfirm = true;
			dialog.Hide();
		}

		private void BackButton_Tapped(object sender, TappedRoutedEventArgs e) {
			dialog.Hide();
		}

		private void CloseButton_Tapped(object sender, TappedRoutedEventArgs e) {
			string tag = (sender as Button).Tag as string;
			if(input.Contains(tag)) {
				input.Remove(tag);
				MyInputBox.Text = MyInputBox.Text.Replace(tag, "").Replace("  ", " ");
			}
		}

		private void MyInputBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args) {
			input.Clear();
			foreach(string item in sender.Text.Trim().Split(" ").Where(s => !string.IsNullOrEmpty(s))) {
				input.Add(item);
			}
		}


	}

}
