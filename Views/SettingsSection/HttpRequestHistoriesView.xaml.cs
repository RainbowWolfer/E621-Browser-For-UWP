using E621Downloader.Models;
using E621Downloader.Models.Debugging;
using E621Downloader.Models.Networks;
using E621Downloader.Models.Posts;
using E621Downloader.Pages;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using static E621Downloader.Models.Debugging.HttpRequestHistories;

namespace E621Downloader.Views.SettingsSection {
	public sealed partial class HttpRequestHistoriesView: UserControl {
		public ObservableCollection<HttpRequestHistoriesViewItem> Items { get; } = new();
		public HttpRequestHistoriesView() {
			this.InitializeComponent();
			UpdateInfo(null);
			Items.Clear();
			HttpRequestHistories.Items.ForEach(i => {
				Items.Add(new HttpRequestHistoriesViewItem(i));
			});
			MainListView.SelectedIndex = 0;
			LoadingRing.Visibility = Visibility.Collapsed;
		}

		private void MainListView_SelectionChanged(object sender, SelectionChangedEventArgs e) {
			if(e.AddedItems.FirstOrDefault() is HttpRequestHistoriesViewItem item) {
				UpdateInfo(item.Item);
			}
		}

		private void UpdateInfo(RequestHistoryItem item) {
			if(item == null) {
				TitleText.Text = "";
				URLText.Text = "";
				StartText.Text = "";
				MethodText.Text = "";
				ResultText.Text = "";
				CodeText.Text = "";
				TimeText.Text = "";
				TypeText.Text = "";
				SizeText.Text = "";
				HintText.Text = "";
			} else {
				TitleText.Text = item.Method.ToUpper() + " " + item.Result.ToString().ToUpper();
				URLText.Text = item.Url;
				StartText.Text = item.StartDateTime.ToString();
				MethodText.Text = item.Method;
				ResultText.Text = item.Result.ToString();
				CodeText.Text = item.StatusCode + " - " + (int)item.StatusCode;
				TimeText.Text = item.Time + "ms";
				TypeText.Text = item.ContentType.ToString();
				SizeText.Text = item.ContentSize + " byte(s)";
				HintText.Text = string.IsNullOrWhiteSpace(item.HintText) ? "None" : item.HintText;
			}
		}

		private void ClearButton_Click(object sender, RoutedEventArgs e) {
			Items.Clear();
			HttpRequestHistories.Items.Clear();
			UpdateInfo(null);
		}
	}

	public class HttpRequestHistoriesViewItem {
		public RequestHistoryItem Item { get; set; }
		public string Date => Methods.GetTime(Item.StartDateTime);
		public string IconGlyph => Item.Method switch {
			"Put" => "\uE143",
			"Post" => "\uE140",
			"Get" => "\uE13F",
			"Delete" => "\uE107",
			"Patch" => "\uE1CA",
			_ => "\uE11B",
		};

		public string ConsumeTime => $"{Math.Round(Item.Time / 1000d, 1)}s";

		public string ResultIconGlyph => Item.Result switch {
			HttpResultType.Canceled => "\uE10A",
			HttpResultType.Success => "\uE10B",
			HttpResultType.Error => "\uE783",
			_ => "\uE11B",
		};

		public Brush ResultIconBrush => new SolidColorBrush(Item.Result switch {
			HttpResultType.Canceled => PicturePage.GetColor(Rating.suggestive),
			HttpResultType.Success => PicturePage.GetColor(Rating.safe),
			HttpResultType.Error => PicturePage.GetColor(Rating.explict),
			_ => PicturePage.GetColor(null),
		});

		public HttpRequestHistoriesViewItem(RequestHistoryItem item) {
			Item = item;
		}
	}
}
