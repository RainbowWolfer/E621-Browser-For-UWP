using E621Downloader.Models.Networks;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
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
	public sealed partial class CustomHostInputDialog: UserControl {
		public string InputText => Input.Text;
		public bool Confirm { get; private set; } = false;
		private readonly ContentDialog dialog;
		private readonly string origin;

		private CancellationTokenSource cts;

		public CustomHostInputDialog(ContentDialog dialog, string origin) {
			this.InitializeComponent();
			this.dialog = dialog;
			Acceptbutton.IsEnabled = false;
			this.origin = origin;
			Input.Text = origin;
			Input.SelectionStart = origin.Length;
		}

		private void Input_TextChanged(object sender, TextChangedEventArgs e) {
			if(string.IsNullOrWhiteSpace(Input.Text)) {
				Acceptbutton.IsEnabled = false;
				HintPanel.Visibility = Visibility.Visible;
				HintText.Text = "Empty input is not allowed";
			} else if(Input.Text == origin) {
				Acceptbutton.IsEnabled = false;
				HintPanel.Visibility = Visibility.Visible;
				HintText.Text = "It is the same as before";
			} else if(Uri.CheckHostName(Input.Text) == UriHostNameType.Unknown) {
				Acceptbutton.IsEnabled = false;
				HintPanel.Visibility = Visibility.Visible;
				HintText.Text = "It is not a valid host name";
			} else {
				Acceptbutton.IsEnabled = true;
				HintPanel.Visibility = Visibility.Collapsed;
			}
		}

		private async void Acceptbutton_Tapped(object sender, TappedRoutedEventArgs e) {
			if(cts != null) {
				cts.Cancel();
			}
			cts = new CancellationTokenSource();
			Acceptbutton.IsEnabled = false;
			ButtonContent.Visibility = Visibility.Collapsed;
			LoadingPanel.Visibility = Visibility.Visible;
			HttpResult<string> result = await Data.ReadURLAsync($"http://{Input.Text}/posts?limit=1", cts.Token);
			if(result.Result == HttpResultType.Success) {
				Confirm = true;
				dialog.Hide();
			} else {
				ButtonContent.Visibility = Visibility.Visible;
				LoadingPanel.Visibility = Visibility.Collapsed;
				HintPanel.Visibility = Visibility.Visible;
				HintText.Text = "Host Name is not supported";
				Acceptbutton.IsEnabled = true;
			}
		}

		private void BackButton_Tapped(object sender, TappedRoutedEventArgs e) {
			if(cts != null) {
				cts.Cancel();
			}
			Confirm = false;
			dialog.Hide();
		}
	}
}
