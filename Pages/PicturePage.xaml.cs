using E621Downloader.Models;
using E621Downloader.Models.Download;
using E621Downloader.Models.Inerfaces;
using E621Downloader.Models.Locals;
using E621Downloader.Models.Networks;
using E621Downloader.Models.Posts;
using E621Downloader.Views;
using E621Downloader.Views.CommentsSection;
using Microsoft.Toolkit.Uwp.Helpers;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.System;
using Windows.System.UserProfile;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using static System.Net.WebRequestMethods;

namespace E621Downloader.Pages {
	public sealed partial class PicturePage: Page, IPage {
		public static string[] CurrentTags { get; set; }
		public Post PostRef { get; private set; }
		public PathType PostType { get; private set; }
		public readonly ObservableCollection<GroupTagListWithColor> tags = new();
		private readonly Dictionary<string, E621Tag> tags_pool = new();//should i refresh on every entry?
		public readonly List<E621Comment> comments = new();

		private bool commentsLoading;
		private bool commentsLoaded;
		private string commentsPostID;

		private string path;

		public bool EnableAutoPlay => LocalSettings.Current?.mediaAutoPlay ?? false;
		public string Title => PostRef == null ? "# No Post".Language() :
			$"#{PostRef.id} ({PostRef.rating.ToUpper()})";

		private CancellationTokenSource cts;

		private DataPackage imageDataPackage;
		private bool TextInputing { get; set; } = false;

		public int? Progress {
			get => progress;
			private set {
				progress = value;
				if(value == null) {
					LoadingBar.Visibility = Visibility.Collapsed;
				} else if(0 < value && value <= 100) {
					LoadingBar.Visibility = Visibility.Visible;
					LoadingBar.IsIndeterminate = false;
					LoadingBar.Value = (double)value;
				} else {
					LoadingBar.Visibility = Visibility.Visible;
					LoadingBar.IsIndeterminate = true;
				}
			}
		}
		private LoadPoolItem Loader { get; set; }

		private static Dictionary<string, VoteType> Voted { get; } = new();

		private bool isLoadingPost = false;

		public PicturePage() {
			this.InitializeComponent();
			this.NavigationCacheMode = NavigationCacheMode.Enabled;
			this.DataContextChanged += (s, c) => Bindings.Update();
			MyMediaPlayer.MediaPlayer.IsLoopingEnabled = true;
#if DEBUG
			DebugItem.Visibility = Visibility.Visible;
#else
			DebugItem.Visibility = Visibility.Collapsed;
#endif

		}

		protected override async void OnNavigatedTo(NavigationEventArgs e) {
			base.OnNavigatedTo(e);
			this.FocusModeUpdate();
			MyMediaPlayer.IsFullWindow = false;
			object p = e.Parameter;
			MainPage.ClearPicturePageParameter();
			MyMediaPlayer.AutoPlay = EnableAutoPlay;
			bool showNoPostGrid = false;
			this.imageDataPackage = null;
			CancelCTS();
			if(p == null && PostRef == null && PostsBrowserPage.HasLoaded()) {
				List<Post> posts = PostsBrowserPage.GetCurrentPosts();
				App.PostsList.UpdatePostsList(posts);
				p = posts.FirstOrDefault();
				App.PostsList.Current = p;
			}
			if(p is Post post) {
				Loader = LoadPool.SetNew(post);
				if(PostRef == post) {
					NoPostGrid.Visibility = Visibility.Collapsed;//just in case
					MainGrid.Visibility = Visibility.Visible;
					return;
				}
				PostRef = post;
				UpdateDownloadButton(false);
				await LoadFromPost(post);
				PostType = PathType.PostID;
				path = this.PostRef.id;
			} else if(p is MixPost mix) {
				Loader = LoadPool.SetNew(mix.PostRef);
				switch(mix.Type) {
					case PathType.PostID:
						if(mix.PostRef == this.PostRef) {
							return;
						}
						if(mix.PostRef != null) {
							this.PostRef = mix.PostRef;
						} else {
							cts = new CancellationTokenSource();
							this.PostRef = await Post.GetPostByIDAsync(cts.Token, mix.ID);
							mix.PostRef = this.PostRef;
						}
						UpdateDownloadButton(false);
						await LoadFromPost(this.PostRef);
						PostType = PathType.PostID;
						path = mix.ID;
						break;
					case PathType.Local:
						if(!mix.HasLocalLoaded) {
							(StorageFile file, MetaFile meta) = await Local.GetDownloadFile(mix.MetaFile.FilePath);
							if(file == null || meta == null) {
								break;
							}
							mix.ImageFile = file;
							mix.MetaFile = meta;
						}
						this.PostRef = mix.MetaFile.MyPost;
						UpdateDownloadButton(true);
						await LoadFromLocal(mix);
						PostType = PathType.Local;
						path = mix.LocalPath;
						break;
					default:
						throw new PathTypeException();
				}
			} else if(p is ILocalImage local) {
				Loader = LoadPool.SetNew(local.ImagePost);
				if(PostRef == local.ImagePost) {
					return;
				}
				PostRef = local.ImagePost;
				UpdateDownloadButton(true);
				await LoadFromLocal(local);
				PostType = PathType.Local;
				path = local.ImageFile.Path;
			} else if(p is string postID && !string.IsNullOrEmpty(postID)) {
				Loader = LoadPool.GetLoader(postID);
				if(this.PostRef?.id == postID) {
					return;
				}
				RemoveGroup();
				TagsLoadingRing.Visibility = Visibility.Visible;
				if(App.PostsPool.ContainsKey(postID)) {
					this.PostRef = App.PostsPool[postID];
				} else {
					cts = new CancellationTokenSource();
					SetIsLoadingPost(true);
					this.PostRef = await Post.GetPostByIDAsync(cts.Token, postID);
					SetIsLoadingPost(false);
					if(this.PostRef == null) {
						return;
					} else {
						if(App.PostsPool.ContainsKey(postID)) {
							App.PostsPool[postID] = this.PostRef;
						} else {
							App.PostsPool.Add(postID, this.PostRef);
						}
					}
				}
				Loader = LoadPool.SetNew(this.PostRef);
				UpdateDownloadButton(false);
				await LoadFromPost(this.PostRef);
				PostType = PathType.PostID;
				path = this.PostRef.id;
			} else if(this.PostRef == null && p == null) {
				Progress = null;
				showNoPostGrid = true;
				PostType = PathType.PostID;
			}
			TagsListView.ScrollIntoView(TagsListView.Items.FirstOrDefault());
			UpdateTagsGroup(PostRef?.tags);
			NoPostGrid.Visibility = showNoPostGrid ? Visibility.Visible : Visibility.Collapsed;
			MainGrid.Visibility = !showNoPostGrid ? Visibility.Visible : Visibility.Collapsed;
			TagsLoadingRing.Visibility = Visibility.Collapsed;
			TitleText.Text = Title;
			SetIsLoadingPost(false);
			UpdateScore();
			UpdateRatingColor();
			UpdateTypeIcon();
			UpdateSoundIcon();
			UpdateVoteButtons();
			UpdateFavoriteButton();
			UpdateDescriptionSection();
			UpdateOthers();
			UpdateSetAs();
			UpdateOpenInPhotos();
			ResetImage();
			if(this.PostRef != null && p != null) {
				if(MainSplitView.DisplayMode == SplitViewDisplayMode.Overlay) {
					MainSplitView.IsPaneOpen = false;
				}
				//InformationPivot.SelectedIndex = 0;
				commentsLoaded = false;
				commentsLoading = false;
				comments.Clear();
				CommentsListView.Items.Clear();
				if(MainSplitView.DisplayMode == SplitViewDisplayMode.Inline && InformationPivot.SelectedIndex == 1) {
					LoadCommentsAsync();
				}

				ChildrenGridView.Items.Clear();
				foreach(string item in this.PostRef.relationships.children) {
					var i = new ImageHolderForPicturePage() {
						Post_ID = item,
						Height = 220,
						Width = 235,
						Origin = this.PostRef,
					};
					ChildrenGridView.Items.Add(i);
				}
				ParentImageHolder.Post_ID = this.PostRef.relationships.parent_id;
				ParentImageHolder.Origin = this.PostRef;

				Local.History.AddPostID(this.PostRef.id);
			}

			if(hasExecutedPause && MyMediaPlayer.Source != null && MyMediaPlayer.MediaPlayer.PlaybackSession.PlaybackState == MediaPlaybackState.Paused) {
				MyMediaPlayer.MediaPlayer.Play();
				hasExecutedPause = false;
			}
		}

