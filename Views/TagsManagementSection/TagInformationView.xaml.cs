using E621Downloader.Models.Posts;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Data.Html;
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
	public sealed partial class TagInformationView: Page {
		private ContentDialog dialog;

		private string[] tmp_tags;
		private E621Tag tag;
		public TagInformationView() {
			this.InitializeComponent();
		}
		protected override void OnNavigatedTo(NavigationEventArgs e) {
			base.OnNavigatedTo(e);
			if(e.Parameter is object[] objs) {
				dialog = objs[0] as ContentDialog;
				tmp_tags = objs[1] as string[];
				tag = objs[2] as E621Tag;

				TextBlock_Name.Text = $"Name: {(tag == null ? "Not Found" : tag.name)}";
				TextBlock_Count.Text = $"Count: {(tag == null ? 0 : tag.post_count)}";
				if(tag?.Wiki != null) {
					var doc = new HtmlDocument();
					doc.LoadHtml(tag.Wiki.body);
					TextBlock_Description.Text = $"Description: \n{doc.Text}";
				}
			}
		}

		private void BackButton_Tapped(object sender, TappedRoutedEventArgs e) {
			(dialog.Content as Frame).Navigate(typeof(TagsSelectionView), new object[] { dialog, tmp_tags });
		}
	}
}
