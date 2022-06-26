using E621Downloader.Models;
using E621Downloader.Models.Inerfaces;
using E621Downloader.Models.Locals;
using E621Downloader.Models.Posts;
using E621Downloader.Views;
using Microsoft.Toolkit.Uwp.UI.Controls;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using SymbolIconSource = Microsoft.UI.Xaml.Controls.SymbolIconSource;

namespace E621Downloader.Pages {
	public sealed partial class SpotPage: Page, IPage {
		public static SpotPage Instance;
		private int CurrentAmount => (int)AmountSlider.Value;
		private string[] selectedTags = new string[0];
		private string[] inputTags = new string[0];
		private bool internalChange = true;
		private bool onTask;
		public List<Post> Posts { get; private set; } = new List<Post>();
		private CancellationTokenSource cts = new();

		public SpotPage() {
			Instance = this;
			this.InitializeComponent();
			this.SaveLocalSettingsAsync();
			this.NavigationCacheMode = NavigationCacheMode.Enabled;
			UpdateScoreLimit();
		}
		private void Page_Loaded(object sender, RoutedEventArgs e) {
			UpdateTypesRadioButtonEnable();
			switch(LocalSettings.Current.spot_fileType) {
				case FileType.Png:
					RadioButton_PNG.IsChecked = true;
					break;
				case FileType.Jpg:
					RadioButton_JPG.IsChecked = true;
					break;
				case FileType.Gif:
					RadioButton_GIF.IsChecked = true;
					break;
				case FileType.Webm:
					RadioButton_WEBM.IsChecked = true;
					break;
			}
			IncludeSafeCheckBox.IsChecked = LocalSettings.Current.spot_includeSafe;
			IncludeQuestionableCheckBox.IsChecked = LocalSettings.Current.spot_includeQuestoinable;
			IncludeExplicitCheckBox.IsChecked = LocalSettings.Current.spot_includeExplicit;
			internalChange = false;
		}
		protected override void OnNavigatedTo(NavigationEventArgs e) {
			base.OnNavigatedTo(e);
			this.FocusModeUpdate();
			onTask = true;
		}
		protected override void OnNavigatedFrom(NavigationEventArgs e) {
			base.OnNavigatedFrom(e);
			onTask = false;
		}
		private async void SaveLocalSettingsAsync() {
			while(true) {
				if(onTask) {
					LocalSettings.Save();
				}
				await Task.Delay(2000);
			}
		}

		private void HamburgerButton_Tapped(object sender, TappedRoutedEventArgs e) {
			MainSplitView.IsPaneOpen = !MainSplitView.IsPaneOpen;
		}

		private void AmountSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e) {
			AmountText.Text = "Amount".Language() + ": " + (int)e.NewValue;
			LocalSettings.Current.spot_amount = (int)e.NewValue;
		}

		private async void TagsButton_Tapped(object sender, TappedRoutedEventArgs e) {
			SpotTagsSelection content = new();
			ContentDialog dialog = new() {
				Title = "Select Your Tags".Language(),
				Content = content,
				PrimaryButtonText = "Confirm".Language(),
				SecondaryButtonText = "Back".Language(),
			};
			content.Initialize(inputTags, selectedTags);
			if(await dialog.ShowAsync() == ContentDialogResult.Primary) {
				inputTags = content.GetInputTags();
				selectedTags = content.GetSelectedTags();
				UpdateTagsText(inputTags, selectedTags);
			}
		}

		private void UpdateTagsText(string[] input, string[] selected) {
			InputTagsText.Text = input.Length == 0 ? "All Tags".Language() : "Input".Language() + $": {input.Length}";
			SelectedTagsText.Text = selected.Length == 0 ? "No Followings Selected".Language() : "Following".Language() + $": {selected.Length}";
		}

