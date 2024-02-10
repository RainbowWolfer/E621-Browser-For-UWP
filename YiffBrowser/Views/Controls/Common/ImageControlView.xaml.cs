using Windows.Foundation;
using Windows.UI.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Imaging;

namespace YiffBrowser.Views.Controls.Common {
	public sealed partial class ImageControlView : UserControl {

		public event TypedEventHandler<ImageControlView, RoutedEventArgs> ImageOpened;
		public event TypedEventHandler<ImageControlView, ExceptionRoutedEventArgs> ImageFailed;


		public BitmapImage Bitmap {
			get => (BitmapImage)GetValue(BitmapProperty);
			set => SetValue(BitmapProperty, value);
		}

		public static readonly DependencyProperty BitmapProperty = DependencyProperty.Register(
			nameof(Bitmap),
			typeof(BitmapImage),
			typeof(ImageControlView),
			new PropertyMetadata(null)
		);



		public ImageControlView() {
			this.InitializeComponent();
		}

		public Image GetImage() => MainImage;
		public ScrollViewer GetScrollViewer() => MyScrollViewer;

		private void MainImage_ImageOpened(object sender, RoutedEventArgs e) {
			ImageOpened?.Invoke(this, e);
		}

		private void MainImage_ImageFailed(object sender, ExceptionRoutedEventArgs e) {
			ImageFailed?.Invoke(this, e);
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

	}
}
