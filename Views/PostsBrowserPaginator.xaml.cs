using E621Downloader.Models;
using E621Downloader.Models.Networks;
using E621Downloader.Models.Posts;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Windows.System;
using Windows.UI;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

namespace E621Downloader.Views {
	public sealed partial class PostsBrowserPaginator: UserControl {
		public Action<int> OnPageNavigate { get; set; }
		public Action OnRefresh { get; set; }

		private int maxPage;
		private int currentPage;
		private LoadingStatus isloading = LoadingStatus.None;

		public LoadingStatus IsLoading {
			get => isloading;
			set {
				isloading = value;
				RefreshButtonWithAnimation.From = RefreshButton.Width;
				RefreshButtonOpacityAnimation.From = RefreshButton.Opacity;
				switch(isloading) {
					case LoadingStatus.Loading:
						PaginatorPanel.Visibility = Visibility.Collapsed;
						LoadingGrid.Visibility = Visibility.Visible;
						LoadingBar.ShowError = false;
						RefreshButtonWithAnimation.To = 0;
						RefreshButtonOpacityAnimation.To = 0;
						break;
					case LoadingStatus.Result:
						PaginatorPanel.Visibility = Visibility.Visible;
						LoadingGrid.Visibility = Visibility.Collapsed;
						LoadingBar.ShowError = false;
						RefreshButtonWithAnimation.To = 0;
						RefreshButtonOpacityAnimation.To = 0;
						break;
					case LoadingStatus.Error:
						PaginatorPanel.Visibility = Visibility.Collapsed;
						LoadingGrid.Visibility = Visibility.Visible;
						LoadingBar.ShowError = true;
						RefreshButtonWithAnimation.To = 44;
						RefreshButtonOpacityAnimation.To = 1;
						break;
					case LoadingStatus.None:
					default:
						PaginatorPanel.Visibility = Visibility.Collapsed;
						LoadingGrid.Visibility = Visibility.Collapsed;
						LoadingBar.ShowError = false;
						RefreshButtonWithAnimation.To = 0;
						RefreshButtonOpacityAnimation.To = 0;
						break;
				}
				LoadingTransitionStoryboard.Begin();
			}
		}

		public int MaxPage {
			get => maxPage;
			set {
				maxPage = value;
			}
		}
		public int CurrentPage {
			get => currentPage;
			set {
				currentPage = value;
			}
		}

		public PostsBrowserPaginator() {
			this.InitializeComponent();
			Reset();
		}

		public void Reset() {
			IsLoading = LoadingStatus.None;
		}

		public async Task LoadPaginator(string[] tags, CancellationToken? token = null) {
			IsLoading = LoadingStatus.Loading;
			DataResult<E621Paginator> result = await E621Paginator.GetAsync(tags, CurrentPage, token);
			switch(result.ResultType) {
				case HttpResultType.Success:
					CurrentPage = result.Data.currentPage;
					MaxPage = result.Data.GetMaxPage();
					UpdatePaginator(CurrentPage, MaxPage);
					IsLoading = LoadingStatus.Result;
					break;
				case HttpResultType.Error:
					IsLoading = LoadingStatus.Error;
					break;
				case HttpResultType.Canceled:
					IsLoading = LoadingStatus.None;
					break;
				default:
					IsLoading = LoadingStatus.None;
					break;
			}
		}

		public void LoadPaginator(int currentPage, int maxPage) {
			CurrentPage = currentPage;
			MaxPage = maxPage;
			UpdatePaginator(CurrentPage, MaxPage);
			IsLoading = LoadingStatus.Result;
		}

		public void SetLoadCountText(int loaded, int max) {
			LoadCountText.Text = $"{loaded}/{max}";
		}

