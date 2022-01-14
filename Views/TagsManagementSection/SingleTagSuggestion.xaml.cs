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
		public string Count {
			get {
				if(autoComplete.post_count > 1000) {
					int a = autoComplete.post_count / 1000;
					int length = $"{autoComplete.post_count}".Length;
					int pow = (int)Math.Pow(10, length - 1);
					int head = int.Parse($"{autoComplete.post_count}".First().ToString());
					int b = (autoComplete.post_count - pow * head) / (pow / 10);
					if(b == 0) {
						return $"{a}K";
					} else {
						return $"{a}.{b}K";
					}
				} else {
					return $"{autoComplete.post_count}";
				}
			}
		}

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
