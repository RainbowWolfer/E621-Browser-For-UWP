using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using YiffBrowser.Helpers;
using YiffBrowser.Models.E621;

namespace YiffBrowser.Views.Controls {
	public sealed partial class ImageViewItem : UserControl {
		public event TypedEventHandler<ImageViewItem, ImageViewItemViewModel> ImageClick;
		public event TypedEventHandler<ImageViewItem, ImageViewItemViewModel> SelectThis;
		public event TypedEventHandler<ImageViewItem, ImageViewItemViewModel> DownloadThis;

		public Func<bool> IsInSelectionMode { get; set; }

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



		public bool UseImageSize {
			get => (bool)GetValue(UseImageSizeProperty);
			set => SetValue(UseImageSizeProperty, value);
		}

		public static readonly DependencyProperty UseImageSizeProperty = DependencyProperty.Register(
			nameof(UseImageSize),
			typeof(bool),
			typeof(ImageViewItem),
			new PropertyMetadata(true)
		);



		public bool IsSelected {
			get => (bool)GetValue(IsSelectedProperty);
			set => SetValue(IsSelectedProperty, value);
		}

		public static readonly DependencyProperty IsSelectedProperty = DependencyProperty.Register(
			nameof(IsSelected),
			typeof(bool),
			typeof(ImageViewItem),
			new PropertyMetadata(false, OnIsSelectedChanged)
		);

		private static void OnIsSelectedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
			if (d is ImageViewItem view) {
				view.ViewModel.IsSelected = (bool)e.NewValue;
			}
		}

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

		private void SelectThisItem_Click(object sender, RoutedEventArgs e) {
			SelectThis?.Invoke(this, ViewModel);
		}

		private void DownloadThisItem_Click(object sender, RoutedEventArgs e) {
			DownloadThis?.Invoke(this, ViewModel);
		}

		private void SampleImageBrush_ImageOpened(object sender, RoutedEventArgs e) {
			BitmapImage bitmap = (SampleImageBrush.ImageSource as BitmapImage);
			bitmap.Stop();
			bitmap.DecodePixelType = DecodePixelType.Logical;
			if (UseImageSize) {
				bitmap.DecodePixelWidth = (int)SampleImage.ActualWidth;
				bitmap.DecodePixelHeight = (int)SampleImage.ActualHeight;
			} else {
				//bitmap.DecodePixelWidth = (int)SampleImage.ActualWidth;
				//bitmap.DecodePixelHeight = (int)SampleImage.ActualHeight;
			}
			UpdateScaleCenter();
		}

		private void PreviewImageBrush_ImageOpened(object sender, RoutedEventArgs e) {
			BitmapImage bitmap = (PreviewImageBrush.ImageSource as BitmapImage);
			bitmap.Stop();
			bitmap.DecodePixelType = DecodePixelType.Logical;
			if (UseImageSize) {
				bitmap.DecodePixelWidth = (int)PreviewImage.ActualWidth;
				bitmap.DecodePixelHeight = (int)PreviewImage.ActualHeight;
			}
			UpdateScaleCenter();
		}

		private void Image_PointerEntered(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e) {
			if (IsInSelectionMode?.Invoke() ?? false) {
				ButtonBorder.BorderThickness = new Thickness(!IsSelected ? 2 : 0);
				ImageScaleXAnimation.To = 1.05;
				ImageScaleYAnimation.To = 1.05;
				ImageScaleStoryboard.Begin();
			} else {
				ImageScaleXAnimation.To = 1.05;
				ImageScaleYAnimation.To = 1.05;
				ImageScaleStoryboard.Begin();
			}
		}

		private void Image_PointerExited(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e) {
			ImageScaleXAnimation.To = 1;
			ImageScaleYAnimation.To = 1;
			ImageScaleStoryboard.Begin();
			ButtonBorder.BorderThickness = new Thickness(0);
		}

		private void UserControl_Loaded(object sender, RoutedEventArgs e) {
			UpdateScaleCenter();
		}

		private void UpdateScaleCenter() {
			ImageScale.CenterX = ContentGrid.ActualWidth / 2;
			ImageScale.CenterY = ContentGrid.ActualHeight / 2;
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
		private BitmapImage sampleImage;
		private bool isSampleLoading = false;
		private double sampleLoadingProgress = 0;
		private bool sampleLoadingIsIndeterminate = true;
		private bool isSelected;
		private Brush isSelectedBrush;

		public bool IsSelected {
			get => isSelected;
			set => SetProperty(ref isSelected, value, () => {
				if (value) {
					IsSelectedBrush = new SolidColorBrush((Color)Application.Current.Resources["SystemAccentColor"]);
				} else {
					IsSelectedBrush = (Brush)Application.Current.Resources["CardBackgroundFillColorDefaultBrush"];
				}
			});
		}

		public Brush IsSelectedBrush {
			get => isSelectedBrush;
			set => SetProperty(ref isSelectedBrush, value);
		}

		public string PreviewImageURL {
			get => previewImageURL;
			set => SetProperty(ref previewImageURL, value.NotBlankCheck() ?? "");
		}

		public string SampleImageURL {
			get => sampleImageURL;
			set => SetProperty(ref sampleImageURL, value, OnSampleImageURLChanged);
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

		private void OnPreviewLoaded() {
			SampleImageURL = Post.Sample.URL;
			ImageLoadStage = LoadStage.Preview;
		}

		private void OnSampleLoaded() {
			ImageLoadStage = LoadStage.Sample;
			HidePreviewImage = true;
			IsSampleLoading = false;
		}

		private void OpenInBrowser() {
			@$"https://e621.net/posts/{Post.ID}".OpenInBrowser();
		}

		public ImageViewItemViewModel() {
			IsSelected = false;
			LoopAssign();
		}

		private BitmapImage SampleImage {
			get => sampleImage;
			set {
				if (sampleImage != null) {
					sampleImage.DownloadProgress -= SampleImage_DownloadProgress;
				}
				sampleImage = value;
				if (sampleImage != null) {
					sampleImage.DownloadProgress += SampleImage_DownloadProgress;
				}
			}
		}

		private ImageBrush SampleImageBrush { get; set; }

		public ICommand LoadedCommand => new DelegateCommand<ImageViewItem>(Loaded);
		private void Loaded(ImageViewItem view) {
			SampleImageBrush = (ImageBrush)view.FindName(nameof(SampleImageBrush));
		}
		private void OnSampleImageURLChanged() {
			SampleImage = null;
			IsSampleLoading = true;
		}

		private async void LoopAssign() {
			while (true) {
				await Task.Delay(500);
				if (SampleImage != null) {
					continue;
				}
				SampleImage = SampleImageBrush?.ImageSource as BitmapImage;
			}
		}

		private void SampleImage_DownloadProgress(object sender, DownloadProgressEventArgs e) {
			SampleLoadingIsIndeterminate = false;
			IsSampleLoading = true;
			SampleLoadingProgress = e.Progress;
		}

		public bool IsSampleLoading {
			get => isSampleLoading;
			set => SetProperty(ref isSampleLoading, value);
		}

		public double SampleLoadingProgress {
			get => sampleLoadingProgress;
			set => SetProperty(ref sampleLoadingProgress, value);
		}

		public bool SampleLoadingIsIndeterminate {
			get => sampleLoadingIsIndeterminate;
			set => SetProperty(ref sampleLoadingIsIndeterminate, value);
		}

	}

	public enum LoadStage {
		None, Preview, Sample
	}
}