		private void UpdatePaginator(int currentPage, int maxPage) {
			PageButtons.Visibility = Visibility.Visible;
			PageButtons.Children.Clear();

			FrameworkElement CreateButton(object content, Action<Button> action = null) {
				FrameworkElement result;
				if(action != null) {
					result = new Button() {
						Content = content.ToString(),
						Background = new SolidColorBrush(Colors.Transparent),
						BorderThickness = new Thickness(0),
						Height = 40,
						Width = 40,
					};
					result.Tapped += (s, e) => {
						action.Invoke(s as Button);
					};
				} else {
					result = new TextBlock() {
						Text = content.ToString(),
						Height = 40,
						Width = 40,
						TextAlignment = TextAlignment.Center,
						Margin = new Thickness(0, 0, 0, -20),
					};
				}
				if(content is int page && page == currentPage) {
					if(result is Button button) {
						button.FontWeight = FontWeights.ExtraBold;
						button.FontSize += 2;
						button.Margin = new Thickness(0, 0, 0, -15);
					} else if(result is TextBlock tb) {
						tb.FontWeight = FontWeights.ExtraBold;
						tb.FontSize += 2;
						tb.Margin = new Thickness(0, 0, 0, -15);
					}
				}
				return result;
			}
			void NavigatePage(int i) {
				int page;
				if(i == -1) {
					page = Math.Clamp(currentPage - 1, 1, maxPage);
				} else if(i == -2) {
					page = Math.Clamp(currentPage + 1, 1, maxPage);
				} else {
					page = i;
				}
				Debug.WriteLine(page);
				OnPageNavigate?.Invoke(page);
			}

			void P1(Button b) => NavigatePage(-1);
			void P2(Button b) => NavigatePage(-2);

			if(maxPage == 0) {
				PageButtons.Children.Add(CreateButton("<"));
				PageButtons.Children.Add(CreateButton("1"));
				PageButtons.Children.Add(CreateButton(">"));
				PageInputText.Visibility = Visibility.Collapsed;
				ForwardButton.Visibility = Visibility.Collapsed;
				return;
			}
			PageInputText.Visibility = Visibility.Visible;
			ForwardButton.Visibility = Visibility.Visible;

			List<FrameworkElement> btns = new() {
				CreateButton('<', currentPage > 1 ? P1 : null)
			};

			if(currentPage > 6) {
				btns.Add(CreateButton('1', b => NavigatePage(1)));
				btns.Add(CreateButton("..."));
				btns.Add(CreateButton(currentPage - 4, b => NavigatePage(currentPage - 4)));
				btns.Add(CreateButton(currentPage - 3, b => NavigatePage(currentPage - 3)));
				btns.Add(CreateButton(currentPage - 2, b => NavigatePage(currentPage - 2)));
				btns.Add(CreateButton(currentPage - 1, b => NavigatePage(currentPage - 1)));
			} else {
				for(int i = 1; i < currentPage; i++) {
					int index = i;//it works! don't change it.
					btns.Add(CreateButton(i, b => NavigatePage(index)));
				}
			}

			btns.Add(CreateButton(currentPage));

			if(maxPage - currentPage > 6) {
				btns.Add(CreateButton(currentPage + 1, b => NavigatePage(currentPage + 1)));
				btns.Add(CreateButton(currentPage + 2, b => NavigatePage(currentPage + 2)));
				btns.Add(CreateButton(currentPage + 3, b => NavigatePage(currentPage + 3)));
				btns.Add(CreateButton(currentPage + 4, b => NavigatePage(currentPage + 4)));
				btns.Add(CreateButton("..."));
			} else {
				for(int i = currentPage + 1; i < maxPage; i++) {
					int index = i;
					btns.Add(CreateButton(i, b => NavigatePage(index)));
				}
			}
			if(currentPage != maxPage) {
				btns.Add(CreateButton(maxPage, b => NavigatePage(maxPage)));
			}
			btns.Add(CreateButton('>', currentPage < maxPage ? P2 : null));

			foreach(FrameworkElement item in btns) {
				PageButtons.Children.Add(item);
			}

		}

		private async void ForwardButton_Click(object sender, RoutedEventArgs e) {
			await Forward(PageInputText.Text);
			//PageInputText.Text = "";
		}

		private void RefreshButton_Tapped(object sender, TappedRoutedEventArgs e) {
			OnRefresh?.Invoke();
		}

		private async void PageInputText_KeyDown(object sender, KeyRoutedEventArgs e) {
			if(e.Key == MainPage.SEARCH_KEY) {
				MainPage.Instance.DelayInputKeyListener();
			}
			if(e.Key == VirtualKey.Enter) {
				await Forward(PageInputText.Text);
				PageInputText.Text = "";
			} else if(e.Key == VirtualKey.Escape) {
				PageInputText.Text = "";
			}
		}

		private async Task Forward(string text) {
			if(int.TryParse(text, out int page)) {
				if(page < 1 || page > maxPage) {
					await MainPage.CreatePopupDialog("Error".Language(), "({{0}}) can only be in 1-{{1}}".Language(page, maxPage));
				} else {
					OnPageNavigate?.Invoke(page);
				}
			} else {
				await MainPage.CreatePopupDialog("Error".Language(), "({{0}}) is not a valid number".Language(text));
			}
		}
	}

	public enum LoadingStatus {
		None, Loading, Result, Error,
	}
}
