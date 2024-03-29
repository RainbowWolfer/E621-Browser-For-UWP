﻿using E621Downloader.Models.Download;
using Windows.Networking.BackgroundTransfer;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace E621Downloader.Views.DownloadSection {
	public sealed partial class SimpleDownloadProgressBar: UserControl {
		public const char icon_complete = '\uE10B';
		public const char icon_downloading = '\uE118';
		public const char icon_paused = '\uE103';
		public const char icon_error = '\uE783';
		public const char icon_starting = '\uE10C';

		private DownloadBlock block;
		public DownloadInstance Instance { get; private set; }

		public SimpleDownloadProgressBar() {
			this.InitializeComponent();
		}

		public void SetParent(DownloadBlock block) {
			this.block = block;
		}

		public void SetTarget(DownloadInstance instance) {
			this.Instance = instance;
			SetBarValue(instance.DownloadProgress);
			SetIcon(instance);
			instance.SubDownloadingAction = (p) => {
				SetBarValue(instance.DownloadProgress);
				SetIcon(instance);
			};
		}

		public void SetVisible(bool visible) {
			MyIcon.Visibility = visible ? Visibility.Visible : Visibility.Collapsed;
			MyProgressBar.Visibility = visible ? Visibility.Visible : Visibility.Collapsed;
			MyEllipse.Visibility = visible ? Visibility.Collapsed : Visibility.Visible;
		}

		public void SetDownloadIcon() {
			MyIcon.Glyph = icon_downloading.ToString();
		}

		public void SetPauseIcon() {
			MyIcon.Glyph = icon_paused.ToString();
		}

		public void SetCompleteIcon() {
			MyIcon.Glyph = icon_complete.ToString();
		}

		public void SetErrorIcon() {
			MyIcon.Glyph = icon_error.ToString();
		}
		public void SetStartIcon() {
			MyIcon.Glyph = icon_starting.ToString();
		}

		public void SetTooltip(string content) {

		}

		public void SetIcon(DownloadInstance instance) {
			if(instance.TotalBytesToReceive == 0) {
				SetStartIcon();
			} else if(instance.Status == BackgroundTransferStatus.Completed || instance.BytesReceived == instance.TotalBytesToReceive) {
				SetCompleteIcon();
			} else if(instance.Status == BackgroundTransferStatus.Running) {
				SetDownloadIcon();
			} else if(instance.Status == BackgroundTransferStatus.Error) {
				SetErrorIcon();
			} else {
				SetPauseIcon();
			}
		}

		public void SetBarValue(double value) {
			MyProgressBar.Value = value;
		}
	}
}
