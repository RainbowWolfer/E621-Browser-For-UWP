using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

namespace E621Downloader.Views {
	public sealed partial class MyHyperLinkButton: UserControl {
		//twitter
		//www.twitter
		//pbs.twimg -> twitter
		//www.furaffinity
		//d.furaffinity
		//www.deviantart
		//deviantart
		//inkbunny
		//www.inkbunny

		//www.instagram
		//instagram

		//
		private readonly string url;

		public MyHyperLinkButton(string url) {
			this.InitializeComponent();
			this.url = url;
			URLText.Text = url;
			if(url.StartsWith("https://")) {
				url = url.Substring(8);
			} else if(url.StartsWith("http://")) {
				url = url.Substring(7);
			}
			if(url.Contains("tumblr")) {//something.tumblr.com
				IconImage.Source = new BitmapImage(new Uri("ms-appx:///Icons/tumblr-icon.png"));
			}
			if(url.StartsWith("twitter") || url.StartsWith("www.twitter") || url.StartsWith("pbs.twimg")) {
				IconImage.Source = new BitmapImage(new Uri("ms-appx:///Icons/Twitter-icon.png"));
			} else if(url.StartsWith("www.furaffinity") || url.StartsWith("furaffinity") || url.StartsWith("d.furaffinity")) {
				IconImage.Source = new BitmapImage(new Uri("ms-appx:///Icons/Furaffinity-icon.png"));
			} else if(url.StartsWith("www.deviantart") || url.StartsWith("deviantart")) {
				IconImage.Source = new BitmapImage(new Uri("ms-appx:///Icons/DeviantArt-icon.png"));
			} else if(url.StartsWith("inkbunny") || url.StartsWith("www.inkbunny")) {
				IconImage.Source = new BitmapImage(new Uri("ms-appx:///Icons/InkBunny-icon.png"));
			} else if(url.StartsWith("www.weasyl.com") || url.StartsWith("weasyl.com")) {
				IconImage.Source = new BitmapImage(new Uri("ms-appx:///Icons/weasyl-icon.png"));
			} else if(url.StartsWith("www.pixiv") || url.StartsWith("pixiv")) {
				IconImage.Source = new BitmapImage(new Uri("ms-appx:///Icons/Pixiv-icon.png"));
			} else if(url.StartsWith("www.instagram") || url.StartsWith("instagram")) {
				IconImage.Source = new BitmapImage(new Uri("ms-appx:///Icons/Instagram-icon.png"));
			}
		}

		private async void Button_Tapped(object sender, TappedRoutedEventArgs e) {
			try {
				if(!await Launcher.LaunchUriAsync(new Uri(url))) {
					await MainPage.CreatePopupDialog("Error", "Could not Open Default Browser");
				}
			} catch { }
		}

		private void CopyFlyoutItem_Click(object sender, RoutedEventArgs e) {
			var dataPackage = new DataPackage() {
				RequestedOperation = DataPackageOperation.Copy
			};
			dataPackage.SetText(url);
			Clipboard.SetContent(dataPackage);
		}
	}
}
