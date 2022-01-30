using E621Downloader.Models;
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
	public sealed partial class SpotPage: Page {
		public static SpotPage Instance;
		private int CurrentAmount => (int)AmountSlider.Value;
		private string[] selectedTags = new string[0];
		private string[] inputTags = new string[0];
		private bool internalChange = true;
		private bool onTask;
		public List<Post> Posts { get; private set; } = new List<Post>();
		private CancellationTokenSource cts = new CancellationTokenSource();

		public SpotPage() {
			Instance = this;
			this.InitializeComponent();
			this.SaveLocalSettingsAsync();
			this.NavigationCacheMode = NavigationCacheMode.Enabled;
			UpdateScoreLimit();
		}
		private void Page_Loaded(object sender, RoutedEventArgs e) {
			AllowWebmCheckBox.IsChecked = LocalSettings.Current.spot_allowWebm;
			AllowGifCheckBox.IsChecked = LocalSettings.Current.spot_allowGif;
			AllowImageCheckBox.IsChecked = LocalSettings.Current.spot_allowImage;
			IncludeSafeCheckBox.IsChecked = LocalSettings.Current.spot_includeSafe;
			IncludeQuestionableCheckBox.IsChecked = LocalSettings.Current.spot_includeQuestoinable;
			IncludeExplicitCheckBox.IsChecked = LocalSettings.Current.spot_includeExplicit;
			internalChange = false;
		}
		protected override void OnNavigatedTo(NavigationEventArgs e) {
			base.OnNavigatedTo(e);
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
			AmountText.Text = "Amount: " + (int)e.NewValue;
			LocalSettings.Current.spot_amount = (int)e.NewValue;
		}

		private async void TagsButton_Tapped(object sender, TappedRoutedEventArgs e) {
			SpotTagsSelection content = new SpotTagsSelection();
			ContentDialog dialog = new ContentDialog {
				Title = "Select Your Tags",
				Content = content,
				PrimaryButtonText = "Confirm",
				SecondaryButtonText = "Back",
			};
			content.Initialize(inputTags, selectedTags);
			if(await dialog.ShowAsync() == ContentDialogResult.Primary) {
				inputTags = content.GetInputTags();
				selectedTags = content.GetSelectedTags();
				UpdateTagsText(inputTags, selectedTags);
			}
		}

		private void UpdateTagsText(string[] input, string[] selected) {
			InputTagsText.Text = input.Length == 0 ? "All Tags" : $"Input: {input.Length}";
			SelectedTagsText.Text = selected.Length == 0 ? "No Followings Selected" : $"Following: {selected.Length}";
		}

		private async void StartButton_Tapped(object sender, TappedRoutedEventArgs e) {
			LoadingRing.IsActive = true;
			List<string> tags = selectedTags.ToList();
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
					Title = "Parameters Error",
					Subtitle = "Please Select At Least One Rating",
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

			bool WEBM = AllowWebmCheckBox.IsChecked.Value;
			bool GIF = AllowGifCheckBox.IsChecked.Value;
			//bool IMG = AllowImageCheckBox.IsChecked.Value;

			if(WEBM && GIF) {
				//do nothing for all
			} else if(!WEBM && GIF) {
				tags.Add("-type:WEBM");
			} else if(WEBM && !GIF) {
				tags.Add("-type:GIF");
			} else if(!WEBM && !GIF) {
				//var tip = new TeachingTip() {
				//	IsOpen = true,
				//	Target = TypePanel,
				//	Title = "Parameters Error",
				//	Subtitle = "Please Select At Least One Type",
				//	PreferredPlacement = TeachingTipPlacementMode.Right,
				//	IconSource = new SymbolIconSource() {
				//		Symbol = Symbol.Important,
				//	},
				//};
				//TypePanel.Children.Add(tip);
				//tip.Closed += (s, args) => {
				//	TypePanel.Children.Remove(tip);
				//};
				//LoadingRing.IsActive = false;
				//return;
			}

			(int, int) range = GetScoreRange();
			tags.Add($"score:{range.Item1}..{range.Item2}");
			List<Post> posts = await Post.GetPostsByRandomAsync(cts.Token, CurrentAmount, tags.ToArray());
			if(posts == null || posts.Count == 0) {
				await MainPage.CreatePopupDialog("Error", "There is No Post(s) Found");
			} else {
				MainGridView.Items.Clear();
				for(int i = 0; i < posts.Count; i++) {
					ImageHolder holder = new ImageHolder(this, posts[i], i, PathType.PostID, posts[i].id) {
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

		private void AllowWebmCheckBox_Checked(object sender, RoutedEventArgs e) {
			if(internalChange) {
				return;
			}
			CloseTypePanelTeachingTips();
			LocalSettings.Current.spot_allowWebm = AllowWebmCheckBox.IsChecked.Value;
		}

		private void AllowGifCheckBox_Checked(object sender, RoutedEventArgs e) {
			if(internalChange) {
				return;
			}
			CloseTypePanelTeachingTips();
			LocalSettings.Current.spot_allowGif = AllowGifCheckBox.IsChecked.Value;
		}

		private void AllowImageCheckBox_Checked(object sender, RoutedEventArgs e) {
			if(internalChange) {
				return;
			}
			CloseTypePanelTeachingTips();
			LocalSettings.Current.spot_allowImage = AllowImageCheckBox.IsChecked.Value;
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
			string start = slider.RangeStart <= slider.Minimum ? "Minimum" : $"{slider.RangeStart}";
			string end = slider.RangeEnd >= slider.Maximum ? "Maximum" : $"{slider.RangeEnd}";
			Debug.WriteLine(start + " : " + end);
			ScoreLimitText.Text = $"Score Limit: ({start} - {end})";
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
			ScoreLimitText.Text = $"Score Limit: ({from} - {to})";
		}

		private (int, int) GetScoreRange() {
			return ((int)FromSlider.Value * 100, (int)(60 - ToSlider.Value) * 100);
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
	}
}
