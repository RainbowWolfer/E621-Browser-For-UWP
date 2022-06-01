using E621Downloader.Models;
using E621Downloader.Models.Posts;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
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
		private bool isSelected = false;

		public int Category => autoComplete.category;
		public string CompleteName => autoComplete.name;
		public string AntecedentName => autoComplete.antecedent_name;
		public string Count => Methods.NumberToK(autoComplete.post_count);

		public bool IsSelected {
			get => isSelected;
			set {
				isSelected = value;
				RectangleWidthAnimation.From = CategoryRectangle.Width;
				RectangleWidthAnimation.To = value ? 8 : 4;
				SelectionStoryboard.Begin();

				if(isSelected) {
					RootPanel.BorderBrush = new SolidColorBrush(Colors.Red);
					RootPanel.BorderThickness = new Thickness(1.5);
					RootPanel.CornerRadius = new CornerRadius(5);
					CategoryRectangle.RadiusX = 0;
					CategoryRectangle.RadiusY = 0;
				} else {
					RootPanel.BorderBrush = new SolidColorBrush(Colors.Transparent);
					RootPanel.BorderThickness = new Thickness(0);
					RootPanel.CornerRadius = new CornerRadius(0);
					CategoryRectangle.RadiusX = 2;
					CategoryRectangle.RadiusY = 2;
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

		public void SetSelected(bool value) {
			IsSelected = value;
		}

	}
}
