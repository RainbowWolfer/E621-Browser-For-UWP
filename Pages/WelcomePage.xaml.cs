using E621Downloader.Models.Networks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace E621Downloader.Pages {
	public sealed partial class WelcomePage: Page {
		public WelcomePage() {
			this.InitializeComponent();
		}
		protected override void OnNavigatedTo(NavigationEventArgs e) {
			base.OnNavigatedTo(e);
			if(e.Parameter is long c) {
				CountText.Text = c.ToString();
			}
			MascotEntrance.Begin();

			PostsEntrance.BeginTime = new TimeSpan(0, 0, 0, 0, 500);
			PostsEntrance.Begin();

			HintEntrance.BeginTime = new TimeSpan(0, 0, 0, 1, 0);
			HintEntrance.Begin();
		}
	}
}
