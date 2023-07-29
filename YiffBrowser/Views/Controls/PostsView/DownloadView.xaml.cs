using Microsoft.Toolkit.Uwp.UI;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.Storage;
using Windows.System;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using YiffBrowser.Helpers;
using YiffBrowser.Interfaces;
using YiffBrowser.Models;
using YiffBrowser.Models.E621;
using YiffBrowser.Services.Locals;

namespace YiffBrowser.Views.Controls.PostsView {
	public sealed partial class DownloadView : UserControl, IContentDialogView {
		private ContentDialog dialog;

		public static readonly ContentDialogParameters contentDialogParameters = new() {
			Title = "Select Download Folder",
			CloseText = "Back",
			SecondaryText = "Open Download Folder",
			PrimaryText = "Download",
			DefaultButton = ContentDialogButton.Primary,
			MaxWidth = ContentDialogParameters.DEFAULT_MAX_WIDTH,
		};

		public DownloadView(E621Post[] posts, bool isInSelectionMode) {
			this.InitializeComponent();
			ViewModel.RequestFocusCreateNewFolderTextBox += ViewModel_RequestFocusCreateNewFolderTextBox;
			ViewModel.Posts = posts;
			ViewModel.IsInSelectionMode = isInSelectionMode;
		}

		public ContentDialog Dialog {
			get => dialog;
			set {
				dialog = value;
				ViewModel.Dialog = dialog;

				dialog.Closing += (s, e) => {
					if (e.Result == ContentDialogResult.Secondary) {
						e.Cancel = true;
					}
					if (e.Result == ContentDialogResult.Primary && ViewModel.IsLoading) {
						e.Cancel = true;
					}
				};

				dialog.SecondaryButtonClick += async (s, e) => {
					string path = Local.DownloadFolder.Path;
					await Launcher.LaunchFolderAsync(Local.LocalFolder, new FolderLauncherOptions() {
						DesiredRemainingView = ViewSizePreference.UseMore,
					});
				};
			}
		}

		private async void ViewModel_RequestFocusCreateNewFolderTextBox() {
			await Task.Delay(10);
			CreateNewFolderTextBox.Focus(FocusState.Programmatic);
		}

		public DownloadViewResult GetResult() => ViewModel.GetResult();
	}

	public class DownloadViewResult {
		public string FolderName { get; set; }
		public bool MultiplePages { get; set; }
		public int FromPage { get; set; }
		public int ToPage { get; set; }

		public DownloadViewResult(string folderName, int fromPage, int toPage) {
			FolderName = folderName;
			MultiplePages = true;
			FromPage = fromPage;
			ToPage = toPage;
		}
		public DownloadViewResult(string folderName) {
			FolderName = folderName;
			MultiplePages = false;
			FromPage = -1;
			ToPage = -1;
		}
	}

	public class DownloadViewModel : BindableBase {
		public event Action RequestFocusCreateNewFolderTextBox;

		private DownloadFolderInfo selectedItem;
		private bool showCreateNewFolder = false;
		private string newFolderName = string.Empty;
		private bool sortAscending;
		private int sortMethodSelectedIndex = 0;
		private bool creatingFolder;
		private bool isLoading;
		private CollectionSearchFilter<DownloadFolderInfo> filter;
		private E621Post[] posts;
		private int totalFileCount;
		private int totalFileSize;
		private bool chooseMultiPages;
		private bool isInSelectionMode;
		private int pageStart;
		private int pageEnd;

		public ContentDialog Dialog { get; set; }

		public ICommand SearchCommand => new DelegateCommand<AutoSuggestBoxQuerySubmittedEventArgs>(Search);

		public ObservableCollection<DownloadFolderInfo> Folders { get; } = new ObservableCollection<DownloadFolderInfo>();

		public CollectionSearchFilter<DownloadFolderInfo> Filter {
			get => filter;
			set => SetProperty(ref filter, value);
		}

		public DownloadFolderInfo SelectedItem {
			get => selectedItem;
			set => SetProperty(ref selectedItem, value, OnSelectedItemChanged);
		}

		public bool ShowCreateNewFolder {
			get => showCreateNewFolder;
			set => SetProperty(ref showCreateNewFolder, value, () => {
				if (value) {
					RequestFocusCreateNewFolderTextBox?.Invoke();
				}
			});
		}

		public string NewFolderName {
			get => newFolderName;
			set => SetProperty(ref newFolderName, value);
		}

		public ICommand EnterCreateNewFolderCommand => new DelegateCommand(EnterCreateNewFolder);

		private void EnterCreateNewFolder() {
			ShowCreateNewFolder = true;
		}

		public ICommand AcceptCreateNewFolderCommand => new DelegateCommand(AcceptCreateNewFolder);

		private void AcceptCreateNewFolder() {
			CreateNewFolder(NewFolderName.Trim());
			NewFolderName = string.Empty;
			ShowCreateNewFolder = false;
		}

		public bool CreatingFolder {
			get => creatingFolder;
			set => SetProperty(ref creatingFolder, value);
		}

		private async void CreateNewFolder(string name) {
			if (name.IsBlank()) {
				return;
			}
			CreatingFolder = true;

			try {
				StorageFolder folder = await Local.DownloadFolder.CreateFolderAsync(name, CreationCollisionOption.FailIfExists);

				Folders.Add(new DownloadFolderInfo(folder.Path));
				Filter.Refresh();

			} catch (Exception ex) {
				Debug.WriteLine(ex);
			} finally {
				CreatingFolder = false;
				IEnumerable<DownloadFolderInfo> filtered = Filter.CollectionSource.OfType<DownloadFolderInfo>();
				DownloadFolderInfo found = filtered.FirstOrDefault(x => x.FolderName == name);
				if (found != null) {
					SelectedItem = found;
				}
			}
		}

