using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using YiffBrowser.Helpers;
using YiffBrowser.Models.E621;
using YiffBrowser.Services.Networks;

namespace YiffBrowser.Views.Controls.SearchViews {
	public sealed partial class SearchView : UserControl {
		public SearchView(ContentDialog dialog, string searchText = "") {
			this.InitializeComponent();
			ViewModel.Dialog = dialog;
			ViewModel.RequestSearchBoxFocus += () => SearchTextBox.Focus(FocusState.Programmatic);

			if (searchText.IsNotBlank()) {
				ViewModel.SearchText = searchText + " ";
				ViewModel.SearchTextSelectionStart = searchText.Length + 1;
			}
		}

		public bool IsConfirmDialog() => ViewModel.ConfirmDialog;

		public string GetSearchText() => ViewModel.SearchText.Trim().ToLower();

		public string[] GetSearchTags() => GetSearchText().Split(" ").Where(s => s.IsNotBlank()).ToArray();

		private void EnterKey_Invoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args) {
			ViewModel.EnterKey(args);
		}

		private void UpKey_Invoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args) {
			ViewModel.UpKey(args);
		}

		private void DownKey_Invoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args) {
			ViewModel.DownKey(args);
		}
	}

	public class SearchViewModel : BindableBase {
		public event Action RequestSearchBoxFocus;

		public ContentDialog Dialog { get; set; }

		private string searchText = "";

		private string alternativeHintText;
		private int searchTextSelectionStart;

		private bool isLoading;
		private bool isRandomTagLoading;

		public ObservableCollection<SearchTagItem> AutoCompletes { get; } = new();
		private int CurrentSelectedIndex { get; set; } = -1;

		public bool ConfirmDialog { get; private set; } = false;

		public string SearchText {
			get => searchText;
			set => SetProperty(ref searchText, value, OnSearchTextChanged);
		}

		public int SearchTextSelectionStart {
			get => searchTextSelectionStart;
			set => SetProperty(ref searchTextSelectionStart, value);
		}

		public string AlternativeHintText {
			get => alternativeHintText;
			set => SetProperty(ref alternativeHintText, value);
		}

		public bool IsLoading {
			get => isLoading;
			set => SetProperty(ref isLoading, value);
		}

		public bool IsRandomTagLoading {
			get => isRandomTagLoading;
			set => SetProperty(ref isRandomTagLoading, value);
		}

		private bool InternalChange { get; set; }

		public string Word { get; set; }
		public string[] CurrentTags { get; set; }

		public ICommand OnSearchTextBoxLoadedCommand => new DelegateCommand(OnSearchTextBoxLoaded);
		public ICommand OnSearchTextSelectionChangedCommand => new DelegateCommand<RoutedEventArgs>(OnSearchTextSelectionChanged);

		private void OnSearchTextBoxLoaded() => FocusSearchBox();

		private void OnSearchTextSelectionChanged(RoutedEventArgs o) {
			if (Word == null || SearchText.Length == 0) {
				return;
			}

			if (InternalChange) {
				InternalChange = false;
				return;
			}

			//only if word changes
			string current = GetCurrentWord();
			if (current != Word) {
				Word = current;

				if (current.Length <= 2 || current.Contains(':')) {
					AutoCompletes.Clear();
					CancelLoading();
				} else {
					LoadAutoSuggestionAsync(Word);
				}
			}

			CalculateCurrentTags();
		}

		private CancellationTokenSource cts = null;

		private void OnSearchTextChanged() {
			if (InternalChange) {
				InternalChange = false;
				return;
			}

			if (SearchText.IsBlank()) {
				Word = null;
			}

			PostSearch postSearchResult = GetPostSearchResult(SearchText, out string resultPostID);

			if (postSearchResult != PostSearch.None && resultPostID.IsNotBlank()) {
				if (postSearchResult == PostSearch.PostID) {
					AlternativeHintText = "Post ID Detected";
				} else if (postSearchResult == PostSearch.URL) {
					AlternativeHintText = "URL Detected";
				}

			} else {
				AlternativeHintText = string.Empty;
				Word = GetCurrentWord();

				if (Word.Length <= 2 || Word.Contains(':')) {
					AutoCompletes.Clear();
					CancelLoading();
				} else {
					LoadAutoSuggestionAsync(Word);
				}

				CalculateCurrentTags();
			}

			UpdateMetaViews();
		}

		private void CancelLoading() {
			cts?.Cancel();
			cts = null;
			IsLoading = false;
		}

		private async void LoadAutoSuggestionAsync(string tag) {
			cts?.Cancel();
			CancellationTokenSource _cts = new();
			cts = _cts;

			try {
				await Task.Delay(500, _cts.Token);
			} catch (TaskCanceledException) {
				IsLoading = false;
				return;
			}

			IsLoading = true;

			AutoCompletes.Clear();

			E621AutoComplete[] completes = await E621API.GetE621AutoCompleteAsync(tag, _cts.Token);

			if (_cts.IsCancellationRequested) {
				IsLoading = false;
				return;
			}

			foreach (E621AutoComplete item in completes) {
				AutoCompletes.Add(new SearchTagItem(item));
			}

			CurrentSelectedIndex = -1;

			IsLoading = false;
		}

		private void CalculateCurrentTags() {
			List<string> tags = new();
			foreach (string item in SearchText.Trim().Split(" ").Where(s => s.IsNotBlank()).ToList()) {
				tags.Add(item.ToLower());
			}
			CurrentTags = tags.ToArray();
		}

		private string GetCurrentWord() {
			return GetCurrentWord(out int _);
		}

		private string GetCurrentWord(out int index) {
			index = 0;
			int start = SearchTextSelectionStart;
			string text = SearchText;
			int spaceInStart = 0;
			foreach (var item in text) {
				if (char.IsWhiteSpace(item)) {
					spaceInStart++;
				} else {
					break;
				}
			}

			if (start > text.Length || (start >= text.Length || text[start] == ' ') && (start > 0 && text[start - 1] == ' ')) {
				return "";
			} else {
				string[] tags = text.Trim().Split(' ').Where(t => t.IsNotBlank()).ToArray();
				string word = "";

				int length = spaceInStart;
				for (int i = 0; i < tags.Length; i++) {
					length += tags[i].Length + 1;
					if (length > start) {
						word = tags[i];
						index = length;
						break;
					}
				}

				return word;
			}

		}

		private static PostSearch GetPostSearchResult(string text, out string resultPostID) {
			resultPostID = null;
			string[] split = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
			foreach (string item in split) {
				if (item.Length >= 2 && item.OnlyContainDigits()) {
					resultPostID = item;
					return PostSearch.PostID;
				} else if (item.StartsWith("https://e621.net/posts/") ||
					item.StartsWith("https://e926.net/posts/") ||
					item.StartsWith("e621.net/posts/") ||
					item.StartsWith("926.net/posts/")
				) {
					string endPostID = "";
					int startIndex = item.LastIndexOf('/');
					int endIndex = item.LastIndexOf('?');
					if (endIndex == -1) {
						endIndex = item.Length;
					}
					for (int i = startIndex + 1; i < endIndex; i++) {
						endPostID += item[i];
					}
					if (endPostID.Length >= 2 && endPostID.OnlyContainDigits()) {
						resultPostID = endPostID;
						return PostSearch.URL;
					}
				}
			}
			return PostSearch.None;
		}

		private SearchTagItem GetSelectedItem() => AutoCompletes.FirstOrDefault(x => x.IsSelected);

		public ICommand HotOptionCommand => new DelegateCommand(HotOption);
		public ICommand RandomOptionCommand => new DelegateCommand(RandomOption);

		private void HotOption() {
			SearchText = "order:rank";
			Confirm();
		}

		private async void RandomOption() {
			if (IsRandomTagLoading) {
				return;
			}
			IsRandomTagLoading = true;
			E621Post[] posts = await E621API.GetPostsByTagsAsync(new E621PostParameters() {
				Page = 1,
				Tags = new string[] { "limit:1", "order:random" },
				UsePageLimit = false,
			});
			if (posts.IsNotEmpty()) {
				E621Post post = posts.First();
				List<string> all = post.Tags.GetAllTags();
				string tag = all[new Random().Next(all.Count)];
				SearchText = (SearchText + " " + tag).Trim();
			}
			FocusSearchBox();
			PutSelectionAtTheEnd();
			IsRandomTagLoading = false;
		}


		public ICommand ItemClickCommand => new DelegateCommand<ItemClickEventArgs>(ItemClick);

		public ICommand BackCommand => new DelegateCommand(Back);
		public ICommand ConfirmCommand => new DelegateCommand(Confirm);

		public void EnterKey(KeyboardAcceleratorInvokedEventArgs args) {
			SearchTagItem selected = GetSelectedItem();
			if (selected != null) {
				InternalChange = true;

				string tag = selected.AutoComplete.name;
				string word = GetCurrentWord();
				SearchText = SearchText.Replace(word, tag);

				AutoCompletes.Clear();
				PutSelectionAtTheEnd();
			} else {
				Confirm();
			}
			args.Handled = true;
		}

		public void UpKey(KeyboardAcceleratorInvokedEventArgs args) {
			SelectionUp();
			args.Handled = true;
		}

		public void DownKey(KeyboardAcceleratorInvokedEventArgs args) {
			SelectionDown();
			args.Handled = true;
		}

		private void SelectionUp() {
			if (--CurrentSelectedIndex < 0) {
				CurrentSelectedIndex = AutoCompletes.Count - 1;
			}
			for (int i = 0; i < AutoCompletes.Count; i++) {
				AutoCompletes[i].IsSelected = i == CurrentSelectedIndex;
			}
		}

		private void SelectionDown() {
			if (++CurrentSelectedIndex >= AutoCompletes.Count) {
				CurrentSelectedIndex = 0;
			}
			for (int i = 0; i < AutoCompletes.Count; i++) {
				AutoCompletes[i].IsSelected = i == CurrentSelectedIndex;
			}
		}

		private void ItemClick(ItemClickEventArgs args) {
			SearchTagItem clickedItem = (SearchTagItem)args.ClickedItem;
			E621AutoComplete item = clickedItem.AutoComplete;
			string tag = item.name;

			int lastSpace = SearchText.LastIndexOf(' ');
			if (lastSpace == -1) {
				SearchText = tag;
			} else {
				string cut = SearchText.Substring(0, lastSpace).Trim();
				SearchText = cut + " " + tag;
			}

			AutoCompletes.Clear();
			CalculateCurrentTags();

			InternalChange = true;

			FocusSearchBox();
			PutSelectionAtTheEnd();
		}

		private void FocusSearchBox() {
			RequestSearchBoxFocus?.Invoke();
		}

		private void PutSelectionAtTheEnd() {
			SearchTextSelectionStart = SearchText.Length;
		}

		private void Back() {
			CancelLoading();
			Dialog.Hide();
		}

		private void Confirm() {
			CancelLoading();
			ConfirmDialog = true;
			Dialog.Hide();
		}

		#region Meta

		private string orderDropDownText = "Order";
		private string typeDropDownText = "Type";
		private string ratingDropDownText = "Rating";
		private bool inPoolCheck;
		private bool deletedCheck;

		public string OrderDropDownText {
			get => orderDropDownText;
			set => SetProperty(ref orderDropDownText, value);
		}

		public string TypeDropDownText {
			get => typeDropDownText;
			set => SetProperty(ref typeDropDownText, value);
		}

		public string RatingDropDownText {
			get => ratingDropDownText;
			set => SetProperty(ref ratingDropDownText, value);
		}

		public bool InPoolCheck {
			get => inPoolCheck;
			set => SetProperty(ref inPoolCheck, value, OnInPoolCheckChanged);
		}

		public bool DeletedCheck {
			get => deletedCheck;
			set => SetProperty(ref deletedCheck, value, OnDeletedCheckChanged);
		}

		private void OnInPoolCheckChanged() {
			if (InPoolCheck) {
				ReplaceMeta("inpool:", "inpool:true");
			} else {
				ReplaceMeta("inpool:", "");
			}
		}

		private void OnDeletedCheckChanged() {
			if (DeletedCheck) {
				ReplaceMeta("status:", "status:deleted");
			} else {
				ReplaceMeta("status:", "");
			}
		}

		public ICommand OrderDropDownCommand => new DelegateCommand<string>(OrderDropDown);
		public ICommand TypeDropDownCommand => new DelegateCommand<string>(TypeDropDown);
		public ICommand RatingDropDownCommand => new DelegateCommand<string>(RatingDropDown);

		private void OrderDropDown(string meta) {
			ReplaceMeta("order:", meta);
		}

		private void TypeDropDown(string meta) {
			ReplaceMeta("type:", meta);
		}

		private void RatingDropDown(string meta) {
			ReplaceMeta("rating:", meta);
		}

		private string FindMeta(string root) {//order:score
			string text = " " + SearchText + " ";
			string textLower = text.ToLower();
			int startIndex = textLower.IndexOf(root);
			if (startIndex == -1) {
				return null;
			}
			int endIndex = -1;
			for (int i = startIndex; i < textLower.Length; i++) {
				if (textLower[i] == ' ') {
					endIndex = i;
					break;
				}
			}
			if (endIndex == -1) {
				return null;
			}
			return text.Substring(startIndex, endIndex - startIndex);
		}

		private void ReplaceMeta(string root, string target) {
			InternalChange = true;
			string found = FindMeta(root);
			if (found.IsNotBlank()) {
				SearchText = SearchText.Replace(found, target).Trim();
			} else {
				if (target.IsNotBlank()) {
					SearchText = (SearchText.Trim() + " " + target).Trim();
				}
			}
			FocusSearchBox();
			PutSelectionAtTheEnd();
		}

		private void UpdateMetaViews() {
			if (CurrentTags == null) {
				return;
			}

			if (CurrentTags.Contains("order:new")) {
				OrderDropDownText = "New";
			} else if (CurrentTags.Contains("order:rank")) {
				OrderDropDownText = "Rank";
			} else if (CurrentTags.Contains("order:random")) {
				OrderDropDownText = "Random";
			} else if (CurrentTags.Contains("order:favorite")) {
				OrderDropDownText = "Favorite";
			} else if (CurrentTags.Contains("order:score")) {
				OrderDropDownText = "Score";
			} else {
				OrderDropDownText = "Order";
			}

			if (CurrentTags.Contains("type:jpg")) {
				TypeDropDownText = "JPG";
			} else if (CurrentTags.Contains("type:png")) {
				TypeDropDownText = "PNG";
			} else if (CurrentTags.Contains("type:gif")) {
				TypeDropDownText = "GIF";
			} else if (CurrentTags.Contains("type:webm")) {
				TypeDropDownText = "WEBM";
			} else if (CurrentTags.Contains("type:anim")) {
				TypeDropDownText = "ANIM";
			} else {
				TypeDropDownText = "Type";
			}

			if (CurrentTags.Contains("rating:safe")) {
				RatingDropDownText = "Safe";
			} else if (CurrentTags.Contains("rating:questionable")) {
				RatingDropDownText = "Questionable";
			} else if (CurrentTags.Contains("rating:explicit")) {
				RatingDropDownText = "Explicit";
			} else {
				RatingDropDownText = "Rating";
			}
		}

		#endregion

	}

	public class SearchTagItem : BindableBase {
		private E621AutoComplete autoComplete;
		private bool isSelected;

		public E621AutoComplete AutoComplete {
			get => autoComplete;
			set => SetProperty(ref autoComplete, value);
		}

		public bool IsSelected {
			get => isSelected;
			set => SetProperty(ref isSelected, value);
		}

		public SearchTagItem(E621AutoComplete autoComplete) {
			AutoComplete = autoComplete;
			IsSelected = false;
		}
	}

	public enum PostSearch {
		None, URL, PostID
	}
}
