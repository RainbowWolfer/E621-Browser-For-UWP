using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using YiffBrowser.Helpers;
using YiffBrowser.Models.E621;
using YiffBrowser.Services.Locals;
using YiffBrowser.Services.Networks;

namespace YiffBrowser.Views.Controls.TagsInfoViews {
	public sealed partial class TagsInfoView : UserControl {
		public string[] Tags {
			get => (string[])GetValue(TagsProperty);
			set => SetValue(TagsProperty, value);
		}

		public static readonly DependencyProperty TagsProperty = DependencyProperty.Register(
			nameof(Tags),
			typeof(string[]),
			typeof(TagsInfoView),
			new PropertyMetadata(Array.Empty<string>())
		);



		public TagsInfoView() {
			this.InitializeComponent();
		}

	}

	public class TagsInfoViewModel : BindableBase {
		private string[] tags;
		private bool isFollowChecked;
		private bool isBlockChecked;

		public ObservableCollection<TagInfoViewPivotItem> TagItems { get; } = new();

		public ObservableCollection<ListingCheckViewItem> ListingsCheckItems { get; } = new();

		public string[] Tags {
			get => tags;
			set => SetProperty(ref tags, value, OnTagsChanged);
		}

		private void OnTagsChanged() {
			TagItems.Clear();
			foreach (string tag in Tags) {
				TagItems.Add(new TagInfoViewPivotItem(tag));
			}

			isFollowChecked = Local.Listing.ContainFollows(Tags.ToFullString());
			isBlockChecked = Local.Listing.ContainBlocks(Tags.ToFullString());

			RaisePropertyChanged(nameof(IsFollowChecked));
			RaisePropertyChanged(nameof(IsBlockChecked));
		}

		public bool IsFollowChecked {
			get => isFollowChecked;
			set => SetProperty(ref isFollowChecked, value, OnIsFollowCheckedChanged);
		}

		private void OnIsFollowCheckedChanged() {
			foreach (string tag in Tags) {
				if (IsFollowChecked) {
					Local.Listing.AddFollow(tag);
				} else {
					Local.Listing.RemoveFollow(tag);
				}
			}
			Listing.Write();
		}

		public bool IsBlockChecked {
			get => isBlockChecked;
			set => SetProperty(ref isBlockChecked, value, OnIsBlockCheckedChanged);
		}

		private void OnIsBlockCheckedChanged() {
			foreach (string tag in Tags) {
				if (IsBlockChecked) {
					Local.Listing.AddBlock(tag);
				} else {
					Local.Listing.RemoveBlock(tag);
				}
			}
			Listing.Write();
		}

		public ICommand FollowSideToggleOpeningCommand => new DelegateCommand(FollowSideToggleOpening);
		public ICommand BlockSideToggleOpeningCommand => new DelegateCommand(BlockSideToggleOpening);

		public ICommand SideToggleClosingCommand => new DelegateCommand(SideToggleClosing);

		private void FollowSideToggleOpening() {
			InitializeListingsCheckItem(true);
		}

		private void BlockSideToggleOpening() {
			InitializeListingsCheckItem(false);
		}

		private void InitializeListingsCheckItem(bool isFollowOrBlock) {
			ListingsCheckItems.Clear();

			List<ListingItem> follows = isFollowOrBlock ? Local.Listing.Follows : Local.Listing.Blocks;

			foreach (ListingItem list in follows) {
				ListingCheckViewItem item = new(Tags.ToFullString(), list);
				item.CheckedChanged += (s, e) => {
					if (s.Item.IsActive) {
						if (isFollowOrBlock) {
							IsFollowChecked = e;
						} else {
							IsBlockChecked = e;
						}
					}
				};
				ListingsCheckItems.Add(item);
			}
		}


		private void SideToggleClosing() {
			foreach (ListingCheckViewItem item in ListingsCheckItems) {
				if (item.IsChecked) {//add tag to selected item
					if (!item.Item.Tags.Contains(item.Tag)) {
						item.Item.Tags.Add(item.Tag);
					}
				} else {
					if (item.Item.Tags.Contains(item.Tag)) {
						item.Item.Tags.Remove(item.Tag);
					}
				}
			}

			Listing.Write();
		}

	}

	public class ListingCheckViewItem : BindableBase {
		public event TypedEventHandler<ListingCheckViewItem, bool> CheckedChanged;

		private bool isChecked;
		private ListingItem item;
		private string tag;

		public ListingItem Item {
			get => item;
			set => SetProperty(ref item, value);
		}
		public string Tag {
			get => tag;
			set => SetProperty(ref tag, value);
		}

		public bool IsChecked {
			get => isChecked;
			set => SetProperty(ref isChecked, value, OnIsCheckedChanged);
		}

		private void OnIsCheckedChanged() {
			CheckedChanged?.Invoke(this, IsChecked);
		}

		public ListingCheckViewItem(string tag, ListingItem item) {
			Tag = tag.Trim().ToLower();
			Item = item;

			IsChecked = item.Tags.Contains(Tag);
		}

	}

	public class TagInfoViewPivotItem : BindableBase {
		private string tag;

		private string count = "";
		private bool isLoading = false;

		private string wikiContent = "";

		private E621Wiki e621Wiki;
		private E621Tag e621Tag;

		private Color categoryColor = Colors.Transparent;
		private string categoryName = null;

		public Color CategoryColor {
			get => categoryColor;
			set => SetProperty(ref categoryColor, value);
		}
		public string CategoryName {
			get => categoryName;
			set => SetProperty(ref categoryName, value);
		}

		public string Tag {
			get => tag;
			set => SetProperty(ref tag, value, OnTagChanged);
		}

		public string Count {
			get => count;
			set => SetProperty(ref count, value);
		}

		public bool IsLoading {
			get => isLoading;
			set => SetProperty(ref isLoading, value);
		}

		public E621Tag E621Tag {
			get => e621Tag;
			set => SetProperty(ref e621Tag, value);
		}

		public E621Wiki E621Wiki {
			get => e621Wiki;
			set => SetProperty(ref e621Wiki, value);
		}

		public string WikiContent {
			get => wikiContent;
			set => SetProperty(ref wikiContent, value);
		}


		public TagInfoViewPivotItem(string tag) {
			Tag = tag;
		}

		private async void OnTagChanged() {
			IsLoading = true;

			E621Tag = await E621API.GetE621TagAsync(tag);
			if (E621Tag != null) {
				Count = $"({E621Tag.post_count.NumberToK()})";
				CategoryColor = E621Tag.GetCatrgoryColor(E621Tag.category);
				CategoryName = E621Tag.GetCategory(E621Tag.category);
			}

			E621Wiki = await E621API.GetE621WikiAsync(tag);
			WikiContent = E621Wiki?.body?.NotBlankCheck() ?? "No wiki found";

			IsLoading = false;
		}

	}
}