		private void CancelCTS() {
			if(cts != null) {
				try {
					cts.Cancel();
					cts.Dispose();
				} finally {
					cts = null;
				}
			}
		}

		private bool hasExecutedPause = false;
		protected override void OnNavigatedFrom(NavigationEventArgs e) {
			base.OnNavigatedFrom(e);
			if(!LocalSettings.Current.mediaBackgroundPlay && MyMediaPlayer.Source != null && MyMediaPlayer.MediaPlayer.PlaybackSession.PlaybackState == MediaPlaybackState.Playing) {
				MyMediaPlayer.MediaPlayer.Pause();
				hasExecutedPause = true;
			}
		}

		private async Task LoadFromPost(Post post) {
			if(MainImage.Source is BitmapImage source) {
				source.UriSource = null;
			}
			MainImage.Source = null;
			if(string.IsNullOrWhiteSpace(post.file.url)) {
				MyMediaPlayer.Visibility = Visibility.Collapsed;
				MyScrollViewer.Visibility = Visibility.Collapsed;
				Progress = null;
				await MainPage.CreatePopupDialog("Error".Language(), "Post".Language() + $"({post.id}) " + "has no valid file URL".Language());
				return;
			}
			FileType type = GetFileType(post);
			switch(type) {
				case FileType.Png:
				case FileType.Jpg:
				case FileType.Gif: {
					Progress = 0;
					PreviewImage.Source = null;
					MyMediaPlayer.Visibility = Visibility.Collapsed;
					MyScrollViewer.Visibility = Visibility.Visible;
					if(Loader.ImageFile != null && Loader.ImageFile.UriSource != null) {
						MainImage.Source = Loader.ImageFile;
						PreviewImage.Visibility = Visibility.Collapsed;
					} else {
						BitmapImage preview = Loader.GetBestPreviewImage();
						if(preview != null) {
							PreviewImage.Source = preview;
						}
						PreviewImage.Visibility = Visibility.Visible;
						MainImage.Source = new BitmapImage(new Uri(post.file.url));
						(MainImage.Source as BitmapImage).DownloadProgress += (s, e) => {
							Progress = e.Progress;
						};
					}

					imageDataPackage = new DataPackage() {
						RequestedOperation = DataPackageOperation.Copy,
					};
					imageDataPackage.SetBitmap(RandomAccessStreamReference.CreateFromUri(new Uri(post.file.url)));
					MyMediaPlayer.MediaPlayer.Source = null;
				}
				break;
				case FileType.Webm: {
					MyMediaPlayer.MediaPlayer.Source = null;
					Progress = null;
					MyMediaPlayer.Visibility = Visibility.Visible;
					MyScrollViewer.Visibility = Visibility.Collapsed;
					if(!string.IsNullOrEmpty(post.file.url)) {
						Progress = 0;
						BitmapImage preview = Loader.GetBestPreviewImage();
						if(preview != null) {
							PreviewImage.Source = preview;
							PreviewImage.Visibility = Visibility.Visible;
						}
						MyMediaPlayer.Source = MediaSource.CreateFromUri(new Uri(post.file.url));
						(MyMediaPlayer.Source as MediaSource).StateChanged += async (s, e) => {
							Debug.WriteLine($"From {e.OldState} To {e.NewState}");
							if(e.NewState == MediaSourceState.Opened) {
								await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => {
									PreviewImage.Visibility = Visibility.Collapsed;
									PreviewImage.Source = null;
									Progress = null;
									if(EnableAutoPlay) {
										MyMediaPlayer.MediaPlayer.Play();
									}
								});
							} else if(e.NewState == MediaSourceState.Failed) {

							}
						};
					}
				}
				break;
				case FileType.Anim:
					MyMediaPlayer.Visibility = Visibility.Collapsed;
					MyScrollViewer.Visibility = Visibility.Collapsed;
					Progress = null;
					break;
				default:
					await MainPage.CreatePopupDialog("Error".Language(), "Type".Language() + $" ({type}) " + "not supported".Language());
					break;
			}
		}

		private async Task LoadFromLocal(ILocalImage local) {
			FileType type = GetFileType(local.ImagePost);
			HintText.Visibility = Visibility.Collapsed;
			PreviewImage.Visibility = Visibility.Collapsed;
			PreviewImage.Source = null;
			switch(type) {
				case FileType.Png:
				case FileType.Jpg:
				case FileType.Gif:
					Progress = 0;
					MyMediaPlayer.Visibility = Visibility.Collapsed;
					MyScrollViewer.Visibility = Visibility.Visible;
					try {
						using IRandomAccessStream randomAccessStream = await local.ImageFile.OpenAsync(FileAccessMode.Read);
						BitmapImage result = new();
						await result.SetSourceAsync(randomAccessStream);
						MainImage.Source = result;
						imageDataPackage = new DataPackage() {
							RequestedOperation = DataPackageOperation.Copy,
						};
						imageDataPackage.SetBitmap(RandomAccessStreamReference.CreateFromFile(local.ImageFile));
					} catch(Exception e) {
						await MainPage.CreatePopupDialog("Error".Language(), "Local Post".Language() + $"({local.ImagePost.id}) - {local.ImageFile.Path} " + "Load Failed".Language() + $"\n{e.Message}");
					}
					MyMediaPlayer.Source = null;
					Progress = null;
					break;
				case FileType.Webm:
					Progress = null;
					MyMediaPlayer.Visibility = Visibility.Visible;
					MyScrollViewer.Visibility = Visibility.Collapsed;
					MyMediaPlayer.Source = MediaSource.CreateFromStorageFile(local.ImageFile);
					MainImage.Source = null;
					break;
				case FileType.Anim:
					MyMediaPlayer.Visibility = Visibility.Collapsed;
					MyScrollViewer.Visibility = Visibility.Collapsed;
					HintText.Text = $"Type SWF not supported".Language();
					HintText.Visibility = Visibility.Visible;
					Progress = null;
					break;
				default:
					HintText.Text = "Type".Language() + $" ({type}) " + "not supported".Language();
					HintText.Visibility = Visibility.Visible;
					await MainPage.CreatePopupDialog("Error".Language(), "Type".Language() + $" ({type}) " + "not supported".Language());
					break;
			}
		}