		private async void StartButton_Tapped(object sender, TappedRoutedEventArgs e) {
			LoadingRing.IsActive = true;
			MainGridView.Items.Clear();
			NoDataHint.Visibility = Visibility.Collapsed;
			List<string> tags = selectedTags.Concat(inputTags).ToList();
			bool S = IncludeSafeCheckBox.IsChecked.Value;
			bool Q = IncludeQuestionableCheckBox.IsChecked.Value;
			bool E = IncludeExplicitCheckBox.IsChecked.Value;

			if(S && Q && E) {
				//do nothing for all
			} else if(S && Q && !E) {
				tags.Add("-rating:e");
			} else if(S && !Q && E) {
				tags.Add("-rating:q");
			} else if(!S && Q && E) {
				tags.Add("-rating:s");
			} else if(S && !Q && !E) {
				tags.Add("rating:s");
			} else if(!S && Q && !E) {
				tags.Add("rating:q");
			} else if(!S && !Q && E) {
				tags.Add("rating:e");
			} else if(!S && !Q && !E) {
				var tip = new TeachingTip() {
					IsOpen = true,
					Target = RatingPanel,
					Title = "Parameters Error".Language(),
					Subtitle = "Please Select At Least One Rating".Language(),
					PreferredPlacement = TeachingTipPlacementMode.Right,
					IconSource = new SymbolIconSource() {
						Symbol = Symbol.Important,
					},
				};
				RatingPanel.Children.Add(tip);
				tip.Closed += (s, args) => {
					RatingPanel.Children.Remove(tip);
				};
				LoadingRing.IsActive = false;
				return;
			}

			string fileType = "";
			switch(LocalSettings.Current.spot_fileType) {
				case FileType.Png:
					fileType = "type:png";
					break;
				case FileType.Jpg:
					fileType = "type:jpg";
					break;
				case FileType.Gif:
					fileType = "type:gif";
					break;
				case FileType.Webm:
					fileType = "type:webm";
					break;
				case FileType.Anim:
					break;
				default:
					break;
			}

			switch(LocalSettings.Current.spot_FilterType) {
				case SpotFilterType.All:
					fileType = "";
					break;
				case SpotFilterType.Exclude:
					fileType = "-" + fileType;
					break;
				case SpotFilterType.Specify:
					break;
				default:
					fileType = "";
					break;
			}

			if(!string.IsNullOrWhiteSpace(fileType)) {
				tags.Add(fileType);
			}

			(int from, int to) = GetScoreRange();
			tags.Add($"score:{from}..{to}");
			if(cts != null) {
				cts.Cancel();
				cts.Dispose();
				cts = null;
			}
			cts = new CancellationTokenSource();
			List<Post> posts = await Post.GetPostsByRandomAsync(cts.Token, CurrentAmount, tags.ToArray());
			if(posts == null) {
				LoadingRing.IsActive = false;
				return;
			} else if(posts.Count == 0) {
				NoDataHint.Visibility = Visibility.Visible;
			} else {
				MainGridView.Items.Clear();
				for(int i = 0; i < posts.Count; i++) {
					ImageHolder holder = new(this, posts[i], i, PathType.PostID, posts[i].id) {
						Height = 400,
						Width = 400,
					};
					MainGridView.Items.Add(holder);
				}
			}
			this.Posts = posts;
			LoadingRing.IsActive = false;
		}

		private void CloseRatingPanelTeachingTips() {
			foreach(TeachingTip item in RatingPanel.Children.Where(i => i is TeachingTip)) {
				item.IsOpen = false;
			}
		}

		private void CloseTypePanelTeachingTips() {
			foreach(TeachingTip item in TypePanel.Children.Where(i => i is TeachingTip)) {
				item.IsOpen = false;
			}
		}

		private void IncludeSafeCheckBox_Checked(object sender, RoutedEventArgs e) {
			if(internalChange) {
				return;
			}
			CloseRatingPanelTeachingTips();
			LocalSettings.Current.spot_includeSafe = IncludeSafeCheckBox.IsChecked.Value;
		}

		private void IncludeQuestionableCheckBox_Checked(object sender, RoutedEventArgs e) {
			if(internalChange) {
				return;
			}
			CloseRatingPanelTeachingTips();
			LocalSettings.Current.spot_includeQuestoinable = IncludeQuestionableCheckBox.IsChecked.Value;
		}

		private void IncludeExplicitCheckBox_Checked(object sender, RoutedEventArgs e) {
			if(internalChange) {
				return;
			}
			CloseRatingPanelTeachingTips();
			LocalSettings.Current.spot_includeExplicit = IncludeExplicitCheckBox.IsChecked.Value;
		}

		private void ScoreRangeSlider_ValueChanged(object sender, RangeChangedEventArgs e) {
			var slider = sender as RangeSelector;
			string start = slider.RangeStart <= slider.Minimum ? "Minimum".Language() : $"{slider.RangeStart}";
			string end = slider.RangeEnd >= slider.Maximum ? "Maximum".Language() : $"{slider.RangeEnd}";
			Debug.WriteLine(start + " : " + end);
			ScoreLimitText.Text = "Score Limit".Language() + $": ({start} - {end})";
		}

