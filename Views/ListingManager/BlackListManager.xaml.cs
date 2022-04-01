using E621Downloader.Models.Locals;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
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

namespace E621Downloader.Views.ListingManager {
	public sealed partial class BlackListManager: UserControl {
		public event Action<string> OnNewListSubmit;
		public event Action<SingleListing, string> OnNewTagSubmit;
		public event Action<string[]> OnPasteImport;

		public event Action<ListingItem, string> OnListingRename;
		public event Action<SingleListing> OnListingDelete;
		public event Action<ListingItem> OnListingSetAsDefault;

		public event Action<SingleListing, ListingItemDetailItem> OnTagDelete;

		private readonly List<SingleListing> listings;

		public ObservableCollection<ListingItem> Items { get; } = new ObservableCollection<ListingItem>();
		public ObservableCollection<ListingItemDetailItem> Tags { get; } = new ObservableCollection<ListingItemDetailItem>();

		private readonly Brush orgin_rename_brush;

		public BlackListManager(List<SingleListing> listings) {
			this.InitializeComponent();
			this.DataContextChanged += (s, e) => Bindings.Update();
			this.listings = listings;
			Update(listings);
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
				PasteImportButton.IsEnabled = !string.IsNullOrWhiteSpace(text);
				PasteImportButton.Tag = text;
				if(PasteImportButton.IsEnabled) {
					ToolTipService.SetToolTip(PasteImportButton, $"Paste Text:\n{text}");
				} else {
					ToolTipService.SetToolTip(PasteImportButton, "");
				}
			} catch {
				ToolTipService.SetToolTip(PasteImportButton, "");
				PasteImportButton.IsEnabled = false;
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
			DefaultCheckBox.IsChecked = item.IsDefault;
			DefaultCheckBox.IsEnabled = !item.IsDefault;
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
			DeleteConfirmTeachingTip.Subtitle = $"Are you sure to delete ({tag.Name}) with {tag.Tags.Count} tags";
			DeleteConfirmTeachingTip.IsOpen = true;
		}

		private void PasteImportButton_Click(object sender, RoutedEventArgs e) {
			var tag = PasteImportButton.Tag as string;
			bool error = false;
			List<string> result = new List<string>();
			if(string.IsNullOrWhiteSpace(tag)) {
				error = true;
			} else {
				result = tag.Trim().Split('\n', '\r', '\t', ' ').ToList();
			}
			if(error) {
				CenteredTeachingTip.Subtitle = "Paste Format Error";
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
			item.IsEnabled = !tag.IsDefault && listings.Count > 1;
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
			item.IsEnabled = !tag.IsDefault;
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
				RenameHint.Text = "Cannot be empty";
				return;
			} else if(text == originalName) {
				RenameHint.Visibility = Visibility.Visible;
				RenameHint.Text = "Same as before";
				return;
			} else if(Items.Any(i => i.Name == text)) {
				RenameHint.Visibility = Visibility.Visible;
				RenameHint.Text = "Already existed";
				return;
			}
			OnListingRename?.Invoke(item, text);
			RenameTeachingTip.IsOpen = false;
		}

		private void TagDeleteItem_Click(object sender, RoutedEventArgs e) {
			var element = (MenuFlyoutItem)sender;
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
			DataPackage dataPackage = new DataPackage() {
				RequestedOperation = DataPackageOperation.Copy,
			};
			dataPackage.SetText(string.Join('\n', Tags.Select(t => t.Tag)));
			Clipboard.SetContent(dataPackage);
		}
	}

	public class ListingItem: INotifyPropertyChanged {
		public event PropertyChangedEventHandler PropertyChanged;
		private void RaiseProperty(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
		public SingleListing Listing { get; set; }
		public string Icon => Listing.IsCloud ? "\uEBC3" : "\uEA41";
		public string Name {
			get => Listing.Name;
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
