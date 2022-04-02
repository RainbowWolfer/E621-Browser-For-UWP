﻿using System;
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
				ConfirmText.Text = "Add";
			} else {
				ConfirmIcon.Glyph = "\uE001";
				ConfirmText.Text = "Confirm";
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
				HintText.Text = $"Cannot be the Same as Before";
				HintPanel.Visibility = Visibility.Visible;
				return;
			} else if(existedNames.Contains(input)) {
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