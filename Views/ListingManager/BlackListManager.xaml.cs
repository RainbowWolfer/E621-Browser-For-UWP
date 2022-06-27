using E621Downloader.Models;
using E621Downloader.Models.Locals;
using E621Downloader.Models.Networks;
using E621Downloader.Models.Posts;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using ProgressBar = Microsoft.UI.Xaml.Controls.ProgressBar;

namespace E621Downloader.Views.ListingManager {
	/// <summary>
	/// This is for both black list and follow list.
	/// </summary>
	public sealed partial class BlackListManager: UserControl {
		public event Action<string> OnNewListSubmit;
		public event Action<SingleListing, string> OnNewTagSubmit;
		public event Action<string[]> OnPasteImport;

		public event Action<ListingItem, string> OnListingRename;
		public event Action<SingleListing> OnListingDelete;
		public event Action<ListingItem> OnListingSetAsDefault;

		public event Action<SingleListing, ListingItemDetailItem> OnTagDelete;
		public event Action<SingleListing> OnTagsClearAll;

		public event Action<string[]> OnCloudSync;

		private readonly Dictionary<string, string> tagsInfo = new();

		private readonly List<SingleListing> listings;

		public ObservableCollection<ListingItem> Items { get; } = new ObservableCollection<ListingItem>();
		public ObservableCollection<ListingItemDetailItem> Tags { get; } = new ObservableCollection<ListingItemDetailItem>();

		private readonly Brush orgin_rename_brush;
		private ProgressBar cloudLoadingBar;
		private SingleListing current;

		public string HostCloud => $"{Data.GetSimpleHost()} Cloud";

		private CancellationTokenSource tagLoading_cts;

		public BlackListManager(List<SingleListing> listings) {
			this.InitializeComponent();
			this.DataContextChanged += (s, e) => Bindings.Update();
			this.listings = listings;
			Update(listings);
			ListingListView.SelectedItem = Items.FirstOrDefault(i => i.IsDefault) ?? Items.FirstOrDefault();
			AddButtonInput.OnTextSubmit += text => OnNewListSubmit?.Invoke(text);
			TagAddButtonInput.OnTextSubmit += text => {
				var item = (ListingItem)ListingListView.SelectedItem;
				if(item == null) {
					return;
				}
				OnNewTagSubmit?.Invoke(item.Listing, text);
			};
			Clipboard.ContentChanged += (s, e) => UpdatePastImportEnable();
			UpdatePastImportEnable();
			orgin_rename_brush = RenameBox.BorderBrush;
		}

		public void OnClose() {
			CancelTagLoadingCTS();
		}

		private void CancelTagLoadingCTS() {
			try {
				if(tagLoading_cts != null) {
					tagLoading_cts.Cancel();
					tagLoading_cts.Dispose();
				}
			} catch(ObjectDisposedException) {
			} finally {
				tagLoading_cts = null;
			}
		}

		public void Update(List<SingleListing> listings) {
			Items.Clear();
			foreach(SingleListing item in listings) {
				Items.Add(new ListingItem(item));
			}
			AddButtonInput.Existing = listings.Select(l => l.Name).ToList();
		}

		public async void UpdatePastImportEnable() {
			try {
				DataPackageView dataPackageView = Clipboard.GetContent();
				string text = null;
				if(dataPackageView.Contains(StandardDataFormats.Text)) {
					text = await dataPackageView.GetTextAsync();
				}
				bool enable = !string.IsNullOrWhiteSpace(text);
				PasteImportButton.IsEnabled = enable;
				PasteImportButton.Tag = text;
				if(PasteImportButton.IsEnabled) {
					ToolTipService.SetToolTip(PasteImportButton, "Paste Text".Language() + $":\n{text}");
				} else {
					ToolTipService.SetToolTip(PasteImportButton, "");
				}

				ImportClipboardItem.IsEnabled = enable;
				ImportClipboardItem.Tag = text;
				if(ImportClipboardItem.IsEnabled) {
					ToolTipService.SetToolTip(ImportClipboardItem, "Paste Text".Language() + $":\n{text}");
				} else {
					ToolTipService.SetToolTip(ImportClipboardItem, "");
				}
			} catch {
				ToolTipService.SetToolTip(PasteImportButton, "");
				PasteImportButton.IsEnabled = false;

				ToolTipService.SetToolTip(ImportClipboardItem, "");
				ImportClipboardItem.IsEnabled = false;
			}
		}

