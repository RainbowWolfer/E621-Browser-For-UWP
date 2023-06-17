using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Windows.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using YiffBrowser.Helpers;
using YiffBrowser.Models.E621;
using YiffBrowser.Services.Locals;
using YiffBrowser.Views.Controls.PostsView;

namespace YiffBrowser.Views.Controls {
	public sealed partial class PostsInfoListView : UserControl {



		public PostsInfoViewParameters Parameters {
			get => (PostsInfoViewParameters)GetValue(ParametersProperty);
			set => SetValue(ParametersProperty, value);
		}

		public static readonly DependencyProperty ParametersProperty = DependencyProperty.Register(
			nameof(Parameters),
			typeof(PostsInfoViewParameters),
			typeof(PostsInfoListView),
			new PropertyMetadata(null, OnParametersChanged)
		);

		private static void OnParametersChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
			if (d is not PostsInfoListView view) {
				return;
			}

			if (e.NewValue is PostsInfoViewParameters parameters) {
				view.UpdatePostsInfo(parameters);
			} else {
				view.Clear();
			}
			view.MyPostsInfoListView.SelectedIndex = 0;
		}

		private void Clear() {
			List.Clear();
			List.Add(new PostsInfoList("Hot Tags (Top 20)", new List<PostInfoLine>()));
		}

		private void UpdatePostsInfo(PostsInfoViewParameters parameters) {
			List.Clear();

			Dictionary<string, int> tags = CountTags(parameters.Blocks).Where(x => Local.Listing.ContainBlocks(x.Key)).ToDictionary(x => x.Key, x => x.Value);
			List<PostInfoLine> list = new();
			foreach (KeyValuePair<string, int> item in tags) {
				PostInfoLine blockLine = new(item.Key, $"{item.Value}") {
					ShowBlockView = true,
				};
				blockLine.OnBlockView += async tag => {
					E621Post[] posts = parameters.Blocks.Where(x => x.Tags.GetAllTags().Contains(tag)).ToArray();
					await PostsBriefDisplayView.ShowAsDialog(posts, $"Block posts of tag : {tag}");
				};
				list.Add(blockLine);
			}
			List.Add(new PostsInfoList("Blacklist", list));


			AddToList("Hot Tags (Top 20)", parameters.Posts, 20);
		}

		private void AddToList(string name, IEnumerable<E621Post> posts, int limit = int.MaxValue) {
			Dictionary<string, int> tags = CountTags(posts);
			List<PostInfoLine> list = new();

			int count = 0;
			foreach (KeyValuePair<string, int> item in tags) {
				list.Add(new PostInfoLine(item.Key, $"{item.Value}"));
				if (count++ > limit) {
					break;
				}
			}
			List.Add(new PostsInfoList(name, list));
		}

		private static Dictionary<string, int> CountTags(IEnumerable<E621Post> posts) {
			Dictionary<string, int> allTags = new();

			foreach (E621Post item in posts) {
				foreach (string tag in item.Tags.GetAllTags()) {
					if (allTags.ContainsKey(tag)) {
						allTags[tag]++;
					} else {
						allTags.Add(tag, 1);
					}
				}
			}

			Dictionary<string, int> tags = allTags.OrderByDescending(o => o.Value).ToDictionary(x => x.Key, x => x.Value);

			return tags;
		}


		public ObservableCollection<PostsInfoList> List { get; } = new();



		public PostsInfoListView() {
			this.InitializeComponent();
		}

		private async void ManageButton_Click(object sender, RoutedEventArgs e) {
			await ListingsManager.ShowAsDialog(false);
			//call refresh
		}
	}

	public class PostInfoLine : BindableBase {
		public event Action<string> OnBlockView;

		private string name;
		private string detail;
		private bool showBlockView;

		public string Name {
			get => name;
			set => SetProperty(ref name, value);
		}

		public string Detail {
			get => detail;
			set => SetProperty(ref detail, value);
		}

		public bool ShowBlockView {
			get => showBlockView;
			set => SetProperty(ref showBlockView, value);
		}

		public ICommand BlockViewCommand => new DelegateCommand(BlockView);

		private void BlockView() {
			OnBlockView?.Invoke(name);
		}

		public ICommand CopyCommand => new DelegateCommand(Copy);

		private void Copy() {
			Name.CopyToClipboard();
		}

		public PostInfoLine(string name, string detail) {
			Name = name;
			Detail = detail;
		}

		public override string ToString() {
			return $"Name ({Name}) - Detail ({Detail})";
		}
	}

	public class PostsInfoList : ObservableCollection<PostInfoLine> {
		public string Key { get; set; }
		public PostsInfoList(string key) : base() {
			this.Key = key;
		}
		public PostsInfoList(string key, List<PostInfoLine> content) : base() {
			this.Key = key;
			content.ForEach(s => this.Add(s));
		}
	}

	public record PostsInfoViewParameters(E621Post[] Posts, E621Post[] Blocks);
}
