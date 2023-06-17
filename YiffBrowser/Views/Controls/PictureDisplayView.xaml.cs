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
				view.ResetImage();
			}
		}

		public int FileSize {
			get => (int)GetValue(FileSizeProperty);
			set => SetValue(FileSizeProperty, value);
		}

		public static readonly DependencyProperty FileSizeProperty = DependencyProperty.Register(
			nameof(FileSize),
			typeof(int),
			typeof(PictureDisplayView),
			new PropertyMetadata(0, OnFileSizeChanged)
		);

		private static void OnFileSizeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
			if (d is PictureDisplayView view) {
				view.ViewModel.FileSize = (int)e.NewValue;
			}
		}

		private void MainImage_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e) {

		}

		private void MainImage_PointerWheelChanged(object sender, PointerRoutedEventArgs e) {
			PointerPoint point = e.GetCurrentPoint(MainImage);
			double posX = point.Position.X;
			double posY = point.Position.Y;

			double scroll = point.Properties.MouseWheelDelta > 0 ? 1.2 : 0.8;
			Zoom(posX, posY, scroll);
		}

		private void MainImage_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e) {
			ImageTransform.TranslateX += e.Delta.Translation.X;
			ImageTransform.TranslateY += e.Delta.Translation.Y;

			Limit();

			//if (InputKeyboardSource.GetKeyStateForCurrentThread(VirtualKey.Control).HasFlag(CoreVirtualKeyStates.Down)) {

			//}
		}


		private void MainImage_ImageOpened(object sender, RoutedEventArgs e) {
			OnImageLoadedCommand?.Execute(ViewModel.Bitmap);
			ViewModel.ShowProgress = false;
		}

		private void MainImage_ImageFailed(object sender, ExceptionRoutedEventArgs e) {
			OnImageLoadedCommand?.Execute(null);
			ViewModel.ShowProgressError = true;
		}


		private void Zoom(double ratio) {
			double newScaleX = ImageTransform.ScaleX * ratio;
			double newScaleY = ImageTransform.ScaleY * ratio;
			double newTransX = ratio > 1 ?
				(ImageTransform.TranslateX - (MainImage.ActualWidth / 2 * 0.2 * ImageTransform.ScaleX)) :
				(ImageTransform.TranslateX - (MainImage.ActualWidth / 2 * -0.2 * ImageTransform.ScaleX));
			double newTransY = ratio > 1 ?
				(ImageTransform.TranslateY - (MainImage.ActualHeight / 2 * 0.2 * ImageTransform.ScaleY)) :
				(ImageTransform.TranslateY - (MainImage.ActualHeight / 2 * -0.2 * ImageTransform.ScaleY));
			if (newScaleX < 1 || newScaleY < 1) {
				newScaleX = 1;
				newScaleY = 1;
				newTransX = 0;
				newTransY = 0;
			}
			if (newScaleX > 10 || newScaleY > 10) {
				//newScaleX = 10;
				//newScaleY = 10;
				return;
			}
			ImageTransform.ScaleX = newScaleX;
			ImageTransform.ScaleY = newScaleY;
			ImageTransform.TranslateX = newTransX;
			ImageTransform.TranslateY = newTransY;
			Limit();
		}

		private void Zoom(double x, double y, double zoom, double scale = 0.2) {
			double newScaleX = ImageTransform.ScaleX * zoom;
			double newScaleY = ImageTransform.ScaleY * zoom;

			double newTransX = zoom > 1 ?
				(ImageTransform.TranslateX - (x * scale * ImageTransform.ScaleX)) :
				(ImageTransform.TranslateX - (x * -scale * ImageTransform.ScaleX));
			double newTransY = zoom > 1 ?
				(ImageTransform.TranslateY - (y * scale * ImageTransform.ScaleY)) :
				(ImageTransform.TranslateY - (y * -scale * ImageTransform.ScaleY));

			if (newScaleX < 1 || newScaleY < 1) {
				newScaleX = 1;
				newScaleY = 1;
				newTransX = 0;
				newTransY = 0;
			}
			if (newScaleX > 10 || newScaleY > 10) {
				return;
			}

			//make animations?
			ImageTransform.ScaleX = newScaleX;
			ImageTransform.ScaleY = newScaleY;
			ImageTransform.TranslateX = newTransX;
			ImageTransform.TranslateY = newTransY;

			Limit();
		}

		private bool IsHorScaleAbove() {
			return MainImage.ActualWidth * ImageTransform.ScaleX > MyScrollViewer.ActualWidth;
		}

		private bool IsVerScaleAbove() {
			return MainImage.ActualHeight * ImageTransform.ScaleY > MyScrollViewer.ActualHeight;
		}

		private void Limit() {
			double horLimit = (MyScrollViewer.ActualWidth - MainImage.ActualWidth) / 2;
			if (IsHorScaleAbove()) {
				if (ImageTransform.TranslateX > -horLimit) {
					ImageTransform.TranslateX = -horLimit;
				}
				double offset = (MyScrollViewer.ActualWidth - MainImage.ActualWidth * ImageTransform.ScaleX);
				if (ImageTransform.TranslateX < offset - horLimit) {
					ImageTransform.TranslateX = offset - horLimit;
				}
			} else {
				if (ImageTransform.TranslateX < -horLimit) {
					ImageTransform.TranslateX = -horLimit;
				}
				double offset = (MyScrollViewer.ActualWidth - MainImage.ActualWidth * ImageTransform.ScaleX);
				if (ImageTransform.TranslateX > offset - horLimit) {
					ImageTransform.TranslateX = offset - horLimit;
				}
			}
			double verLimit = (MyScrollViewer.ActualHeight - MainImage.ActualHeight) / 2;
			if (IsVerScaleAbove()) {
				if (ImageTransform.TranslateY > 0) {
					ImageTransform.TranslateY = 0;
				}
				double offset = MyScrollViewer.ActualHeight - MainImage.ActualHeight * ImageTransform.ScaleY;
				if (ImageTransform.TranslateY < offset) {
					ImageTransform.TranslateY = offset;
				}
			} else {
				if (ImageTransform.TranslateY < 0) {
					ImageTransform.TranslateY = 0;
				}
				if (ImageTransform.TranslateY > verLimit) {
					ImageTransform.TranslateY = verLimit;
				}
			}
		}

		public void ResetImage() {
			ImageTransform.TranslateX = 0;
			ImageTransform.TranslateY = 0;
			ImageTransform.ScaleX = 1;
			ImageTransform.ScaleY = 1;
		}





		public Image GetMainImage() => MainImage;
		public FrameworkElement GetTargetImage() => MyScrollViewer;


		public PictureDisplayView() {
			this.InitializeComponent();
		}

	}

	public class PictureDisplayViewModel : BindableBase {
		private int progress = 0;
		private bool showProgress = true;
		private bool isProgressIndeterminate = true;
		private bool showProgressError = false;

		private BitmapImage bitmap = null;
		private int fileSize;
		private int downloadedFileSize;

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

		public int FileSize {
			get => fileSize;
			set => SetProperty(ref fileSize, value);
		}

		public int DownloadedFileSize {
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
