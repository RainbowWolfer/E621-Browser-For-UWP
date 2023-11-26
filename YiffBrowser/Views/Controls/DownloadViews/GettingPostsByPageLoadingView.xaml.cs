using Prism.Mvvm;
using System.Collections.Generic;
using System.Windows.Input;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using YiffBrowser.Interfaces;
using YiffBrowser.Models.E621;

namespace YiffBrowser.Views.Controls.DownloadViews {
	public sealed partial class GettingPostsByPageLoadingView : UserControl, IContentDialogView {
		public event TypedEventHandler<GettingPostsByPageLoadingView, RoutedEventArgs> Cancel;

		public ContentDialog Dialog { get; set; }

		public static readonly DependencyProperty CancelCommandProperty = DependencyProperty.Register(
			nameof(CancelCommand),
			typeof(ICommand),
			typeof(GettingPostsByPageLoadingView),
			new PropertyMetadata(null)
		);

		public static readonly DependencyProperty FromPageProperty = DependencyProperty.Register(
			nameof(FromPage),
			typeof(int),
			typeof(GettingPostsByPageLoadingView),
			new PropertyMetadata(0)
		);

		public static readonly DependencyProperty ToPageProperty = DependencyProperty.Register(
			nameof(ToPage),
			typeof(int),
			typeof(GettingPostsByPageLoadingView),
			new PropertyMetadata(0)
		);

		public static readonly DependencyProperty CurrentPageProperty = DependencyProperty.Register(
			nameof(CurrentPage),
			typeof(int),
			typeof(GettingPostsByPageLoadingView),
			new PropertyMetadata(0)
		);

		public static readonly DependencyProperty PostsProperty = DependencyProperty.Register(
			nameof(Posts),
			typeof(IList<E621Post>),
			typeof(GettingPostsByPageLoadingView),
			new PropertyMetadata(null, OnPostsChanged)
		);

		public ICommand CancelCommand {
			get => (ICommand)GetValue(CancelCommandProperty);
			set => SetValue(CancelCommandProperty, value);
		}

		public int FromPage {
			get => (int)GetValue(FromPageProperty);
			set => SetValue(FromPageProperty, value);
		}

		public int ToPage {
			get => (int)GetValue(ToPageProperty);
			set => SetValue(ToPageProperty, value);
		}

		public int CurrentPage {
			get => (int)GetValue(CurrentPageProperty);
			set => SetValue(CurrentPageProperty, value);
		}

		public IList<E621Post> Posts {
			get => (IList<E621Post>)GetValue(PostsProperty);
			set => SetValue(PostsProperty, value);
		}

		private static void OnPostsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
			if (d is not GettingPostsByPageLoadingView view) {
				return;
			}
			view.ViewModel.Posts = (IList<E621Post>)e.NewValue;
		}

		public GettingPostsByPageLoadingView() {
			this.InitializeComponent();
		}

		private void CancelButton_Click(object sender, RoutedEventArgs e) {
			Cancel?.Invoke(this, e);
			CancelCommand?.Execute(e);
		}

		public void UpdateCountAndSize() {
			ViewModel.OnPostsChanged();
		}
	}

	internal class GettingPostsByPageLoadingViewModel : BindableBase {
		private IList<E621Post> posts;
		private int count;
		private long size;

		public IList<E621Post> Posts {
			get => posts;
			set => SetProperty(ref posts, value, OnPostsChanged);
		}

		public int Count {
			get => count;
			set => SetProperty(ref count, value);
		}

		public long Size {
			get => size;
			set => SetProperty(ref size, value);
		}

		public void OnPostsChanged() {
			Count = Posts.Count;
			long size = 0;
			foreach (E621Post post in Posts) {
				size += post.File.Size;
			}
			Size = size;
		}

	}
}