		public static FileType GetFileType(Post post) {
			return post.file.ext.ToLower().Trim() switch {
				"jpg" => FileType.Jpg,
				"png" => FileType.Png,
				"gif" => FileType.Gif,
				"anim" or "swf" => FileType.Anim,
				"webm" => FileType.Webm,
				_ => throw new Exception($"New Type({post.file.ext}) Found"),
			};
		}

		private void UpdateDownloadButton(bool isLocal) {
			if(isLocal) {
				DownloadText.Text = "Local".Language();
				DownloadIcon.Glyph = "\uE159";
				DownloadButton.IsEnabled = false;
			} else {
				DownloadText.Text = "Download".Language();
				DownloadIcon.Glyph = "\uE118";
				DownloadButton.IsEnabled = true;
			}
		}

		private void UpdateScore() {
			if(PostRef == null) {
				return;
			}
			ScoreText.Text = $"{PostRef.score.total}";
			ToolTipService.SetToolTip(ScoreText, "Upvote".Language() + $": {PostRef.score.up}\n" + "Downvote".Language() + $": {Math.Abs(PostRef.score.down)}");
		}

		private void UpdateTypeIcon() {
			if(PostRef == null) {
				return;
			}
			TypeIcon.Glyph = PostRef.file.ext.ToLower().Trim() switch {
				"jpg" or "png" => "\uEB9F",
				"gif" => "\uF4A9",
				"webm" => "\uE714",
				_ => "\uE9CE",
			};
			ToolTipService.SetToolTip(TypeIcon, "Type".Language() + $": {PostRef.file.ext.Trim().ToCamelCase()}");
		}

		private void UpdateSoundIcon() {
			if(PostRef == null) {
				return;
			}
			var list = PostRef.tags.GetAllTags();
			if(list.Contains("sound_warning")) {
				SoundIcon.Visibility = Visibility.Visible;
				SoundIcon.Foreground = new SolidColorBrush(GetColor(Rating.explict));//represent red
				ToolTipService.SetToolTip(SoundIcon, "This Video Has 'Sound_Warning' Tag".Language());
			} else if(list.Contains("sound")) {
				SoundIcon.Visibility = Visibility.Visible;
				SoundIcon.Foreground = new SolidColorBrush(GetColor(Rating.suggestive));//represent yellow
				ToolTipService.SetToolTip(SoundIcon, "This Video Has 'Sound' Tag".Language());
			} else {
				SoundIcon.Visibility = Visibility.Collapsed;
			}
		}

		private void UpdateRatingColor() {
			if(PostRef == null) {
				return;
			}
			string rating = PostRef.rating.ToLower().Trim();
			Color color;
			string tooltip;
			if(rating == "e") {
				color = GetColor(Rating.explict);
				tooltip = "Rating".Language() + ": Explicit";
			} else if(rating == "q") {
				color = GetColor(Rating.suggestive);
				tooltip = "Rating".Language() + ": Questionable";
			} else if(rating == "s") {
				color = GetColor(Rating.safe);
				tooltip = "Rating".Language() + ": Safe";
			} else {
				color = GetColor(null);
				tooltip = "No Rating".Language();
			}
			TitleText.Foreground = new SolidColorBrush(color);
			ToolTipService.SetToolTip(TitleText, tooltip);
			ToolTipService.SetPlacement(TitleText, PlacementMode.Bottom);
		}

		private void UpdateTagsGroup(Tags tags) {
			if(tags == null) {
				return;
			}
			RemoveGroup();
			bool isDark = App.GetApplicationTheme() == ApplicationTheme.Dark;

			AddNewGroup("Artist".Language(), tags.artist.ToGroupTag(E621Tag.GetCatrgoryColor(TagCategory.Artists, isDark)));
			AddNewGroup("Copyright".Language(), tags.copyright.ToGroupTag(E621Tag.GetCatrgoryColor(TagCategory.Copyrights, isDark)));
			AddNewGroup("Species".Language(), tags.species.ToGroupTag(E621Tag.GetCatrgoryColor(TagCategory.Species, isDark)));
			AddNewGroup("Character".Language(), tags.character.ToGroupTag(E621Tag.GetCatrgoryColor(TagCategory.Characters, isDark)));
			AddNewGroup("General".Language(), tags.general.ToGroupTag(E621Tag.GetCatrgoryColor(TagCategory.General, isDark)));
			AddNewGroup("Meta".Language(), tags.meta.ToGroupTag(E621Tag.GetCatrgoryColor(TagCategory.Meta, isDark)));
			AddNewGroup("Invalid".Language(), tags.invalid.ToGroupTag(E621Tag.GetCatrgoryColor(TagCategory.Invalid, isDark)));
			AddNewGroup("Lore".Language(), tags.lore.ToGroupTag(E621Tag.GetCatrgoryColor(TagCategory.Lore, isDark)));

		}

		private void RemoveGroup() {
			tags.Clear();
		}

		private void AddNewGroup(string title, List<GroupTag> content) {
			if(content == null) {
				return;
			}
			if(content.Count == 0) {
				return;
			}
			tags.Add(new GroupTagListWithColor(title, content));
		}

		private async void LoadCommentsAsync() {
			comments.Clear();
			CommentsListView.Items.Clear();
			commentsLoaded = false;
			commentsLoading = true;
			LoadingSection.Visibility = Visibility.Visible;
			CommentsHint.Visibility = Visibility.Collapsed;
			commentsPostID = PostRef.id;
			E621Comment[] list = await E621Comment.GetAsync(PostRef.id);
			CancelLoadAvatars();
			if(list != null && list.Length > 0) {
				foreach(E621Comment item in list.Reverse()) {
					comments.Add(item);
					CommentsListView.Items.Add(new CommentView(item));
				}
				LoadAllAvatars();
			} else {
				CommentsHint.Visibility = Visibility.Visible;
			}
			LoadingSection.Visibility = Visibility.Collapsed;
			commentsLoading = false;
			commentsLoaded = true;
		}

		private void CancelLoadAvatars() {
			foreach(CommentView item in CommentsListView.Items) {
				try {
					item.Cts?.Cancel();
					item.Cts?.Dispose();
				} finally {
					item.Cts = null;
				}
			}
		}

		private async void LoadAllAvatars() {
			foreach(CommentView item in CommentsListView.Items) {
				item.EnableLoadingRing();
			}
			foreach(CommentView item in CommentsListView.Items) {
				await item.LoadAvatar();
			}
		}


		private void UpdateVoteButtons() {
			if(this.PostRef == null) {
				return;
			}

			if(Voted.ContainsKey(PostRef.id)) {
				VoteType type = Voted[PostRef.id];
				UpVoteButton.IsChecked = type == VoteType.Up;
				DownVoteButton.IsChecked = type == VoteType.Down;
			} else {
				UpVoteButton.IsChecked = false;
				DownVoteButton.IsChecked = false;
			}
			UpVoteButton.IsEnabled = E621User.Current != null;
			DownVoteButton.IsEnabled = E621User.Current != null;
		}

		private void UpdateFavoriteButton() {
			if(this.PostRef == null) {
				return;
			}
			FavoriteButton.IsEnabled = E621User.Current != null;
			FavoriteListButton.IsEnabled = true;
			bool enable = this.PostRef.is_favorited;
			FavoriteButton.IsChecked = enable;
			if(enable) {
				FavoriteText.Text = "Favorited".Language();
				FavoriteIcon.Glyph = "\uEB52";
			} else {
				FavoriteText.Text = "Favorite".Language();
				FavoriteIcon.Glyph = "\uEB51";
			}
			FavoriteListButton.IsChecked = enable;
		}

