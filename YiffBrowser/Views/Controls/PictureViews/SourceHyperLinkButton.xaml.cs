using Prism.Commands;
using Prism.Mvvm;
using System.Windows.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using YiffBrowser.Helpers;

namespace YiffBrowser.Views.Controls.PictureViews {
	public sealed partial class SourceHyperLinkButton : UserControl {

		public string URL {
			get => (string)GetValue(URLProperty);
			set => SetValue(URLProperty, value);
		}

		public static readonly DependencyProperty URLProperty = DependencyProperty.Register(
			nameof(URL),
			typeof(string),
			typeof(SourceHyperLinkButton),
			new PropertyMetadata(string.Empty)
		);

		public SourceHyperLinkButton() {
			this.InitializeComponent();
		}
	}

	public class SourceHyperLinkButtonViewModel : BindableBase {
		private string url = string.Empty;
		private string iconPath = null;

		public string URL {
			get => url;
			set => SetProperty(ref url, value, OnURLChanged);
		}

		public string IconPath {
			get => iconPath;
			set => SetProperty(ref iconPath, value);
		}

		private void OnURLChanged() {
			if (URL.IsBlank()) {
				return;
			}
			string _url = URL;
			if (_url.StartsWith("https://")) {
				_url = _url.Substring(8);
			} else if (_url.StartsWith("http://")) {
				_url = _url.Substring(7);
			}
			string path = null;
			if (_url.Contains("tumblr")) {//something.tumblr.com
				path = "/Resources/Icons/tumblr-icon.png";
			}
			if (_url.StartsWith("twitter") || _url.StartsWith("www.twitter") || _url.StartsWith("pbs.twimg")) {
				path = "/Resources/Icons/Twitter-icon.png";
			} else if (_url.StartsWith("www.furaffinity") || _url.StartsWith("furaffinity") || _url.StartsWith("d.furaffinity")) {
				path = "/Resources/Icons/Furaffinity-icon.png";
			} else if (_url.StartsWith("www.deviantart") || _url.StartsWith("deviantart")) {
				path = "/Resources/Icons/DeviantArt-icon.png";
			} else if (_url.StartsWith("www.inkbunny") || _url.StartsWith("inkbunny")) {
				path = "/Resources/Icons/InkBunny-icon.png";
			} else if (_url.StartsWith("www.weasyl.com") || _url.StartsWith("weasyl.com")) {
				path = "/Resources/Icons/weasyl-icon.png";
			} else if (_url.StartsWith("www.pixiv") || _url.StartsWith("pixiv")) {
				path = "/Resources/Icons/Pixiv-icon.png";
			} else if (_url.StartsWith("www.instagram") || _url.StartsWith("instagram")) {
				path = "/Resources/Icons/Instagram-icon.png";
			} else if (_url.StartsWith("www.patreon") || _url.StartsWith("patreon")) {
				path = "/Resources/Icons/Patreon-icon.png";
			} else if (_url.StartsWith("www.subscribestar") || _url.StartsWith("subscribestar")) {
				path = "/Resources/Icons/SubscribeStar-icon.png";
			} else if (_url.StartsWith("mega")) {
				path = "/Resources/Icons/Mega-icon.png";
			} else if (_url.StartsWith("furrynetwork")) {
				path = "/Resources/Icons/FurryNetwork-icon.png";
			} else if (_url.StartsWith("t.me")) {
				path = "/Resources/Icons/Telegram-icon.png";
			} else if (_url.StartsWith("newgrounds") || _url.StartsWith("www.newgrounds")) {
				path = "/Resources/Icons/NewGrounds-icon.png";
			}

			IconPath = path;
		}

		public ICommand ClickCommand => new DelegateCommand(Click);
		public ICommand CopyCommand => new DelegateCommand(Copy);

		private void Click() {
			URL.OpenInBrowser();
		}

		private void Copy() {
			URL.CopyToClipboard();
		}
	}
}
