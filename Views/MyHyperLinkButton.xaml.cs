using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace E621Downloader.Views {
	public sealed partial class MyHyperLinkButton: UserControl {
		private string url;
		public MyHyperLinkButton(string url) {
			this.InitializeComponent();
			this.url = url;
			URLText.Text = url;
		}

		private async void Button_Tapped(object sender, TappedRoutedEventArgs e) {
			try {
				if(!await Launcher.LaunchUriAsync(new Uri(url))) {
					await MainPage.CreatePopupDialog("Error", "Could not Open Default Browser");
				}
			} catch {
				//if(!url.ToLower().Trim().StartsWith("http")) {

				//}
			}
		}

		private void Button_RightTapped(object sender, RightTappedRoutedEventArgs e) {
			MenuFlyout flyout = new() {
				Placement = FlyoutPlacementMode.Auto,
			};
			MenuFlyoutItem item_copy = new() {
				Icon = new FontIcon() { Glyph = "\uE8C8" },
				Text = "Copy",
			};
			flyout.Items.Add(item_copy);
			flyout.ShowAt(sender as Button);
		}
	}
}
