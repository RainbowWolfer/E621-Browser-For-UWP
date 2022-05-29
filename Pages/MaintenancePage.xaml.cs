using E621Downloader.Models.Networks;
using Microsoft.Toolkit.Uwp.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace E621Downloader.Pages {
	public sealed partial class MaintenancePage: Page {
		public MaintenancePage() {
			this.InitializeComponent();

			string hex = App.GetApplicationTheme() switch {
				ApplicationTheme.Light => "#3E89C3",
				ApplicationTheme.Dark => "#012E57",
				_ => Application.Current.RequestedTheme == ApplicationTheme.Light ? "#3E89C3" : "#012E57",
			};

			MainGrid.Background = new SolidColorBrush(hex.ToColor());


		}

		private void MouseEnter() {
			IconMouseOverStoryboard.Begin();
			IconMouseOverAnimation.From = IconImage.Height;
			IconMouseOverAnimation.To = 250;
		}

		private void MouseExit() {
			IconMouseOverStoryboard.Begin();
			IconMouseOverAnimation.From = IconImage.Height;
			IconMouseOverAnimation.To = 200;
		}

		private void IconImage_PointerEntered(object sender, PointerRoutedEventArgs e) {
			MouseEnter();
		}

		private void IconImage_PointerExited(object sender, PointerRoutedEventArgs e) {
			MouseExit();
		}

		private void IconImage_PointerCaptureLost(object sender, PointerRoutedEventArgs e) {
			MouseExit();
		}

		private async void IconImage_Tapped(object sender, TappedRoutedEventArgs e) {
			if(!await Launcher.LaunchUriAsync(new Uri($"https://{Data.GetHost()}"))) {
				await MainPage.CreatePopupDialog("Error", "Could not Open Default Browser");
			}
		}
	}
}
