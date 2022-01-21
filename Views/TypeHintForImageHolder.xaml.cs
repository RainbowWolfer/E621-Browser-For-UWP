using E621Downloader.Models.Posts;
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
	public sealed partial class TypeHintForImageHolder: UserControl {
		private Post post;
		public Post PostRef {
			get => post;
			set {
				post = value;
				Load(value);
			}
		}

		public TypeHintForImageHolder() {
			this.InitializeComponent();
		}

		private void Load(Post post) {
			if(post == null) {
				return;
			}
			string ext = post.file.ext.Trim().ToLower();
			if(ext == "webm") {
				TypeBorder.Visibility = Visibility.Visible;
				TypeTextBlock.Text = "WEBM";
			} else if(ext == "anim") {
				TypeBorder.Visibility = Visibility.Visible;
				TypeTextBlock.Text = "ANIM";
			} else {
				TypeBorder.Visibility = Visibility.Collapsed;
				TypeTextBlock.Text = "";
			}
		}
	}
}
