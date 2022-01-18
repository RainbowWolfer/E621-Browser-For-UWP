using E621Downloader.Models.Networks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

namespace E621Downloader.Pages {
	public sealed partial class WelcomePage: Page {
		private bool complete1 = false;
		private bool complete2 = false;
		public WelcomePage() {
			this.InitializeComponent();
		}
		protected override void OnNavigatedTo(NavigationEventArgs e) {
			base.OnNavigatedTo(e);
			if(e.Parameter is long c && c != -1) {
				CountText.Text = c.ToString();
			} else {
				CountText.Text = "OFFLINE";
			}
			MascotEntrance.Begin();
			HintEntrance.Completed += (sender, arg) => complete1 = true;

			PostsEntrance.BeginTime = new TimeSpan(0, 0, 0, 0, 500);
			PostsEntrance.Begin();
			HintEntrance.Completed += (sender, arg) => complete2 = true;

			HintEntrance.BeginTime = new TimeSpan(0, 0, 0, 1, 0);
			HintEntrance.Begin();
		}

		private void MascotImage_Tapped(object sender, TappedRoutedEventArgs e) {
			MainPage.NavigateToPostsBrowser(1, "order:rank");
		}

		private void MascotImage_PointerEntered(object sender, PointerRoutedEventArgs e) {
			if(!complete1 || !complete2) {
				return;
			}
			MascotTransition.Children[0].SetValue(DoubleAnimation.FromProperty, ImageCompositeTransform.ScaleX);
			MascotTransition.Children[0].SetValue(DoubleAnimation.ToProperty, 1.1);
			MascotTransition.Children[1].SetValue(DoubleAnimation.FromProperty, ImageCompositeTransform.ScaleY);
			MascotTransition.Children[1].SetValue(DoubleAnimation.ToProperty, 1.1);
			MascotTransition.Children[2].SetValue(DoubleAnimation.FromProperty, PostsPanelCompositeTransform.TranslateY);
			MascotTransition.Children[2].SetValue(DoubleAnimation.ToProperty, 30);
			MascotTransition.Begin();
		}

		private void MascotImage_PointerExited(object sender, PointerRoutedEventArgs e) {
			if(!complete1 || !complete2) {
				return;
			}
			MascotTransition.Children[0].SetValue(DoubleAnimation.FromProperty, ImageCompositeTransform.ScaleX);
			MascotTransition.Children[0].SetValue(DoubleAnimation.ToProperty, 1);
			MascotTransition.Children[1].SetValue(DoubleAnimation.FromProperty, ImageCompositeTransform.ScaleY);
			MascotTransition.Children[1].SetValue(DoubleAnimation.ToProperty, 1);
			MascotTransition.Children[2].SetValue(DoubleAnimation.FromProperty, PostsPanelCompositeTransform.TranslateY);
			MascotTransition.Children[2].SetValue(DoubleAnimation.ToProperty, 0);
			MascotTransition.Begin();
		}

	}
}
