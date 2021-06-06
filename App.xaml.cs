using E621Downloader.Models;
using E621Downloader.Models.Locals;
using Newtonsoft.Json;
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
using Windows.Networking.BackgroundTransfer;
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

		public static bool showNullImage;
		public static bool showBlackListed;
		public static bool safemode;

		public App() {
			Instance = this;
			this.InitializeComponent();
			this.Suspending += OnSuspending;

			Local.Initialize();

			Test();
		}

		private async void Test() {
			//while(Local.DownloadFolder == null) {
			//	await Task.Delay(10);
			//}
			//List<MetaFile> metas = await Local.GetAllMetaFiles();
		}

		public static bool CompareTwoArray<T>(IEnumerable<T> a, IEnumerable<T> b) {
			T[] ar = a.ToArray();
			T[] br = b.ToArray();
			if(ar.Length != br.Length) {
				return false;
			}
			for(int i = 0; i < ar.Length; i++) {
				if(!ar[i].Equals(br[i])) {
					return false;
				}
			}
			return true;
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
