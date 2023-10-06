using Microsoft.Toolkit.Uwp.UI;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using YiffBrowser.Helpers;
using YiffBrowser.Models;
using YiffBrowser.Models.E621;
using YiffBrowser.Services.Locals;
using YiffBrowser.Views.Controls.TagsInfoViews;
using YiffBrowser.Views.Pages.E621;

namespace YiffBrowser.Views.Controls {
	public sealed partial class PostTagsListView : UserControl {
		public readonly ObservableCollection<GroupTagListWithColor> tags = new();

		public Tags Tags {
			get => (Tags)GetValue(TagsProperty);
			set => SetValue(TagsProperty, value);
		}

		public static readonly DependencyProperty TagsProperty = DependencyProperty.Register(
			nameof(Tags),
			typeof(Tags),
			typeof(PostTagsListView),
			new PropertyMetadata(Array.Empty<string>(), OnTagsChanged)
		);

		private static void OnTagsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
			if (d is PostTagsListView view) {
				view.OriginalTags = (Tags)e.NewValue;
				view.UpdateTagsGroup(view.OriginalTags);
			}
		}

		public bool ShowAddMinusButton {
			get => (bool)GetValue(ShowAddMinusButtonProperty);
			set => SetValue(ShowAddMinusButtonProperty, value);
		}

		public static readonly DependencyProperty ShowAddMinusButtonProperty = DependencyProperty.Register(
			nameof(ShowAddMinusButton),
			typeof(bool),
			typeof(PostTagsListView),
			new PropertyMetadata(true)
		);

		public bool ShowDialogTagInfo {
			get => (bool)GetValue(ShowDialogTagInfoProperty);
			set => SetValue(ShowDialogTagInfoProperty, value);
		}

		public static readonly DependencyProperty ShowDialogTagInfoProperty = DependencyProperty.Register(
			nameof(ShowDialogTagInfo),
			typeof(bool),
			typeof(PostTagsListView),
			new PropertyMetadata(true)
		);

		private Tags OriginalTags { get; set; }

		public PostTagsListView() {
			InitializeComponent();
		}

		private void UpdateTagsGroup(Tags tags) {
			if (tags == null) {
				return;
			}
			RemoveGroup();

			AddNewGroup($"Artist ({tags.Artist?.Count})", ToGroupTag(tags.Artist), E621Tag.GetCategoryColor(E621TagCategory.Artists));
			AddNewGroup($"Copyright ({tags.Copyright?.Count})", ToGroupTag(tags.Copyright), E621Tag.GetCategoryColor(E621TagCategory.Copyrights));
			AddNewGroup($"Species ({tags.Species?.Count})", ToGroupTag(tags.Species), E621Tag.GetCategoryColor(E621TagCategory.Species));
			AddNewGroup($"Director ({tags.Director?.Count})", ToGroupTag(tags.Director), E621Tag.GetCategoryColor(E621TagCategory.Director));
			AddNewGroup($"Character ({tags.Character?.Count})", ToGroupTag(tags.Character), E621Tag.GetCategoryColor(E621TagCategory.Characters));
			AddNewGroup($"General ({tags.General?.Count})", ToGroupTag(tags.General), E621Tag.GetCategoryColor(E621TagCategory.General));
			AddNewGroup($"Meta ({tags.Meta?.Count})", ToGroupTag(tags.Meta), E621Tag.GetCategoryColor(E621TagCategory.Meta));
			AddNewGroup($"Invalid ({tags.Invalid?.Count})", ToGroupTag(tags.Invalid), E621Tag.GetCategoryColor(E621TagCategory.Invalid));
			AddNewGroup($"Lore ({tags.Lore?.Count})", ToGroupTag(tags.Lore), E621Tag.GetCategoryColor(E621TagCategory.Lore));

		}

		public List<GroupTag> ToGroupTag(List<string> tags) {
			List<GroupTag> result = new();
			foreach (string tag in tags ?? new List<string>()) {
				GroupTag item = new(tag) {
					ShowAddMinusButton = ShowAddMinusButton
				};
				item.InfoAction += Item_InfoAction;
				item.AddAction += Item_AddAction;
				item.MinusAction += Item_MinusAction;
				result.Add(item);
			}
			return result;
		}

		private void Item_MinusAction(string tag) {
			E621HomePageViewModel.CreateConcatTag(tag, false);
		}

		private void Item_AddAction(string tag) {
			E621HomePageViewModel.CreateConcatTag(tag, true);
		}

