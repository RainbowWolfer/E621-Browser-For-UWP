﻿using E621Downloader.Models;
using E621Downloader.Models.Posts;
using E621Downloader.Pages;
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
		public Post PostRef { get; private set; }

		public event OnImageLoadedEventHandler OnImagedLoaded;

		private int _spanCol;
		private int _spanRow;
		public int SpanCol {
			get => _spanCol;
			set {
				_spanCol = value;
				VariableSizedWrapGrid.SetColumnSpan(this, value);
			}
		}
		public int SpanRow {
			get => _spanRow;
			set {
				_spanRow = value;
				VariableSizedWrapGrid.SetRowSpan(this, value);
			}
		}
		public string LoadUrl => PostRef.sample.url;

		private bool _isSelected;
		public bool IsSelected {
			get => _isSelected;
			set {
				_isSelected = value;
				BorderGrid.BorderThickness = new Thickness(value ? 4 : 0);
			}
		}

		public BitmapImage Image { get; private set; }

		private bool isLoaded;

		public ImageHolder(Post post) {
			this.PostRef = post;
			this.InitializeComponent();
			OnImagedLoaded += (b) => this.Image = b;
			if(LoadUrl != null) {
				if(post.file.ext == "webm") {
					TypeBorder.Visibility = Visibility.Visible;
					TypeTextBlock.Text = "WEBM";
				} else if(post.file.ext == "anim") {
					TypeBorder.Visibility = Visibility.Visible;
					TypeTextBlock.Text = "ANIM";
				} else {
					TypeBorder.Visibility = Visibility.Collapsed;
					TypeTextBlock.Text = "";
				}
				Debug.WriteLine(post.file.ext);
				MyImage.Source = new BitmapImage(new Uri(LoadUrl));
			} else {
				MyProgressRing.IsActive = false;
				MyProgressRing.Visibility = Visibility.Collapsed;
				FailureTextBlock.Text = "Failed";
				if(PostsBrowser.Instance.ShowNullImage) {
					this.Visibility = Visibility.Visible;
					VariableSizedWrapGrid.SetColumnSpan(this, SpanCol);
					VariableSizedWrapGrid.SetRowSpan(this, SpanRow);
				} else {
					this.Visibility = Visibility.Collapsed;
					VariableSizedWrapGrid.SetColumnSpan(this, 0);
					VariableSizedWrapGrid.SetRowSpan(this, 0);
				}
			}

		}
		/**
		private async Task LoadImageAsync(WriteableBitmap bitmap) {
			Debug.WriteLine(PostRef.id + "Start");
			try {
				LoadingPanel.Visibility = Visibility.Visible;
				isLoaded = false;
				MyImage.Source = await ImageConvertor.GetWriteableBitmapAsync(LoadUrl);
				LoadingPanel.Visibility = Visibility.Collapsed;
				isLoaded = true;
			} catch(Exception e) {
				Debug.WriteLine("ERROR_" + e.Message);
			}
			Debug.WriteLine(PostRef.id + "End");
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
		*/
		private void Grid_Tapped(object sender, TappedRoutedEventArgs e) {
			if(!this.isLoaded) {
				//open browser;
				//return;
			}
			if(LoadUrl == null) {
				return;
			}
			if(PostsBrowser.Instance.multipleSelectionMode) {
				IsSelected = !IsSelected;
				PostsBrowser.Instance.SelectFeedBack(this);
			} else {
				var dataPackage = new DataPackage() {
					RequestedOperation = DataPackageOperation.Copy
				};
				dataPackage.SetText(LoadUrl);
				Clipboard.SetContent(dataPackage);

				//MainPage.NavigateToPicturePage(PostRef);
				MainPage.Instance.parameter_picture = PostRef;
				MainPage.SelectNavigationItem(PageTag.Picture);
			}
		}

		private void MyImage_ImageOpened(object sender, RoutedEventArgs e) {
			LoadingPanel.Visibility = Visibility.Collapsed;
			//Debug.WriteLine("\n" + (DateTime.Now - startTime) + "\n");
			OnImagedLoaded?.Invoke((BitmapImage)MyImage.Source);
		}

		private void MyImage_ImageFailed(object sender, ExceptionRoutedEventArgs e) {
			MyProgressRing.IsActive = false;
			//MyLoadingTextBlock.Text = "FAILED";
			Debug.WriteLine("FAILED");
		}
	}
}
