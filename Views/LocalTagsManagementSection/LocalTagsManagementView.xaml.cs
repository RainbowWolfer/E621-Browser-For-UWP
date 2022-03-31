using E621Downloader.Models.Locals;
using E621Downloader.Pages;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace E621Downloader.Views.LocalTagsManagementSection {
	public sealed partial class LocalTagsManagementView: UserControl {
		private readonly ContentDialog dialog;
		public bool HandleConfirm { get; private set; }
		private readonly ObservableCollection<string> input = new ObservableCollection<string>();

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
