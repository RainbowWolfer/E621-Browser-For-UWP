﻿using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.UI.Xaml.Controls;
using YiffBrowser.Models.E621;

namespace YiffBrowser.Views.Controls.CustomControls {
	internal class Paginator : Control {

		public Paginator() {

		}


	}

	public class PaginatorViewModel(E621Paginator paginator) : BindableBase {
		private bool isLoading;

		public bool IsLoading {
			get => isLoading;
			set => SetProperty(ref isLoading, value);
		}

		public E621Paginator Paginator { get; } = paginator;

		public ObservableCollection<PaginatorItemViewModel> Items { get; } = new();
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
	}
}
