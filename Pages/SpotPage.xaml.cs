using E621Downloader.Models;
using E621Downloader.Models.Locals;
using E621Downloader.Models.Posts;
using E621Downloader.Views;
using Microsoft.Toolkit.Uwp.UI.Controls;
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

namespace E621Downloader.Pages {
	public sealed partial class SpotPage: Page {
		public static SpotPage Instance;
		private int CurrentAmount => (int)AmountSlider.Value;
		private string[] selectedTags = new string[0];
		private string[] inputTags = new string[0];
		private bool internalChange = true;
		private bool onTask;
		public List<Post> Posts { get; private set; } = new List<Post>();
		private int _size;
		public int Size {
			get => _size;
			private set {
				_size = value;
				if(MainGridView == null) {
					return;
				}
				foreach(ImageHolder item in MainGridView.Items) {
					item.Height = value;
					item.Width = value;
				}
			}
		}
		public SpotPage() {
			Instance = this;
			this.InitializeComponent();
			this.SaveLocalSettingsAsync();
			this.NavigationCacheMode = NavigationCacheMode.Enabled;
		}
		private void Page_Loaded(object sender, RoutedEventArgs e) {
			AllowWebmCheckBox.IsChecked = LocalSettings.Current.spot_allowWebm;
			AllowGifCheckBox.IsChecked = LocalSettings.Current.spot_allowGif;
			AllowBlackListCheckBox.IsChecked = LocalSettings.Current.spot_allowBlackList;
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
			SelectedTagsText.Text = selected.Length == 0 ? "From All Followings" : $"Following: {selected.Length}";
		}

		private async void StartButton_Tapped(object sender, TappedRoutedEventArgs e) {
			LoadingRing.IsActive = true;
			List<Post> posts = await Post.GetPostsByRandom(CurrentAmount, selectedTags);
			if(posts == null || posts.Count == 0) {
				MainPage.CreateInstantDialog("Error", "There is No Post(s) Found");
			} else {
				MainGridView.Items.Clear();
				for(int i = 0; i < posts.Count; i++) {
					ImageHolder holder = new ImageHolder(posts[i], i) {
						Height = 400,
						Width = 400,
					};
					MainGridView.Items.Add(holder);
				}
			}
			this.Posts = posts;
			LoadingRing.IsActive = false;
		}

		private void AllowWebmCheckBox_Checked(object sender, RoutedEventArgs e) {
			if(internalChange) {
				return;
			}
			LocalSettings.Current.spot_allowWebm = AllowWebmCheckBox.IsChecked.Value;
		}

		private void AllowGifCheckBox_Checked(object sender, RoutedEventArgs e) {
			if(internalChange) {
				return;
			}
			LocalSettings.Current.spot_allowGif = AllowGifCheckBox.IsChecked.Value;
		}

		private void IncludeSafeCheckBox_Checked(object sender, RoutedEventArgs e) {
			if(internalChange) {
				return;
			}
			LocalSettings.Current.spot_includeSafe = IncludeSafeCheckBox.IsChecked.Value;
		}

		private void IncludeQuestionableCheckBox_Checked(object sender, RoutedEventArgs e) {
			if(internalChange) {
				return;
			}
			LocalSettings.Current.spot_includeQuestoinable = IncludeQuestionableCheckBox.IsChecked.Value;
		}

		private void IncludeExplicitCheckBox_Checked(object sender, RoutedEventArgs e) {
			if(internalChange) {
				return;
			}
			LocalSettings.Current.spot_includeExplicit = IncludeExplicitCheckBox.IsChecked.Value;
		}

		private void AllowBlackListCheckBox_Checked(object sender, RoutedEventArgs e) {
			if(internalChange) {
				return;
			}
			LocalSettings.Current.spot_allowBlackList = AllowBlackListCheckBox.IsChecked.Value;
		}

		private void ScoreRangeSlider_ValueChanged(object sender, RangeChangedEventArgs e) {
			var slider = sender as RangeSelector;
			string start = slider.RangeStart <= slider.Minimum ? "Minimum" : $"{slider.RangeStart}";
			string end = slider.RangeEnd >= slider.Maximum ? "Maximum" : $"{slider.RangeEnd}";
			Debug.WriteLine(start + " : " + end);
			ScoreLimitText.Text = $"Score Limit: ({start} - {end})";
		}

		private void SizeButton_Click(object sender, RoutedEventArgs e) {
			int s = Size;
			if(s + 25 > 500) {
				s = 200;
			} else {
				s += 25;
			}
			SizeSlider.Value = s;
		}

		private void SizeSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e) {
			Size = (int)e.NewValue;
			ToolTipService.SetToolTip(SizeButton, "Current Size : " + Size);
		}
	}
}