		public void FocusListingsLastItem() {
			ListingListView.Focus(FocusState.Pointer);
			ListingListView.SelectedIndex = ListingListView.Items.Count - 1;
		}

		public void SetDefault(ListingItem item) {
			foreach(ListingItem i in Items) {
				i.IsDefault = item == i;
			}
			if(ListingListView.SelectedItem is ListingItem current) {
				DefaultCheckBox.IsChecked = current.IsDefault;
				DefaultCheckBox.IsEnabled = !current.IsDefault;
			}
		}

		private void ListingListView_SelectionChanged(object sender, SelectionChangedEventArgs e) {
			if(e.AddedItems != null && e.AddedItems.Count != 0 && e.AddedItems.FirstOrDefault() is ListingItem item) {
				LoadTags(item.Listing);
			}
		}

		public void LoadTags(SingleListing item, bool selectLast = false) {
			this.current = item;
			DefaultCheckBox.IsChecked = item.IsDefault;
			/*
			 *	if(!item.IsCloud) {
			 *		checkboxEnable = !item.IsDefault;
			 *	} else {
			 *		if(E621User.Current == null) {
			 *			checkboxEnable = false;
			 *		} else {
			 *			checkboxEnable = !item.IsDefault;
			 *		}
			 *	}
			 */
			bool defaultCheckboxEnable = !item.IsCloud ? !item.IsDefault : E621User.Current != null && !item.IsDefault;
			DefaultCheckBox.IsEnabled = defaultCheckboxEnable;
			ActionsButton.IsEnabled = !item.IsCloud || E621User.Current != null;
			Tags.Clear();
			NoDataGrid.Visibility = Visibility.Collapsed;
			if(item.Tags.Count == 0) {
				NoDataGrid.Visibility = Visibility.Visible;
			} else {
				NoDataGrid.Visibility = Visibility.Collapsed;
				foreach(string tag in item.Tags) {
					Tags.Add(new ListingItemDetailItem(tag));
				}
			}
			TagAddButtonInput.Initialize();
			if(selectLast) {
				DetailListingListView.SelectedIndex = item.Tags.Count - 1;
			} else {
				DetailListingListView.SelectedIndex = 0;
			}
			DetailListingListView.ScrollIntoView(DetailListingListView.SelectedItem);
			TagAddButtonInput.Existing = item.Tags;
			NoDataStoryboard.Begin();
			MoreInfoButton.Visibility = item.IsCloud ? Visibility.Visible : Visibility.Collapsed;
			DownloadCloudItem.Visibility = item.IsCloud ? Visibility.Visible : Visibility.Collapsed;
			UploadCloudItem.Visibility = item.IsCloud ? Visibility.Visible : Visibility.Collapsed;
			NormalNoDataPanel.Visibility = !item.IsCloud ? Visibility.Visible : Visibility.Collapsed;
			if(item.IsCloud && item.Tags.Count == 0) {
				CloudNoDataSyncPanel.Visibility = Visibility.Visible;
			} else {
				CloudNoDataSyncPanel.Visibility = Visibility.Collapsed;
			}
			CloudSyncButton.IsEnabled = E621User.Current != null;
			UploadCloudItem.IsEnabled = E621User.Current != null && item.Tags.Count != 0;
			DownloadCloudItem.IsEnabled = E621User.Current != null;
			ClearAllTagsItem.IsEnabled = item.Tags.Count != 0;
		}

