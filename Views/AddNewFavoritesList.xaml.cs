using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace E621Downloader.Views {
	public sealed partial class AddNewFavoritesList: UserControl {
		public bool IsAdd { get; private set; }
		public string Input => InputBox.Text;
		private readonly ContentDialog dialog;
		private readonly string[] existedNames;
		public AddNewFavoritesList(ContentDialog dialog, IEnumerable<string> existedNames) {
			this.InitializeComponent();
			this.dialog = dialog;
			this.existedNames = existedNames.ToArray();
		}

		private void BackButton_Tapped(object sender, TappedRoutedEventArgs e) {
			IsAdd = false;
			dialog.Hide();
		}

		private void AddButton_Tapped(object sender, TappedRoutedEventArgs e) {
			IsAdd = true;
			dialog.Hide();
		}

		private void InputBox_TextChanged(object sender, TextChangedEventArgs e) {
			string input = InputBox.Text.Trim();
			AddButton.IsEnabled = false;
			if(string.IsNullOrWhiteSpace(input)) {
				return;
			}
			if(existedNames.Contains(input)) {
				HintText.Text = $"\"{input}\" Already Exists";
				HintPanel.Visibility = Visibility.Visible;
				return;
			} else {
				HintPanel.Visibility = Visibility.Collapsed;
			}
			AddButton.IsEnabled = true;
		}
	}
}
