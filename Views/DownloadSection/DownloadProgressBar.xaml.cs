﻿using E621Downloader.Models;
using E621Downloader.Models.Download;
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
		public DownloadInstance Instance { get; private set; }
		public DownloadProgressBar(DownloadInstance instance) {
			this.InitializeComponent();
			Instance = instance;
			UpdateInfo();
			NameTextBlock.Text = "Posts: " + instance.PostRef.id;
			InfoTextBlock.Text = instance.PostRef.file.url;

			Instance.DownloadingAction = (p) => UpdateInfo();
			Debug.WriteLine(Instance.Status);
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
		}

		private void PauseButton_Tapped(object sender, TappedRoutedEventArgs e) {
			if(e.Handled) {
				return;
			}
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
			
		}
	}
}