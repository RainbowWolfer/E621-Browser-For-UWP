using E621Downloader.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

namespace E621Downloader.Views {
	public delegate void OnImageLoadedEventHandler(BitmapImage bitmap);
	public sealed partial class ImageHolder: UserControl {
		public E621Article ArticleRef { get; private set; }

		public event OnImageLoadedEventHandler OnImagedLoaded;

		private string LoadUrl => ArticleRef.url_preview;

		public BitmapImage Image { get; private set; }

		private bool isLoaded;

		private DateTime startTime;
		public ImageHolder(E621Article article) {
			this.ArticleRef = article;
			this.InitializeComponent();
			startTime = DateTime.Now;
			MyLoadingTextBlock.Text = "Loading From : " + LoadUrl;
			OnImagedLoaded += (b) => this.Image = b;
			//LoadImageAsync2();
			//App.Instance.RegisterDonwload(LoadImageAsync());
		}
		private async Task LoadImageAsync(/*WriteableBitmap bitmap*/) {
			Debug.WriteLine(ArticleRef.id + "Start");
			try {
				LoadingPanel.Visibility = Visibility.Visible;
				isLoaded = false;
				MyImage.Source = await ImageConvertor.GetWriteableBitmapAsync(LoadUrl);
				LoadingPanel.Visibility = Visibility.Collapsed;
				isLoaded = true;
			} catch(Exception e) {
				Debug.WriteLine("ERROR_" + e.Message);
			}
			Debug.WriteLine(ArticleRef.id + "End");
		}
		private async void LoadImageAsync2() {
			LoadingPanel.Visibility = Visibility.Visible;
			isLoaded = false;
			var rass = RandomAccessStreamReference.CreateFromUri(new Uri(LoadUrl));
			using(IRandomAccessStream stream = await rass.OpenReadAsync()) {
				var bitmapImage = new BitmapImage();
				await bitmapImage.SetSourceAsync(stream);
				MyImage.Source = bitmapImage;
			}
			LoadingPanel.Visibility = Visibility.Collapsed;
			isLoaded = true;
		}

		private void Grid_Tapped(object sender, TappedRoutedEventArgs e) {
			if(!this.isLoaded) {
				//open browser;
			}

			var dataPackage = new DataPackage {
				RequestedOperation = DataPackageOperation.Copy
			};
			dataPackage.SetText(LoadUrl);
			Clipboard.SetContent(dataPackage);

			MainPage.NavigateToPicturePage(ArticleRef);

		}

		private void MyImage_ImageOpened(object sender, RoutedEventArgs e) {
			LoadingPanel.Visibility = Visibility.Collapsed;
			//Debug.WriteLine("\n" + (DateTime.Now - startTime) + "\n");
			OnImagedLoaded?.Invoke((BitmapImage)MyImage.Source);
		}

		private void MyImage_ImageFailed(object sender, ExceptionRoutedEventArgs e) {
			MyProgressRing.IsActive = false;
			MyLoadingTextBlock.Text = "FAILED";
			Debug.WriteLine("FAILED");
		}
	}
}
