using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using YiffBrowser.Views.Pages.E621;

namespace YiffBrowser.Views.Controls.LocalViews {
	public sealed partial class FileItemDetailView : UserControl {



		public FileItem FileItem {
			get => (FileItem)GetValue(FileItemProperty);
			set => SetValue(FileItemProperty, value);
		}

		public static readonly DependencyProperty FileItemProperty = DependencyProperty.Register(
			nameof(FileItem),
			typeof(FileItem),
			typeof(FileItemDetailView),
			new PropertyMetadata(null)
		);



		public FileItemDetailView() {
			InitializeComponent();
		}

	}

	internal class FileItemDetailViewModel : BindableBase {
		private FileItem fileItem;

		public FileItem FileItem {
			get => fileItem;
			set => SetProperty(ref fileItem, value, OnFileItemChanged);
		}

		private void OnFileItemChanged() {
			
		}

		public FileItemDetailViewModel() {

		}

	}
}
