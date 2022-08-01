using E621Downloader.Models;
using E621Downloader.Models.E621;
using E621Downloader.Models.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Media.Imaging;

namespace E621Downloader.Views {
	public sealed partial class ImageHolderForPicturePage: UserControl {
		private string post_ID;
		//private bool isLoading;

		public string Post_ID {
			get => post_ID;
			set {
				//if(IsLoading) {
				//	return;
				//}
				post_ID = value;
				Load(post_ID);
			}
		}

		private readonly ProgressLoader progress;

		public E621Post Origin { get; set; }
		public E621Post Target { get; private set; }

		private CancellationTokenSource cts = new();

		private async void Load(string post_id) {
			Clear();
			if(string.IsNullOrWhiteSpace(post_id)) {
				progress.Value = null;
				MyImage.Source = null;
				return;
			}
			progress.Value = 0;
			Target = await E621Post.GetPostByIDAsync(post_id, cts.Token);
			TypeHint.PostRef = Target;
			BottomInfo.PostRef = Target;
			if(Target == null) {
				return;
			}
			if(Target.flags.deleted) {
				HintText.Text = "Post was Deleted".Language();
				progress.Value = null;
			} else {
				BitmapImage image = new(new Uri(Target.sample.url ?? Target.preview.url ?? "ms-appx:///Assets/e621.png"));
				MyImage.Source = image;
				image.ImageOpened += (s, e) => {
					progress.Value = null;
				};

				Methods.ProdedureLoading(PreviewImage, MyImage, Target, new LoadPoolItemActions() {
					OnUrlsEmpty = () => {
						progress.Value = null;
						HintText.Text = "Error".Language();
						this.Visibility = Visibility.Visible;
					},
					OnSampleUrlEmpty = () => {
						progress.Value = null;
						HintText.Text = "Empty URL".Language();
					},
					OnPreviewStart = () => {
						HintText.Text = "";
					},
					OnPreviewProgress = p => {
						progress.Value = p;
					},
					OnPreviewExists = () => {
						progress.Value = null;
					},
					OnPreviewOpened = b => {
						progress.Value = null;
					},
					OnPreviewFailed = () => {
						progress.Value = 0;
					},
					OnSampleStart = b => {

					},
					OnSampleProgress = p => {
						progress.Value = p;
					},
					OnSampleExists = () => {
						progress.Value = null;
					},
					OnSampleOpened = b => {
						progress.Value = null;
					},
					OnSampleFailed = () => {
						progress.Value = null;
						HintText.Text = "Error".Language();
					},
				});

			}
			ToolTipService.SetToolTip(this, new ToolTipContentForPost(Target));
			ToolTipService.SetPlacement(this, PlacementMode.Bottom);
		}

		public void Clear() {
			if(PreviewImage.Source is BitmapImage pi) {
				pi.UriSource = null;
			}
			if(MyImage.Source is BitmapImage mi) {
				mi.UriSource = null;
			}
			PreviewImage.Source = null;
			MyImage.Source = null;
		}

		public ImageHolderForPicturePage() {
			this.InitializeComponent();
			this.progress = new ProgressLoader(LoadingRing);
			this.Tapped += async (s, e) => {
				if(Target == null || Target.flags.deleted) {
					return;
				}
				List<E621Post> siblings = new();
				MainPage.CreateInstantDialog("Please Wait".Language(), "Loading Siblings".Language());
				if(!string.IsNullOrWhiteSpace(Origin.relationships.parent_id)) {
					siblings.Add(Target);
					List<E621Post> result = (await E621Post.GetPostsByTagsAsync(cts.Token, 1, $"parent:{Origin.relationships.parent_id}")).Where(p => CheckPostAvailable(p)).ToList();
					siblings.AddRange(result);
					App.PostsList.Current = Target;
				} else {
					siblings.Add(Origin);
					foreach(string item in Origin.relationships.children) {
						E621Post p = await E621Post.GetPostByIDAsync(item, cts.Token);
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

		private bool CheckPostAvailable(E621Post p) {
			return !p.flags.deleted;
		}
	}
}
