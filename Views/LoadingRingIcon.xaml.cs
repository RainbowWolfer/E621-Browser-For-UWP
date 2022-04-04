using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace E621Downloader.Views {
	public sealed partial class LoadingRingIcon: UserControl {
		private LoadingType loadingType;

		public LoadingType LoadingType {
			get => loadingType;
			set {
				loadingType = value;
				LoadingRingOpacityAnimtaion.From = LoadingRing.Opacity;
				LoadingRingTransformAnimation.From = LoadingRingTransform.Y;
				AcceptIconOpacityAnimation.From = AcceptIcon.Opacity;
				AcceptIconTransformAnimation.From = AcceptIconTransform.Y;
				switch(loadingType) {
					case LoadingType.None:
						LoadingRingOpacityAnimtaion.To = 0;
						LoadingRingTransformAnimation.To = 20;
						AcceptIconOpacityAnimation.To = 0;
						AcceptIconTransformAnimation.To = -20;
						break;
					case LoadingType.Loading:
						LoadingRingOpacityAnimtaion.To = 1;
						LoadingRingTransformAnimation.To = 0;
						AcceptIconOpacityAnimation.To = 0;
						AcceptIconTransformAnimation.To = -20;
						break;
					case LoadingType.Done:
						LoadingRingOpacityAnimtaion.To = 0;
						LoadingRingTransformAnimation.To = 20;
						AcceptIconOpacityAnimation.To = 1;
						AcceptIconTransformAnimation.To = 0;
						break;
					default:
						return;
				}
				StopLoadingStoryboard.Begin();
			}
		}
		public LoadingRingIcon() {
			this.InitializeComponent();
		}

		private void Grid_Tapped(object sender, TappedRoutedEventArgs e) {
			//switch(LoadingType) {
			//	case LoadingType.None:
			//		LoadingType = LoadingType.Loading;
			//		break;
			//	case LoadingType.Loading:
			//		LoadingType = LoadingType.Done;
			//		break;
			//	case LoadingType.Done:
			//		LoadingType = LoadingType.None;
			//		break;
			//	default:
			//		break;
			//}
		}
	}

	public enum LoadingType {
		None, Loading, Done
	}
}
