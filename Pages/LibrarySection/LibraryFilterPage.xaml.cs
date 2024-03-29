﻿using E621Downloader.Models;
using E621Downloader.Models.Locals;
using E621Downloader.Models.Utilities;
using E621Downloader.Views.FoldersSelectionSection;
using E621Downloader.Views.LibrarySection;
using E621Downloader.Views.LocalTagsManagementSection;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

namespace E621Downloader.Pages.LibrarySection {
	public sealed partial class LibraryFilterPage: Page, ILibraryGridPage {
		private LibraryPage libraryPage;
		public LibraryFilterArgs Args { get; private set; }

		private bool initializing = true;
		private bool isLoading = false;

		public bool IsLoading {
			get => isLoading;
			set {
				isLoading = value;
				SearchButton.IsEnabled = !isLoading;
				if(isLoading) {
					TitleBar.EnableRefreshButton = false;
					TitleBar.EnableSortButtons = false;
					LoadingGrid.Visibility = Visibility.Visible;
					GroupView.ClearNoDataGrid();
				} else {
					TitleBar.EnableRefreshButton = true;
					TitleBar.EnableSortButtons = true;
					LoadingGrid.Visibility = Visibility.Collapsed;
				}
			}
		}

		public LibraryFilterPage() {
			this.InitializeComponent();
			this.InitializeIconsAnimation();
			TitleBar.OnExpandedChanged += TitleBar_OnExpandedChanged;
			TitleBar.OnRefresh += TitleBar_OnRefresh;
			TitleBar.OnSearchInput += TitleBar_OnSearchInput;
			TitleBar.OnSearchSubmit += TitleBar_OnSearchSubmit;
			TitleBar.OnAsecDesOrderChanged += TitleBar_OnAsecDesOrderChanged;
			TitleBar.OnOrderTypeChanged += TitleBar_OnOrderTypeChanged;
			TitleBar.EnableClearEmptyFile = false;
		}

		protected override void OnNavigatedTo(NavigationEventArgs e) {
			base.OnNavigatedTo(e);
			if(e.Parameter is LibraryFilterArgs args) {
				this.Args = args;
				libraryPage = args.Parent;
				TitleBar.Title = args.Title;

				GroupView.Library = libraryPage;
				GroupView.ViewType = libraryPage.ViewType;

				TitleBar.IsExpanded = args.IsExpanded;
				SCheckBox.IsChecked = args.SafeCheck;
				QCheckBox.IsChecked = args.QuestionableCheck;
				ECheckBox.IsChecked = args.ExplicitCheck;
				ImageCheckBox.IsChecked = args.ImageCheck;
				GifCheckBox.IsChecked = args.GifCheck;
				WebmCheckBox.IsChecked = args.WebmCheck;
				FromSlider.Value = args.MinimumScore;
				ToSlider.Value = args.MaximumScore;
				UpdateScoreLimit();
				UpdateSelectedCount();
				UpdateAndOrText();
				UpdateTotalCountText();

				if(args.Files != null) {
					GroupView.SetImages(args.Files);
				}
			}

			initializing = false;
		}

		private void InitializeIconsAnimation() {
			CreateIconStoryboard(RatingIcon);
			CreateIconStoryboard(TypeIcon);
			CreateIconStoryboard(ScoreIcon);
		}

		private Storyboard CreateIconStoryboard(FontIcon icon) {
			Storyboard storyboard = new();
			DoubleAnimation doubleAnimation = new() {
				EnableDependentAnimation = true,
				EasingFunction = new ExponentialEase() {
					Exponent = 7,
					EasingMode = EasingMode.EaseOut,
				},
			};
			Storyboard.SetTarget(doubleAnimation, icon);
			Storyboard.SetTargetProperty(doubleAnimation, "FontSize");
			storyboard.Children.Add(doubleAnimation);
			icon.Tag = storyboard;
			icon.PointerEntered += (s, e) => {
				doubleAnimation.From = icon.FontSize;
				doubleAnimation.To = 58;
				storyboard.Begin();
			};
			icon.PointerExited += (s, e) => {
				doubleAnimation.From = icon.FontSize;
				doubleAnimation.To = 48;
				storyboard.Begin();
			};
			return storyboard;
		}

		private void TitleBar_OnExpandedChanged(bool b) {
			this.Args.IsExpanded = b;
			ExpanderHeightAnimation.From = FilterGrid.Height;
			ExpanderHeightAnimation.To = b ? 250 : 0;
			ExpanderStoryBoard.Begin();
			FilterGrid.Margin = b ? new Thickness(10) : new Thickness(10, 0, 10, 0);
		}
		private void TitleBar_OnOrderTypeChanged(OrderType orderType) {
			Args.OrderType = orderType;
			UpdateImages();
		}

		private void TitleBar_OnAsecDesOrderChanged(OrderEnum orderEnum) {
			Args.Order = orderEnum;
			UpdateImages();
		}

		private void TitleBar_OnSearchSubmit(string text) {
			UpdateImages(text);
		}

