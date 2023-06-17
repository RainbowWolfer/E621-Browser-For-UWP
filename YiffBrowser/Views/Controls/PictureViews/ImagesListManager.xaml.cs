using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using YiffBrowser.Helpers;
using YiffBrowser.Models.E621;

namespace YiffBrowser.Views.Controls.PictureViews {
	public sealed partial class ImagesListManager : UserControl {

		public ICommand ItemClickCommand {
			get => (ICommand)GetValue(ItemClickCommandProperty);
			set => SetValue(ItemClickCommandProperty, value);
		}

		public static readonly DependencyProperty ItemClickCommandProperty = DependencyProperty.Register(
			nameof(ItemClickCommand),
			typeof(ICommand),
			typeof(ImagesListManager),
			new PropertyMetadata(null)
		);

		public E621Post[] PostsList {
			get => (E621Post[])GetValue(PostsListProperty);
			set => SetValue(PostsListProperty, value);
		}

		public static readonly DependencyProperty PostsListProperty = DependencyProperty.Register(
			nameof(PostsList),
			typeof(E621Post[]),
			typeof(ImagesListManager),
			new PropertyMetadata(Array.Empty<E621Post>(), OnPostsListChanged)
		);

		private static void OnPostsListChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
			if (d is not ImagesListManager view) {
				return;
			}

			view.Items.Clear();
			foreach (E621Post item in (E621Post[])e.NewValue) {
				view.Items.Add(new ImagesListManagerItem(item, view));
			}
		}

		public E621Post CurrentPost {
			get => (E621Post)GetValue(CurrentPostProperty);
			set => SetValue(CurrentPostProperty, value);
		}

		public static readonly DependencyProperty CurrentPostProperty = DependencyProperty.Register(
			nameof(CurrentPost),
			typeof(E621Post),
			typeof(ImagesListManager),
			new PropertyMetadata(null, OnCurrentPostChanged)
		);

		private static void OnCurrentPostChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
			if (d is not ImagesListManager view) {
				return;
			}

			for (int i = 0; i < view.Items.Count; i++) {
				ImagesListManagerItem item = view.Items[i];
				item.IsSelected = item.Post == (E621Post)e.NewValue;
				if (item.IsSelected) {
					view.CurrentCountText.Text = (i + 1).ToString();
					view.PutToCenter(item);
				}
			}
			view.WholeCountText.Text = view.Items.Count.ToString();
		}

		public bool IsLocked {
			get => (bool)GetValue(IsLockedProperty);
			set => SetValue(IsLockedProperty, value);
		}

		public static readonly DependencyProperty IsLockedProperty =DependencyProperty.Register(
			nameof(IsLocked),
			typeof(bool),
			typeof(ImagesListManager),
			new PropertyMetadata(false)
		);



		public ObservableCollection<ImagesListManagerItem> Items { get; } = new ObservableCollection<ImagesListManagerItem>();

		public ImagesListManager() {
			this.InitializeComponent();

		}

		private void PhotosListGrid_PointerEntered(object sender, PointerRoutedEventArgs e) {
			GridExpandAnimation.To = 34;
			GridExpandStoryboard.Begin();
		}

		private void PhotosListGrid_PointerExited(object sender, PointerRoutedEventArgs e) {
			GridExpandAnimation.To = 0;
			GridExpandStoryboard.Begin();
		}

		private void PutToCenter(ImagesListManagerItem item) {
			int index = Items.IndexOf(item);
			int count = Items.Count;
			int margin = 3;
			int width = 80;

			double offsetX = index * (width + margin);

			//var found = PhotosListView.Items.Where(i => i is ListViewItem lvi && lvi.Content is PhotosListItem pli && pli == item).FirstOrDefault();
			////var found = PhotosListView.Items.Where(i => i is ListViewItem lvi && lvi == item);
			////await PhotosListView.SmoothScrollIntoViewWithItemAsync(found);

			//await Dispatcher.RunAsync(CoreDispatcherPriority.High, () => {
			//	PhotosListView.ScrollToCenterOfView(found);
			//});

			ListScroll.ChangeView(offsetX, 0, 1);
		}

		private void ImageBrush_ImageOpened(object sender, RoutedEventArgs e) {
			ImageBrush ib = (ImageBrush)sender;
			BitmapImage bi = ib.ImageSource as BitmapImage;
			bi.Stop();
		}
	}

	public class ImagesListManagerItem : BindableBase {
		private readonly ImagesListManager manager;

		private E621Post post;

		private string imageURL = null;
		private string typeIcon = null;
		private bool showLoading;

		private bool isSelected = false;

		private Brush borderBrush = new SolidColorBrush(Colors.Beige);
		private bool isMouseOn;

		public ImagesListManagerItem(E621Post post, ImagesListManager manager) {
			Post = post;
			this.manager = manager;
		}

		public E621Post Post {
			get => post;
			set => SetProperty(ref post, value, OnPostChanged);
		}

		public string ImageURL {
			get => imageURL;
			set => SetProperty(ref imageURL, value);
		}

		public string TypeIcon {
			get => typeIcon;
			set => SetProperty(ref typeIcon, value);
		}

		public bool ShowLoading {
			get => showLoading;
			set => SetProperty(ref showLoading, value);
		}

		public Brush BorderBrush {
			get => borderBrush;
			set => SetProperty(ref borderBrush, value);
		}

		public bool IsMouseOn {
			get => isMouseOn;
			set => SetProperty(ref isMouseOn, value);
		}

		public bool IsSelected {
			get => isSelected;
			set => SetProperty(ref isSelected, value, OnIsSelectedChanged);
		}

		private void OnIsSelectedChanged() {
			BorderBrush = new SolidColorBrush(IsSelected ? Colors.Red : Colors.Beige);
		}

		private void OnPostChanged() {
			if (Post == null || Post.HasNoValidURLs()) {
				return;
			}

			ShowLoading = true;

			ImageURL = Post.Sample.URL;
			FileType fileType = Post.GetFileType();
			TypeIcon = fileType switch {
				FileType.Gif => "\uF4A9",
				FileType.Webm => "\uE102",
				_ => null,
			};
		}

		public ICommand ImageOpenedCommand => new DelegateCommand<RoutedEventArgs>(ImageOpened);
		public ICommand ImageFailedCommand => new DelegateCommand(ImageFailed);

		private void ImageOpened(RoutedEventArgs args) {
			ShowLoading = false;
		}

		private void ImageFailed() {
			ShowLoading = false;
		}

		public ICommand PointerEnteredCommand => new DelegateCommand(PointerEntered);
		public ICommand PointerExitedCommand => new DelegateCommand(PointerExited);

		private void PointerEntered() {
			IsMouseOn = true;
		}

		private void PointerExited() {
			IsMouseOn = false;
		}

		public ICommand ClickCommand => new DelegateCommand(Click);

		private void Click() {
			manager.ItemClickCommand?.Execute(Post);
		}
	}
}
