using Prism.Commands;
using Prism.Mvvm;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Windows.Input;
using Windows.Foundation;
using Windows.UI.Xaml.Controls;
using YiffBrowser.Models.E621;

namespace YiffBrowser.Views.Controls.CustomControls {
	internal class Paginator : Control {

		public Paginator() {

		}


	}

	public class PaginatorViewModel : BindableBase {
		public event TypedEventHandler<PaginatorViewModel, int> RequestNavigatePage;

		public E621Paginator Paginator { get; }

		public ObservableCollection<PaginatorItemViewModel> Items { get; } = new();

		public PaginatorViewModel(E621Paginator paginator) {
			Paginator = paginator;
			Items.CollectionChanged += Items_CollectionChanged;
		}

		private DelegateCommand<int?> NavigateCommand => new(Navigate);

		private void Navigate(int? pageNumber) {
			if (pageNumber == null || pageNumber <= 0 || pageNumber > Paginator.GetMaxPage()) {
				return;
			}
			RequestNavigatePage?.Invoke(this, pageNumber.Value);
		}

		private void Items_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
			if (e.NewItems != null) {
				foreach (PaginatorItemViewModel item in e.NewItems.OfType<PaginatorItemViewModel>()) {
					item.NavigatePageCommand = NavigateCommand;
				}
			}
			if (e.OldItems != null) {
				foreach (PaginatorItemViewModel item in e.OldItems.OfType<PaginatorItemViewModel>()) {
					item.NavigatePageCommand = null;
				}
			}
		}

		public void Update() {
			Items.Clear();
			int max = Paginator.GetMaxPage();
			int current = Paginator.CurrentPage;

			if (current > 5) {
				Items.Add(new PaginatorItemViewModel(1));
				Items.Add(new PaginatorItemViewModel());
				Items.Add(new PaginatorItemViewModel(current - 3));
				Items.Add(new PaginatorItemViewModel(current - 2));
				Items.Add(new PaginatorItemViewModel(current - 1));
			} else {
				for (int i = 1; i < current; i++) {
					Items.Add(new PaginatorItemViewModel(i));
				}
			}

			Items.Add(new PaginatorItemViewModel(current, true));


			if (max - current > 5) {
				Items.Add(new PaginatorItemViewModel(current + 1));
				Items.Add(new PaginatorItemViewModel(current + 2));
				Items.Add(new PaginatorItemViewModel(current + 3));
				Items.Add(new PaginatorItemViewModel());
			} else {
				for (int i = current + 1; i < max; i++) {
					Items.Add(new PaginatorItemViewModel(i));
				}
			}

			if (current != max) {
				Items.Add(new PaginatorItemViewModel(max));
			}

		}

	}

	public class PaginatorItemViewModel : BindableBase {
		private int pageNumber;
		private bool actualPage;
		private ICommand navigatePageCommand;

		public int PageNumber {
			get => pageNumber;
			set => SetProperty(ref pageNumber, value);
		}

		public bool ActualPage {
			get => actualPage;
			set => SetProperty(ref actualPage, value);
		}

		public ICommand NavigatePageCommand {
			get => navigatePageCommand;
			set => SetProperty(ref navigatePageCommand, value);
		}

		public bool IsCurrentPage { get; set; }

		public PaginatorItemViewModel(int pageNumber, bool isCurrent = false) {
			PageNumber = pageNumber;
			ActualPage = true;
			IsCurrentPage = isCurrent;
		}

		public PaginatorItemViewModel() {
			PageNumber = 0;
			ActualPage = false;
		}


	}
}
