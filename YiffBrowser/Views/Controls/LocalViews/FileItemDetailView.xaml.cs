using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Diagnostics;
using System.Windows.Input;
using Windows.Foundation;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Imaging;
using YiffBrowser.Helpers;
using YiffBrowser.Models.E621;
using YiffBrowser.Views.Controls.Common;
using YiffBrowser.Views.Pages.E621;

namespace YiffBrowser.Views.Controls.LocalViews {
	public sealed partial class FileItemDetailView : UserControl {

		public FileItemDetailViewModel ViewModel {
			get => (FileItemDetailViewModel)GetValue(ViewModelProperty);
			set => SetValue(ViewModelProperty, value);
		}

		public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(
			nameof(ViewModel),
			typeof(FileItemDetailViewModel),
			typeof(FileItemDetailView),
			new PropertyMetadata(new FileItemDetailViewModel(), OnViewModelChanged)
		);

		private static void OnViewModelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
			if (d is not FileItemDetailView view) {
				return;
			}
			view.ViewModel.MainImage = view.MainImage;
		}

		public FileItemDetailView() {
			InitializeComponent();
		}

		private Point startPosition;

		private void MainGrid_PointerReleased(object sender, PointerRoutedEventArgs e) {
			Point endPos = e.GetCurrentPoint(MainGrid).Position;
			if (startPosition.Distance(endPos) >= 20d) {
				return;
			}
			//IsShowingImagesListManager = !IsShowingImagesListManager;
		}

		private void MainGrid_PointerPressed(object sender, PointerRoutedEventArgs e) {
			startPosition = e.GetCurrentPoint(MainGrid).Position;
		}

		private void MainGrid_Tapped(object sender, TappedRoutedEventArgs e) {

		}
	}

	public class FileItemDetailViewModel : BindableBase {
		public Image MainImage { get; set; }

		private FileItem fileItem;
		private bool isMedia;
		private bool showMoreInfoSplitView;
		private bool isSidePaneOverlay;

		public FileItem FileItem {
			get => fileItem;
			set {
				if (fileItem != null) {
					fileItem.PostChanged -= FileItem_PostChanged;
				}
				SetProperty(ref fileItem, value, OnFileItemChanged);
				if (fileItem != null) {
					fileItem.PostChanged += FileItem_PostChanged;
				}
			}
		}

		public PostDetailControlViewModel ControlViewModel { get; } = new PostDetailControlViewModel();

		public bool IsMedia {
			get => isMedia;
			set => SetProperty(ref isMedia, value);
		}

		public bool ShowMoreInfoSplitView {
			get => showMoreInfoSplitView;
			set => SetProperty(ref showMoreInfoSplitView, value);
		}

		public bool IsSidePaneOverlay {
			get => isSidePaneOverlay;
			set => SetProperty(ref isSidePaneOverlay, value);
		}

		private void FileItem_PostChanged(FileItem sender, E621Post post) {
			if (post == null) {
				return;
			}


		}

		private async void OnFileItemChanged() {
			if (FileItem == null) {
				return;
			}

			IsMedia = FileItem.File.FileType == ".webm" || FileItem.File.FileType == ".mp4";

			try {
				using IRandomAccessStream fileStream = await FileItem.File.OpenAsync(FileAccessMode.Read);
				BitmapImage bitmapImage = new();
				await bitmapImage.SetSourceAsync(fileStream);
				MainImage.Source = bitmapImage;
			} catch (Exception ex) {
				Debug.WriteLine(ex);
			}

		}

		public ICommand OpenMoreInfoCommand => new DelegateCommand(OpenMoreInfo);

		private void OpenMoreInfo() {
			ShowMoreInfoSplitView = !ShowMoreInfoSplitView;
		}

		public FileItemDetailViewModel() {

		}

	}
}
