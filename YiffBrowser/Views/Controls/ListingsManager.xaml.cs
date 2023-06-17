using Microsoft.UI.Xaml.Controls;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.ApplicationModel.DataTransfer;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using YiffBrowser.Helpers;
using YiffBrowser.Models.E621;
using YiffBrowser.Services.Locals;
using YiffBrowser.Services.Networks;

namespace YiffBrowser.Views.Controls {
	public sealed partial class ListingsManager : UserControl {
		public ListingsManager(bool followsOrBlocks) {
			this.InitializeComponent();
			ViewModel.RequestDeleteListing += ViewModel_RequestDeleteListing;
			ViewModel.RequestRenameListing += ViewModel_RequestRenameListing;

			ViewModel.FollowsOrBlocks = followsOrBlocks;
		}

		private void ViewModel_RequestRenameListing(ListingViewItem item) {
			ListViewItem found = (ListViewItem)ListingsListView.ContainerFromItem(item);
			if (found != null) {
				RenameTeachingTip.Target = found;
				RenameTeachingTip.PreferredPlacement = TeachingTipPlacementMode.Bottom;
			} else {
				RenameTeachingTip.PreferredPlacement = TeachingTipPlacementMode.Center;
			}
		}

		private void ViewModel_RequestDeleteListing(ListingViewItem item) {
			ListViewItem found = (ListViewItem)ListingsListView.ContainerFromItem(item);
			if (found != null) {
				DeleteConfirmTeachingTip.Target = found;
				DeleteConfirmTeachingTip.PreferredPlacement = TeachingTipPlacementMode.Bottom;
			} else {
				DeleteConfirmTeachingTip.PreferredPlacement = TeachingTipPlacementMode.Center;
			}
			DeleteConfirmTeachingTip.Subtitle = $"Are you sure to delete ({item.Item.Name}) with {item.Item.Tags.Count} tags";
		}

		public List<ListingItem> GetResult() => ViewModel.ListingItems.Select(x => x.Item).ToList();


		public static async Task ShowAsDialog(bool followsOrBlocks) {
			ListingsManager view = new(followsOrBlocks);
			await view.CreateContentDialog(new ContentDialogParameters() {
				Title = followsOrBlocks ? "Follows" : "Blocks",
				CloseText = "Back",
				MaxWidth = ContentDialogParameters.DEFAULT_MAX_WIDTH,
			}).ShowDialogAsync();

			if (followsOrBlocks) {
				Local.Listing.Follows = view.GetResult();
			} else {
				Local.Listing.Blocks = view.GetResult();
			}

			Listing.Write();
		}
	}

	public class ListingsManagerViewModel : BindableBase {
		public event Action<ListingViewItem> RequestDeleteListing;
		public event Action<ListingViewItem> RequestRenameListing;

		private bool isCenterTipOpen;
		private bool isDeleteListTipOpen;
		private bool isRenameListTipOpen;

		private string centerTipTitle = "Warning";
		private string centerTipSubtitle = "";

		private bool followsOrBlocks;

		private ListingViewItem checkedItem = null;
		private int selectedIndex = -1;
		private int tagItemsSelectedIndex = 0;

		private ListingViewItem toBeManipulatedListItem = null;

		private string renameListingText = "";

		private string[] existListNames;
		private string[] existTagNames;
		private ObservableCollection<TagViewItem> itemTags;

		private bool enablePasting;
		private bool isCurrentListingCloud;

		private string confirmTipNo = "No";
		private string confirmTipYes = "Yes";
		private string confirmTipSubtitle;
		private string confirmTipTitle;
		private bool isConfirmTipOpen = false;
		private ICommand confirmTipCommand;

		public string[] PasteTagsContent { get; private set; }

		public ObservableCollection<ListingViewItem> ListingItems { get; } = new();

		public ObservableCollection<TagViewItem> ItemTags {
			get => itemTags;
			set => SetProperty(ref itemTags, value);
		}