		private void TitleBar_OnSearchInput(VirtualKey key) {
			if(key == MainPage.SEARCH_KEY) {
				MainPage.Instance.DelayInputKeyListener();
			}
		}

		private async void TitleBar_OnRefresh() {
			await Load();
		}

		private async void FoldersButton_Click(object sender, RoutedEventArgs e) {
			if(libraryPage.RootFoldersArgs.Folders == null || libraryPage.RootFoldersArgs.Folders.Count == 0) {
				return;
			}
			var foldersView = new FoldersSelectionView(libraryPage.RootFoldersArgs.Folders, Args.SelectedFolders);
			if(await new ContentDialog() {
				Title = "Manage Your Search Tags".Language(),
				Content = foldersView,
				PrimaryButtonText = "Confirm".Language(),
				CloseButtonText = "Back".Language(),
			}.ShowAsync() == ContentDialogResult.Primary) {
				Args.SelectedFolders.Clear();
				string tooltip = "";
				int count = 0;
				foreach(StorageFolder item in foldersView.GetSelected()) {
					Args.SelectedFolders.Add(item);
					tooltip += item.DisplayName + "\n";
					count++;
				}
				ToolTipService.SetToolTip(sender as Button, tooltip.Trim());
				UpdateSelectedCount();
			}
		}

		private async void TagsButton_Click(object sender, RoutedEventArgs e) {
			var dialog = new ContentDialog() {
				Title = "Manage Your Search Tags".Language(),
			};
			var content = new LocalTagsManagementView(dialog, Args.SelectedTags.ToArray());
			dialog.Content = content;
			await dialog.ShowAsync();

			if(content.HandleConfirm) {
				Args.SelectedTags.Clear();
				string tooltip = "";
				foreach(string item in content.GetResult()) {
					Args.SelectedTags.Add(item);
					tooltip += item + "\n";
				}
				ToolTipService.SetToolTip(sender as Button, tooltip.Trim());
				UpdateSelectedCount();
			}
		}

		private void UpdateSelectedCount() {
			SelectedTagsCount.Text = $"{Args.SelectedTags.Count()}";
			SelectedFoldersCount.Text = $"{Args.SelectedFolders.Count()}";
		}

		public void UpdateSize(int size) {
			GroupView.UpdateSize(size);
		}

		public LibraryItemsGroupView GetGroupView() => GroupView;

		private async void SearchButton_Click(object sender, RoutedEventArgs e) {
			bool result = await Load();
			if(result && Args.Files.Count >= 10) {
				await Task.Delay(100);
				TitleBar.IsExpanded = false;
			}
		}

		private async Task<bool> Load() {
			if(!Args.SafeCheck && !Args.QuestionableCheck && !Args.ExplicitCheck) {
				InsufficientRequirements.PreferredPlacement = TeachingTipPlacementMode.Bottom;
				InsufficientRequirements.Target = RatingIcon;
				InsufficientRequirements.Subtitle = "Please Select Least One CheckBox".Language();
				InsufficientRequirements.IsOpen = true;
				return false;
			} else if(!Args.ImageCheck && !Args.GifCheck && !Args.WebmCheck) {
				InsufficientRequirements.PreferredPlacement = TeachingTipPlacementMode.Bottom;
				InsufficientRequirements.Target = TypeIcon;
				InsufficientRequirements.Subtitle = "Please Select Least One CheckBox".Language();
				InsufficientRequirements.IsOpen = true;
				return false;
			} else if(Args.SelectedFolders.Count == 0) {
				InsufficientRequirements.PreferredPlacement = TeachingTipPlacementMode.Right;
				InsufficientRequirements.Target = FoldersButton;
				InsufficientRequirements.Subtitle = "Please Select Least One Folder".Language();
				InsufficientRequirements.IsOpen = true;
				return false;
			}
			IsLoading = true;
			GroupView.ClearItems();
			LoadingText.Text = "Loading".Language();
			int foldersCount = Args.SelectedFolders.Count;
			var result = new List<(MetaFile meta, BitmapImage image, StorageFile file)>();
			for(int i = 0; i < Args.SelectedFolders.Count; i++) {
				StorageFolder folder = Args.SelectedFolders[i];
				IReadOnlyList<StorageFile> files = await folder.GetFilesAsync();
				var list = await Local.GetMetaFiles(folder, (next, index, length) => {
					LoadingText.Text = "Loading".Language() +
						"\n(" + "File".Language() + $": {next.Name}) {index}/{length}" +
						"\n(" + "Folder".Language() + $": /{folder.Name}/) {i + 1}/{foldersCount}";
				});
				result.AddRange(list);
			}
			result = result.Where(l => {
				string rating = l.meta.MyPost.rating.ToLower();
				if(!Args.SafeCheck && rating == "s") {
					return false;
				}
				if(!Args.QuestionableCheck && rating == "q") {
					return false;
				}
				if(!Args.ExplicitCheck && rating == "e") {
					return false;
				}
				string fileType = l.file.FileType.ToLower();
				if(!Args.ImageCheck && (fileType.ToLower() == ".png" || fileType.ToLower() == ".jpg")) {
					return false;
				}
				if(!Args.GifCheck && fileType.ToLower() == ".gif") {
					return false;
				}
				if(!Args.WebmCheck && fileType.ToLower() == "webm") {
					return false;
				}
				int score = l.meta.MyPost.score.total;
				var (min, max) = GetScore();
				if(score < min || score > max) {
					return false;
				}
				if(Args.SelectedTags.Count != 0) {
					var tags = new List<Pair<string, bool>>(
						Args.SelectedTags.Select(t => new Pair<string, bool>(t, false))
					);
					foreach(string tag in l.meta.MyPost.tags.GetAllTags()) {
						foreach(Pair<string, bool> pair in tags) {
							if(pair.Key == tag) {
								pair.Value = true;
								break;
							}
						}
					}
					if(Args.IsAnd) {
						return tags.All(t => t.Value);
					} else {
						return tags.Any(t => t.Value);
					}
				}
				return true;
			}).ToList();
			Args.Files = result;
			UpdateImages();
			UpdateTotalCountText();
			IsLoading = false;
			return true;
		}