		private void UpdateDescriptionSection() {
			DescriptionText.Text = PostRef != null && !string.IsNullOrEmpty(PostRef.description) ? PostRef.description : "No Description".Language();
			SourcesView.Items.Clear();
			if(PostRef != null) {
				foreach(string item in PostRef.sources) {
					SourcesView.Items.Add(new MyHyperLinkButton(item));
				}
				if(PostRef.sources.Count == 0) {
					SourcesText.Text = "No Source".Language();
				} else if(PostRef.sources.Count == 1) {
					SourcesText.Text = "Source".Language();
				} else {
					SourcesText.Text = "Sources".Language();
				}
			} else {
				SourcesText.Text = "";
			}
		}

		private void SetIsLoadingPost(bool loading) {
			isLoadingPost = loading;
			Visibility visibility;
			if(loading) {
				MainSplitView.IsPaneOpen = false;
				MoreInfoButton.IsEnabled = false;
				visibility = Visibility.Collapsed;
				LoadingIcon.Visibility = Visibility.Visible;
			} else {
				MoreInfoButton.IsEnabled = true;
				visibility = Visibility.Visible;
				LoadingIcon.Visibility = Visibility.Collapsed;
			}
			UpVoteButton.Visibility = visibility;
			ScoreText.Visibility = visibility;
			DownVoteButton.Visibility = visibility;
			Separator3.Visibility = visibility;
			FavoriteButton.Visibility = visibility;
			FavoriteListButton.Visibility = visibility;
			DownloadButton.Visibility = visibility;
			LeftButton.IsEnabled = !loading;
			RightButton.IsEnabled = !loading;
		}

		private void UpdateOthers() {
			if(PostRef == null) {
				return;
			}
			bool pool = PostRef.pools != null && PostRef.pools.Count > 0;
			PoolsHintText.Visibility = !pool ? Visibility.Visible : Visibility.Collapsed;
			PoolsListView.Visibility = pool ? Visibility.Visible : Visibility.Collapsed;

			bool parent = !string.IsNullOrWhiteSpace(PostRef.relationships.parent_id);
			ParentHintText.Visibility = !parent ? Visibility.Visible : Visibility.Collapsed;
			ParentImageHolderParent.Visibility = parent ? Visibility.Visible : Visibility.Collapsed;

			bool children = PostRef.relationships.children != null && PostRef.relationships.children.Count > 0;
			ChildrenHintText.Visibility = !children ? Visibility.Visible : Visibility.Collapsed;
			ChildrenGridView.Visibility = children ? Visibility.Visible : Visibility.Collapsed;
		}

		private void UpdateSetAs() {
			if(PostRef == null) {
				return;
			}
			SetAsItem.IsEnabled = IsAbleToSetAs(PostRef);
		}

		private void UpdateOpenInPhotos() {
			if(PostRef == null) {
				return;
			}
			PhotosItem.Visibility = PostType == PathType.Local ? Visibility.Visible : Visibility.Collapsed;
		}

		public static bool IsAbleToSetAs(Post post) {
			FileType type = GetFileType(post);
			return type == FileType.Png || type == FileType.Jpg || type == FileType.Gif;
		}

		private void MainImage_ImageOpened(object sender, RoutedEventArgs e) {
			Progress = null;
			PreviewImage.Visibility = Visibility.Collapsed;
			PreviewImage.Source = null;
			Loader.ImageFile = MainImage.Source as BitmapImage;
		}

