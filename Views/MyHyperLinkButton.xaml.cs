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
			string path = null;
			if(url.Contains("tumblr")) {//something.tumblr.com
				path = "ms-appx:///Icons/tumblr-icon.png";
			}
			if(url.StartsWith("twitter") || url.StartsWith("www.twitter") || url.StartsWith("pbs.twimg")) {
				path = "ms-appx:///Icons/Twitter-icon.png";
			} else if(url.StartsWith("www.furaffinity") || url.StartsWith("furaffinity") || url.StartsWith("d.furaffinity")) {
				path = "ms-appx:///Icons/Furaffinity-icon.png";
			} else if(url.StartsWith("www.deviantart") || url.StartsWith("deviantart")) {
				path = "ms-appx:///Icons/DeviantArt-icon.png";
			} else if(url.StartsWith("www.inkbunny") || url.StartsWith("inkbunny")) {
				path = "ms-appx:///Icons/InkBunny-icon.png";
			} else if(url.StartsWith("www.weasyl.com") || url.StartsWith("weasyl.com")) {
				path = "ms-appx:///Icons/weasyl-icon.png";
			} else if(url.StartsWith("www.pixiv") || url.StartsWith("pixiv")) {
				path = "ms-appx:///Icons/Pixiv-icon.png";
			} else if(url.StartsWith("www.instagram") || url.StartsWith("instagram")) {
				path = "ms-appx:///Icons/Instagram-icon.png";
			} else if(url.StartsWith("www.patreon") || url.StartsWith("patreon")) {
				path = "ms-appx:///Icons/Patreon-icon.png";
			} else if(url.StartsWith("www.subscribestar") || url.StartsWith("subscribestar")) {
				path = "ms-appx:///Icons/SubscribeStar-icon.png";
			} else if(url.StartsWith("mega")) {
				path = "ms-appx:///Icons/Mega-icon.png";
			} else if(url.StartsWith("furrynetwork")) {
				path = "ms-appx:///Icons/FurryNetwork-icon.png";
			} else if(url.StartsWith("t.me")) {
				path = "ms-appx:///Icons/Telegram-icon.png";
			} else if(url.StartsWith("newgrounds") || url.StartsWith("www.newgrounds")) {
				path = "ms-appx:///Icons/NewGrounds-icon.png";
			}

			if(path != null) {
				IconImage.Source = new BitmapImage(new Uri(path));
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
