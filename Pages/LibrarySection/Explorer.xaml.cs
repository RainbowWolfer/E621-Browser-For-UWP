using E621Downloader.Models.Locals;
using E621Downloader.Views.FoldersSelectionSection;
using E621Downloader.Views.LocalTagsManagementSection;
using E621Downloader.Views.TagsManagementSection;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

namespace E621Downloader.Pages.LibrarySection {
	public sealed partial class Explorer: Page {
		private LibraryPage libraryPage;
		private readonly List<StorageFolder> folders;
		private readonly List<StorageFolder> orginalFolders;
		public ObservableCollection<ItemBlock> items;
		private readonly List<ItemBlock> originalItems;
		private readonly List<FontIcon> folderIcons;

		private readonly List<string> selectedTags;

		private readonly List<Grid> itemGrids;

		public int ItemWidth => libraryPage.Size;
		public int ItemHeight => libraryPage.Size - 30;

		public int FolderFontSize => libraryPage.Size < 200 ? 60 : 120;

		public LibraryTab CurrentLibraryTab { get; private set; }
		public ItemBlock CurrentItemBlock { get; private set; }

		//private bool refreshNeeded;

		public Explorer() {
			this.InitializeComponent();
			//this.DataContextChanged += (s, e) => Bindings.Update();

			items = new ObservableCollection<ItemBlock>();
			folders = new List<StorageFolder>();
			originalItems = new List<ItemBlock>();
			orginalFolders = new List<StorageFolder>();
			selectedTags = new List<string>();
			itemGrids = new List<Grid>();
			folderIcons = new List<FontIcon>();
		}

		protected async override void OnNavigatedTo(NavigationEventArgs e) {
			base.OnNavigatedTo(e);
			FilterGrid.Visibility = Visibility.Collapsed;
			itemGrids.Clear();
			folderIcons.Clear();
			if(e.Parameter is object[] objs) {
				if(objs.Length >= 2 && objs[1] is LibraryPage libraryPage) {
					this.libraryPage = libraryPage;
					libraryPage.current = this;
					if(objs[0] is LibraryTab tab) {//side tab
						CurrentLibraryTab = tab;
						if(tab.title == "Home") {//click home
							StorageFolder[] downloadsFolders = await Local.GetDownloadsFolders();
							if(downloadsFolders == null) {
								HintText.Text = "You have not select your download folder\nPlease go to the settings page and choose your download folder";
								HintText.Visibility = Visibility.Visible;
							} else {
								foreach(StorageFolder folder in downloadsFolders) {
									folders.Add(folder);
									var myitem = new ItemBlock() {
										parentFolder = folder,
										parent = null,
									};
									items.Add(myitem);
									originalItems.Add(myitem);
								}
								if(folders.Count == 0) {
									HintText.Text = "No Download Folders Found";
									HintText.Visibility = Visibility.Visible;
								}
							}
						} else if(tab.title == "Filter") {//click filter page
							Debug.WriteLine("Filter VIEW");
							FilterGrid.Visibility = Visibility.Visible;
							foreach(StorageFolder folder in await Local.GetDownloadsFolders()) {
								//folders.Add(folder);
								orginalFolders.Add(folder);
							}


						} else {//click folder
							List<(MetaFile, BitmapImage, StorageFile)> v = await Local.GetMetaFiles(tab.folder.DisplayName);
							foreach((MetaFile, BitmapImage, StorageFile) item in v) {
								if(!item.Item1.FinishedDownloading) {
									continue;
								}
								var myitem = new ItemBlock() {
									meta = item.Item1,
									thumbnail = item.Item2,
									imageFile = item.Item3,
								};
								items.Add(myitem);
								originalItems.Add(myitem);
							}
						}
					} else if(objs[0] is ItemBlock parent) {//click folder in page
						CurrentItemBlock = parent;
						List<(MetaFile, BitmapImage, StorageFile)> v = await Local.GetMetaFiles(parent.Name);
						foreach((MetaFile, BitmapImage, StorageFile) item in v) {
							if(!item.Item1.FinishedDownloading) {
								continue;
							}
							var myitem = new ItemBlock() {
								meta = item.Item1,
								thumbnail = item.Item2,
								imageFile = item.Item3,
								parent = parent,
							};
							items.Add(myitem);
							originalItems.Add(myitem);
						}
					} else {
						throw new Exception("?");
					}
				}
			}
			//refreshNeeded = false;
			MyProgressRing.IsActive = false;
			libraryPage.EnableRefreshButton(false);
		}

