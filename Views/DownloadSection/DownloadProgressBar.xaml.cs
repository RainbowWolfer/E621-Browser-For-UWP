using E621Downloader.Models;
using E621Downloader.Models.Download;
using E621Downloader.Pages.DownloadSection;
using System;
using System.Collections.Generic;
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
using Windows.UI.Xaml.Navigation;

namespace E621Downloader.Views.DownloadSection {
	public sealed partial class DownloadProgressBar: UserControl {
		public DownloadDetailsPage PageParent { get; private set; }
		public DownloadInstance Instance { get; private set; }
		public DownloadProgressBar(DownloadDetailsPage parent, DownloadInstance instance) {
			this.InitializeComponent();
			PageParent = parent;
			Instance = instance;
			UpdateInfo();
			NameTextBlock.Text = "Posts: " + instance.PostRef.id;
			InfoTextBlock.Text = instance.PostRef.file.url;

			Instance.DedicatedDownloadingAction = (p) => {
				UpdateInfo();
				if(p >= 1 || instance.Status == BackgroundTransferStatus.Completed) {
					//Debug.WriteLine("Downloaded" + p);
					PageParent.MoveToDownloaded(instance);
					PageParent.UpdateDownloadsInfo();
				}
			};
			//Debug.WriteLine(Instance.Status);
			if(Instance.Status == BackgroundTransferStatus.Running) {
				TextBlock_PauseButton.Text = "Pause";
				FontIcon_PauseButton.Glyph = "\uE769";
			} else {
				TextBlock_PauseButton.Text = "Resume";
				FontIcon_PauseButton.Glyph = "\uE102";
			}

		}

		private void UpdateInfo() {
			MyProgressBar.Value = Instance.DownloadProgress;
			//InfoTextBlock.Text = Instance.PostRef.file.url;
			PercentageTextBlok.Text = string.Format("{0}%   {1} KB / {2} KB", Instance.Percentage, Instance.ReceivedKB, Instance.TotalKB);
			if(Instance.TotalBytesToReceive == Instance.BytesReceived && Instance.TotalBytesToReceive != 0) {
				DownloadingPanel.Visibility = Visibility.Collapsed;
				DownloadedPanel.Visibility = Visibility.Visible;
			}
		}

		private void PauseButton_Tapped(object sender, TappedRoutedEventArgs e) {
			//if(e.Handled) {
			//	return;
			//}
			if(Instance.Status == BackgroundTransferStatus.Running) {
				Instance.Pause();
				TextBlock_PauseButton.Text = "Resume";
				FontIcon_PauseButton.Glyph = "\uE102";
			} else {
				Instance.Resume();
				TextBlock_PauseButton.Text = "Pause";
				FontIcon_PauseButton.Glyph = "\uE769";
			}
		}

		private void CancelButton_Tapped(object sender, TappedRoutedEventArgs e) {
			Instance.Cancel();
		}

		private void OpenButton_Tapped(object sender, TappedRoutedEventArgs e) {
			MainPage.NavigateToPicturePage(Instance.metaFile.MyPost, Array.Empty<string>());
		}

		private void InfoTextBlock_RightTapped(object sender, RightTappedRoutedEventArgs e) {
			MenuFlyout flyout = new() {
				Placement = FlyoutPlacementMode.Bottom,
			};
			MenuFlyoutItem copy_item = new() {
				Text = "Copy URL",
				Icon = new FontIcon() { Glyph = "\uE8C8" },
			};
			flyout.Items.Add(copy_item);
			flyout.ShowAt(sender as TextBlock);
		}
	}
}
