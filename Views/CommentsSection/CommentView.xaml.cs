using E621Downloader.Models;
using E621Downloader.Models.Networks;
using E621Downloader.Models.Posts;
using E621Downloader.Pages;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.Core;
using Windows.Storage.Streams;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

namespace E621Downloader.Views.CommentsSection {
	public sealed partial class CommentView: UserControl, INotifyPropertyChanged {
		public static Dictionary<string, bool?> Voted { get; } = new();

		public event PropertyChangedEventHandler PropertyChanged;
		private void RaiseProperty(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

		private bool upVote;
		private bool downVote;

		public E621Comment Comment { get; set; }
		public E621User User { get; set; }
		public CancellationTokenSource Cts { get; set; }

		public int Score {
			get {
				int up = UpVote ? 1 : 0;
				int down = DownVote ? -1 : 0;
				return Comment.score + up + down;
			}
		}

		public bool UpVote {
			get => upVote;
			set {
				upVote = value;
				UpVoteIcon.Foreground = upVote ? new SolidColorBrush((Color)Resources["SystemAccentColor"]) : (Brush)Resources["ButtonForegroundThemeBrush"];
				RaiseProperty(nameof(Score));
				RaiseProperty(nameof(UpVote));
			}
		}

		public bool DownVote {
			get => downVote;
			set {
				downVote = value;
				DownVoteIcon.Foreground = downVote ? new SolidColorBrush((Color)Resources["SystemAccentColor"]) : (Brush)Resources["ButtonForegroundThemeBrush"];
				RaiseProperty(nameof(Score));
				RaiseProperty(nameof(DownVote));
			}
		}

		public CommentView(E621Comment comment) {
			this.InitializeComponent();
			this.Comment = comment;
			this.DataContextChanged += (s, c) => Bindings.Update();
			if(Voted.ContainsKey(comment.id)) {
				bool? updown = Voted[comment.id];
				if(updown == true) {
					UpVote = true;
				} else if(updown == false) {
					DownVote = true;
				}
			}
		}

		public async Task LoadAvatar() {
			Cts = new CancellationTokenSource();
			AvatorLoadingRing.IsActive = true;
			try {
				User = await E621User.GetAsync(Comment.creator_id, Cts.Token);
			} catch {
				return;
			}
			string url = "";
			if(User != null && Cts != null) {
				try {
					url = await E621User.GetAvatarURLAsync(User, Cts.Token);
				} catch {
					return;
				}
			}
			BitmapImage bi;
			if(!string.IsNullOrEmpty(url)) {
				try {
					HttpResult<InMemoryRandomAccessStream> result = await Data.ReadImageStreamAsync(url, Cts.Token);
					if(result.Result == HttpResultType.Success) {
						bi = new BitmapImage();
						await bi.SetSourceAsync(result.Content);
						ToolTipService.SetToolTip(Avatar, "Post".Language() + $": {User.avatar_id}");
						Avatar.Tapped += Avatar_Tapped;
					} else {
						bi = App.DefaultAvatar;
						ToolTipService.SetToolTip(Avatar, "Avatar Load Fail".Language() + $"\n{result.Helper}");
					}
				} catch {
					return;
				}
			} else {
				bi = App.DefaultAvatar;
				ToolTipService.SetToolTip(Avatar, "No Avatar".Language());
			}
			AvatorLoadingRing.IsActive = false;
			Avatar.Source = bi;
		}

		public void EnableLoadingRing() {
			AvatorLoadingRing.IsActive = true;
		}

		private async void Avatar_Tapped(object sender, TappedRoutedEventArgs e) {
			if(User == null || User.avatar_id == null) {
				return;
			}
			MainPage.CreateInstantDialog("Please Wait".Language() + "...", "Loading Post".Language() + $": {User.avatar_id}");
			Post post = await Post.GetPostByIDAsync(Cts.Token, User.avatar_id);
			MainPage.HideInstantDialog();
			MainPage.NavigateToPicturePage(post, Array.Empty<string>());
		}

		private async void UpVoteButton_Tapped(object sender, TappedRoutedEventArgs e) {
			if(Cts == null || Cts.IsCancellationRequested) {
				return;
			}
			UpVoteButton.IsEnabled = false;
			DownVoteButton.IsEnabled = false;
			UpVoteIcon.Glyph = "\uE10C";
			if(UpVote) {
				DataResult<E621Vote> result = await E621Vote.VoteComment(Comment.id, 1, false, Cts.Token);
				if(result.ResultType == HttpResultType.Success) {
					UpVote = false;
				}
			} else {
				DataResult<E621Vote> result = await E621Vote.VoteComment(Comment.id, 1, true, Cts.Token);
				if(result.ResultType == HttpResultType.Success) {
					UpVote = true;
					DownVote = false;
				}
			}
			UpVoteButton.IsEnabled = true;
			DownVoteButton.IsEnabled = true;
			UpVoteIcon.Glyph = "\uE96D";
			UpdateVoted();
		}

		private async void DownVoteButton_Tapped(object sender, TappedRoutedEventArgs e) {
			if(Cts == null || Cts.IsCancellationRequested) {
				return;
			}
			UpVoteButton.IsEnabled = false;
			DownVoteButton.IsEnabled = false;
			DownVoteIcon.Glyph = "\uE10C";
			if(DownVote) {//cancel downvote
				DataResult<E621Vote> result = await E621Vote.VoteComment(Comment.id, -1, false, Cts.Token);
				if(result.ResultType == HttpResultType.Success) {
					DownVote = false;
				}
			} else {//do downvote
				DataResult<E621Vote> result = await E621Vote.VoteComment(Comment.id, -1, true, Cts.Token);
				if(result.ResultType == HttpResultType.Success) {
					DownVote = true;
					UpVote = false;
				}
			}
			UpVoteButton.IsEnabled = true;
			DownVoteButton.IsEnabled = true;
			DownVoteIcon.Glyph = "\uE96E";
			UpdateVoted();
		}

		private void UpdateVoted() {
			bool? updown;
			if(UpVote) {
				updown = true;
			} else if(DownVote) {
				updown = false;
			} else {
				updown = null;
			}
			if(Voted.ContainsKey(Comment.id)) {
				Voted[Comment.id] = updown;
			} else {
				Voted.Add(Comment.id, updown);
			}
		}

		private void ReplyButton_Tapped(object sender, TappedRoutedEventArgs e) {

		}

		private void ReportButton_Tapped(object sender, TappedRoutedEventArgs e) {

		}
	}
}