		private async void Item_InfoAction(string tag) {
			if (ShowDialogTagInfo) {
				await new TagsInfoView() {
					Tags = new string[] { tag },
				}.CreateContentDialog(new ContentDialogParameters() {
					CloseText = "Back",
				}).ShowAsyncSafe();
			}
		}

		private void InfoButton_Click(object sender, RoutedEventArgs e) {
			if (ShowDialogTagInfo) {
				return;
			}
			Button button = sender as Button;
			Flyout flyout = button.Resources["InfoButtonFlyout"] as Flyout;
			(flyout.Content as TagSimpleInfoPopupView).TagName = button.Tag as string;
			flyout.ShowAt(button);
		}

		private void RemoveGroup() {
			tags.Clear();
		}

		private void AddNewGroup(string title, List<GroupTag> content, Color color) {
			if (content == null) {
				return;
			}
			if (content.Count == 0) {
				return;
			}
			foreach (GroupTag item in content) {
				item.Color = color;
			}
			tags.Add(new GroupTagListWithColor(title, content, color));
		}



		//public ICommand ItemClickCommand {
		//	get => (ICommand)GetValue(ItemClickCommandProperty);
		//	set => SetValue(ItemClickCommandProperty, value);
		//}

		//public static readonly DependencyProperty ItemClickCommandProperty = DependencyProperty.Register(
		//	nameof(ItemClickCommand),
		//	typeof(ICommand),
		//	typeof(PostTagsListView),
		//	new PropertyMetadata(null)
		//);

		private void TagsListView_ItemClick(object sender, ItemClickEventArgs e) {
			if (!ShowDialogTagInfo) {
				return;
			}
			//ItemClickCommand?.Execute(e.ClickedItem);
			GroupTag group = (GroupTag)e.ClickedItem;
			E621HomePageViewModel.CreateNewTag(group.Content);
		}

		private void AutoSuggestBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args) {
			Tags tags = OriginalTags.CreateNewBySearch(args.QueryText);
			UpdateTagsGroup(tags);
		}

	}

	public class GroupTag : BindableBase {
		public event Action<string> InfoAction;
		public event Action<string> MinusAction;
		public event Action<string> AddAction;

		private string content;
		private Color color;
		private bool isInFollows;
		private bool isInBlocks;
		private bool showAddMinusButton = true;

		public string Content {
			get => content;
			set => SetProperty(ref content, value);
		}

		public Color Color {
			get => color;
			set => SetProperty(ref color, value);
		}

		public bool IsInFollows {
			get => isInFollows;
			set => SetProperty(ref isInFollows, value);
		}

		public bool IsInBlocks {
			get => isInBlocks;
			set => SetProperty(ref isInBlocks, value);
		}

		public bool ShowAddMinusButton {
			get => showAddMinusButton;
			set => SetProperty(ref showAddMinusButton, value);
		}

		public ICommand InfoCommand => new DelegateCommand(Info);
		public ICommand MinusCommand => new DelegateCommand(Minus);
		public ICommand AddCommand => new DelegateCommand(Add);

		public ICommand FollowCommand => new DelegateCommand<string>(Follow);
		public ICommand BlockCommand => new DelegateCommand<string>(Block);

		private void Info() => InfoAction?.Invoke(Content);
		private void Minus() => MinusAction?.Invoke(Content);
		private void Add() => AddAction?.Invoke(Content);

		private void Follow(string addOrRemove) {
			if (addOrRemove == "Add") {
				Local.Listing.AddFollow(Content);
			} else {
				Local.Listing.RemoveFollow(Content);
			}
			Update();
			Listing.Write();
		}

		private void Block(string addOrRemove) {
			if (addOrRemove == "Add") {
				Local.Listing.AddBlock(Content);
			} else {
				Local.Listing.RemoveBlock(Content);
			}
			Update();
			Listing.Write();
		}

		public ICommand CopyCommand => new DelegateCommand(Copy);

		private void Copy() {
			Content.CopyToClipboard();
		}

		public GroupTag(string content) {
			Content = content;
			Update();
		}

		public void Update() {
			IsInFollows = Local.Listing.ContainFollows(content);
			IsInBlocks = Local.Listing.ContainBlocks(content);
		}
	}

	public class GroupTagListWithColor : ObservableCollection<GroupTag> {
		public string Key { get; set; }
		public Color Color { get; set; }

		public GroupTagListWithColor(string key, List<GroupTag> content, Color color) : base(content) {
			Key = key;
			Color = color;
		}

		public override string ToString() {
			return $"{Key}";
		}

	}
}
