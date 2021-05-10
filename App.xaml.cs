using E621Downloader.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
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

namespace E621Downloader {
	public sealed partial class App: Application {
		public static App Instance;
		public readonly List<Task> downloadsQueue;
		public App() {
			Instance = this;
			this.InitializeComponent();
			this.Suspending += OnSuspending;
			downloadsQueue = new List<Task>();
			//LoadImage();
		}

		//private async void LoadImage() {
		//	string url = "https://3er1viui9wo30pkxh1v2nh4w-wpengine.netdna-ssl.com/wp-content/uploads/prod/2014/08/BillGatesHeadshot-BOD.jpg";

		//	var rass = RandomAccessStreamReference.CreateFromUri(new Uri(url));
		//	using(IRandomAccessStream stream = await rass.OpenReadAsync()) {
		//		var bitmapImage = new BitmapImage();
		//		bitmapImage.SetSource(stream);
		//		//ImageStudent.Source = bitmapImage;
		//	}
		//}
		public void RegisterDonwload(Task task) {
			task.Wait();
			downloadsQueue.Add(task);
			//downloadsQueue[0].IsCompleted
		}

		protected override void OnLaunched(LaunchActivatedEventArgs e) {
			if(!(Window.Current.Content is Frame rootFrame)) {
				rootFrame = new Frame();

				rootFrame.NavigationFailed += OnNavigationFailed;

				if(e.PreviousExecutionState == ApplicationExecutionState.Terminated) {
					//TODO: Load state from previously suspended application
				}

				Window.Current.Content = rootFrame;
			}

			if(e.PrelaunchActivated == false) {
				if(rootFrame.Content == null) {
					rootFrame.Navigate(typeof(MainPage), e.Arguments);
				}
				Window.Current.Activate();
			}
		}

		void OnNavigationFailed(object sender, NavigationFailedEventArgs e) {
			throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
		}
		private void OnSuspending(object sender, SuspendingEventArgs e) {
			var deferral = e.SuspendingOperation.GetDeferral();
			//TODO: Save application state and stop any background activity
			deferral.Complete();
		}
	}
	public enum LOR {
		Left, Right
	}
}