		private void ListingRenameItem_Click(object sender, RoutedEventArgs e) {
			var tag = (SingleListing)((MenuFlyoutItem)sender).Tag;
			ListingItem listing = Items.FirstOrDefault(i => i.Listing == tag);
			var found = (ListViewItem)ListingListView.ContainerFromItem(listing);
			RenameTeachingTip.Tag = listing;
			if(found != null) {
				RenameTeachingTip.Target = found;
				RenameTeachingTip.PreferredPlacement = TeachingTipPlacementMode.Bottom;
			} else {
				RenameTeachingTip.PreferredPlacement = TeachingTipPlacementMode.Center;
			}
			RenameBox.Text = tag.Name;
			RenameHint.Text = "";
			RenameHint.Visibility = Visibility.Collapsed;
			RenameTeachingTip.IsOpen = true;
		}

		private void ListingDeleteItem_Click(object sender, RoutedEventArgs e) {
			var element = (MenuFlyoutItem)sender;
			var tag = (SingleListing)element.Tag;
			if(tag.Tags.Count == 0) {
				OnListingDelete?.Invoke(tag);
				return;
			}
			DeleteConfirmTeachingTip.Tag = tag;
			var found = (ListViewItem)ListingListView.ContainerFromItem(Items.FirstOrDefault(i => i.Listing == tag));
			if(found != null) {
				DeleteConfirmTeachingTip.Target = found;
				DeleteConfirmTeachingTip.PreferredPlacement = TeachingTipPlacementMode.Bottom;
			} else {
				DeleteConfirmTeachingTip.PreferredPlacement = TeachingTipPlacementMode.Center;
			}
			//DeleteConfirmTeachingTip.Subtitle = $"Are you sure to delete ({tag.Name}) with {tag.Tags.Count} tags";
			DeleteConfirmTeachingTip.Subtitle = "Are you sure to delete ({{0}}) with {{1}} tags".Language(tag.Name, tag.Tags.Count);
			DeleteConfirmTeachingTip.IsOpen = true;
		}

		private void PasteImportButton_Click(object sender, RoutedEventArgs e) {
			var tag = PasteImportButton.Tag as string;
			bool error = false;
			List<string> result = new();
			if(string.IsNullOrWhiteSpace(tag)) {
				error = true;
			} else {
				result = tag.Trim().Split('\n', '\r', '\t', ' ').ToList();
			}
			if(error) {
				CenteredTeachingTip.Subtitle = "Paste Format Error".Language();
				CenteredTeachingTip.IsOpen = true;
			} else {
				OnPasteImport?.Invoke(result.ToArray());
			}
		}

		private void DeleteConfirmTeachingTip_ActionButtonClick(TeachingTip sender, object args) {
			if(sender.Tag is SingleListing tag) {
				DeleteConfirmTeachingTip.IsOpen = false;
				OnListingDelete?.Invoke(tag);
			}
		}

		private void ListingDeleteItem_Loaded(object sender, RoutedEventArgs e) {
			var element = (MenuFlyoutItem)sender;
			var tag = (SingleListing)element.Tag;

			var item = (MenuFlyoutItem)sender;
			item.IsEnabled = !tag.IsDefault && !tag.IsCloud && listings.Count > 1;
		}

		private void ListingSetDefaultItem_Click(object sender, RoutedEventArgs e) {
			var element = (MenuFlyoutItem)sender;
			var tag = (ListingItem)element.Tag;
			SetDefault(tag);
			OnListingSetAsDefault?.Invoke(tag);
		}

		private void ListingSetDefaultItem_Loaded(object sender, RoutedEventArgs e) {
			var element = (MenuFlyoutItem)sender;
			var tag = (ListingItem)element.Tag;

			var item = (MenuFlyoutItem)sender;
			bool checkboxEnable = !tag.IsCloud ? !tag.IsDefault : E621User.Current != null && !tag.IsDefault;
			item.IsEnabled = checkboxEnable;
		}

