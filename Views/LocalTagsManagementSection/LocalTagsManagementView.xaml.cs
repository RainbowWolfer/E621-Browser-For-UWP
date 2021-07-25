using E621Downloader.Models.Locals;
using E621Downloader.Pages;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace E621Downloader.Views.LocalTagsManagementSection {
	public sealed partial class LocalTagsManagementView: UserControl {
		private readonly ContentDialog dialog;
		public bool HandleConfirm { get; private set; }
		public ObservableCollection<GroupTagList> tags;
		private GroupTagList local;
		private GroupTagList input;

		public ObservableCollection<LocalTagsInfo> localTags;
		public LocalTagsManagementView(ContentDialog dialog, GroupTagList defaultInput) {
			this.InitializeComponent();
			this.dialog = dialog;
			HandleConfirm = false;
			tags = new ObservableCollection<GroupTagList>();
			localTags = new ObservableCollection<LocalTagsInfo>();
			if(defaultInput == null) {
				input = AddNewGroup("Input", new List<string>());
			} else {
				input = AddNewGroup(defaultInput.Key, defaultInput.ToList());
				Debug.WriteLine(input.Key);
				foreach(string item in input) {
					MyInputBox.Text += item + " ";
				}
			}
			local = AddNewGroup("Local", new List<string>());
			input.CollectionChanged += (s, e) => UpdateConfirmButtonToolTip();
			local.CollectionChanged += (s, e) => UpdateConfirmButtonToolTip();
			LoadLocalTags();
		}

		private async void LoadLocalTags() {
			LoadingPanel.Visibility = Visibility.Visible;
			localTags.Clear();
			List<LocalTagsInfo> list = (await GetLocalTagsInfo()).OrderByDescending(s => s.count).ToList();
			foreach(LocalTagsInfo item in list) {
				localTags.Add(item);
			}
			CountTextBlock.Text = "Tags Found : " + list.Count;
			LoadingPanel.Visibility = Visibility.Collapsed;
		}

		private async Task<List<LocalTagsInfo>> GetLocalTagsInfo() {
			var tags = new List<LocalTagsInfo>();
			foreach(MetaFile item in await Local.GetAllMetaFiles()) {
				foreach(string t in item.MyPost.tags.GetAllTags()) {
					LocalTagsInfo.AddToList(tags, new LocalTagsInfo(t));
				}
			}
			return tags;
		}

		private GroupTagList AddNewGroup(string title, List<string> content) {
			if(content == null) {
				return null;
			}
			var result = new GroupTagList(title, content);
			tags.Add(result);
			return result;
		}

		public List<string> GetResult() {
			var result = new List<string>();
			foreach(string item in input) {
				result.Add(item);
			}
			foreach(string item in local) {
				if(!result.Contains(item)) {
					result.Add(item);
				}
			}
			return result;
		}

		public GroupTagList GetInput() => input;

		private void UpdateConfirmButtonToolTip() {
			string text = "";
			foreach(string item in GetResult()) {
				text += item + "\n";
			}
			ToolTipService.SetToolTip(ConfirmButton, text.Trim());
		}

		private void ConfirmButton_Tapped(object sender, TappedRoutedEventArgs e) {
			//input.Add("213weq");
			HandleConfirm = true;
			dialog.Hide();
		}

		private void BackButton_Tapped(object sender, TappedRoutedEventArgs e) {
			//local.Add("jiwpns2");
			dialog.Hide();
		}

		private void CloseButton_Tapped(object sender, TappedRoutedEventArgs e) {
			string tag = (sender as Button).Tag as string;
			if(input.Contains(tag)) {
				input.Remove(tag);
				MyInputBox.Text = MyInputBox.Text.Replace(tag, "").Replace("  ", " ");
			}
			if(local.Contains(tag)) {
				local.Remove(tag);
				for(int i = 0; i < LocalList.SelectedItems.Count; i++) {
					if(LocalList.SelectedItems[i] is LocalTagsInfo info && info.name == tag) {
						LocalList.SelectedItems.RemoveAt(i);
						break;
					}
				}
			}
		}

		//private void LocalList_ItemClick(object sender, ItemClickEventArgs e) {
		//	var info = e.ClickedItem as LocalTagsInfo;

		//}

		private void MyInputBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args) {
			input.Clear();
			foreach(string item in sender.Text.Trim().Split(" ").Where(s => !string.IsNullOrEmpty(s))) {
				input.Add(item);
			}
		}

		private void LocalList_SelectionChanged(object sender, SelectionChangedEventArgs e) {
			local.Clear();
			foreach(var item in LocalList.SelectedItems) {
				if(item is LocalTagsInfo info) {
					local.Add(info.name);
				}
			}
		}
	}

	public class LocalTagsInfo {
		public string name;
		public int count = 1;

		public LocalTagsInfo(string name) {
			this.name = name;
		}

		public static void AddToList(List<LocalTagsInfo> list, LocalTagsInfo newOne) {
			foreach(LocalTagsInfo item in list) {
				if(item.name == newOne.name) {
					item.count++;
					return;
				}
			}
			list.Add(newOne);
		}
	}
}
