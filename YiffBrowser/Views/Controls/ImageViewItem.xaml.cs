using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Linq;
using System.Numerics;
using System.Windows.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using YiffBrowser.Helpers;
using YiffBrowser.Models.E621;

namespace YiffBrowser.Views.Controls {
	public sealed partial class ImageViewItem : UserControl {
		public event Action<ImageViewItem, ImageViewItemViewModel> ImageClick;

		public event Action OnPostDeleted;

		public E621Post Post {
			get => (E621Post)GetValue(PostProperty);
			set => SetValue(PostProperty, value);
		}

		public static readonly DependencyProperty PostProperty = DependencyProperty.Register(
			nameof(Post),
			typeof(E621Post),
			typeof(ImageViewItem),
			new PropertyMetadata(null, OnPostChanged)
		);

		private static void OnPostChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
			if (d is not ImageViewItem view) {
				return;
			}
			if (e.NewValue is E621Post post) {
				view.ViewModel.Post = post;
			}
		}

		public bool ShowPostInfo {
			get => (bool)GetValue(ShowPostInfoProperty);
			set => SetValue(ShowPostInfoProperty, value);
		}

		public static readonly DependencyProperty ShowPostInfoProperty = DependencyProperty.Register(
			nameof(ShowPostInfo),
			typeof(bool),
			typeof(ImageViewItem),
			new PropertyMetadata(true)
		);

		public ICommand Command {
			get => (ICommand)GetValue(CommandProperty);
			set => SetValue(CommandProperty, value);
		}

		public static readonly DependencyProperty CommandProperty = DependencyProperty.Register(
			nameof(Command),
			typeof(ICommand),
			typeof(ImageViewItem),
			new PropertyMetadata(null)
		);



		public bool IsSelected {
			get => (bool)GetValue(IsSelectedProperty);
			set => SetValue(IsSelectedProperty, value);
		}

		public static readonly DependencyProperty IsSelectedProperty = DependencyProperty.Register(
			nameof(IsSelected),
			typeof(bool),
			typeof(ImageViewItem),
			new PropertyMetadata(false)
		);



		public ImageViewItem() {
			this.InitializeComponent();
			TypeHintBorder.Translation += new Vector3(0, 0, 32);
		}

		private void Button_Click(object sender, RoutedEventArgs e) {
			if (Post.Flags.deleted == true) {
				return;
			} else if (Post.HasNoValidURLs()) {
				return;
			}
			ImageClick?.Invoke(this, ViewModel);
			Command?.Execute(e);
		}

		public FrameworkElement GetSampleImage() => SampleImage;
		public FrameworkElement GetPreviewImage() => PreviewImage;

		public FrameworkElement GetCurrentImage() {
			if (!ViewModel.HidePreviewImage) {
				return GetPreviewImage();
			} else {
				return GetSampleImage();
			}
		}
	}

	public class ImageViewItemViewModel : BindableBase {
		private E621Post post;
		private string typeHint;

		private LoadStage imageLoadStage = LoadStage.None;

		private string previewImageURL;
		private string sampleImageURL;
		private string errorLoadingHint;
		private bool hidePreviewImage;

		public string PreviewImageURL {
			get => previewImageURL;
			set => SetProperty(ref previewImageURL, value.NotBlankCheck() ?? "");
		}

		public string SampleImageURL {
			get => sampleImageURL;
			set => SetProperty(ref sampleImageURL, value);
		}

		public E621Post Post {
			get => post;
			set => SetProperty(ref post, value, OnPostChanged);
		}

		public string TypeHint {
			get => typeHint;
			set => SetProperty(ref typeHint, value);
		}

		public LoadStage ImageLoadStage {
			get => imageLoadStage;
			set => SetProperty(ref imageLoadStage, value);
		}

		public string ErrorLoadingHint {
			get => errorLoadingHint;
			set => SetProperty(ref errorLoadingHint, value);
		}

		public bool HidePreviewImage {
			get => hidePreviewImage;
			set => SetProperty(ref hidePreviewImage, value);
		}

		private void OnPostChanged() {
			if (new string[] { "gif", "webm", "swf" }.Contains(Post.File.Ext.ToLower())) {
				TypeHint = Post.File.Ext.ToUpper();
			}
			if (Post.Flags.deleted == true) {
				ErrorLoadingHint = "Post Deleted";
			} else if (Post.HasNoValidURLs()) {
				ErrorLoadingHint = "Try login to show this post";
			} else {
				PreviewImageURL = Post.Preview.URL;
			}
		}

		public ICommand OnPreviewLoadedCommand => new DelegateCommand(OnPreviewLoaded);
		public ICommand OnSampleLoadedCommand => new DelegateCommand(OnSampleLoaded);
		public ICommand OpenInBrowserCommand => new DelegateCommand(OpenInBrowser);
		public ICommand OpenCommand => new DelegateCommand(Open);

		private void OnPreviewLoaded() {
			SampleImageURL = Post.Sample.URL;
			ImageLoadStage = LoadStage.Preview;
		}

		private void OnSampleLoaded() {
			ImageLoadStage = LoadStage.Sample;
			HidePreviewImage = true;
		}

		private void OpenInBrowser() {
			@$"https://e621.net/posts/{Post.ID}".OpenInBrowser();
		}

		private void Open() {

		}

	}

	public enum LoadStage {
		None, Preview, Sample
	}
}