		private void DefaultCheckBox_Click(object sender, RoutedEventArgs e) {
			var item = ListingListView.SelectedItem as ListingItem;
			SetDefault(item);
			OnListingSetAsDefault?.Invoke(item);
		}

		private void RenameTeachingTip_ActionButtonClick(TeachingTip sender, object args) {
			SubmitRename();
		}

		private void RenameBox_KeyDown(object sender, KeyRoutedEventArgs e) {
			if(e.Key == VirtualKey.Enter) {
				SubmitRename();
			}
		}

		private void RenameBox_TextChanging(TextBox sender, TextBoxTextChangingEventArgs args) {
			string text = RenameBox.Text.Trim();
			string originalName = null;
			if(RenameTeachingTip.Tag is ListingItem item) {
				originalName = item.Name;
			}
			if(string.IsNullOrWhiteSpace(text)) {
				RenameBox.BorderBrush = new SolidColorBrush(Colors.Red);
			} else if(Items.Any(i => i.Name == text && i.Name != originalName)) {
				RenameBox.BorderBrush = new SolidColorBrush(Colors.Red);
			} else {
				RenameBox.BorderBrush = orgin_rename_brush;
			}
			RenameHint.Visibility = Visibility.Collapsed;
		}

		private void SubmitRename() {
			string text = RenameBox.Text.Trim();
			string originalName = null;
			if(RenameTeachingTip.Tag is ListingItem item) {
				originalName = item.Name;
			} else {
				RenameTeachingTip.IsOpen = false;
				return;
			}
			if(string.IsNullOrWhiteSpace(text)) {
				RenameHint.Visibility = Visibility.Visible;
				RenameHint.Text = "Cannot be empty".Language();
				return;
			} else if(text == originalName) {
				RenameHint.Visibility = Visibility.Visible;
				RenameHint.Text = "Same as before".Language();
				return;
			} else if(Items.Any(i => i.Name == text)) {
				RenameHint.Visibility = Visibility.Visible;
				RenameHint.Text = "Already existed".Language();
				return;
			}
			OnListingRename?.Invoke(item, text);
			RenameTeachingTip.IsOpen = false;
		}

		private void TagDeleteItem_Click(object sender, RoutedEventArgs e) {
			var element = (FrameworkElement)sender;
			var tag = (ListingItemDetailItem)element.Tag;
			if(ListingListView.SelectedItem is ListingItem item) {
				Tags.Remove(tag);
				OnTagDelete?.Invoke(item.Listing, tag);
				if(DetailListingListView.SelectedIndex == -1) {
					DetailListingListView.SelectedIndex = 0;
				}
			}
		}

		private void ExportClipboardItem_Click(object sender, RoutedEventArgs e) {
			string text = string.Join('\n', Tags.Select(t => t.Tag.Trim()));
			if(!string.IsNullOrWhiteSpace(text)) {
				DataPackage dataPackage = new() {
					RequestedOperation = DataPackageOperation.Copy,
				};
				dataPackage.SetText(text);
				Clipboard.SetContent(dataPackage);
			}
		}

		private void TagCopyItem_Click(object sender, RoutedEventArgs e) {
			var element = (MenuFlyoutItem)sender;
			var tag = (string)element.Tag;
			if(!string.IsNullOrWhiteSpace(tag)) {
				DataPackage dataPackage = new() {
					RequestedOperation = DataPackageOperation.Copy,
				};
				dataPackage.SetText(tag);
				Clipboard.SetContent(dataPackage);
			}
		}

		private void ListingRenameItem_Loaded(object sender, RoutedEventArgs e) {
			var element = (MenuFlyoutItem)sender;
			var tag = (SingleListing)element.Tag;
			element.IsEnabled = !tag.IsCloud;
		}

		private async void DownloadCloudItem_Click(object sender, RoutedEventArgs e) {
			if(cloudLoadingBar != null) {
				cloudLoadingBar.Visibility = Visibility.Visible;
			}
			await LoadBlackList();
			if(cloudLoadingBar != null) {
				cloudLoadingBar.Visibility = Visibility.Collapsed;
			}
		}

