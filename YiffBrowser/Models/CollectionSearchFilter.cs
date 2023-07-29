using Microsoft.Toolkit.Uwp.UI;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;
using YiffBrowser.Helpers;

namespace YiffBrowser.Models {
	public class CollectionSearchFilter<T> : BindableBase {
		private string searchKey = string.Empty;

		public AdvancedCollectionView CollectionSource { get; }

		private Func<T, string> GetNameFunc { get; set; }

		public CollectionSearchFilter(ObservableCollection<T> collection, Func<T, string> predicate) {
			GetNameFunc = predicate;
			CollectionSource = new AdvancedCollectionView(collection, true) {
				Source = collection,
				Filter = x => {
					if (GetNameFunc != null) {
						string name = GetNameFunc?.Invoke((T)x);
						return name.ToLower().Trim().Contains(SearchKey.ToLower().Trim());
					} else {
						return true;
					}
				}
			};

		}

		public void UpdateSort(params SortDescription[] sorts) {
			CollectionSource.SortDescriptions.Clear();
			foreach (SortDescription sort in sorts) {
				CollectionSource.SortDescriptions.Add(sort);
			}
			RefreshSort();
		}

		public void RefreshSort() {
			CollectionSource.RefreshSorting();
		}

		public ICollectionView View => CollectionSource;

		public string SearchKey {
			get => searchKey;
			set => SetProperty(ref searchKey, value ?? string.Empty, OnSearchKeyChanged);
		}

		private void OnSearchKeyChanged() {
			CollectionSource.Refresh();
		}

		public void Search(string key) {
			SearchKey = key;
		}

		public void Refresh() {
			CollectionSource.Refresh();
		}
	}
}
