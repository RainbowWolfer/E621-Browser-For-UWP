using Microsoft.Toolkit.Uwp.UI;
using Prism.Mvvm;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Windows.UI.Xaml.Data;
using YiffBrowser.Helpers;

namespace YiffBrowser.Models {
	public class CollectionSearchFilter<T> : BindableBase, IDisposable {
		private string searchKey = string.Empty;

		public AdvancedCollectionView CollectionSource { get; }

		private Func<T, string> GetNameFunc { get; set; }

		private readonly ObservableCollection<T> collection;
		public CollectionSearchFilter(ObservableCollection<T> collection, Func<T, string> predicate) {
			this.collection = collection ?? throw new ArgumentNullException(nameof(collection));
			GetNameFunc = predicate ?? throw new ArgumentNullException(nameof(predicate));
			CollectionSource = new AdvancedCollectionView(collection, true) {
				Source = collection,
				Filter = x => {
					if (GetNameFunc != null) {
						string name = GetNameFunc.Invoke((T)x);
						return name.SearchFor(SearchKey);
					} else {
						return true;
					}
				}
			};
			collection.CollectionChanged += Collection_CollectionChanged;
		}

		private void Collection_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
			RaiseChanges();
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

		public int AfterFilterCount => View.Count;
		public bool AfterFilterIsEmpty => View.Count == 0;
		public bool DifferentFromAfterFilter => View.Count != collection.Count;

		public string SearchKey {
			get => searchKey;
			set => SetProperty(ref searchKey, value ?? string.Empty, OnSearchKeyChanged);
		}

		private void OnSearchKeyChanged() {
			RefreshFilter();
		}

		public void RefreshFilter() {
			CollectionSource.Refresh();
			RaiseChanges();
		}

		private void RaiseChanges() {
			RaisePropertyChanged(nameof(AfterFilterCount));
			RaisePropertyChanged(nameof(AfterFilterIsEmpty));
			RaisePropertyChanged(nameof(DifferentFromAfterFilter));
		}

		public void Search(string key) {
			SearchKey = key.Trim();
		}

		public void Refresh() {
			CollectionSource.Refresh();
		}

		public void Dispose() {
			collection.CollectionChanged -= Collection_CollectionChanged;
		}
	}
}
