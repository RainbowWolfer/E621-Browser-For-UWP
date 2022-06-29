using E621Downloader.Models;
using E621Downloader.Models.Networks;
using E621Downloader.Models.Posts;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace E621Downloader.Views.TagsManagementSection {
	public sealed partial class TagsSelectionView: Page {
		private readonly ContentDialog dialog;
		public ResultType Result { get; private set; } = ResultType.None;
		private readonly Dictionary<string, E621Tag> tags_pool = new();
		private readonly List<string> currentTags = new();

		private int currentSelectedIndex = -1;
		//private bool hasSelectionChanged = false;

		private QuickOptionsOrder order = QuickOptionsOrder.Default;
		private QuickOptionsType type = QuickOptionsType.All;
		private QuickOptionsRating rating = QuickOptionsRating.All;
		private bool isInPool = false;
		private bool isDeleted = false;

		public QuickOptionsOrder Order {
			get => order;
			private set {
				order = value;
				OrderDropDownButton.Content = order switch {
					QuickOptionsOrder.Default => "Order".Language(),
					QuickOptionsOrder.New => "New".Language(),
					QuickOptionsOrder.Score => "Score".Language(),
					QuickOptionsOrder.Favorite => "Favorite".Language(),
					QuickOptionsOrder.Rank => "Rank".Language(),
					QuickOptionsOrder.Random => "Random".Language(),
					_ => "Not Found".Language(),
				};
			}
		}

		public QuickOptionsType Type {
			get => type;
			private set {
				type = value;
				TypeDropDownButton.Content = type switch {
					QuickOptionsType.All => "Type".Language(),
					QuickOptionsType.PNG => "PNG",
					QuickOptionsType.JPG => "JPG",
					QuickOptionsType.GIF => "GIF",
					QuickOptionsType.WEBM => "WEBM",
					QuickOptionsType.ANIM => "ANIM",
					_ => "Not Found".Language(),
				};
			}
		}

		public QuickOptionsRating Rating {
			get => rating;
			private set {
				rating = value;
				RatingDropDownButton.Content = rating switch {
					QuickOptionsRating.All => "Rating".Language(),
					QuickOptionsRating.Safe => "Safe",
					QuickOptionsRating.Questionable => "Questionable",
					QuickOptionsRating.Explicit => "Explicit",
					_ => "Not Found".Language(),
				};
			}
		}

		public bool IsInPool {
			get => isInPool;
			private set {
				isInPool = value;
				IsInPoolBox.IsChecked = isInPool;
			}
		}

		public bool IsDeleted {
			get => isDeleted;
			private set {
				isDeleted = value;
				IsDeletedBox.IsChecked = isDeleted;
			}
		}

		private bool internalChange = false;//through list selection & quick options click

		public TagsSelectionView(ContentDialog dialog, string[] tags) {
			this.InitializeComponent();
			this.dialog = dialog;
			foreach(string item in tags) {
				MySuggestBox.Text += item + " ";
			}
			MySuggestBox.Text = MySuggestBox.Text.Trim();
			MySuggestBox.SelectionStart = MySuggestBox.Text.Length;
			dialog.Closing += Dialog_Closing;
			FocusTextBox();


			MySuggestBox_TextChanged(null, null);
			CalculateCurrentTags();

		}

		private bool ableToHide = true;

		private void Dialog_Closing(ContentDialog sender, ContentDialogClosingEventArgs args) {
			//SingleTagSuggestion selected = GetSelectedItem();
			//if(selected != null) {
			//	selected.IsSelected = false;
			//} else {
			//	Hide();
			//	args.Cancel = false;
			//}
			args.Cancel = !ableToHide;
		}

		private void EscapeKey_Invoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args) {
			SingleTagSuggestion selected = GetSelectedItem();
			if(selected != null) {
				selected.IsSelected = false;
				ableToHide = false;
				args.Handled = true;
			} else {
				ableToHide = true;
				Result = ResultType.None;
				Hide();
				args.Handled = true;
			}
		}

		private bool itemClick = false;
		private void AutoCompletesListView_ItemClick(object sender, ItemClickEventArgs e) {
			var item = (SingleTagSuggestion)e.ClickedItem;
			var tag = item.CompleteName;
			//change last
			//var last = GetLast(MySuggestBox.Text);
			int lastSpace = MySuggestBox.Text.LastIndexOf(' ');
			if(lastSpace == -1) {
				MySuggestBox.Text = tag;
			} else {
				//MySuggestBox.Text = MySuggestBox.Text.Trim() + " " + tag;
				string cut = MySuggestBox.Text.Substring(0, lastSpace).Trim();
				MySuggestBox.Text = cut + " " + tag;
			}
			AutoCompletesListView.Items.Clear();
			CalculateCurrentTags();
			itemClick = true;
			FocusTextBox();
			PutSelectionAtTheEnd();
		}

		private string word = null;

		private void MySuggestBox_SelectionChanged(object sender, RoutedEventArgs e) {
			if(this.word == null || MySuggestBox.Text.Length == 0) {
				return;
			}
			if(itemClick || internalChange) {
				itemClick = false;
				internalChange = false;
				return;
			}
			//only if word changes
			string current = GetCurrentWord();
			if(current != this.word) {
				this.word = current;

				if(current.Length <= 2 || current.Contains(":")) {
					AutoCompletesListView.Items.Clear();
					CancelLoading();
				} else {
					SetLoadingbar(false);
					DelayLoad(current);
				}
			}
			CalculateCurrentTags();
		}

		private void MySuggestBox_TextChanged(object sender, TextChangedEventArgs e) {
			if(itemClick || internalChange) {
				itemClick = false;
				internalChange = false;
				return;
			}
			if(MySuggestBox.Text.Length == 0) {
				this.word = null;
			}
			this.word = GetCurrentWord();
			if(word.Length <= 2 || this.word.Contains(":")) {
				AutoCompletesListView.Items.Clear();
				CancelLoading();
			} else {
				SetLoadingbar(false);
				DelayLoad(this.word);
			}

			CalculateCurrentTags();

			if(currentTags.Contains("order:new")) {
				Order = QuickOptionsOrder.New;
			} else if(currentTags.Contains("order:rank")) {
				Order = QuickOptionsOrder.Rank;
			} else if(currentTags.Contains("order:random")) {
				Order = QuickOptionsOrder.Random;
			} else if(currentTags.Contains("order:favorite")) {
				Order = QuickOptionsOrder.Favorite;
			} else if(currentTags.Contains("order:score")) {
				Order = QuickOptionsOrder.Score;
			} else {
				Order = QuickOptionsOrder.Default;
			}

			if(currentTags.Contains("type:jpg")) {
				Type = QuickOptionsType.JPG;
			} else if(currentTags.Contains("type:png")) {
				Type = QuickOptionsType.PNG;
			} else if(currentTags.Contains("type:gif")) {
				Type = QuickOptionsType.GIF;
			} else if(currentTags.Contains("type:webm")) {
				Type = QuickOptionsType.WEBM;
			} else if(currentTags.Contains("type:anim")) {
				Type = QuickOptionsType.ANIM;
			} else {
				Type = QuickOptionsType.All;
			}

			if(currentTags.Contains("rating:safe")) {
				Rating = QuickOptionsRating.Safe;
			} else if(currentTags.Contains("rating:questionable")) {
				Rating = QuickOptionsRating.Questionable;
			} else if(currentTags.Contains("rating:explicit")) {
				Rating = QuickOptionsRating.Explicit;
			} else {
				Rating = QuickOptionsRating.All;
			}

			IsInPoolBox.IsChecked = currentTags.Contains("inpool:true");
			IsDeletedBox.IsChecked = currentTags.Contains("status:deleted");
		}

		private string GetCurrentWord() {
			return GetCurrentWord(out int _);
		}

		private string GetCurrentWord(out int index) {
			index = 0;
			int start = MySuggestBox.SelectionStart;
			string text = MySuggestBox.Text;
			int spaceInStart = 0;
			foreach(var item in text) {
				if(char.IsWhiteSpace(item)) {
					spaceInStart++;
				} else {
					break;
				}
			}

			if((start >= text.Length || text[start] == ' ') && (start > 0 && text[start - 1] == ' ')) {
				return "";
			} else {
				string[] tags = text.Trim().Split(' ').Where(t => !string.IsNullOrWhiteSpace(t)).ToArray();
				string word = "";

				int length = spaceInStart;
				for(int i = 0; i < tags.Length; i++) {
					length += tags[i].Length + 1;
					if(length > start) {
						word = tags[i];
						index = length;
						break;
					}
				}

				return word;
			}

		}

		private CancellationTokenSource delay_cts;
		private async void DelayLoad(string tag) {
			if(delay_cts != null) {
				delay_cts.Cancel();
				delay_cts.Dispose();
			}
			delay_cts = new CancellationTokenSource();
			try {
				await Task.Delay(200, delay_cts.Token);
			} catch(TaskCanceledException) {
				return;
			}
			await LoadAutoSuggestionAsync(tag);
		}

		private void CancelLoading() {
			if(delay_cts != null) {
				delay_cts.Cancel();
				delay_cts.Dispose();
			}
			delay_cts = null;
			SetLoadingbar(false);
		}

		private CancellationTokenSource cts;

		private async Task LoadAutoSuggestionAsync(string tag) {
			try {
				if(cts != null) {
					cts.Cancel();
					cts.Dispose();
				}
			} catch { }
			cts = new CancellationTokenSource();
			SetLoadingbar(true);
			AutoCompletesListView.Items.Clear();
			(E621AutoComplete[] acs, HttpResultType result) = await E621AutoComplete.GetAsync(tag, cts.Token);
			AutoCompletesListView.Items.Clear();
			if(MySuggestBox.Text.Length <= 2) {
				SetLoadingbar(false);
				return;
			}
			foreach(E621AutoComplete item in acs) {
				AutoCompletesListView.Items.Add(new SingleTagSuggestion(item));
			}
			var last = GetItems().LastOrDefault();
			if(last != null) {
				last.Loaded += (s, e) => {
					SetLoadingbar(false);
				};
			}
			//GetItems().FirstOrDefault()?.SetSelected(true);
			currentSelectedIndex = -1;
			//hasSelectionChanged = false;
		}

		private List<SingleTagSuggestion> GetItems() {
			return AutoCompletesListView.Items.Cast<SingleTagSuggestion>().ToList();
		}

		private SingleTagSuggestion GetSelectedItem() {
			return GetItems().Find(i => i.IsSelected);
		}

		private void CalculateCurrentTags() {
			currentTags.Clear();
			foreach(string item in MySuggestBox.Text.Trim().Split(" ").Where(s => !string.IsNullOrEmpty(s)).ToList()) {
				currentTags.Add(item.ToLower());
			}
		}

		private void SetLoadingbar(bool active) {
			LoadingBar.IsIndeterminate = active;
			LoadingBar.Visibility = active ? Visibility.Visible : Visibility.Collapsed;
		}

		public E621Tag GetE621Tag(string tag) {
			return tags_pool.ContainsKey(tag) ? tags_pool[tag] : null;
		}

		public void RegisterE621Tag(string tag, E621Tag e621tag) {
			if(tags_pool.ContainsKey(tag)) {
				tags_pool[tag] = e621tag;
			} else {
				tags_pool.Add(tag, e621tag);
			}
		}


		public void RemoveTag(string tag) {
			MySuggestBox.Text = MySuggestBox.Text.Replace(tag, "").Trim();
		}

		public string[] GetTags() => currentTags.ToArray();

		private void DialogBackButton_Tapped(object sender, TappedRoutedEventArgs e) {
			ableToHide = true;
			Result = ResultType.None;
			Hide();
		}

		private void SearchButton_Tapped(object sender, TappedRoutedEventArgs e) {
			ableToHide = true;
			Result = ResultType.Search;
			Hide();
		}

		//private string GetLast(string value) {
		//	int lastSpace = value.LastIndexOf(' ');
		//	if(lastSpace != -1) {
		//		return value.Substring(lastSpace, value.Length - lastSpace).Trim();
		//	} else {
		//		return value;
		//	}
		//}

		public enum ResultType {
			None, Search, Hot, Random
		}

		public void FocusTextBox() {
			MySuggestBox.Focus(FocusState.Programmatic);
		}

		public void PutSelectionAtTheEnd() {
			MySuggestBox.SelectionStart = MySuggestBox.Text.Length;
		}

		private void Hide() {
			if(cts != null) {
				cts.Cancel();
				cts.Dispose();
			}
			cts = null;
			dialog.Hide();
		}

		private void EnterKey_Invoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args) {
			SingleTagSuggestion selected = GetSelectedItem();
			if(selected != null) {
				//submit text change
				internalChange = true;
				string tag = selected.CompleteName;
				string word = GetCurrentWord(out int index);
				MySuggestBox.Text = MySuggestBox.Text.Replace(word, tag);
				//MySuggestBox.SelectionStart = index;
				PutSelectionAtTheEnd();
				AutoCompletesListView.Items.Clear();
			} else {
				Result = ResultType.Search;
				Hide();
			}
			args.Handled = true;
		}

		private void SelectionUp() {
			List<SingleTagSuggestion> list = GetItems();
			if(--currentSelectedIndex < 0) {
				currentSelectedIndex = list.Count - 1;
			}
			for(int i = 0; i < list.Count; i++) {
				list[i].IsSelected = i == currentSelectedIndex;
			}
			//if(list.Count == 0 || list.Count == 1) {
			//	hasSelectionChanged = false;
			//} else {
			//	hasSelectionChanged = true;
			//}
		}

		private void SelectionDown() {
			List<SingleTagSuggestion> list = GetItems();
			if(++currentSelectedIndex >= list.Count) {
				currentSelectedIndex = 0;
			}
			for(int i = 0; i < list.Count; i++) {
				list[i].IsSelected = i == currentSelectedIndex;
			}
			//if(list.Count == 0 || list.Count == 1) {
			//	hasSelectionChanged = false;
			//} else {
			//	hasSelectionChanged = true;
			//}
		}

		private void UpKey_Invoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args) {
			SelectionUp();
			args.Handled = true;
		}

		private void DownKey_Invoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args) {
			SelectionDown();
			args.Handled = true;
		}

		private void MySuggestBox_Loaded(object sender, RoutedEventArgs e) {
			FocusTextBox();
		}

		private void MySuggestBox_PreviewKeyDown(object sender, KeyRoutedEventArgs e) {
			if(e.Key == VirtualKey.Down) {
				SelectionDown();
				e.Handled = true;
			}
			if(e.Key == VirtualKey.Up) {
				SelectionUp();
				e.Handled = true;
			}
		}

		private void HotItem_Click(object sender, RoutedEventArgs e) {
			ableToHide = true;
			Result = ResultType.Hot;
			Hide();
		}

		private void RandomItem_Click(object sender, RoutedEventArgs e) {
			ableToHide = true;
			Result = ResultType.Random;
			Hide();
		}

		private string FindMeta(string root) {//order:score
			string text = " " + MySuggestBox.Text + " ";
			string textLowwer = text.ToLower();
			int startIndex = textLowwer.IndexOf(root);
			Debug.WriteLine(startIndex);
			if(startIndex == -1) {
				return null;
			}
			int endIndex = -1;
			for(int i = startIndex; i < textLowwer.Length; i++) {
				if(textLowwer[i] == ' ') {
					endIndex = i;
					break;
				}
			}
			if(endIndex == -1) {
				return null;
			}
			return text.Substring(startIndex, endIndex - startIndex);
		}

		private void ReplaceMeta(string root, string target) {
			string found = FindMeta(root);
			if(!string.IsNullOrWhiteSpace(found)) {
				MySuggestBox.Text = MySuggestBox.Text.Replace(found, target).Trim();
			} else {
				if(!string.IsNullOrWhiteSpace(target)) {
					//add it
					MySuggestBox.Text = (MySuggestBox.Text.Trim() + " " + target).Trim();
				}
			}
			FocusTextBox();
			PutSelectionAtTheEnd();
		}

		private void ItemOrderDefault_Click(object sender, RoutedEventArgs e) {
			internalChange = true;
			Order = QuickOptionsOrder.Default;
			ReplaceMeta("order:", "");
		}

		private void ItemOrderNew_Click(object sender, RoutedEventArgs e) {
			internalChange = true;
			Order = QuickOptionsOrder.New;
			ReplaceMeta("order:", "order:new");
		}

		private void ItemOrderScore_Click(object sender, RoutedEventArgs e) {
			internalChange = true;
			Order = QuickOptionsOrder.Score;
			ReplaceMeta("order:", "order:score");
		}

		private void ItemOrderFavorite_Click(object sender, RoutedEventArgs e) {
			internalChange = true;
			Order = QuickOptionsOrder.Favorite;
			ReplaceMeta("order:", "order:favorite");
		}

		private void ItemOrderRank_Click(object sender, RoutedEventArgs e) {
			internalChange = true;
			Order = QuickOptionsOrder.Rank;
			ReplaceMeta("order:", "order:rank");
		}

		private void ItemOrderRandom_Click(object sender, RoutedEventArgs e) {
			internalChange = true;
			Order = QuickOptionsOrder.Random;
			ReplaceMeta("order:", "order:random");
		}

		private void ItemTypeAll_Click(object sender, RoutedEventArgs e) {
			internalChange = true;
			Type = QuickOptionsType.All;
			ReplaceMeta("type:", "");
		}

		private void ItemTypeJPG_Click(object sender, RoutedEventArgs e) {
			internalChange = true;
			Type = QuickOptionsType.JPG;
			ReplaceMeta("type:", "type:jpg");
		}

		private void ItemTypePNG_Click(object sender, RoutedEventArgs e) {
			internalChange = true;
			Type = QuickOptionsType.PNG;
			ReplaceMeta("type:", "type:png");
		}

		private void ItemTypeGIF_Click(object sender, RoutedEventArgs e) {
			internalChange = true;
			Type = QuickOptionsType.GIF;
			ReplaceMeta("type:", "type:gif");
		}

		private void ItemTypeWEBM_Click(object sender, RoutedEventArgs e) {
			internalChange = true;
			Type = QuickOptionsType.WEBM;
			ReplaceMeta("type:", "type:webm");
		}

		private void ItemTypeANIM_Click(object sender, RoutedEventArgs e) {
			internalChange = true;
			Type = QuickOptionsType.ANIM;
			ReplaceMeta("type:", "type:anim");
		}

		private void ItemRatingAll_Click(object sender, RoutedEventArgs e) {
			internalChange = true;
			Rating = QuickOptionsRating.All;
			ReplaceMeta("rating:", "");
		}

		private void ItemRatingSafe_Click(object sender, RoutedEventArgs e) {
			internalChange = true;
			Rating = QuickOptionsRating.Safe;
			ReplaceMeta("rating:", "rating:safe");
		}

		private void ItemRatingQuestionable_Click(object sender, RoutedEventArgs e) {
			internalChange = true;
			Rating = QuickOptionsRating.Questionable;
			ReplaceMeta("rating:", "rating:questionable");
		}

		private void ItemRatingExplicit_Click(object sender, RoutedEventArgs e) {
			internalChange = true;
			Rating = QuickOptionsRating.Explicit;
			ReplaceMeta("rating:", "rating:explicit");
		}

		private void IsInPoolBox_Click(object sender, RoutedEventArgs e) {
			internalChange = true;
			IsInPool = !IsInPool;
			if(isInPool) {
				ReplaceMeta("inpool:", "inpool:true");
			} else {
				ReplaceMeta("inpool:", "");
			}
			MoreFlyout.Hide();
		}

		private void IsDeletedBOx_Click(object sender, RoutedEventArgs e) {
			internalChange = true;
			isDeleted = !isDeleted;
			if(isDeleted) {
				ReplaceMeta("status:", "status:deleted");
			} else {
				ReplaceMeta("status:", "");
			}
			MoreFlyout.Hide();
		}

	}


	public enum QuickOptionsOrder {
		Default, New, Score, Favorite, Rank, Random
	}

	public enum QuickOptionsType {
		All, PNG, JPG, GIF, WEBM, ANIM
	}

	public enum QuickOptionsRating {
		All, Safe, Questionable, Explicit
	}
}
