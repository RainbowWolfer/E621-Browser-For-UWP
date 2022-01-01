using E621Downloader.Models.Posts;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
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

namespace E621Downloader.Views.TagsManagementSection {
	public sealed partial class TagsSelectionView: Page {
		public bool handleSearch = false;
		public ObservableCollection<string> tags;
		private ContentDialog dialog;

		//private string Tag_ID => $"ID: {tag?.id}";
		//private string Tag_Name => $"Name: {tag?.name}";
		//private string Tag_Count => $"Count: {tag?.post_count}";
		//private string Wiki_Description => $"Description: {wiki?.body}";

		public TagsSelectionView() {
			this.InitializeComponent();
			this.tags = new ObservableCollection<string>();
		}

		protected override void OnNavigatedTo(NavigationEventArgs e) {
			base.OnNavigatedTo(e);
			if(e.Parameter is object[] objs) {
				this.dialog = objs[0] as ContentDialog;
				foreach(string item in objs[1] as string[]) {
					//this.tags.Add(item);
					MySuggestBox.Text += item + " ";
				}
				MySuggestBox.Text = MySuggestBox.Text.Trim();
			}
		}

		private void MySuggestBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args) {
			this.tags.Clear();
			//if(args.Reason != AutoSuggestionBoxTextChangeReason.ProgrammaticChange) {
			foreach(string item in sender.Text.Trim().Split(" ").Where(s => !string.IsNullOrEmpty(s))) {
				this.tags.Add(item);
			}
			//}
			string last = this.tags.LastOrDefault();
			if(!string.IsNullOrWhiteSpace(last)) {
				//E621Tag[] found = await E621Tag.GetAsync(last);
				//if(found != null) {
				//	Debug.Write("Tags : ");
				//	foreach(E621Tag item in found) {
				//		Debug.Write(item + " ");
				//	}
				//	Debug.WriteLine("");
				//}
			}
		}

		private async void InfoButton_Tapped(object sender, TappedRoutedEventArgs e) {
			//MainSplitView.IsPaneOpen = true;
			string tag = (sender as Button).Tag as string;
			E621Tag e621tag = (await E621Tag.GetAsync(tag))?.FirstOrDefault();
			if(e621tag != null && !e621tag.IsWikiLoaded) {
				await e621tag.LoadWikiAsync();
			}
			//TextBlock_ID.Text = Tag_ID;
			//TextBlock_Name.Text = Tag_Name;
			//TextBlock_Count.Text = Tag_Count;
			//TextBlock_Description.Text = Wiki_Description;
			(dialog.Content as Frame).Navigate(typeof(TagInformationView), new object[] { dialog, tags.ToArray(), e621tag });
		}

		private void DeleteButton_Tapped(object sender, TappedRoutedEventArgs e) {
			string tag = (sender as Button).Tag as string;
			tags.Remove(tag);
			MySuggestBox.Text = MySuggestBox.Text.Replace(tag, "").Trim();
		}

		//private void BackButton_Tapped(object sender, TappedRoutedEventArgs e) {
		//	MainSplitView.IsPaneOpen = false;
		//}

		//private void MainSplitView_PaneClosing(SplitView sender, SplitViewPaneClosingEventArgs args) {
		//	args.Cancel = true;
		//}

		private void SearchButton_Tapped(object sender, TappedRoutedEventArgs e) {
			handleSearch = true;
			dialog.Hide();
		}

		private void DialogBackButton_Tapped(object sender, TappedRoutedEventArgs e) {
			dialog.Hide();
		}
	}
}
