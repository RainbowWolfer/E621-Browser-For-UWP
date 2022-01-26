using System;
using System.Collections.Generic;
using System.Diagnostics;
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
	public sealed partial class CustomHostInputDialog: UserControl {
		public string InputText => Input.Text;
		public bool Confirm { get; private set; } = false;
		private readonly ContentDialog dialog;
		private readonly string origin;

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
			} else if(Input.Text == origin) {
				Acceptbutton.IsEnabled = false;
			} else if(Uri.CheckHostName(Input.Text) == UriHostNameType.Unknown) {
				Acceptbutton.IsEnabled = false;
			} else {
				Acceptbutton.IsEnabled = true;
			}
			Debug.WriteLine(Uri.CheckHostName(Input.Text));
		}

		private void Acceptbutton_Tapped(object sender, TappedRoutedEventArgs e) {
			Confirm = true;
			dialog.Hide();
		}

		private void BackButton_Tapped(object sender, TappedRoutedEventArgs e) {
			Confirm = false;
			dialog.Hide();
		}
	}
}
