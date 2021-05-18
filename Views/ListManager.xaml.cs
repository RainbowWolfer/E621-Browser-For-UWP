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
		public readonly ObservableCollection<string> tags;

		private ContentDialog parent;

		private string title;

		public ListManager(string[] tags, ContentDialog contentControl) {
			this.InitializeComponent();
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
		}


		private async void ListView_ItemClick(object sender, ItemClickEventArgs e) {
			parent.Hide();
			string content = (string)e.ClickedItem;
			MainPage.SelectNavigationItem(PageTag.Home);
			await Task.Delay(100);
			//PostsBrowser.Instance.LoadPosts(Post.GetPostsByTags(1, tag), tag);
			await PostsBrowser.Instance.LoadAsync(1, content);
		}

		private void DeleteButton_Tapped(object sender, TappedRoutedEventArgs e) {
			tags.Remove((string)(sender as Button).Tag);
			parent.Title = title + ": " + tags.Count;
		}

		private void Grid_KeyDown(object sender, KeyRoutedEventArgs e) {
			//MyListView.SelectedItem = MyListView.Items[20];
			MyListView.ScrollIntoView(MyListView.Items[new Random().Next(0, 100)]);

			string s = e.Key.ToString().ToLower();
			if(s.Length == 1) {
				foreach(string item in MyListView.Items) {
					string str = item.ToLower();

				}
			}
		}
	}

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
