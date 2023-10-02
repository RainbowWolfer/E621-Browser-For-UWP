using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using YiffBrowser.Helpers;
using YiffBrowser.Models.E621;

namespace YiffBrowser.Views.Controls.PostsView {
	public sealed partial class PostsBriefDisplayView : UserControl {


		public E621Post[] Posts {
			get => (E621Post[])GetValue(PostsProperty);
			set => SetValue(PostsProperty, value);
		}

		public static readonly DependencyProperty PostsProperty = DependencyProperty.Register(
			nameof(Posts),
			typeof(E621Post[]),
			typeof(PostsBriefDisplayView),
			new PropertyMetadata(Array.Empty<E621Post>(), OnPostsChanged)
		);

		private static void OnPostsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {

		}

		public PostsBriefDisplayView() {
			this.InitializeComponent();
		}


		public static async Task ShowAsDialog(IEnumerable<E621Post> posts, string title = null) {
			await new PostsBriefDisplayView() {
				Posts = posts.ToArray(),
			}.CreateContentDialog(new ContentDialogParameters() {
				Title = title,
				CloseText = "Back",
				MaxWidth = ContentDialogParameters.DEFAULT_MAX_WIDTH,
			}).ShowAsyncSafe();
		}
	}
}