		private void MainImage_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e) {
			Debug.WriteLine("!?!");
			//ImageTransform.ScaleX = ImageTransform.ScaleX < 2 ? 3 : 1;
			//ImageTransform.ScaleY = ImageTransform.ScaleY < 2 ? 3 : 1;
		}

		private void MainImage_PointerWheelChanged(object sender, PointerRoutedEventArgs e) {
			PointerPoint point = e.GetCurrentPoint(MainImage);
			double posX = point.Position.X;
			double posY = point.Position.Y;
			//Debug.WriteLine($"{(int)posX}-{(int)posY} <=> {(int)MainImage.ActualWidth / 2}-{(int)MainImage.ActualHeight / 2}");

			double scroll = point.Properties.MouseWheelDelta > 0 ? 1.2 : 0.8;

			double newScaleX = ImageTransform.ScaleX * scroll;
			double newScaleY = ImageTransform.ScaleY * scroll;

			double newTransX = scroll > 1 ?
				(ImageTransform.TranslateX - (posX * 0.2 * ImageTransform.ScaleX)) :
				(ImageTransform.TranslateX - (posX * -0.2 * ImageTransform.ScaleX));
			double newTransY = scroll > 1 ?
				(ImageTransform.TranslateY - (posY * 0.2 * ImageTransform.ScaleY)) :
				(ImageTransform.TranslateY - (posY * -0.2 * ImageTransform.ScaleY));

			if(newScaleX < 1 || newScaleY < 1) {
				newScaleX = 1;
				newScaleY = 1;
				newTransX = 0;
				newTransY = 0;
			}
			if(newScaleX > 10 || newScaleY > 10) {
				return;
			}

			ImageTransform.ScaleX = newScaleX;
			ImageTransform.ScaleY = newScaleY;
			ImageTransform.TranslateX = newTransX;
			ImageTransform.TranslateY = newTransY;

			Limit();
		}

		private void MainImage_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e) {
			ImageTransform.TranslateX += e.Delta.Translation.X;
			ImageTransform.TranslateY += e.Delta.Translation.Y;

			Limit();
		}

		private void ZoomIn() {
			if(MainPage.Instance?.IsInSearchPopup ?? false) {
				return;
			}
			Zoom(1.2);
		}

		private void ZoomOut() {
			if(MainPage.Instance?.IsInSearchPopup ?? false) {
				return;
			}
			Zoom(0.8);
		}

		private void Zoom(double ratio) {
			double newScaleX = ImageTransform.ScaleX * ratio;
			double newScaleY = ImageTransform.ScaleY * ratio;
			double newTransX = ratio > 1 ?
				(ImageTransform.TranslateX - (MainImage.ActualWidth / 2 * 0.2 * ImageTransform.ScaleX)) :
				(ImageTransform.TranslateX - (MainImage.ActualWidth / 2 * -0.2 * ImageTransform.ScaleX));
			double newTransY = ratio > 1 ?
				(ImageTransform.TranslateY - (MainImage.ActualHeight / 2 * 0.2 * ImageTransform.ScaleY)) :
				(ImageTransform.TranslateY - (MainImage.ActualHeight / 2 * -0.2 * ImageTransform.ScaleY));
			if(newScaleX < 1 || newScaleY < 1) {
				newScaleX = 1;
				newScaleY = 1;
				newTransX = 0;
				newTransY = 0;
			}
			if(newScaleX > 10 || newScaleY > 10) {
				newScaleX = 10;
				newScaleY = 10;
				return;
			}
			ImageTransform.ScaleX = newScaleX;
			ImageTransform.ScaleY = newScaleY;
			ImageTransform.TranslateX = newTransX;
			ImageTransform.TranslateY = newTransY;
			Limit();
		}

		private bool IsHorScaleAbove() {
			return MainImage.ActualWidth * ImageTransform.ScaleX > MyScrollViewer.ActualWidth;
		}

		private bool IsVerScaleAbove() {
			return MainImage.ActualHeight * ImageTransform.ScaleY > MyScrollViewer.ActualHeight;
		}

		private void Limit() {
			double horLimit = (MyScrollViewer.ActualWidth - MainImage.ActualWidth) / 2;
			if(IsHorScaleAbove()) {
				if(ImageTransform.TranslateX > -horLimit) {
					ImageTransform.TranslateX = -horLimit;
				}
				double offset = (MyScrollViewer.ActualWidth - MainImage.ActualWidth * ImageTransform.ScaleX);
				if(ImageTransform.TranslateX < offset - horLimit) {
					ImageTransform.TranslateX = offset - horLimit;
				}
			} else {
				if(ImageTransform.TranslateX < -horLimit) {
					ImageTransform.TranslateX = -horLimit;
				}
				double offset = (MyScrollViewer.ActualWidth - MainImage.ActualWidth * ImageTransform.ScaleX);
				if(ImageTransform.TranslateX > offset - horLimit) {
					ImageTransform.TranslateX = offset - horLimit;
				}
			}
			double verLimit = (MyScrollViewer.ActualHeight - MainImage.ActualHeight) / 2;
			if(IsVerScaleAbove()) {
				if(ImageTransform.TranslateY > 0) {
					ImageTransform.TranslateY = 0;
				}
				double offset = MyScrollViewer.ActualHeight - MainImage.ActualHeight * ImageTransform.ScaleY;
				if(ImageTransform.TranslateY < offset) {
					ImageTransform.TranslateY = offset;
				}
			} else {
				if(ImageTransform.TranslateY < 0) {
					ImageTransform.TranslateY = 0;
				}
				if(ImageTransform.TranslateY > verLimit) {
					ImageTransform.TranslateY = verLimit;
				}
			}
		}

		private void ResetImage() {
			ImageTransform.TranslateX = 0;
			ImageTransform.TranslateY = 0;
			ImageTransform.ScaleX = 1;
			ImageTransform.ScaleY = 1;
		}

		private void TagsListView_ItemClick(object sender, ItemClickEventArgs e) {
			string[] result_tags;
			if(LocalSettings.Current.concatTags) {
				result_tags = MainPage.GetCurrentTags().Append(((GroupTag)e.ClickedItem).Content).ToArray();
			} else {
				result_tags = new string[] { ((GroupTag)e.ClickedItem).Content };
			}
			MainPage.NavigateToPostsBrowser(1, result_tags);
		}

		private async void BlackListButton_Tapped(object sender, TappedRoutedEventArgs e) {
			var btn = sender as Button;
			string tag = btn.Tag as string;
			if(Local.Listing.CheckBlackList(tag)) {
				await Local.Listing.RemoveBlackList(tag);
				((FontIcon)btn.Content).Glyph = "\uE108";
				ToolTipService.SetToolTip(btn, "Add To Blacklist".Language());
			} else {
				await Local.Listing.AddBlackList(tag);
				((FontIcon)btn.Content).Glyph = "\uEA43";
				ToolTipService.SetToolTip(btn, "Remove From Blacklist".Language());

				if(Local.Listing.CheckFollowingList(tag)) {
					await Local.Listing.RemoveFollowingList(tag);
					var followListButton = (btn.Parent as RelativePanel).Children.OfType<Button>().ToList().Find(b => b.Name == "FollowListButton");
					((FontIcon)followListButton.Content).Glyph = "\uE109";
					ToolTipService.SetToolTip(followListButton, "Add To Follow List".Language());
				}
			}
		}

		private async void FollowListButton_Tapped(object sender, TappedRoutedEventArgs e) {
			var btn = sender as Button;
			string tag = btn.Tag as string;
			if(Local.Listing.CheckFollowingList(tag)) {
				await Local.Listing.RemoveFollowingList(tag);
				((FontIcon)btn.Content).Glyph = "\uE109";
				ToolTipService.SetToolTip(btn, "Add To Follow List".Language());
			} else {
				await Local.Listing.AddFollowingList(tag);
				((FontIcon)btn.Content).Glyph = "\uE74D";
				ToolTipService.SetToolTip(btn, "Remove From Follow List");

				if(Local.Listing.CheckBlackList(tag)) {
					await Local.Listing.RemoveBlackList(tag);
					Button blackListButton = (btn.Parent as RelativePanel).Children.OfType<Button>().ToList().Find(b => b.Name == "BlackListButton");
					((FontIcon)blackListButton.Content).Glyph = "\uE108";
					ToolTipService.SetToolTip(blackListButton, "Add To Blacklist".Language());
				}
			}
		}

		private void BlackListButton_Loaded(object sender, RoutedEventArgs e) {
			var btn = sender as Button;
			string tag = btn.Tag as string;
			if(Local.Listing.CheckBlackList(tag)) {
				((FontIcon)btn.Content).Glyph = "\uEA43";
				ToolTipService.SetToolTip(btn, "Remove From BlackList".Language());
			} else {
				((FontIcon)btn.Content).Glyph = "\uE108";
				ToolTipService.SetToolTip(btn, "Add To BlackList".Language());
			}
		}

		private void FollowListButton_Loaded(object sender, RoutedEventArgs e) {
			var btn = sender as Button;
			string tag = btn.Tag as string;
			if(Local.Listing.CheckFollowingList(tag)) {
				((FontIcon)btn.Content).Glyph = "\uE74D";
				ToolTipService.SetToolTip(btn, "Remove From Follow List".Language());
			} else {
				((FontIcon)btn.Content).Glyph = "\uE109";
				ToolTipService.SetToolTip(btn, "Add To Follow List".Language());
			}
		}

		private async void DownloadButton_Tapped(object sender, TappedRoutedEventArgs e) {
			await Download();
		}

		private async Task Download() {
			if(PostType == PathType.Local) {
				return;
			}
			if(await DownloadsManager.CheckDownloadAvailableWithDialog()) {
				if(await DownloadsManager.RegisterDownload(PostRef, CurrentTags)) {
					MainPage.CreateTip_SuccessDownload(this);
				} else {
					await MainPage.CreatePopupDialog("Error".Language(), "Downloads Failed".Language());
				}
			}
		}

		private void MoreInfoButton_Tapped(object sender, TappedRoutedEventArgs e) {
			MainSplitView.IsPaneOpen = !MainSplitView.IsPaneOpen;
			if(MainSplitView.IsPaneOpen && InformationPivot.SelectedIndex == 1 && commentsPostID != PostRef.id) {
				LoadCommentsAsync();
			}
		}

		private async void InfoButton_Tapped(object sender, TappedRoutedEventArgs e) {
			string tag = (sender as Button).Tag as string;
			string name = tag;
			var dialog = new ContentDialog() {
				Title = "Tag Information".Language() + $": {name}",
				CloseButtonText = "Back".Language(),
				Content = new TagInformationDisplay(tags_pool, tag),
			};
			MySubGrid.Children.Add(dialog);
			dialog.Closed += (s, args) => MySubGrid.Children.Remove(dialog);
			await dialog.ShowAsync(ContentDialogPlacement.Popup);
		}

		private void SplitViewModeSwitch_Toggled(object sender, RoutedEventArgs e) {
			if(MainSplitView == null) {
				return;
			}
			MainSplitView.DisplayMode = SplitViewModeSwitch.IsOn ? SplitViewDisplayMode.Overlay : SplitViewDisplayMode.Inline;
		}

		private void LeftButton_PointerEntered(object sender, PointerRoutedEventArgs e) {
			(sender as Button).Opacity = 1;
		}

		private void LeftButton_PointerExited(object sender, PointerRoutedEventArgs e) {
			(sender as Button).Opacity = 0.2;
		}

		private void LeftButton_Tapped(object sender, TappedRoutedEventArgs e) {
			GoLeft();
		}

		private void RightButton_PointerEntered(object sender, PointerRoutedEventArgs e) {
			(sender as Button).Opacity = 1;
		}

		private void RightButton_PointerExited(object sender, PointerRoutedEventArgs e) {
			(sender as Button).Opacity = 0.2;
		}

		private void RightButton_Tapped(object sender, TappedRoutedEventArgs e) {
			GoRight();
		}

		private void GoLeft() {
			if(isLoadingPost || cts_SetAs != null || (MainPage.Instance?.IsInSearchPopup ?? false)) {
				return;
			}
			MyMediaPlayer.IsFullWindow = false;
			MainPage.NavigateToPicturePage(App.PostsList.GoLeft());
		}

		private void GoRight() {
			if(isLoadingPost || cts_SetAs != null || (MainPage.Instance?.IsInSearchPopup ?? false)) {
				return;
			}
			MyMediaPlayer.IsFullWindow = false;
			MainPage.NavigateToPicturePage(App.PostsList.GoRight());
		}

		private void InformationPivot_SelectionChanged(object sender, SelectionChangedEventArgs e) {
			if(e.AddedItems != null && e.AddedItems.Count > 0 && e.AddedItems[0] is PivotItem item && item.Tag as string == "Comments" && !commentsLoaded && !commentsLoading) {
				if(PostRef != null) {
					LoadCommentsAsync();
				} else {
					CommentsHint.Visibility = Visibility.Visible;
				}
			}
		}

		private async void MoreInfoItem_Click(object sender, RoutedEventArgs e) {
			if(PostRef == null) {
				return;
			}
			var dialog = new ContentDialog() {
				Title = "More Info".Language(),
				Content = new PostMoreInfoDialog(PostRef),
				PrimaryButtonText = "Back".Language(),
			};
			await dialog.ShowAsync();
		}

		private async void BrowserItem_Click(object sender, RoutedEventArgs e) {
			if(PostRef == null) {
				return;
			}
			await Methods.OpenBrowser($"https://{Data.GetHost()}/posts/{PostRef.id}");
		}

		private void CopyItem_Click(object sender, RoutedEventArgs e) {
			if(PostRef == null) {
				return;
			}
			var dataPackage = new DataPackage() {
				RequestedOperation = DataPackageOperation.Copy
			};
			dataPackage.SetText($"{PostRef.id}");
			Clipboard.SetContent(dataPackage);
		}

		private void CopyImageItem_Click(object sender, RoutedEventArgs e) {
			if(imageDataPackage == null) {
				return;
			}
			Clipboard.SetContent(imageDataPackage);
		}

		private async void DebugItem_Click(object sender, RoutedEventArgs e) {
			if(PostRef == null) {
				return;
			}
			var dialog = new ContentDialog() {
				Title = "Debug Info".Language(),
				Content = new PostDebugView(PostRef),
				PrimaryButtonText = "Back".Language(),
			};
			await dialog.ShowAsync();
		}

		private async void FavoriteButton_Tapped(object sender, TappedRoutedEventArgs e) {
			e.Handled = true;
			await FavoriteE621();
		}

		private async Task FavoriteE621() {
			if(FavoriteButton.IsEnabled == false) {
				return;
			}
			FavoriteButton.IsEnabled = false;
			FavoriteListButton.IsEnabled = false;
			FavoriteText.Text = "Pending".Language();
			FavoriteIcon.Glyph = "\uE10C";
			if(cts == null) {
				cts = new CancellationTokenSource();
			}
			if(!this.PostRef.is_favorited) {
				HttpResult<string> result = await Favorites.PostAsync(this.PostRef.id, cts.Token);
				if(result.Result == HttpResultType.Success) {
					FavoriteText.Text = "Favorited".Language();
					FavoriteIcon.Glyph = "\uEB52";
					this.PostRef.is_favorited = true;
				} else if(result.Result == HttpResultType.Canceled) {
					return;
				} else {
					FavoriteText.Text = "Favorite".Language();
					FavoriteIcon.Glyph = "\uEB51";
					MainPage.CreateTip(this, result.StatusCode.ToString(), result.Helper, Symbol.Important, "OK".Language());
					FavoriteButton.IsChecked = false;
					this.PostRef.is_favorited = false;
				}
			} else {
				HttpResult<string> result = await Favorites.DeleteAsync(this.PostRef.id, cts.Token);
				if(result.Result == HttpResultType.Success) {
					FavoriteText.Text = "Favorite".Language();
					FavoriteIcon.Glyph = "\uEB51";
					this.PostRef.is_favorited = false;
				} else if(result.Result == HttpResultType.Canceled) {
					return;
				} else {
					FavoriteText.Text = "Favorited".Language();
					FavoriteIcon.Glyph = "\uEB52";
					MainPage.CreateTip(this, result.StatusCode.ToString(), result.Helper, Symbol.Important, "OK".Language());
					FavoriteButton.IsChecked = true;
					this.PostRef.is_favorited = true;
				}
			}
			FavoriteButton.IsEnabled = true;
			FavoriteListButton.IsEnabled = true;
			FavoriteButton.IsChecked = this.PostRef.is_favorited;
			FavoriteListButton.IsChecked = this.PostRef.is_favorited;
		}

		private void ToggleTagsButton_Tapped(object sender, TappedRoutedEventArgs e) {
			DisplayTagsView();
		}

		private void DisplayTagsView(bool? enabled = null) {
			double from = TagsListView.Width;
			TagsDisplay.Children[0].SetValue(DoubleAnimation.FromProperty, from);
			double to;
			if(enabled == true) {
				to = 250;
				ToggleTagsButtonIcon.Glyph = "\uE8A0";
			} else if(enabled == false) {
				to = 0;
				ToggleTagsButtonIcon.Glyph = "\uE89F";
			} else {
				if(TagsListView.Width <= 125) {
					to = 250;
					ToggleTagsButtonIcon.Glyph = "\uE8A0";
				} else {
					to = 0;
					ToggleTagsButtonIcon.Glyph = "\uE89F";
				}
			}
			TagsDisplay.Children[0].SetValue(DoubleAnimation.ToProperty, to);
			TagsDisplay.Begin();
		}

		private void GotoLibraryButton_Tapped(object sender, TappedRoutedEventArgs e) {
			MainPage.NavigateTo(PageTag.Library);
		}

		private void GotoHomeButton_Tapped(object sender, TappedRoutedEventArgs e) {
			MainPage.NavigateTo(PageTag.PostsBrowser);
		}

		private void RelativePanel_RightTapped(object sender, RightTappedRoutedEventArgs e) {
			string tag = (string)((Panel)sender).Tag;
			MenuFlyout flyout = new();
			MenuFlyoutItem item_copy = new() {
				Text = "Copy Tag".Language(),
				Icon = new FontIcon() { Glyph = "\uE8C8" },
			};
			item_copy.Click += (s, arg) => {
				if(PostRef == null) {
					return;
				}
				var dataPackage = new DataPackage() {
					RequestedOperation = DataPackageOperation.Copy
				};
				dataPackage.SetText(tag);
				Clipboard.SetContent(dataPackage);
			};
			flyout.Items.Add(item_copy);
			MenuFlyoutItem item_concat = new() {
				Text = "Concat Search".Language(),
				Icon = new FontIcon() { Glyph = "\uE109" },
			};
			item_concat.Click += (s, arg) => {
				string[] concat_tags = MainPage.GetCurrentTags().Append(tag).ToArray();
				MainPage.NavigateToPostsBrowser(1, concat_tags);
			};
			flyout.Items.Add(item_concat);
			MenuFlyoutItem item_overlay = new() {
				Text = "Overlay Search".Language(),
				Icon = new FontIcon() { Glyph = "\uE71E" },
			};
			item_overlay.Click += (s, arg) => {
				MainPage.NavigateToPostsBrowser(1, tag);
			};
			flyout.Items.Add(item_overlay);
			if(flyout.Items.Count != 0) {
				FrameworkElement senderElement = sender as FrameworkElement;
				var p = e.GetPosition(sender as UIElement);
				flyout.ShowAt(sender as UIElement, p);
			}
		}

		private async void PoolsListView_ItemClick(object sender, ItemClickEventArgs e) {
			string id = (string)e.ClickedItem;
			MainPage.CreateInstantDialog("Please Wait".Language(), "Loading Pool".Language() + $" ({id})");
			E621Pool pool = await E621Pool.GetAsync(id);
			MainPage.HideInstantDialog();
			MainPage.NavigateToPostsBrowser(pool);
		}

		private void FavoriteListButton_Tapped(object sender, TappedRoutedEventArgs e) {
			var flyout = new Flyout();
			var content = new PersonalFavoritesList(flyout, PostType, path) {
				Width = 200,
				IsInitialFavorited = this.PostRef.is_favorited,
			};
			flyout.Content = content;
			Point pos = e.GetPosition(sender as UIElement);
			pos.Y += 20;
			TextInputing = true;
			flyout.ShowAt(sender as UIElement, new FlyoutShowOptions() {
				Placement = FlyoutPlacementMode.Bottom,
				ShowMode = FlyoutShowMode.Auto,
				Position = pos,
			});
			flyout.Closed += (s, args) => TextInputing = false;
		}

		private async void UpVoteButton_Click(object sender, RoutedEventArgs e) {
			await UpVote();
		}

		private async Task UpVote() {
			if(DownVoteButton.IsEnabled == false || UpVoteButton.IsEnabled == false) {
				return;
			}
			UpVoteIcon.Glyph = "\uE10C";
			UpVoteButton.IsEnabled = false;
			if(cts == null) {
				cts = new CancellationTokenSource();
			}
			if(!await SubmitVote(true, cts.Token)) {
				UpVoteIcon.Glyph = "\uE96D";
				return;
			}
			UpVoteButton.IsEnabled = true;
			UpVoteIcon.Glyph = "\uE96D";
			UpVoteButton.IsChecked = !UpVoteButton.IsChecked;
			if(UpVoteButton.IsChecked == true) {
				DownVoteButton.IsChecked = false;
			}
			VoteType up = UpVoteButton.IsChecked == true ? VoteType.Up : VoteType.None;
			if(Voted.ContainsKey(PostRef.id)) {
				Voted[PostRef.id] = up;
			} else {
				Voted.Add(PostRef.id, up);
			}
		}

		private async void DownVoteButton_Click(object sender, RoutedEventArgs e) {
			await DownVote();
		}

		private async Task DownVote() {
			if(DownVoteButton.IsEnabled == false || UpVoteButton.IsEnabled == false) {
				return;
			}
			DownVoteIcon.Glyph = "\uE10C";
			DownVoteButton.IsEnabled = false;
			if(cts == null) {
				cts = new CancellationTokenSource();
			}
			if(!await SubmitVote(false, cts.Token)) {
				DownVoteIcon.Glyph = "\uE96E";
				return;
			}
			DownVoteButton.IsEnabled = true;
			DownVoteIcon.Glyph = "\uE96E";
			DownVoteButton.IsChecked = !DownVoteButton.IsChecked;
			if(DownVoteButton.IsChecked == true) {
				UpVoteButton.IsChecked = false;
			}
			VoteType down = DownVoteButton.IsChecked == true ? VoteType.Down : VoteType.None;
			if(Voted.ContainsKey(PostRef.id)) {
				Voted[PostRef.id] = down;
			} else {
				Voted.Add(PostRef.id, down);
			}
		}

		private async Task<bool> SubmitVote(bool up, CancellationToken token) {
			DataResult<E621Vote> result = await E621Vote.VotePost(PostRef.id, up ? 1 : -1, true, token);
			if(result.ResultType == HttpResultType.Success) {
				PostRef.score.total = result.Data.score;
				PostRef.score.up = result.Data.up;
				PostRef.score.down = result.Data.down;
				UpdateScore();
			}
			if(result.ResultType == HttpResultType.Canceled) {
				return false;
			} else {
				return true;
			}
		}

		public static void ClearVoted() {
			Voted.Clear();
		}

		private enum VoteType {
			None, Up, Down
		}

		public static async Task SetWallpaperAsync(StorageFile file) {
			if(UserProfilePersonalizationSettings.IsSupported()) {
				var profileSettings = UserProfilePersonalizationSettings.Current;
				//token.ThrowIfCancellationRequested();
				await profileSettings.TrySetWallpaperImageAsync(file);
			}
		}

		public static async Task SetLockScreenAsync(StorageFile file) {
			if(UserProfilePersonalizationSettings.IsSupported()) {
				var profileSettings = UserProfilePersonalizationSettings.Current;
				//token.ThrowIfCancellationRequested();
				await profileSettings.TrySetLockScreenImageAsync(file);
			}
		}

		public static async Task<StorageFile> GetImageFile(PathType type, string pathForLocal, Post post, CancellationToken token) {
			StorageFile file;
			switch(type) {
				case PathType.PostID: {
					token.ThrowIfCancellationRequested();
					file = await Local.WallpapersFolder.CreateFileAsync($"{post.id}.{post.file.ext}", CreationCollisionOption.GenerateUniqueName);
					using HttpClient client = new();
					token.ThrowIfCancellationRequested();
					byte[] buffer = await client.GetByteArrayAsync(post.file.url);
					token.ThrowIfCancellationRequested();
					using Stream stream = await file.OpenStreamForWriteAsync();
					stream.Write(buffer, 0, buffer.Length);
				}
				break;
				case PathType.Local: {
					token.ThrowIfCancellationRequested();
					file = await StorageFile.GetFileFromPathAsync(pathForLocal);
					token.ThrowIfCancellationRequested();
					file = await file.CopyAsync(Local.WallpapersFolder);
				}
				break;
				default:
					throw new Exception();
			}
			return file;
		}

		private ContentDialog dialog;
		private LoadingDialog dialog_content;
		private CancellationTokenSource cts_SetAs = null;
		private int? progress;

		private async void ShowLoadingDialog(string title, string content, Action onCancel = null) {
			dialog_content = new LoadingDialog() {
				DialogContent = content,
				OnCancel = onCancel,
			};
			dialog = new ContentDialog() {
				Title = title,
				Content = dialog_content,
				CloseButtonText = "Cancel".Language(),
			};
			await dialog.ShowAsync();
		}

		private void UpdateDialogContent(string content) {
			if(dialog_content == null) {
				return;
			}
			dialog_content.DialogContent = content;
		}

		private void HideLoadingDialog() {
			dialog?.Hide();
			dialog = null;
		}

		private void CancelLoading() {
			if(cts_SetAs != null) {
				cts_SetAs.Cancel();
				cts_SetAs.Dispose();
			}
			cts_SetAs = null;
		}

		private async void SetAsWallpaperItem_Click(object sender, RoutedEventArgs e) {
			if(sender is not MenuFlyoutItem item || !item.IsEnabled) {
				return;
			}
			CancelLoading();
			ShowLoadingDialog("Loading".Language(), "Getting Image".Language(), CancelLoading);
			cts_SetAs = new CancellationTokenSource();
			try {
				StorageFile file = await GetImageFile(PostType, path, PostRef, cts_SetAs.Token);
				UpdateDialogContent("Setting Wallpaper");
				await SetWallpaperAsync(file);
			} catch(OperationCanceledException) { }
			HideLoadingDialog();
			CancelLoading();
		}

		private async void SetAsLockScreenItem_Click(object sender, RoutedEventArgs e) {
			if(sender is not MenuFlyoutItem item || !item.IsEnabled) {
				return;
			}
			CancelLoading();
			ShowLoadingDialog("Loading".Language(), "Getting Image".Language(), CancelLoading);
			cts_SetAs = new CancellationTokenSource();
			try {
				StorageFile file = await GetImageFile(PostType, path, PostRef, cts_SetAs.Token);
				UpdateDialogContent("Setting Lock-screen".Language());
				await SetLockScreenAsync(file);
			} catch(OperationCanceledException) { }
			HideLoadingDialog();
			CancelLoading();
		}

		public static Color GetColor(Rating? rating) {
			bool isDark = App.GetApplicationTheme() == ApplicationTheme.Dark;
			return rating switch {
				Rating.safe => (isDark ? "#008000" : "#36973E").ToColor(),
				Rating.suggestive => (isDark ? "#FFFF00" : "#EFC50C").ToColor(),
				Rating.explict => (isDark ? "#FF0000" : "#C92A2D").ToColor(),
				_ => (isDark ? "#FFF" : "#000").ToColor(),
			};
		}

		private async void PhotosItem_Click(object sender, RoutedEventArgs e) {
			if(PostType != PathType.Local || string.IsNullOrWhiteSpace(path)) {
				return;
			}

			try {
				StorageFile file = await StorageFile.GetFileFromPathAsync(path);
				await Methods.LaunchFile(file);
			} catch(Exception ex) {
				await new ContentDialog() {
					Title = "Error".Language(),
					Content = $"{"An error occurred while finding this file".Language()}\n {"File Path".Language()}:{path}\n{ex.Message}",
					CloseButtonText = "Back".Language(),
					DefaultButton = ContentDialogButton.Close,
				}.ShowAsync();
				Debug.WriteLine(ex.Message);
			}
		}

		private async void DownloadKey_Invoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args) {
			await Download();
			args.Handled = true;
		}

		private async void FavoriteKey_Invoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args) {
			await FavoriteE621();
			args.Handled = true;
		}

		private void LastFavoriteKey_Invoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args) {
			string target = PersonalFavoritesList.LastAddedList;
			if(string.IsNullOrWhiteSpace(target)) {
				target = FavoritesList.Table.FirstOrDefault()?.Name;
			}
			if(string.IsNullOrWhiteSpace(target)) {
				return;
			}
			/*bool hasChanges = */
			FavoritesList.Modify(new(), new() { target }, path, PostType);

			MainPage.CreateTip(this, "Success".Language(),
				true ? $"{"Added to favorite list".Language()} : {target}" :
					$"{"Failed adding to favorite list".Language()} : {target}",
				Symbol.Accept, "OK".Language(), false, TeachingTipPlacementMode.TopRight, null, 2000);
			args.Handled = true;
		}
		private async void UpVoteKey_Invoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args) {
			await UpVote();
			args.Handled = true;
		}

		private async void DownVoteKey_Invoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args) {
			await DownVote();
			args.Handled = true;
		}


		void IPage.UpdateNavigationItem() {
			MainPage.Instance.currentTag = PageTag.Picture;
			MainPage.Instance.UpdateNavigationItem();
		}

		void IPage.FocusMode(bool enabled) {
			HeaderDisplayAnimatoin.From = HeaderPanel.Height;
			if(enabled) {
				DisplayTagsView(false);
				HeaderDisplayAnimatoin.To = 0;
			} else {
				HeaderDisplayAnimatoin.To = 58;
			}
			HeaderDisplayStoryboard.Begin();
		}

		private bool KeyCondition() {
			return MainPage.Instance.currentTag == PageTag.Picture &&
				!TextInputing &&
				this.PostRef != null;
		}

		private void UpKey_Invoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args) {
			if(!KeyCondition()) {
				return;
			}
			ZoomIn();
		}

		private void LeftKey_Invoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args) {
			if(!KeyCondition()) {
				return;
			}
			GoLeft();
			args.Handled = true;
		}

		private void DownKey_Invoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args) {
			if(!KeyCondition()) {
				return;
			}
			ZoomOut();
		}

		private void RightKey_Invoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args) {
			if(!KeyCondition()) {
				return;
			}
			GoRight();
			args.Handled = true;
		}

		private void MainImage_ImageFailed(object sender, ExceptionRoutedEventArgs e) {
			Progress = 0;
		}
	}

	public class GroupTagListWithColor: ObservableCollection<GroupTag> {
		public string Key { get; set; }
		public GroupTagListWithColor(string key) : base() {
			this.Key = key;
		}
		public GroupTagListWithColor(string key, List<GroupTag> content) : base() {
			this.Key = key;
			content.ForEach(s => this.Add(s));
		}
	}

	public struct GroupTag {
		public string Content { get; set; }
		public Color Color { get; set; }
		public Brush Brush => new SolidColorBrush(this.Color);
		public GroupTag(string content, Color color) {
			Content = content;
			Color = color;
		}
	}

	public class GroupTagList: ObservableCollection<string> {
		public string Key { get; set; }
		public GroupTagList(string key) : base() {
			this.Key = key;
		}
		public GroupTagList(string key, List<string> content) : base() {
			this.Key = key;
			content.ForEach(s => this.Add(s));
		}
	}

	public enum PathType {
		PostID, Local
	}

	public enum FileType {
		Png, Jpg, Gif, Webm, Anim
	}


	public interface ILocalImage {
		Post ImagePost { get; }
		StorageFile ImageFile { get; }
	}

	public class MixPost: ILocalImage {
		public PathType Type { get; set; }
		public string ID { get; set; }
		public string LocalPath { get; set; }
		// -------------------------
		public Post PostRef { get; set; }
		// -------------------------
		public StorageFile ImageFile { get; set; }
		public MetaFile MetaFile { get; set; }
		// -------------------------
		public bool HasLocalLoaded => ImageFile != null && MetaFile != null;
		Post ILocalImage.ImagePost => MetaFile.MyPost;
		StorageFile ILocalImage.ImageFile => ImageFile;

		public MixPost(PathType pathType, string path) {
			Type = pathType;
			switch(Type) {
				case PathType.PostID:
					ID = path;
					break;
				case PathType.Local:
					LocalPath = path;
					break;
				default:
					throw new PathTypeException();
			}
		}

		public void Finish(Post post) {
			if(Type != PathType.PostID) {
				throw new Exception();
			}
			PostRef = post;
		}

		public void Finish(StorageFile storageFile, MetaFile metaFile) {
			if(Type != PathType.Local) {
				throw new Exception();
			}
			ImageFile = storageFile;
			MetaFile = metaFile;
		}

	}
}
