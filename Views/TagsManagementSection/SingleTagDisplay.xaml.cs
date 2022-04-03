using E621Downloader.Models.Posts;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace E621Downloader.Views.TagsManagementSection {
	public sealed partial class SingleTagDisplay: UserControl {
		private TagsSelectionView parent;
		private string tag;
		private readonly CancellationTokenSource source = new CancellationTokenSource();
		private bool hasDisposed = false;
		public SingleTagDisplay(TagsSelectionView parent, string tag) {
			this.InitializeComponent();
			this.parent = parent;
			this.tag = tag;
			this.TagText.Text = tag;
			LoadingIcon.Visibility = Visibility.Visible;
			CountText.Visibility = Visibility.Collapsed;

			CancellationToken token = source.Token;
			Task.Run(async () => {
				await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () => {
					try {
						string description = "";
						E621Tag e621_tag = parent.GetE621Tag(tag);
						if(e621_tag == null) {
							e621_tag = await E621Tag.GetFirstAsync(tag);
						}
						parent.RegisterE621Tag(tag, e621_tag);
						UpdateCountText(e621_tag?.post_count);
						if(e621_tag != null) {
							if(e621_tag.IsWikiLoaded) {
								description = e621_tag.Wiki.body;
							} else {
								E621Wiki e621_wiki = await e621_tag.LoadWikiAsync(token);
								description = e621_wiki?.body;
							}
							parent.RegisterE621Tag(tag, e621_tag);
						}
						UpdateDescription(description);
					} catch(Exception e) {
						Debug.WriteLine(e);
					} finally {
						hasDisposed = true;
						source.Dispose();
					}
				});
			}, token);
		}

		public void CancelLoadingTag() {
			if(!hasDisposed) {
				source?.Cancel();
				source?.Dispose();
			}
		}

		private void UpdateCountText(int? count) {
			LoadingIcon.Visibility = Visibility.Collapsed;
			CountText.Visibility = Visibility.Visible;
			if(count is int i) {
				CountText.Text = $"({i})";
			} else {
				CountText.Text = "";
			}
		}

		private void UpdateDescription(string content) {
			DescriptionText.Text = string.IsNullOrWhiteSpace(content) ? "No Wiki Found" : content;
		}

		private void DeleteButton_Tapped(object sender, TappedRoutedEventArgs e) {
			parent.RemoveTag(tag);
		}

		private void TagExpander_Expanding(Expander sender, ExpanderExpandingEventArgs args) {

		}
	}
}