		private async void UpdateImages(string matchedName = null) {
			List<(MetaFile meta, BitmapImage bitmap, StorageFile file)> files = Args.Files;
			if(!string.IsNullOrWhiteSpace(matchedName)) {
				files = files.Where(f => f.file.Name.Contains(matchedName)).ToList();
			}
			files = (await Explorer.OrderImagesAsync(this, files, Args.OrderType, Args.Order)).ToList();
			GroupView.SetImages(files);
		}

		public void RefreshRequest() {

		}

		private void FromSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e) {
			if(initializing) {
				return;
			}
			if(60 - (int)FromSlider.Value < (int)ToSlider.Value) {
				ToSlider.Value = 60 - (int)FromSlider.Value;
			}
			Args.MinimumScore = (int)FromSlider.Value;
			Args.MaximumScore = (int)ToSlider.Value;
			UpdateScoreLimit();
		}

		private void ToSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e) {
			if(initializing) {
				return;
			}
			if((int)FromSlider.Value > 60 - (int)ToSlider.Value) {
				FromSlider.Value = 60 - (int)ToSlider.Value;
			}
			Args.MinimumScore = (int)FromSlider.Value;
			Args.MaximumScore = (int)ToSlider.Value;
			UpdateScoreLimit();
		}

		public (int from, int to) GetScore() {
			int from = (int)FromSlider.Value * 100;
			int to = (int)(60 - ToSlider.Value) * 100;
			return (from, to + 1);
		}

		private void UpdateScoreLimit() {
			var (from, to) = GetScore();
			ScoreText.Text = $"( {from} - {to} )";
		}

		private void SCheckBox_Checked(object sender, RoutedEventArgs e) {
			if(initializing) {
				return;
			}
			bool isChecked = ((CheckBox)sender).IsChecked.Value;
			Args.SafeCheck = isChecked;
		}

		private void QCheckBox_Checked(object sender, RoutedEventArgs e) {
			if(initializing) {
				return;
			}
			bool isChecked = ((CheckBox)sender).IsChecked.Value;
			Args.QuestionableCheck = isChecked;
		}

		private void ECheckBox_Checked(object sender, RoutedEventArgs e) {
			if(initializing) {
				return;
			}
			bool isChecked = ((CheckBox)sender).IsChecked.Value;
			Args.ExplicitCheck = isChecked;
		}

		private void ImageCheckBox_Checked(object sender, RoutedEventArgs e) {
			if(initializing) {
				return;
			}
			bool isChecked = ((CheckBox)sender).IsChecked.Value;
			Args.ImageCheck = isChecked;
		}

		private void GifCheckBox_Checked(object sender, RoutedEventArgs e) {
			if(initializing) {
				return;
			}
			bool isChecked = ((CheckBox)sender).IsChecked.Value;
			Args.GifCheck = isChecked;
		}

		private void WebmCheckBox_Checked(object sender, RoutedEventArgs e) {
			if(initializing) {
				return;
			}
			bool isChecked = ((CheckBox)sender).IsChecked.Value;
			Args.WebmCheck = isChecked;
		}

		private void AndOrButton_Click(object sender, RoutedEventArgs e) {
			Args.IsAnd = !Args.IsAnd;
			UpdateAndOrText();
		}

		private void UpdateAndOrText() {
			if(Args.IsAnd) {
				AndOrText.Text = "AND".Language();
			} else {
				AndOrText.Text = "OR".Language();
			}
		}

		private void UpdateTotalCountText() {
			if(Args.TotalCount == -1) {
				TotalFoundText.Text = "";
			} else {
				TotalFoundText.Text = "Total Found" + $": {Args.TotalCount}";
			}
		}

		public void DisplayHeader(bool enabled) {
			HeaderGrid.Visibility = enabled ? Visibility.Visible : Visibility.Collapsed;
			FilterGrid.Visibility = enabled ? Visibility.Visible : Visibility.Collapsed;
		}
	}
}
