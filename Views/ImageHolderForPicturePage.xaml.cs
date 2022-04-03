using E621Downloader.Models;
using E621Downloader.Models.Posts;
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
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

namespace E621Downloader.Views {
	public sealed partial class ImageHolderForPicturePage: UserControl {
		private string post_ID;
		private bool isLoading;

		public string Post_ID {
			get => post_ID;
			set {
				if(IsLoading) {
					return;
				}
				post_ID = value;
				Load(post_ID);
			}
		}

		public bool IsLoading {
			get => isLoading;
			set {
				isLoading = value;
				LoadingRing.IsActive = value;
			}
		}

		public Post Origin { get; set; }
		public Post Target { get; private set; }

		private CancellationTokenSource cts = new CancellationTokenSource();

		private async void Load(string post_id) {
			if(string.IsNullOrWhiteSpace(post_id)) {
				IsLoading = false;
				MyImage.Source = null;
				return;
			}
			IsLoading = true;
			Target = await Post.GetPostByIDAsync(cts.Token, post_id);
			TypeHint.PostRef = Target;
			BottomInfo.PostRef = Target;
			if(Target == null) {
				return;
			}
			if(Target.flags.deleted) {
				HintText.Visibility = Visibility.Visible;
				IsLoading = false;
			} else {
				BitmapImage image = new BitmapImage(new Uri(Target.sample.url ?? Target.preview.url ?? "ms-appx:///Assets/e621.png"));
				MyImage.Source = image;
				image.ImageOpened += (s, e) => {
					IsLoading = false;
				};
			}
			ToolTipService.SetToolTip(this, new ToolTipContentForPost(Target));
			ToolTipService.SetPlacement(this, PlacementMode.Bottom);
		}

		public ImageHolderForPicturePage() {
			this.InitializeComponent();
			this.Tapped += async (s, e) => {
				if(Target == null || Target.flags.deleted) {
					return;
				}
				List<Post> siblings = new List<Post>();
				MainPage.CreateInstantDialog("Please Wait", "Loading Siblings");
				if(!string.IsNullOrWhiteSpace(Origin.relationships.parent_id)) {
					siblings.Add(Target);
					List<Post> result = (await Post.GetPostsByTagsAsync(cts.Token, 1, $"parent:{Origin.relationships.parent_id}")).Where(p => CheckPostAvailable(p)).ToList();
					siblings.AddRange(result);
					App.PostsList.Current = Target;
				} else {
					siblings.Add(Origin);
					foreach(string item in Origin.relationships.children) {
						Post p = await Post.GetPostByIDAsync(cts.Token, item);
						if(CheckPostAvailable(p)) {
							siblings.Add(p);
						}
					}
					App.PostsList.Current = siblings.Find(sib => sib.id == Target.id) ?? Origin;
				}
				MainPage.HideInstantDialog();
				App.PostsList.UpdatePostsList(siblings);
				MainPage.NavigateToPicturePage(Target);
			};
		}

		private bool CheckPostAvailable(Post p) {
			return !p.flags.deleted;
		}
	}
}
