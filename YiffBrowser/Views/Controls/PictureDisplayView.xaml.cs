using Prism.Mvvm;
using System;
using System.Windows.Input;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Imaging;
using YiffBrowser.Helpers;
using YiffBrowser.Views.Controls.Common;

namespace YiffBrowser.Views.Controls {
	public sealed partial class PictureDisplayView : UserControl {


		public ICommand OnImageLoadedCommand {
			get => (ICommand)GetValue(OnImageLoadedCommandProperty);
			set => SetValue(OnImageLoadedCommandProperty, value);
		}

		public static readonly DependencyProperty OnImageLoadedCommandProperty = DependencyProperty.Register(
			nameof(OnImageLoadedCommand),
			typeof(ICommand),
			typeof(PictureDisplayView),
			new PropertyMetadata(null)
		);

		public string URL {
			get => (string)GetValue(URLProperty);
			set => SetValue(URLProperty, value);
		}

		public static readonly DependencyProperty URLProperty = DependencyProperty.Register(
			nameof(URL),
			typeof(string),
			typeof(PictureDisplayView),
			new PropertyMetadata(string.Empty, OnURLChanged)
		);

		private static void OnURLChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
			if (d is PictureDisplayView view) {
				view.ViewModel.Initialize((string)e.NewValue);
				view.ImageControl.ResetImage();
			}
		}

		public long FileSize {
			get => (long)GetValue(FileSizeProperty);
			set => SetValue(FileSizeProperty, value);
		}

		public static readonly DependencyProperty FileSizeProperty = DependencyProperty.Register(
			nameof(FileSize),
			typeof(long),
			typeof(PictureDisplayView),
			new PropertyMetadata(0, OnFileSizeChanged)
		);

		private static void OnFileSizeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
			if (d is PictureDisplayView view) {
				view.ViewModel.FileSize = (long)e.NewValue;
			}
		}

		private void ImageControl_ImageOpened(ImageControlView sender, RoutedEventArgs args) {
			OnImageLoadedCommand?.Execute(ViewModel.Bitmap);
			ViewModel.ShowProgress = false;
		}

		private void ImageControl_ImageFailed(ImageControlView sender, ExceptionRoutedEventArgs args) {
			OnImageLoadedCommand?.Execute(null);
			ViewModel.ShowProgressError = true;
		}

		public void ResetImage() => ImageControl.ResetImage();

		public Image GetMainImage() => ImageControl.GetImage();
		public FrameworkElement GetTargetImage() => ImageControl.GetScrollViewer();

		public PictureDisplayView() {
			InitializeComponent();
		}

	}

	public class PictureDisplayViewModel : BindableBase {
		private int progress = 0;
		private bool showProgress = true;
		private bool isProgressIndeterminate = true;
		private bool showProgressError = false;

		private BitmapImage bitmap = null;
		private long fileSize;
		private long downloadedFileSize;

		public int Progress {
			get => progress;
			set => SetProperty(ref progress, value);
		}

		public bool ShowProgress {
			get => showProgress;
			set => SetProperty(ref showProgress, value);
		}

		public bool ShowProgressError {
			get => showProgressError;
			set => SetProperty(ref showProgressError, value);
		}

		public bool IsProgressIndeterminate {
			get => isProgressIndeterminate;
			set => SetProperty(ref isProgressIndeterminate, value);
		}

		public BitmapImage Bitmap {
			get => bitmap;
			set => SetProperty(ref bitmap, value);
		}

		public long FileSize {
			get => fileSize;
			set => SetProperty(ref fileSize, value);
		}

		public long DownloadedFileSize {
			get => downloadedFileSize;
			set => SetProperty(ref downloadedFileSize, value);
		}

		public string URL { get; private set; }

		public void Initialize(string url) {
			URL = url;

			if (Bitmap != null) {
				Bitmap.UriSource = null;
			}
			Bitmap = null;

			ShowProgress = true;
			IsProgressIndeterminate = true;
			ShowProgressError = false;

			DownloadedFileSize = 0;

			if (!url.IsBlank()) {
				Bitmap = new(new Uri(URL));
				Bitmap.DownloadProgress += (s, e) => {
					Progress = e.Progress;
					IsProgressIndeterminate = false;
					DownloadedFileSize = (int)(FileSize * (Progress / 100d));
				};
			}
		}


	}
}
