using E621Downloader.Models;
using E621Downloader.Models.Posts;
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

namespace E621Downloader.Views.TagsManagementSection {
	public sealed partial class SingleTagSuggestion: UserControl {
		private readonly E621AutoComplete autoComplete;
		public int Category => autoComplete.category;
		public string CompleteName => autoComplete.name;
		public string AntecedentName => autoComplete.antecedent_name;
		public string Count => Methods.NumberToK(autoComplete.post_count);

		public SingleTagSuggestion(E621AutoComplete autoComplete) {
			this.InitializeComponent();
			this.autoComplete = autoComplete;
			if(string.IsNullOrWhiteSpace(AntecedentName)) {
				Arrow.Visibility = Visibility.Collapsed;
				AntecedentText.Visibility = Visibility.Collapsed;
			}
			switch(Category) {
				case 0:
					break;
				default:
					break;
			}
		}
	}
}