		private void SizeButton_Click(object sender, RoutedEventArgs e) {

		}

		private void SizeSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e) {

		}

		private const int MIN = -40;
		private const int MAX = 100;
		private void FromSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e) {
			if(60 - (int)FromSlider.Value < (int)ToSlider.Value) {
				ToSlider.Value = 60 - (int)FromSlider.Value;
			}
			UpdateScoreLimit();
		}

		private void ToSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e) {
			if((int)FromSlider.Value > 60 - (int)ToSlider.Value) {
				FromSlider.Value = 60 - (int)ToSlider.Value;
			}
			UpdateScoreLimit();
		}

		private void UpdateScoreLimit() {
			int from = (int)FromSlider.Value * 100;
			int to = (int)(60 - ToSlider.Value) * 100;
			ScoreLimitText.Text = "Score Limit".Language() + $": ({from} - {to + 50})";
		}

		private (int from, int to) GetScoreRange() {
			return ((int)FromSlider.Value * 100, (int)(60 - ToSlider.Value) * 100 + 50);
		}

		private void ClearButton_Tapped(object sender, TappedRoutedEventArgs e) {
			MainGridView.Items.Clear();
		}

		private void ResizeBar_OnSizeChanged(int value) {
			if(MainGridView == null) {
				return;
			}
			foreach(ImageHolder item in MainGridView.Items) {
				item.Height = value;
				item.Width = value;
			}
		}

		private void TypeSwitchButton_Click(object sender, RoutedEventArgs e) {
			switch(LocalSettings.Current.spot_FilterType) {
				case SpotFilterType.All:
					LocalSettings.Current.spot_FilterType = SpotFilterType.Exclude;
					break;
				case SpotFilterType.Exclude:
					LocalSettings.Current.spot_FilterType = SpotFilterType.Specify;
					break;
				case SpotFilterType.Specify:
					LocalSettings.Current.spot_FilterType = SpotFilterType.All;
					break;
				default:
					return;
			}
			UpdateTypesRadioButtonEnable();
		}

		private void UpdateTypesRadioButtonEnable() {
			if(LocalSettings.Current.spot_FilterType == SpotFilterType.All) {
				RadioButton_PNG.IsEnabled = false;
				RadioButton_JPG.IsEnabled = false;
				RadioButton_GIF.IsEnabled = false;
				RadioButton_WEBM.IsEnabled = false;
			} else {
				RadioButton_PNG.IsEnabled = true;
				RadioButton_JPG.IsEnabled = true;
				RadioButton_GIF.IsEnabled = true;
				RadioButton_WEBM.IsEnabled = true;
			}
			TypeSwitchButtonIcon.Glyph = LocalSettings.Current.spot_FilterType switch {
				SpotFilterType.All => "\uE8A9",
				SpotFilterType.Exclude => "\uE152",
				SpotFilterType.Specify => "\uE153",
				_ => "\uE10C",
			};
			TypeSwitchButtonText.Text = LocalSettings.Current.spot_FilterType.ToString().Language();
		}

		private void RadioButton_PNG_Click(object sender, RoutedEventArgs e) {
			RadioButton_PNG.IsChecked = true;
			LocalSettings.Current.spot_fileType = FileType.Png;
		}

		private void RadioButton_JPG_Click(object sender, RoutedEventArgs e) {
			RadioButton_JPG.IsChecked = true;
			LocalSettings.Current.spot_fileType = FileType.Jpg;
		}

		private void RadioButton_GIF_Click(object sender, RoutedEventArgs e) {
			RadioButton_GIF.IsChecked = true;
			LocalSettings.Current.spot_fileType = FileType.Gif;
		}

		private void RadioButton_WEBM_Click(object sender, RoutedEventArgs e) {
			RadioButton_WEBM.IsChecked = true;
			LocalSettings.Current.spot_fileType = FileType.Webm;
		}

		void IPage.UpdateNavigationItem() {
			MainPage.Instance.currentTag = PageTag.Spot;
			MainPage.Instance.UpdateNavigationItem();
		}

		void IPage.FocusMode(bool enabled) {
			if(enabled) {
				MainSplitView.IsPaneOpen = false;
			}
		}
	}

	public enum SpotFilterType {
		All, Exclude, Specify
	}
}
