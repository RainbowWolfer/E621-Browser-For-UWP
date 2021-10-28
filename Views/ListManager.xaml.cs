using E621Downloader.Models;
using E621Downloader.Pages;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
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
	public sealed partial class ListManager: UserControl {
		private List<string> originTags;
		public readonly ObservableCollection<string> tags;
		private int initialCount;

		private ContentDialog parent;

		private string title;

		public ListManager(string[] tags, ContentDialog contentControl) {
			this.InitializeComponent();
			this.originTags = tags.ToList();
			this.initialCount = tags.Length;
			this.title = contentControl.Title as string;
			this.parent = contentControl;
			this.tags = new ObservableCollection<string>();
			foreach(string s in tags) {
				this.tags.Add(s);
			}
			parent.Title = title + ": " + tags.Length;
			OrderToolTip.Content = "Time";
		}

		private void ExportButton_Tapped(object sender, TappedRoutedEventArgs e) {
			string result = "";
			tags.ToList().ForEach(s => result += s + '\n');
			var dataPackage = new DataPackage() {
				RequestedOperation = DataPackageOperation.Copy
			};
			dataPackage.SetText(result);
			Clipboard.SetContent(dataPackage);
			MainPage.CreateTip("Successful", "Hello World");
		}

		public string[] GetCurrentTags() {
			return tags.ToArray();
		}

		private void TimeOrderItem_Click(object sender, RoutedEventArgs e) {
			OrderToolTip.Content = "Time";
		}

		private void AlphaOrderItem_Click(object sender, RoutedEventArgs e) {
			OrderToolTip.Content = "Alpha";
			List<string> tmp = tags.ToList();
			tmp.Sort((a, b) => a.CompareTo(b));
			tags.Clear();
			tmp.ForEach(s => tags.Add(s));
		}


		private async void ListView_ItemClick(object sender, ItemClickEventArgs e) {
			parent.Hide();
			string content = (string)e.ClickedItem;
			MainPage.SelectNavigationItem(PageTag.Home);
			await Task.Delay(100);
			await PostsBrowser.Instance.LoadAsync(1, content);
		}

		private void AddButton_Tapped(object sender, TappedRoutedEventArgs e) {
			string tag = (sender as TextBox).Text.Trim().ToLower();
			originTags.Add(tag);
			tags.Add(tag);
			UpdateTitle();
		}

		private void DeleteButton_Tapped(object sender, TappedRoutedEventArgs e) {
			string tag = (string)(sender as Button).Tag;
			tags.Remove(tag);
			originTags.Remove(tag);
			UpdateTitle();
		}

		private void UpdateTitle() {
			if(initialCount == originTags.Count) {
				parent.Title = $"{title}: {originTags.Count}";
			} else {
				parent.Title = $"{title}: {initialCount} -> {originTags.Count}";
			}
		}

		private void Grid_KeyDown(object sender, KeyRoutedEventArgs e) {

		}

		private void MySuggestBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args) {
			tags.Clear();
			if(string.IsNullOrEmpty(args.QueryText)) {
				foreach(string item in originTags) {
					tags.Add(item);
				}
				return;
			}
			foreach(string item in originTags) {
				if(item.ToLower().Contains(args.QueryText.ToLower())) {
					tags.Add(item);
				}
			}
		}

	}
	[Obsolete("Later in Dev", false)]
	public class TagItem {
		public string tag;
		public DateTime addedTime;
		public bool isBlackListed;

		public TagItem(string tag, DateTime addedTime, bool isBlackListed) {
			this.tag = tag;
			this.addedTime = addedTime;
			this.isBlackListed = isBlackListed;
		}
	}
}
