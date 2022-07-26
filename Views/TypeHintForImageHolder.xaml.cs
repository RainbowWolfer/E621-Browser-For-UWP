using E621Downloader.Models.Posts;
using System.Numerics;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

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
			TypeBorder.Translation += new Vector3(0, 0, 8);
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
			} else if(ext == "gif") {
				TypeBorder.Visibility = Visibility.Visible;
				TypeTextBlock.Text = "GIF";
			} else {
				TypeBorder.Visibility = Visibility.Collapsed;
				TypeTextBlock.Text = "";
			}
		}
	}
}
