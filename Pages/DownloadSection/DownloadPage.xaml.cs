using E621Downloader.Models;
using E621Downloader.Models.Download;
using E621Downloader.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Networking.BackgroundTransfer;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

namespace E621Downloader.Pages.DownloadSection {
	public sealed partial class DownloadPage: Page {
		public static DownloadPage Instance;

		public DownloadPage() {
			Instance = this;
			this.InitializeComponent();
			this.NavigationCacheMode = NavigationCacheMode.Enabled;
			MainFrame.Navigate(typeof(DownloadOverview), null, new EntranceNavigationTransitionInfo());
		}

		protected override void OnNavigatedTo(NavigationEventArgs e) {
			base.OnNavigatedTo(e);
			Debug.WriteLine(MainFrame.GetNavigationState());
			//MainFrame.Navigate();
		}

		public void NavigateTo(Type type, object parameter = null) {
			MainFrame.Navigate(type, parameter);
		}

		private void AutoSuggestBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args) {

		}

		private void TitleButton_Tapped(object sender, TappedRoutedEventArgs e) {
			TitleButton.IsChecked = true;
			TitleButton.IsHitTestVisible = false;
			MyNavigationView.SelectedItem = null;

			MainFrame.Navigate(typeof(DownloadOverview), null, new EntranceNavigationTransitionInfo());

		}

		private void NavigationView_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args) {
			TitleButton.IsChecked = false;
			TitleButton.IsHitTestVisible = true;

			DownloadsGroup group = DownloadsManager.FindGroup(args.InvokedItemContainer.Content as string);
			MainFrame.Navigate(typeof(DownloadDetailsPage), group.downloads, new EntranceNavigationTransitionInfo());

		}

		private void MainFrame_Navigating(object sender, NavigatingCancelEventArgs e) {

		}

		private void MainFrame_Navigated(object sender, NavigationEventArgs e) {

		}
	}
}