		private void MyGridView_ItemClick(object sender, ItemClickEventArgs e) {
			var target = e.ClickedItem as ItemBlock;
			if(target.IsFolder) {
				libraryPage.Navigate(typeof(Explorer), new object[] { target, libraryPage });
				libraryPage.ToTab(folders.Find(f => f.DisplayName == target.Name), target.Name);
			} else {
				App.postsList.UpdatePostsList(items.ToList());
				App.postsList.Current = target;
				MainPage.Instance.parameter_picture = target;
				MainPage.SelectNavigationItem(PageTag.Picture);
			}
		}

		public void Search(string content) {
			//itemGrids.Clear();
			foreach(ItemBlock o in originalItems) {
				if(o.Name.Contains(content)) {
					if(!items.Contains(o)) {
						items.Add(o);
					}
				} else {
					if(items.Contains(o)) {
						items.Remove(o);
					}
				}
			}
		}

		public void ClearSearch() {

		}

		public void RefreshRequest() {
			//refreshNeeded = true;
			libraryPage.EnableRefreshButton(true);
		}

		public void UpdateSize(int width, int height) {
			foreach(Grid item in itemGrids) {
				if(item == null) {
					continue;
				}
				item.Height = height;
				item.Width = width;
			}
			int fontsize = 120;
			int size = Math.Max(width, height);
			if(size < 130) {
				fontsize = 30;
			} else if(size < 200) {
				fontsize = 60;
			}
			foreach(FontIcon item in folderIcons) {
				item.FontSize = fontsize;
			}
		}

		private GroupTagList defaultInput;
		private async void TagsButton_Tapped(object sender, TappedRoutedEventArgs e) {
			var dialog = new ContentDialog() {
				Title = "Manage Your Search Tags",
			};
			var content = new LocalTagsManagementView(dialog, defaultInput);
			dialog.Content = content;
			await dialog.ShowAsync();

			if(content.HandleConfirm) {
				defaultInput = content.GetInput();
				selectedTags.Clear();
				string tooltip = "";
				string text = "Selected Tags";
				int count = 0;
				bool max = false;
				foreach(string item in content.GetResult()) {
					if(defaultInput != null && !defaultInput.Contains(item)) {
						defaultInput.Add(item);
					}
					selectedTags.Add(item);
					tooltip += item + "\n";
					if(count++ < 3) {
						text += " : " + item;
					} else if(!max) {
						max = true;
						text += " ...";
					}
				}
				SelectedTagsText.Text = text.Trim();
				SelectedTagsCount.Text = selectedTags.Count.ToString();
				ToolTipService.SetToolTip(sender as Button, tooltip.Trim());

			}
		}

		private async void FoldersButton_Tapped(object sender, TappedRoutedEventArgs e) {
			if(orginalFolders == null | orginalFolders.Count == 0) {
				return;
			}
			var foldersView = new FoldersSelectionView(orginalFolders, folders);
			if(await new ContentDialog() {
				Title = "Manage Your Search Tags",
				Content = foldersView,
				PrimaryButtonText = "Confirm",
				CloseButtonText = "Back",
			}.ShowAsync() == ContentDialogResult.Primary) {
				folders.Clear();
				string tooltip = "";
				string text = "Active Folders";
				int count = 0;
				bool max = false;
				foreach(StorageFolder item in foldersView.GetSelected()) {
					folders.Add(item);
					tooltip += item.DisplayName + "\n";
					if(count++ < 3) {
						text += " : " + item.DisplayName;
					} else if(!max) {
						max = true;
						text += " ...";
					}
				}
				SelectedFoldersText.Text = text.Trim();
				SelectedFoldersCount.Text = folders.Count.ToString();
				ToolTipService.SetToolTip(sender as Button, tooltip.Trim());
			}
		}