		private async void CloudSyncButton_Click(object sender, RoutedEventArgs e) {
			if(cloudLoadingBar != null) {
				cloudLoadingBar.Visibility = Visibility.Visible;
			}
			await LoadBlackList();
			if(cloudLoadingBar != null) {
				cloudLoadingBar.Visibility = Visibility.Collapsed;
			}
		}

		public async Task LoadBlackList() {
			if(E621User.Current == null) {
				return;
			}
			SingleListing cloud = listings.Find(l => l.IsCloud);
			if(cloud == null) {
				return;
			}
			E621User result = await E621User.GetAsync(E621User.Current.name);
			//await Task.Delay(1000);
			if(result == null) {
				return;
			}
			string[] array = result.blacklisted_tags.Split('\n');
			cloud.Tags = array.ToList();
			LoadTags(cloud);
			OnCloudSync?.Invoke(array);
		}

		private void ProgressBar_Loaded(object sender, RoutedEventArgs e) {
			var bar = (ProgressBar)sender;
			if(bar.Tag is SingleListing listing && listing.IsCloud) {
				cloudLoadingBar = (ProgressBar)sender;
			}
		}

		private void ClearAllTagsItem_Click(object sender, RoutedEventArgs e) {
			if(current == null) {
				return;
			}
			//CenteredClearAllComfirmTeachingTip.Subtitle = $"Are you sure to delete {Tags.Count} tags in ({current.Name}) list?";
			CenteredClearAllComfirmTeachingTip.Subtitle = "Are you sure to delete {{0}} tags in ({{1}}) list?".Language(Tags.Count, current.Name);
			CenteredClearAllComfirmTeachingTip.IsOpen = true;
		}

		private void CenteredClearAllComfirmTeachingTip_ActionButtonClick(TeachingTip sender, object args) {
			if(current == null) {
				return;
			}
			CenteredClearAllComfirmTeachingTip.IsOpen = false;
			OnTagsClearAll?.Invoke(current);
		}

		private void UploadCloudItem_Click(object sender, RoutedEventArgs e) {
			if(E621User.Current == null) {
				return;
			}
			//CenteredUploadComfirmTeachingTip.Subtitle = $"Upload User: ( {E621User.Current.name} )\n" +
			//	$"Warning! This action will override the tags existing in the cloud, do you want to continue?";
			CenteredUploadComfirmTeachingTip.Subtitle = "Upload User: ( {{0}} )\nWarning! This action will override the tags existing in the cloud, do you want to continue?".Language(E621User.Current.name);
			CenteredUploadComfirmTeachingTip.IsOpen = true;
		}

		private async void CenteredUploadComfirmTeachingTip_ActionButtonClick(TeachingTip sender, object args) {
			CenteredUploadComfirmTeachingTip.IsOpen = false;
			if(E621User.Current == null) {
				return;
			}
			if(cloudLoadingBar != null) {
				cloudLoadingBar.Visibility = Visibility.Visible;
			}
			string tags = "";
			if(current != null && current.IsCloud) {
				tags = string.Join('\n', current.Tags);
			} else {
				return;
			}
			HttpResult<string> result = await Data.PutRequestAsync(
				$"https://{Data.GetHost()}/users/{E621User.Current.name}.json",
				new KeyValuePair<string, string>("user[blacklisted_tags]", tags)
			);
			bool success = result.Result == HttpResultType.Success;
			CenteredUploadResultTeachingTip.Subtitle = success ? "Success".Language() : "Error".Language();
			CenteredUploadResultTeachingTip.IsOpen = true;
			if(cloudLoadingBar != null) {
				cloudLoadingBar.Visibility = Visibility.Collapsed;
			}
		}