		public ListingViewItem CheckedItem {
			get => checkedItem;
			set => SetProperty(ref checkedItem, value, OnCheckedItemChanged);
		}

		private void OnCheckedItemChanged() {
			foreach (ListingViewItem item in ListingItems) {
				item.IsSelected = item == CheckedItem;
				item.Item.IsActive = item.IsSelected;
			}
		}

		public bool IsCurrentListingCloud {
			get => isCurrentListingCloud;
			set => SetProperty(ref isCurrentListingCloud, value);
		}

		public int SelectedIndex {
			get => selectedIndex;
			set => SetProperty(ref selectedIndex, value, OnSelectedIndexChanged);
		}

		public ListingViewItem GetSelectedListing() {
			if (SelectedIndex == -1 || ListingItems.Count == 0) {
				return null;
			}

			ListingViewItem item = ListingItems[SelectedIndex];
			return item;
		}

		private void OnSelectedIndexChanged() {
			ItemTags = new ObservableCollection<TagViewItem>();

			ListingViewItem item = GetSelectedListing();
			if (item == null) {
				return;
			}

			IsCurrentListingCloud = item.Item.IsCloud;

			foreach (string tag in item.Item.Tags) {
				ItemTags.Add(CreateTagViewItem(tag));
			}

			TagItemsSelectedIndex = 0;
			ExistTagNames = ItemTags.Select(x => x.Tag).ToArray();

			ItemTags.CollectionChanged += ItemTags_CollectionChanged;
		}

		public int TagItemsSelectedIndex {
			get => tagItemsSelectedIndex;
			set => SetProperty(ref tagItemsSelectedIndex, value);
		}

		private TagViewItem CreateTagViewItem(string tag) {
			TagViewItem item = new(tag);
			item.OnDelete += i => {
				ItemTags.Remove(i);
			};
			return item;
		}

		public bool FollowsOrBlocks {
			get => followsOrBlocks;
			set {
				SetProperty(ref followsOrBlocks, value);
				Initialize();
			}
		}

		private void Initialize() {
			List<ListingItem> items;
			if (FollowsOrBlocks) {
				items = Local.Listing.Follows;
			} else {
				items = Local.Listing.Blocks;
			}
			foreach (ListingItem item in items) {
				ListingItems.Add(CreateNewListItem(item));
			}

			ListingViewItem found = ListingItems.FirstOrDefault(x => x.Item.IsActive);
			if (found != null) {
				found.IsSelected = true;
				SelectedIndex = ListingItems.IndexOf(found);
			} else {
				ListingItems.FirstOrDefault().Item.IsActive = true;
			}

			if (Local.Settings.CheckLocalUser()) {
				if (SelectedIndex == -1) {
					SelectedIndex = 0;
				}
			} else {
				if (ListingItems[SelectedIndex].Item.IsCloud) {
					int index = ListingItems.IndexOf(ListingItems.FirstOrDefault(x => !x.Item.IsCloud));
					if (index == -1) {
						index = 0;
					}
					SelectedIndex = index;
				}
			}
		}

		public bool EnablePasting {
			get => enablePasting;
			set => SetProperty(ref enablePasting, value);
		}

		public ListingsManagerViewModel() {
			ListingItems.CollectionChanged += ListingItems_CollectionChanged;

			Clipboard.ContentChanged += (s, e) => UpdatePastImportEnable();
			UpdatePastImportEnable();
		}

		private async void UpdatePastImportEnable() {
			DataPackageView dataPackageView = Clipboard.GetContent();
			string text = null;
			if (dataPackageView.Contains(StandardDataFormats.Text)) {
				text = await dataPackageView.GetTextAsync();
			}
			EnablePasting = text.IsNotBlank();
			if (EnablePasting) {
				PasteTagsContent = text.Trim().Split('\n', '\r', '\t', ' ').ToArray();
			} else {
				PasteTagsContent = null;
			}
		}

		private void ItemTags_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
			if (ItemTags.IsEmpty()) {
				TagItemsSelectedIndex = -1;
			}

