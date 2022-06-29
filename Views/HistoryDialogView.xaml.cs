using E621Downloader.Models;
using E621Downloader.Models.Locals;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Windows.ApplicationModel.DataTransfer;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace E621Downloader.Views {
	public sealed partial class HistoryDialogView: UserControl {

		private readonly ContentDialog dialog;

		private CancellationTokenSource cts;

		private readonly List<RecentViewView> views = new();

		private bool loaded = false;

		public HistoryDialogView(ContentDialog dialog) {
			this.InitializeComponent();
			this.dialog = dialog;

			Load();
		}

		private void Load() {
			views.Clear();
			Dictionary<RecentType, StackPanel> panels_tags = new() {
				{ RecentType.Today, new StackPanel() },
				{ RecentType.Yesterday, new StackPanel() },
				{ RecentType.ThisMonth, new StackPanel() },
				{ RecentType.LastMonth, new StackPanel() },
				{ RecentType.Earlier, new StackPanel() }
			};

			foreach(HistoryItem item in Local.History.Tags) {
				RecentType type = CalculateDateTimeType(item.Time);
				if(!panels_tags.ContainsKey(type)) {
					throw new Exception($"Recent Type ({type}) Not Found");
				}
				var view = new RecentTagView() {
					Value = item.Value,
					DateTime = item.Time.ToString(),
					Margin = new Thickness(5),
				};
				view.OnDelete = v => {
					Local.History.Tags.RemoveAll(i => i.Value == v);
					panels_tags[type].Children.Remove(view);
				};
				view.OnClick = v => {
					dialog.Hide();
					MainPage.NavigateToPostsBrowser(1, v.Split(','));
					Local.History.AddTag(v.Split(','));
				};
				panels_tags[type].Children.Add(view);
			}

			SearchHistoryPanel.Children.Clear();
			int index = 0;
			foreach(KeyValuePair<RecentType, StackPanel> item in panels_tags) {
				if(item.Value.Children.Count == 0) {
					continue;
				}
				Expander expander = new() {
					Header = GetRecentTypeString(item.Key),
					Content = item.Value,
					IsExpanded = index == 0,
					FontSize = 18,
					HorizontalAlignment = HorizontalAlignment.Stretch,
					HorizontalContentAlignment = HorizontalAlignment.Stretch,
					Padding = new Thickness(5),
				};
				SearchHistoryPanel.Children.Add(expander);
				index++;
			}

			Dictionary<RecentType, StackPanel> panels_views = new() {
				{ RecentType.Today, new StackPanel() },
				{ RecentType.Yesterday, new StackPanel() },
				{ RecentType.ThisMonth, new StackPanel() },
				{ RecentType.LastMonth, new StackPanel() },
				{ RecentType.Earlier, new StackPanel() }
			};

			foreach(HistoryItem item in Local.History.PostIDs) {
				RecentType type = CalculateDateTimeType(item.Time);
				if(!panels_tags.ContainsKey(type)) {
					throw new Exception($"Recent Type ({type}) Not Found");
				}
				var view = new RecentViewView(item.Value) {
					DateTime = item.Time.ToString(),
					Margin = new Thickness(5),
				};
				view.OnDelete = v => {
					Local.History.PostIDs.RemoveAll(i => i.Value == v);
					panels_views[type].Children.Remove(view);
				};
				view.OnClick = v => {
					dialog.Hide();
					App.PostsList.UpdatePostsList(Local.History.PostIDs.Select(p => p.Value).Cast<object>().ToList());
					App.PostsList.Current = v;
					MainPage.NavigateToPicturePage(v, Array.Empty<string>());
				};
				views.Add(view);
				panels_views[type].Children.Add(view);
			}

			ViewHistoryPanel.Children.Clear();
			index = 0;
			foreach(KeyValuePair<RecentType, StackPanel> item in panels_views) {
				if(item.Value.Children.Count == 0) {
					continue;
				}
				Expander expander = new() {
					Header = GetRecentTypeString(item.Key),
					Content = item.Value,
					IsExpanded = index == 0,
					FontSize = 18,
					HorizontalAlignment = HorizontalAlignment.Stretch,
					HorizontalContentAlignment = HorizontalAlignment.Stretch,
					Padding = new Thickness(5),
				};
				ViewHistoryPanel.Children.Add(expander);
				index++;
			}

			UpdateVisibilities();
		}

		private async void StartLoading() {
			CancelLoading();
			cts = new CancellationTokenSource();
			for(int i = 0; i < views.Count; i++) {
				RecentViewView item = views[i];
				if(cts == null) {
					break;
				}
				await item.StartLoading(cts.Token);
			}
		}

		private void UpdateVisibilities() {
			if(SearchHistoryPanel.Children.Count > 0) {
				SearchHistoryPanel.Visibility = Visibility.Visible;
				SearchHistoryEmptyPanel.Visibility = Visibility.Collapsed;
			} else {
				SearchHistoryPanel.Visibility = Visibility.Collapsed;
				SearchHistoryEmptyPanel.Visibility = Visibility.Visible;
			}

			if(ViewHistoryPanel.Children.Count > 0) {
				ViewHistoryPanel.Visibility = Visibility.Visible;
				ViewHistoryEmptyPanel.Visibility = Visibility.Collapsed;
			} else {
				ViewHistoryPanel.Visibility = Visibility.Collapsed;
				ViewHistoryEmptyPanel.Visibility = Visibility.Visible;
			}

		}

		private void MenuFlyoutItem_Click(object sender, RoutedEventArgs e) {
			if(sender is MenuFlyoutItem item && item.Tag is string tag) {
				var dataPackage = new DataPackage() {
					RequestedOperation = DataPackageOperation.Copy
				};
				dataPackage.SetText(tag);
				Clipboard.SetContent(dataPackage);
			}
		}


		public void CancelLoading() {
			if(cts != null) {
				cts.Cancel();
				cts.Dispose();
				cts = null;
			}
		}

		//private void MenuFlyoutItem_Click_Delete_1(object sender, RoutedEventArgs e) {
		//	if(sender is MenuFlyoutItem item && item.Tag is string tag) {
		//		Local.History.RemoveTag(tag);
		//		HistoryItem found = tags.Where(t => t.Value == tag).FirstOrDefault();
		//		if(found != null) {
		//			tags.Remove(found);
		//		}
		//	}
		//	UpdateVisibilities();
		//}

		//private void MenuFlyoutItem_Click_Delete_2(object sender, RoutedEventArgs e) {
		//	if(sender is MenuFlyoutItem item && item.Tag is string tag) {
		//		Local.History.RemovePostID(tag);
		//		HistoryItem found = postIDs.Where(t => t.Value == tag).FirstOrDefault();
		//		if(found != null) {
		//			postIDs.Remove(found);
		//		}
		//	}
		//	UpdateVisibilities();
		//}

		private void ViewHistoryListView_ItemClick(object sender, ItemClickEventArgs e) {
			if(e.ClickedItem is HistoryItem item) {
				dialog.Hide();
				//item.Value;
				//MainPage.NavigateToPicturePage();
			}
		}

		public static RecentType CalculateDateTimeType(DateTime dateTime) {
			DateTime now = DateTime.Now;
			TimeSpan span = now.Subtract(dateTime);
			return span.TotalDays switch {
				< 1 => RecentType.Today,
				< 2 => RecentType.Yesterday,
				< 30 => RecentType.ThisMonth,
				< 60 => RecentType.LastMonth,
				_ => RecentType.Earlier
			};
		}

		private string GetRecentTypeString(RecentType recentType) {
			return recentType switch {
				RecentType.Today => "Today".Language(),
				RecentType.Yesterday => "Yesterday".Language(),
				RecentType.ThisMonth => "This Month".Language(),
				RecentType.LastMonth => "Last Month".Language(),
				RecentType.Earlier => "Earlier".Language(),
				_ => "None".Language(),
			};
		}

		private void Pivot_SelectionChanged(object sender, SelectionChangedEventArgs e) {
			if(e.AddedItems?.FirstOrDefault() is not PivotItem item) {
				return;
			}
			if(item == SearchItem) {

			} else if(item == ViewItem) {
				if(loaded) {
					return;
				}
				loaded = true;
				StartLoading();
			}
		}

		private async void ClearAll_Click(object sender, RoutedEventArgs e) {
			await Local.History.RemoveBeforeDate(RecentType.Today);
			Load();
		}

		private async void ClearBeforeThisMonth_Click(object sender, RoutedEventArgs e) {
			await Local.History.RemoveBeforeDate(RecentType.LastMonth);
			Load();
		}

		private async void ClearBeforeToday_Click(object sender, RoutedEventArgs e) {
			await Local.History.RemoveBeforeDate(RecentType.Yesterday);
			Load();
		}
	}

	public enum RecentType {
		Today, Yesterday, ThisMonth, LastMonth, Earlier
	}
}