		private void ImportClipboardItem_Click(object sender, RoutedEventArgs e) {
			if(current == null) {
				return;
			}
			var tag = (string)ImportClipboardItem.Tag;

			bool error = false;
			List<string> result = new();
			if(string.IsNullOrWhiteSpace(tag)) {
				error = true;
			} else {
				result = tag.Trim().Split('\n', '\r', '\t', ' ').ToList();
			}
			if(error) {
				CenteredTeachingTip.Subtitle = "Paste Format Error".Language();
				CenteredTeachingTip.IsOpen = true;
			} else {
				if(current.Tags.Count == 0) {
					current.Tags = result.ToList();
					LoadTags(current);
				} else {
					CenteredTagsOverrideConfirmTeachingTip.Subtitle = "Are you sure to override current tags?".Language();
					CenteredTagsOverrideConfirmTeachingTip.Tag = result.ToList();
					CenteredTagsOverrideConfirmTeachingTip.IsOpen = true;
				}
			}
		}

		private void CenteredTagsOverrideConfirmTeachingTip_ActionButtonClick(TeachingTip sender, object args) {
			var tag = (List<string>)sender.Tag;
			if(current != null) {
				current.Tags = tag;
				LoadTags(current);
			}
			CenteredTagsOverrideConfirmTeachingTip.IsOpen = false;
		}

		private async void TagInfoButton_Click(object sender, RoutedEventArgs e) {
			var tag = (ListingItemDetailItem)((Button)sender).Tag;
			var item = (ListViewItem)DetailListingListView.ContainerFromItem(tag);
			CancelTagLoadingCTS();
			TagLoadingBar.Visibility = Visibility.Visible;
			TagInfoText.Visibility = Visibility.Collapsed;
			TagInfoTeachingTip.Target = item;
			TagInfoTeachingTip.Title = tag.Tag;
			TagInfoTeachingTip.IsOpen = true;
			string content;
			if(tagsInfo.ContainsKey(tag.Tag)) {
				content = tagsInfo[tag.Tag];
			} else {
				tagLoading_cts = new CancellationTokenSource();
				var result = await E621Tag.GetFirstAsync(tag.Tag, tagLoading_cts.Token);
				if(result == null) {
					return;
				}
				content = "Post Count".Language() + $": {result.post_count}      " + "Tag Category".Language() + $": {E621Tag.GetCategory(result.category)}";
				tagsInfo.Add(tag.Tag, content);
			}
			if(string.IsNullOrEmpty(content)) {
				return;
			}
			TagLoadingBar.Visibility = Visibility.Collapsed;
			TagInfoText.Visibility = Visibility.Visible;
			TagInfoText.Text = content;
		}
	}

	public class ListingItem: INotifyPropertyChanged {
		public event PropertyChangedEventHandler PropertyChanged;
		private void RaiseProperty(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
		public SingleListing Listing { get; set; }
		public bool IsCloud => Listing.IsCloud;
		public string Icon => Listing.IsCloud ? "\uEBC3" : "\uEA41";
		public string Name {
			get => IsCloud ? Data.GetSimpleHost() : Listing.Name;
			set {
				Listing.Name = value;
				RaiseProperty(nameof(Name));
			}
		}
		public List<string> Tags => Listing.Tags;
		public bool IsDefault {
			get => Listing.IsDefault;
			set {
				Listing.IsDefault = value;
				RaiseProperty(nameof(IsDefault));
				RaiseProperty(nameof(AccepctIconVisibility));
			}
		}
		public Visibility CloudLoadingGridVisibility => Listing.IsCloud ? Visibility.Visible : Visibility.Collapsed;
		public Visibility AccepctIconVisibility => IsDefault ? Visibility.Visible : Visibility.Collapsed;
		public string Tooltip => $"{Name} ({Tags.Count})";

		public ListingItem(SingleListing listing) {
			Listing = listing;
		}

		public ListingItem GetSelf() => this;
	}

	public class ListingItemDetailItem {
		public string Tag { get; set; }

		public ListingItemDetailItem(string tag) {
			Tag = tag;
		}

		public ListingItemDetailItem GetSelf() => this;
	}
}
