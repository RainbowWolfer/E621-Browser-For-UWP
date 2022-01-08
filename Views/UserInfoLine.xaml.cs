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
	public sealed partial class UserInfoLine: UserControl {
		private string titleText;
		private string contentText;

		public string TitleText {
			get => titleText;
			set {
				titleText = value;
				TitleTextBlock.Text = TitleText;
			}
		}
		public string ContentText {
			get => contentText;
			set {
				contentText = value;
				ContentTextBlock.Text = ContentText;
			}
		}
		public UserInfoLine() {
			this.InitializeComponent();
		}
	}
}
