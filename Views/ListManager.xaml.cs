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
		private readonly List<string> originTags;
		public readonly ObservableCollection<string> tags;
		private readonly int initialCount;

		private readonly ContentDialog parent;

		private readonly string title;
		private SettingsPage page;

		public ListManager(SettingsPage page, string[] tags, ContentDialog contentControl) {
			this.InitializeComponent();
			this.page = page;
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
			MainPage.CreateTip(page, "Nofitication", "Successfully Exported", Symbol.Accept);
		}

		public string[] GetCurrentTags() => originTags.ToArray();

		private void TimeOrderItem_Click(object sender, RoutedEventArgs e) {
			OrderToolTip.Content = "Time";
			TimeOrderItem.Text = "· Time ·";
			AlphaOrderItem.Text = "Alphabet";
			tags.Clear();
			originTags.ForEach(t => tags.Add(t));
		}

		private void AlphaOrderItem_Click(object sender, RoutedEventArgs e) {
			OrderToolTip.Content = "Alpha";
			TimeOrderItem.Text = "Time";
			AlphaOrderItem.Text = "· Alphabet ·";
			List<string> tmp = tags.ToList();
			tmp.Sort((a, b) => a.CompareTo(b));
			tags.Clear();
			tmp.ForEach(s => tags.Add(s));
		}


		private void ListView_ItemClick(object sender, ItemClickEventArgs e) {
			parent.Hide();
			MainPage.NavigateToPostsBrowser(1, (string)e.ClickedItem);
		}

		private void AddButton_Tapped(object sender, TappedRoutedEventArgs e) {
			string tag = NewText.Text.Trim().ToLower();
			if(string.IsNullOrWhiteSpace(tag) || originTags.Contains(tag)) {
				NewText.Text = "";
				return;
			}
			originTags.Add(tag);
			tags.Add(tag);
			UpdateTitle();
			NewText.Text = "";
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
			UpdateSearch(args.QueryText);
		}

		private void MySuggestBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args) {
			UpdateSearch(sender.Text);
		}

		private void UpdateSearch(string input) {
			tags.Clear();
			if(string.IsNullOrEmpty(input)) {
				foreach(string item in originTags) {
					tags.Add(item);
				}
				return;
			}
			foreach(string item in originTags) {
				if(item.ToLower().Contains(input.ToLower())) {
					tags.Add(item);
				}
			}
		}

	}
}
