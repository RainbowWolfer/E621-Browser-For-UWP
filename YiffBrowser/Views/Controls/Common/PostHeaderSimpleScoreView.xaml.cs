using Prism.Mvvm;
using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using YiffBrowser.Models.E621;

namespace YiffBrowser.Views.Controls.Common {
	public sealed partial class PostHeaderSimpleScoreView : UserControl {

		public E621Post Post {
			get => (E621Post)GetValue(PostProperty);
			set => SetValue(PostProperty, value);
		}

		public static readonly DependencyProperty PostProperty = DependencyProperty.Register(
			nameof(Post),
			typeof(E621Post),
			typeof(PostHeaderSimpleScoreView),
			new PropertyMetadata(null)
		);

		public PostHeaderSimpleScoreView() {
			InitializeComponent();
		}
	}

	internal class PostHeaderSimpleScoreViewModel : BindableBase {
		private E621Post post;

		public E621Post Post {
			get => post;
			set => SetProperty(ref post, value, OnPostChanged);
		}

		private void OnPostChanged() {
			if (Post == null) {
				return;
			}
		}

		public PostHeaderSimpleScoreViewModel() {

		}

	}
}