			ExistTagNames = ItemTags.Select(x => x.Tag).ToArray();

			ListingViewItem listingItem = ListingItems[SelectedIndex];
			listingItem.Item.Tags = ExistTagNames.ToList();
			listingItem.UpdateItem();
		}

		private void ListingItems_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
			if (ListingItems.IsEmpty()) {
				SelectedIndex = -1;
			}

			ExistListNames = ListingItems.Select(x => x.Item.Name).ToArray();
		}

		public bool IsDeleteListTipOpen {
			get => isDeleteListTipOpen;
			set => SetProperty(ref isDeleteListTipOpen, value);
		}

		public bool IsRenameListTipOpen {
			get => isRenameListTipOpen;
			set => SetProperty(ref isRenameListTipOpen, value);
		}

		public bool IsCenterTipOpen {
			get => isCenterTipOpen;
			set => SetProperty(ref isCenterTipOpen, value);
		}
		public string CenterTipTitle {
			get => centerTipTitle;
			set => SetProperty(ref centerTipTitle, value);
		}
		public string CenterTipSubtitle {
			get => centerTipSubtitle;
			set => SetProperty(ref centerTipSubtitle, value);
		}


		public bool IsConfirmTipOpen {
			get => isConfirmTipOpen;
			set => SetProperty(ref isConfirmTipOpen, value);
		}
		public string ConfirmTipTitle {
			get => confirmTipTitle;
			set => SetProperty(ref confirmTipTitle, value);
		}
		public string ConfirmTipSubtitle {
			get => confirmTipSubtitle;
			set => SetProperty(ref confirmTipSubtitle, value);
		}
		public string ConfirmTipYes {
			get => confirmTipYes;
			set => SetProperty(ref confirmTipYes, value);
		}
		public string ConfirmTipNo {
			get => confirmTipNo;
			set => SetProperty(ref confirmTipNo, value);
		}
		public ICommand ConfirmTipCommand {
			get => confirmTipCommand;
			set => SetProperty(ref confirmTipCommand, value);
		}



		public string RenameListingText {
			get => renameListingText;
			set => SetProperty(ref renameListingText, value);
		}

		public string[] ExistListNames {
			get => existListNames;
			set => SetProperty(ref existListNames, value);
		}

		public string[] ExistTagNames {
			get => existTagNames;
			set => SetProperty(ref existTagNames, value);
		}


		public ICommand PasteAsNewListCommand => new DelegateCommand(PasteAsNewList);

		public ICommand ConfirmDeleteListingCommand => new DelegateCommand(ConfirmDeleteListing);
		public ICommand CancelDeleteListingCommand => new DelegateCommand(CancelDeleteListing);
		public ICommand OnNewTagSubmitCommand => new DelegateCommand<string>(OnNewTagSubmit);
		public ICommand OnNewListSubmitCommand => new DelegateCommand<string>(OnNewListSubmit);

		private void OnNewListSubmit(string text) {
			ListingItems.Add(CreateNewListItem(new ListingItem(text)));
		}

		private void OnNewTagSubmit(string text) {
			ItemTags.Insert(0, CreateTagViewItem(text));
			TagItemsSelectedIndex = 0;
		}

		private ListingViewItem CreateNewListItem(ListingItem item) {
			ListingViewItem viewItem = new(item);
			viewItem.OnDelete += p => {
				if (ListingItems.Count(x => !x.Item.IsCloud) <= 1) {
					return;
				}
				RequestDeleteListing?.Invoke(p);
				IsDeleteListTipOpen = true;
				toBeManipulatedListItem = p;
			};
			viewItem.OnRename += p => {
				RequestRenameListing?.Invoke(p);
				RenameListingText = p.Item.Name;
				IsRenameListTipOpen = true;
				toBeManipulatedListItem = p;
			};
			viewItem.OnCheck += p => {
				CheckedItem = p;
			};
			return viewItem;
		}

		private void PasteAsNewList() {
			if (PasteTagsContent.IsEmpty()) {
				IsCenterTipOpen = true;
				CenterTipSubtitle = "Paste Format Error";
				return;
			} else {
				IsCenterTipOpen = false;
			}

			int count = ListingItems.Count;
			string newListName;
			do {
				newListName = $"Paste List - {count++}";
			} while (ListingItems.Any(i => i.Item.Name == newListName));

			ListingItems.Add(CreateNewListItem(new ListingItem(newListName) {
				Tags = PasteTagsContent.ToList(),
			}));

			SelectedIndex = ListingItems.Count - 1;
		}


		private void ConfirmDeleteListing() {
			if (toBeManipulatedListItem == null) {
				return;
			}

			bool reindex = toBeManipulatedListItem == ListingItems[SelectedIndex];
			int index = SelectedIndex;

			ListingItems.Remove(toBeManipulatedListItem);

			if (reindex) {
				SelectedIndex = Math.Clamp(index - 1, 0, ListingItems.Count);
			}

			toBeManipulatedListItem = null;

			IsDeleteListTipOpen = false;
		}

		private void CancelDeleteListing() {
			toBeManipulatedListItem = null;
		}



		public ICommand ConfirmRenameListingCommand => new DelegateCommand(ConfirmRenameListing);
		public ICommand CancelRenameListingCommand => new DelegateCommand(CancelRenameListing);

		private void ConfirmRenameListing() {
			if (toBeManipulatedListItem == null) {
				return;
			}
			string text = RenameListingText.Trim();
			if (text.IsBlank() || (ExistListNames?.Contains(text) ?? false)) {
				return;
			}

			toBeManipulatedListItem.Item.Name = text;
			toBeManipulatedListItem.UpdateItem();

			IsRenameListTipOpen = false;

			toBeManipulatedListItem = null;
		}


		private void CancelRenameListing() {
			toBeManipulatedListItem = null;
		}

		public ICommand ExportClipboardCommand => new DelegateCommand(ExportClipboard);
		public ICommand ImportClipboardCommand => new DelegateCommand(ImportClipboard);
		public ICommand ClearAllCommand => new DelegateCommand(ClearAll);
		public ICommand DownloadCloudCommand => new DelegateCommand(DownloadCloud);
		public ICommand UploadCloudCommand => new DelegateCommand(UploadCloud);

		private void ExportClipboard() {
			ListingViewItem item = GetSelectedListing();
			if (item == null) {
				return;
			}

			List<string> tags = item.Item.Tags;
			string text = string.Join('\n', tags.Select(x => x.Trim()));
			if (text.IsNotBlank()) {
				text.CopyToClipboard();
			}

		}

		private void ImportClipboard() {
			ListingViewItem item = GetSelectedListing();
			if (item == null) {
				return;
			}

			if (PasteTagsContent.IsEmpty()) {
				IsCenterTipOpen = true;
				CenterTipSubtitle = "Paste Format Error";
				return;
			} else {
				IsCenterTipOpen = false;
			}

			foreach (string tag in PasteTagsContent) {
				if (ItemTags.Any(x => x.Tag == tag) || tag.IsBlank()) {
					continue;
				}
				ItemTags.Add(CreateTagViewItem(tag));
				item.Item.Tags.Add(tag);
			}


			ExistTagNames = ItemTags.Select(x => x.Tag).ToArray();
		}

		private void ClearAll() {
			ListingViewItem item = GetSelectedListing();
			if (item == null) {
				return;
			}

			IsConfirmTipOpen = true;
			ConfirmTipTitle = "Clear All Tags";
			ConfirmTipSubtitle = $"Are you sure to clear {item.Item.Tags.Count} tags in listing ({item.Item.Name})";
			ConfirmTipCommand = new DelegateCommand(() => {
				ItemTags.Clear();
				item.Item.Tags.Clear();
				ConfirmTipCommand = null;
				IsConfirmTipOpen = false;
			});

		}

		private async void DownloadCloud() {
			ListingViewItem item = GetSelectedListing();
			if (item == null) {
				return;
			}
			if (!Local.Settings.CheckLocalUser()) {
				return;
			}
			E621User user = await E621API.GetUserAsync(Local.Settings.Username);
			string[] tags = user.blacklisted_tags.Split('\n');

			foreach (string tag in tags) {
				if (ItemTags.Any(x => x.Tag == tag) || tag.IsBlank()) {
					continue;
				}
				ItemTags.Add(CreateTagViewItem(tag));
				item.Item.Tags.Add(tag);
			}

			ExistTagNames = ItemTags.Select(x => x.Tag).ToArray();
		}

		private void UploadCloud() {
			ListingViewItem item = GetSelectedListing();
			if (item == null) {
				return;
			}
			if (!Local.Settings.CheckLocalUser()) {
				return;
			}

			IsConfirmTipOpen = true;
			ConfirmTipTitle = "Upload";
			ConfirmTipSubtitle = $"Are you sure to upload current tags to e621 cloud? This action will override the existing tags in the e621 cloud.";
			ConfirmTipCommand = new DelegateCommand(async () => {
				ConfirmTipCommand = null;
				IsConfirmTipOpen = false;

				item.IsLoading = true;

				bool result = await E621API.UploadBlacklistTags(Local.Settings.Username, item.Item.Tags.ToArray());

				item.IsLoading = false;

				IsCenterTipOpen = true;
				CenterTipTitle = "Upload";
				CenterTipSubtitle = result ? "Upload Successful" : "Upload Failed";

			});

		}

	}

	public class TagViewItem : BindableBase {
		private bool isLoading;
		private E621Tag e621Tag;
		private string tag;

		public TagViewItem(string tag) {
			Tag = tag;
		}

		public event Action<TagViewItem> OnDelete;

		public string Tag {
			get => tag;
			set => SetProperty(ref tag, value);
		}

		public E621Tag E621Tag {
			get => e621Tag;
			set => SetProperty(ref e621Tag, value);
		}

		public ICommand DeleteCommand => new DelegateCommand(Delete);
		public ICommand CopyCommand => new DelegateCommand(Copy);
		public ICommand LoadedCommand => new DelegateCommand<string>(Loaded);

		public bool IsLoading {
			get => isLoading;
			set => SetProperty(ref isLoading, value);
		}

		private async void Loaded(string tag) {
			IsLoading = true;
			E621Tag = await E621API.GetE621TagAsync(tag);
			IsLoading = false;
		}

		private void Delete() {
			OnDelete?.Invoke(this);
		}

		private void Copy() {
			Tag.CopyToClipboard();
		}

	}

	public class ListingViewItem : BindableBase {
		public event Action<ListingViewItem> OnCheck;
		public event Action<ListingViewItem> OnDelete;
		public event Action<ListingViewItem> OnRename;

		private bool isSelected;
		private bool isLoading;

		private bool isEnabled = true;

		public ListingItem Item { get; set; }

		public void UpdateItem() {
			RaisePropertyChanged(nameof(Item));
		}

		public bool IsSelected {
			get => isSelected;
			set => SetProperty(ref isSelected, value);
		}

		public bool IsLoading {
			get => isLoading;
			set => SetProperty(ref isLoading, value);
		}

		public bool IsEnabled {
			get => isEnabled;
			set => SetProperty(ref isEnabled, value);
		}

		public ICommand CheckedCommand => new DelegateCommand<RoutedEventArgs>(Checked);
		private void Checked(RoutedEventArgs args) {
			OnCheck?.Invoke(this);
		}

		public ICommand RenameCommand => new DelegateCommand(Rename);
		public ICommand DeleteCommand => new DelegateCommand(Delete);

		private void Rename() {
			OnRename?.Invoke(this);
		}

		private void Delete() {
			OnDelete?.Invoke(this);
		}

		public ListingViewItem(ListingItem item) {
			Item = item;
			if (item.IsCloud) {
				IsEnabled = Local.Settings.CheckLocalUser();
			}
		}

	}
}
