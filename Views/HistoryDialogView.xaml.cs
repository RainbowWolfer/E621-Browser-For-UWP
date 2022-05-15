using E621Downloader.Models;
using E621Downloader.Models.Locals;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.DataTransfer;
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
	public sealed partial class HistoryDialogView: UserControl {
		private readonly ObservableCollection<HistoryItem> tags = new();
		private readonly ObservableCollection<HistoryItem> postIDs = new();

		private readonly ContentDialog dialog;
		public HistoryDialogView(ContentDialog dialog) {
			this.InitializeComponent();
			foreach(HistoryItem tag in Local.History.Tags) {
				tags.Add(tag);
			}
			foreach(HistoryItem id in Local.History.PostIDs) {
				postIDs.Add(id);
			}

			UpdateVisibilities();

			this.dialog = dialog;
		}

		private void UpdateVisibilities() {
			if(tags.Count > 0) {
				SearchHistoryListView.Visibility = Visibility.Visible;
				SearchHistoryEmptyPanel.Visibility = Visibility.Collapsed;
				SearchHistoryListView.SelectedIndex = 0;
			} else {
				SearchHistoryListView.Visibility = Visibility.Collapsed;
				SearchHistoryEmptyPanel.Visibility = Visibility.Visible;
			}

			if(postIDs.Count > 0) {
				ViewHistoryListView.Visibility = Visibility.Visible;
				ViewHistoryEmptyPanel.Visibility = Visibility.Collapsed;
				ViewHistoryListView.SelectedIndex = 0;
			} else {
				ViewHistoryListView.Visibility = Visibility.Collapsed;
				ViewHistoryEmptyPanel.Visibility = Visibility.Visible;
			}

		}

		private void MenuFlyoutItem_Click(object sender, RoutedEventArgs e) {
			if(sender is MenuFlyoutItem item && item.Tag is string tag) {
				var dataPackage = new DataPackage() {
					RequestedOperation = DataPackageOperation.Copy
				};
				dataPackage.SetText(tag);
				Clipboard.SetContent(dataPackage);
			}
		}

		private void MenuFlyoutItem_Click_Delete_1(object sender, RoutedEventArgs e) {
			if(sender is MenuFlyoutItem item && item.Tag is string tag) {
				Local.History.RemoveTag(tag);
				HistoryItem found = tags.Where(t => t.Value == tag).FirstOrDefault();
				if(found != null) {
					tags.Remove(found);
				}
			}
			UpdateVisibilities();
		}

		private void MenuFlyoutItem_Click_Delete_2(object sender, RoutedEventArgs e) {
			if(sender is MenuFlyoutItem item && item.Tag is string tag) {
				Local.History.RemovePostID(tag);
				HistoryItem found = postIDs.Where(t => t.Value == tag).FirstOrDefault();
				if(found != null) {
					postIDs.Remove(found);
				}
			}
			UpdateVisibilities();
		}

		private void ViewHistoryListView_ItemClick(object sender, ItemClickEventArgs e) {
			if(e.ClickedItem is HistoryItem item) {
				dialog.Hide();
				//item.Value;
				//MainPage.NavigateToPicturePage();
			}
		}
	}
}
