using E621Downloader.Models.Locals;
using E621Downloader.Pages;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

namespace E621Downloader.Views {
	public sealed partial class PersonalFavoritesList: UserControl {
		public ObservableCollection<FavoritesList> Lists { get; } = new ObservableCollection<FavoritesList>();

		private bool addEnable;

		private readonly List<CheckRecord> checkList = new List<CheckRecord>();
		private PathType type;
		private string path;
		public bool AddEnable {
			get => addEnable;
			set {
				addEnable = value;
				AddTextAnimation.Children[0].SetValue(DoubleAnimation.FromProperty, AddGrid.Height);
				AddTextAnimation.Children[0].SetValue(DoubleAnimation.ToProperty, value ? 45 : 0);
				AddTextAnimation.Begin();
			}
		}

		private Flyout flyout;

		public PersonalFavoritesList(Flyout flyout, PathType type, string path) {
			this.InitializeComponent();
			this.flyout = flyout;
			this.type = type;
			this.path = path;
			RefreshLists();
		}

		private void RefreshLists() {
			Lists.Clear();
			checkList.Clear();
			if(FavoritesList.Lists == null || FavoritesList.Lists.Count == 0) {
				HintText.Visibility = Visibility.Visible;
			} else {
				HintText.Visibility = Visibility.Collapsed;
			}
			foreach(FavoritesList item in FavoritesList.Lists) {
				Lists.Add(item);
				checkList.Add(new CheckRecord(item, false));
			}
		}

		private void AcceptButton_Tapped(object sender, TappedRoutedEventArgs e) {
			foreach(CheckRecord item in checkList) {
				if(!item.isChecked) {
					continue;
				}
				switch(type) {
					case PathType.PostID:
						item.list.AddPostID(path);
						break;
					case PathType.Local:
						item.list.AddLocal(path);
						break;
					default:
						throw new PathTypeException();
				}
			}
			flyout.Hide();
		}

		private void AddButton_Tapped(object sender, TappedRoutedEventArgs e) {
			AddEnable = !AddEnable;
			AddButton.Content = AddEnable ? "\uE09C" : "\uE109";
			if(AddEnable) {
				NewTextBox.Focus(FocusState.Keyboard);
			}
		}

		private void NewButton_Tapped(object sender, TappedRoutedEventArgs e) {
			AddNewList(NewTextBox.Text);
			NewTextBox.Text = "";
		}

		private void NewTextBox_PreviewKeyDown(object sender, KeyRoutedEventArgs e) {
			if(e.Key == VirtualKey.Enter) {
				AddNewList(NewTextBox.Text);
			} else if(e.Key == VirtualKey.Escape) {
				AddEnable = false;
			}
			NewTextBox.Text = "";
		}

		private void AddNewList(string name) {
			FavoritesList.AddList(name.Trim());
			RefreshLists();
			AddEnable = false;
		}

		private void CheckBox_Checked(object sender, RoutedEventArgs e) {
			string listName = (string)((CheckBox)sender).Content;
			for(int i = 0; i < checkList.Count; i++) {
				if(listName == checkList[i].list.Name) {
					checkList[i].isChecked = true;
					break;
				}
			}
		}

		private void CheckBox_Unchecked(object sender, RoutedEventArgs e) {
			string listName = (string)((CheckBox)sender).Content;
			for(int i = 0; i < checkList.Count; i++) {
				if(listName == checkList[i].list.Name) {
					checkList[i].isChecked = false;
					break;
				}
			}
		}

		private class CheckRecord {
			public FavoritesList list;
			public bool isChecked;
			public CheckRecord(FavoritesList list, bool isChecked) {
				this.list = list;
				this.isChecked = isChecked;
			}
		}
	}
}