		private async void SearchButton_Tapped(object sender, TappedRoutedEventArgs e) {
			MyProgressRing.IsActive = false;
			MyLoadingBar.IsIndeterminate = true;
			MyLoadingBar.Visibility = Visibility.Visible;
			items.Clear();
			itemGrids.Clear();
			folderIcons.Clear();
			await Task.Delay(100);

			foreach(StorageFolder folder in folders) {
				List<(MetaFile, BitmapImage, StorageFile)> v = await Local.GetMetaFiles(folder.Name);
				foreach((MetaFile, BitmapImage, StorageFile) item in v) {
					if(!GetSelectedRating().Contains(item.Item1.MyPost.rating)) {
						continue;
					}
					int score = item.Item1.MyPost.score.total;
					int min = string.IsNullOrEmpty(MinScoreText.Text) ? int.MinValue : int.Parse(MinScoreText.Text);
					int max = string.IsNullOrEmpty(MaxScoreText.Text) ? int.MaxValue : int.Parse(MaxScoreText.Text);
					if(score < min || score > max) {
						continue;
					}
					List<string> tags = item.Item1.MyPost.tags.GetAllTags();
					foreach(string tag in selectedTags) {
						if(tags.Contains(tag)) {
							var myitem = new ItemBlock() {
								meta = item.Item1,
								thumbnail = item.Item2,
								imageFile = item.Item3,
							};
							items.Add(myitem);
							break;
						}
					}
				}
			}

			await Task.Delay(20);
			MyLoadingBar.IsIndeterminate = false;
			MyLoadingBar.Visibility = Visibility.Collapsed;
		}
		private string[] GetSelectedRating() {
			var r = new List<string>();
			if(SCheckBox.IsChecked == true) {
				r.Add("s");
			}
			if(QCheckBox.IsChecked == true) {
				r.Add("q");
			}
			if(ECheckBox.IsChecked == true) {
				r.Add("e");
			}
			return r.ToArray();
		}

		private void TextBox_TextChanged(object sender, TextChangedEventArgs e) {
			var textbox = sender as TextBox;
			if(!int.TryParse(textbox.Text, out int result) && textbox.Text != "" && textbox.Text != "-") {
				int pos = textbox.SelectionStart - 1;
				textbox.Text = textbox.Text.Remove(pos, 1);
				textbox.SelectionStart = pos;
			}
		}

		private void ItemGrid_Loaded(object sender, RoutedEventArgs e) {
			itemGrids.Add(sender as Grid);
			if((sender as Grid).Children.First() is FontIcon icon) {
				folderIcons.Add(icon);
			}
			if(folderIcons.Count >= items.Count) {
				UpdateSize(libraryPage.Size, libraryPage.Size - 30);
			}
		}
	}

	public class ItemBlock {
		public StorageFolder parentFolder;
		public StorageFile imageFile;
		public MetaFile meta;
		public ItemBlock parent;
		public ImageSource thumbnail;

		public string Name => meta == null ? parentFolder.DisplayName : meta.MyPost.id.ToString();
		public bool IsFolder => parentFolder != null;

		public Visibility FolderIconVisibility => IsFolder ? Visibility.Visible : Visibility.Collapsed;
		public Visibility ImagePreviewVisibility => IsFolder ? Visibility.Collapsed : Visibility.Visible;

		public Visibility TypeBorderVisibility => meta != null && new string[] { "webm", "gif", "anim" }.Contains(meta.MyPost?.file?.ext?.ToLower()) ? Visibility.Visible : Visibility.Collapsed;
		public string TypeText => meta != null && meta.MyPost != null && meta.MyPost.file != null ? meta.MyPost.file.ext : "UNDEFINED";
	}
}