		private void Search(AutoSuggestBoxQuerySubmittedEventArgs args) {
			Filter.Search(args.QueryText);
		}

		public bool SortAscending {
			get => sortAscending;
			set => SetProperty(ref sortAscending, value, () => {
				UpdateSort();
			});
		}

		public ICommand ToggleSortAscending => new DelegateCommand(() => {
			SortAscending = !SortAscending;
		});

		public int SortMethodSelectedIndex {
			get => sortMethodSelectedIndex;
			set => SetProperty(ref sortMethodSelectedIndex, value, () => {
				UpdateSort();
			});
		}

		private void UpdateSort() {
			SortDirection sortDirection = SortAscending ? SortDirection.Ascending : SortDirection.Descending;

			string propertyName = SortMethodSelectedIndex switch {
				0 => "ModifiedDate",
				1 => "CreatedDate",
				2 => "FolderName",
				_ => throw new NotImplementedException(),
			};

			Filter.UpdateSort(
				new SortDescription("IsRoot", SortDirection.Descending),
				new SortDescription(propertyName, sortDirection)
			);
		}

		public string RootPath { get; private set; }

		public DownloadViewModel() {
			if (Local.DownloadFolder == null) {
				throw new ArgumentNullException("Local.DownloadFolder");
			}

		}

		public bool IsLoading {
			get => isLoading;
			set => SetProperty(ref isLoading, value);
		}

		public ICommand LoadedCommand => new DelegateCommand(Loaded);
		private async void Loaded() {
			IsLoading = true;

			RootPath = Local.DownloadFolder.Path;

			await Task.Factory.StartNew(() => {
				string[] allPaths = Directory.GetDirectories(RootPath);

				Folders.Add(new DownloadFolderInfo(RootPath, true));
				foreach (string path in allPaths) {
					Folders.Add(new DownloadFolderInfo(path));
				}
			});

			Filter = new CollectionSearchFilter<DownloadFolderInfo>(Folders, x => x.FolderName);

			SelectedItem = Folders.FirstOrDefault();

			IsLoading = false;
		}

		public ICommand GotFocusCommand => new DelegateCommand(GotFocus);

		public E621Post[] Posts {
			get => posts;
			set => SetProperty(ref posts, value, () => {
				TotalFileCount = Posts.Length;
				TotalFileSize = Posts.Select(x => x.File?.Size ?? 0).Sum();
			});
		}

		private void GotFocus() {
			ShowCreateNewFolder = false;
		}

		private void OnSelectedItemChanged() {
			if (SelectedItem == null) {
				Dialog.PrimaryButtonText = "Download";
			} else {
				Dialog.PrimaryButtonText = $"Download > {SelectedItem.FolderName}";

			}
		}

		public DownloadViewResult GetResult() {
			if (SelectedItem == null) {
				return null;
			}

			DownloadViewResult result;
			if (ChooseMultiPages) {
				result = new(SelectedItem.FolderName, PageStart, PageEnd);
			} else {
				result = new(SelectedItem.FolderName);
			}

			return result;
		}

		public bool IsInSelectionMode {
			get => isInSelectionMode;
			set => SetProperty(ref isInSelectionMode, value);
		}
		public bool ChooseMultiPages {
			get => chooseMultiPages;
			set => SetProperty(ref chooseMultiPages, value);
		}
		public int PageStart {
			get => pageStart;
			set => SetProperty(ref pageStart, value);
		}
		public int PageEnd {
			get => pageEnd;
			set => SetProperty(ref pageEnd, value);
		}

		public int TotalFileCount {
			get => totalFileCount;
			set => SetProperty(ref totalFileCount, value);
		}
		public int TotalFileSize {
			get => totalFileSize;
			set => SetProperty(ref totalFileSize, value);
		}

	}


	public class DownloadFolderInfo : BindableBase {
		private bool isRoot;
		private string folderPath;
		private string toolTip;

		public string FolderName => Path.GetFileName(FolderPath);
		public DateTime CreatedDate { get; private set; }
		public DateTime ModifiedDate { get; private set; }

		public string FolderPath {
			get => folderPath;
			private set => SetProperty(ref folderPath, value, OnFolderPathChanged);
		}

		public bool IsRoot {
			get => isRoot;
			private set => SetProperty(ref isRoot, value);
		}

		public string ToolTip {
			get => toolTip;
			private set => SetProperty(ref toolTip, value);
		}

		public DownloadFolderInfo(string folderPath, bool isRoot = false) {
			FolderPath = folderPath;
			IsRoot = isRoot;
		}

		private void OnFolderPathChanged() {
			DirectoryInfo folderInfo = new(FolderPath);
			string[] files = Directory.GetFiles(FolderPath, "*", SearchOption.TopDirectoryOnly);

			CreatedDate = folderInfo.CreationTime;
			ModifiedDate = folderInfo.LastWriteTime;

			ToolTip = $"{FolderPath}\n" +
				$"Flies Count: {files.Length}\n" +
				$"Created Date: {folderInfo.CreationTime}\n" +
				$"Modified Date: {folderInfo.LastWriteTime}";
		}

	}
}
