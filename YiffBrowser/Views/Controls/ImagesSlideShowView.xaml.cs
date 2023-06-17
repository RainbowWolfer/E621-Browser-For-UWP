using Prism.Mvvm;
using System;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using YiffBrowser.Helpers;

namespace YiffBrowser.Views.Controls {
	public sealed partial class ImagesSlideShowView : UserControl {

		public string[] ImageURLs {
			get => (string[])GetValue(ImageURLsProperty);
			set => SetValue(ImageURLsProperty, value);
		}

		public static readonly DependencyProperty ImageURLsProperty = DependencyProperty.Register(
			nameof(ImageURLs),
			typeof(string[]),
			typeof(ImagesSlideShowView),
			new PropertyMetadata(Array.Empty<string>(), OnChanged)
		);

		private static void OnChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
			if (d is ImagesSlideShowView view) {
				view.ViewModel.ImageURLs = (string[])e.NewValue;
			}
		}

		public ImagesSlideShowView() {
			this.InitializeComponent();
		}

	}

	public class ImagesSlideShowViewModel : BindableBase {
		public const int DELAY = 2500;

		private string[] imageURLs;
		private int selectedIndex;

		public string[] ImageURLs {
			get => imageURLs;
			set => SetProperty(ref imageURLs, value);
		}

		public int SelectedIndex {
			get => selectedIndex;
			set => SetProperty(ref selectedIndex, value);
		}

		public ImagesSlideShowViewModel() {
			TimedMovement();
		}

		private async void TimedMovement() {
			SelectedIndex = 0;
			while (true) {
				await Task.Delay(DELAY);

				if (ImageURLs.IsEmpty()) {
					continue;
				}

				int index = SelectedIndex + 1;
				if (index >= ImageURLs.Length) {
					index = 0;
				}
				SelectedIndex = index;
			}
		}
	}
}
