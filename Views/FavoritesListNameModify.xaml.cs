using E621Downloader.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace E621Downloader.Views {
	public sealed partial class FavoritesListNameModify: UserControl {
		public bool Confirm { get; private set; }
		public string Input => InputBox.Text;
		private readonly ContentDialog dialog;
		private readonly string[] existedNames;
		private readonly string original;
		public FavoritesListNameModify(bool isAdd, ContentDialog dialog, IEnumerable<string> existedNames, string original = "") {
			this.InitializeComponent();
			this.dialog = dialog;
			this.existedNames = existedNames.ToArray();
			this.original = original;
			if(isAdd) {
				ConfirmIcon.Glyph = "\uE109";
				ConfirmText.Text = "Add".Language();
			} else {
				ConfirmIcon.Glyph = "\uE001";
				ConfirmText.Text = "Confirm".Language();
			}
			if(!string.IsNullOrWhiteSpace(original)) {
				InputBox.Text = original;
				InputBox.SelectionStart = original.Length;
			}
		}

		private void BackButton_Tapped(object sender, TappedRoutedEventArgs e) {
			Confirm = false;
			dialog.Hide();
		}

		private void AddButton_Tapped(object sender, TappedRoutedEventArgs e) {
			Confirm = true;
			dialog.Hide();
		}

		private void InputBox_TextChanged(object sender, TextChangedEventArgs e) {
			string input = InputBox.Text.Trim();
			AddButton.IsEnabled = false;
			if(string.IsNullOrWhiteSpace(input)) {
				return;
			}
			if(!string.IsNullOrWhiteSpace(original) && input == original) {
				HintText.Text = $"Cannot be the Same as Before".Language();
				HintPanel.Visibility = Visibility.Visible;
				return;
			} else if(existedNames.Contains(input)) {
				HintText.Text = $"\"{input}\" " + "Already existed".Language();
				HintPanel.Visibility = Visibility.Visible;
				return;
			} else {
				HintPanel.Visibility = Visibility.Collapsed;
			}
			AddButton.IsEnabled = true;
		}
	}
}
