using E621Downloader.Models.Debugging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using static E621Downloader.Models.Debugging.ErrorHistories;

namespace E621Downloader.Views.SettingsSection {
	public sealed partial class HandledExceptionsView: UserControl {
		public ObservableCollection<HandledExceptionItem> Items { get; } = new();

		public HandledExceptionsView() {
			this.InitializeComponent();
			HandledExceptions.ForEach(e => Items.Add(new HandledExceptionItem(e)));
			MainListView.SelectedIndex = 0;
			LoadingRing.Visibility = Visibility.Collapsed;
			if(Items.Count > 0) {
				EmptyGrid.Visibility = Visibility.Collapsed;
				MainListView.Visibility= Visibility.Visible;
				ContentGrid.Visibility = Visibility.Visible;
			} else{
				EmptyGrid.Visibility = Visibility.Visible;
				MainListView.Visibility = Visibility.Collapsed;
				ContentGrid.Visibility = Visibility.Collapsed;
			}
		}

		private void MainListView_SelectionChanged(object sender, SelectionChangedEventArgs e) {
			if(e.AddedItems.FirstOrDefault() is HandledExceptionItem item) {
				UpdateInfo(item.Item);
			}
		}

		private void UpdateInfo(HandledException exception) {
			if(exception == null) {
				TitleText.Text = "";
				MessageText.Text = "";
				StackTraceText.Text = "";
			} else {
				TitleText.Text = exception.ExceptionType;
				MessageText.Text = exception.Message;
				StackTraceText.Text = exception.StackTrace;
			}
		}
	}

	public class HandledExceptionItem {
		public HandledException Item { get; set; }


		public HandledExceptionItem(HandledException item) {
			Item = item;
		}
	}

}
