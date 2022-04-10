using E621Downloader.Models;
using E621Downloader.Models.Locals;
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
		private bool addEnable;
		private readonly PathType type;
		private readonly string path;
		private readonly Flyout flyout;
		private readonly ContentDialog dialog;
		private bool showBackButton = false;
		private bool showE621FavoriteButton = false;
		private bool isInitialFavorited;

		public bool AddEnable {
			get => addEnable;
			set {
				addEnable = value;
				AddTextAnimation.Children[0].SetValue(DoubleAnimation.FromProperty, AddGrid.Height);
				AddTextAnimation.Children[0].SetValue(DoubleAnimation.ToProperty, value ? 45 : 0);
				AddTextAnimation.Begin();
				AddButtonIcon.Glyph = value ? "\uE09C" : "\uE109";
			}
		}

		public IEnumerable<CheckBox> CheckRecords {
			get {
				return CheckListView.Items.Cast<Grid>().Select(g => g.Children[0] as CheckBox);
			}
		}

		public bool ShowBackButton {
			get => showBackButton;
			set => showBackButton = value;
		}

		public Visibility BackButtonVisiblity => ShowBackButton ? Visibility.Visible : Visibility.Collapsed;

		public bool ShowE621FavoriteButton {
			get => showE621FavoriteButton;
			set {
				if(string.IsNullOrWhiteSpace(LocalSettings.Current.user_username) || string.IsNullOrWhiteSpace(LocalSettings.Current.user_api)) {
					showE621FavoriteButton = false;
				} else {
					showE621FavoriteButton = value;
				}
			}
		}

		public Visibility E621FavoriteVisibility => showE621FavoriteButton ? Visibility.Visible : Visibility.Collapsed;

		public bool IsInitialFavorited {
			get => isInitialFavorited;
			set {
				isInitialFavorited = value;
				if(ShowE621FavoriteButton) {
					E621FavoriteButton.IsChecked = value;
					if(value) {
						FavoriteText.Text = "E621 Favorited";
						FavoriteIcon.Glyph = "\uEB52";
					} else {
						FavoriteText.Text = "E621 Favorite";
						FavoriteIcon.Glyph = "\uEB51";
					}
				}
			}
		}

		public PersonalFavoritesList(Flyout flyout, PathType type, string path) {
			this.InitializeComponent();
			this.flyout = flyout;
			this.type = type;
			this.path = path;
			Initialize();
		}

		public PersonalFavoritesList(ContentDialog dialog, PathType type, string path) {
			this.InitializeComponent();
			this.dialog = dialog;
			this.type = type;
			this.path = path;
			Initialize();
		}

		private void Initialize() {
			foreach(FavoritesList item in FavoritesList.Table) {
				CheckListView.Items.Add(GenerateItem(item.Name, item.Items.Count, false, item.Contains(type, path)));
			}
			UpdateHintText();
		}

		private void AcceptButton_Tapped(object sender, TappedRoutedEventArgs e) {
			List<string> newLists = new();
			List<string> containLists = new();
			foreach(CheckBox item in CheckRecords) {
				string name = (string)item.Content;
				if(((CheckBoxTag)item.Tag).IsNew) {
					newLists.Add(name);
				}
				if(item.IsChecked.Value) {
					containLists.Add(name);
				}
			}
			FavoritesList.Modify(newLists, containLists, path, type);
			if(flyout != null) {
				flyout.Hide();
			} else if(dialog != null) {
				dialog.Hide();
			} else {
				throw new Exception("No Parent To Hide");
			}
		}

		private void AddButton_Tapped(object sender, TappedRoutedEventArgs e) {
			AddEnable = !AddEnable;
			if(AddEnable) {
				NewTextBox.Focus(FocusState.Keyboard);
			}
		}

		private bool CheckDuplicateName(string name) {
			return CheckRecords.Any(r => (string)r.Content == name);
		}

		private void NewButton_Tapped(object sender, TappedRoutedEventArgs e) {
			if(string.IsNullOrWhiteSpace(NewTextBox.Text)) {
				return;
			} else if(CheckDuplicateName(NewTextBox.Text.Trim())) {
				return;
			}
			AddNewList(NewTextBox.Text);
			NewTextBox.Text = "";
		}

		private void NewTextBox_PreviewKeyDown(object sender, KeyRoutedEventArgs e) {
			MainPage.Instance.DelayInputKeyListener();
			if(e.Key == VirtualKey.Enter) {
				if(string.IsNullOrWhiteSpace(NewTextBox.Text)) {
					return;
				} else if(CheckDuplicateName(NewTextBox.Text.Trim())) {
					return;
				}
				AddNewList(NewTextBox.Text);
				NewTextBox.Text = "";
			} else if(e.Key == VirtualKey.Escape) {
				AddEnable = false;
				NewTextBox.Text = "";
			}
		}

		private void AddNewList(string name) {
			CheckListView.Items.Add(GenerateItem(name, 0, true));
			AddEnable = false;
			UpdateHintText();
		}

		private void UpdateHintText() {
			if(CheckListView.Items.Count == 0) {
				HintText.Visibility = Visibility.Visible;
			} else {
				HintText.Visibility = Visibility.Collapsed;
			}
		}

		private Grid GenerateItem(string name, int count, bool isNew, bool isChecked = false) {
			Grid grid = new();
			grid.ColumnDefinitions.Add(new ColumnDefinition());
			grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(0, GridUnitType.Auto) });
			CheckBox box = new() {
				Content = name,
				Tag = new CheckBoxTag() { IsNew = isNew },
				FontSize = 16,
				IsChecked = isChecked,
			};
			TextBlock countText = new() {
				Text = $"{count}",
				FontSize = 16,
				HorizontalAlignment = HorizontalAlignment.Right,
				VerticalAlignment = VerticalAlignment.Center,
			};
			Grid.SetColumn(countText, 1);
			grid.Children.Add(box);
			grid.Children.Add(countText);
			return grid;
		}

		private struct CheckBoxTag {
			public bool IsNew { get; set; }
		}

		private void BackButton_Tapped(object sender, TappedRoutedEventArgs e) {
			if(flyout != null) {
				flyout.Hide();
			} else if(dialog != null) {
				dialog.Hide();
			} else {
				throw new Exception("No Parent To Hide");
			}
		}

		private async void E621FavoriteButton_Tapped(object sender, TappedRoutedEventArgs e) {
			e.Handled = true;
			if(!ShowE621FavoriteButton || type != PathType.PostID) {
				return;
			}
			E621FavoriteButton.IsEnabled = false;
			FavoriteText.Text = "Pending";
			FavoriteIcon.Glyph = "\uE10C";

			if(E621FavoriteButton.IsChecked.Value) {
				HttpResult<string> result = await Favorites.PostAsync(path);
				if(result.Result == HttpResultType.Success) {
					FavoriteText.Text = "E621 Favorited";
					FavoriteIcon.Glyph = "\uEB52";
				} else {
					FavoriteText.Text = "E621 Favorite";
					FavoriteIcon.Glyph = "\uEB51";
					MainPage.CreateTip(MainGrid, result.StatusCode.ToString(), result.Helper, Symbol.Important, "OK");
					E621FavoriteButton.IsChecked = false;
				}
			} else {
				HttpResult<string> result = await Favorites.DeleteAsync(path);
				if(result.Result == HttpResultType.Success) {
					FavoriteText.Text = "E621 Favorite";
					FavoriteIcon.Glyph = "\uEB51";
				} else {
					FavoriteText.Text = "E621 Favorited";
					FavoriteIcon.Glyph = "\uEB52";
					MainPage.CreateTip(MainGrid, result.StatusCode.ToString(), result.Helper, Symbol.Important, "OK");
					E621FavoriteButton.IsChecked = true;
				}
			}
			E621FavoriteButton.IsEnabled = true;
		}
	}
}