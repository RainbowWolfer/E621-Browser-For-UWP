using ColorCode.UWP.Common;
using Prism.Mvvm;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using YiffBrowser.Helpers;
using YiffBrowser.Models.E621;
using YiffBrowser.Services.Networks;

namespace YiffBrowser.Views.Controls.TagsInfoViews {
	public sealed partial class TagSimpleInfoPopupView : UserControl {

		public string TagName {
			get => (string)GetValue(TagNameProperty);
			set => SetValue(TagNameProperty, value);
		}

		public static readonly DependencyProperty TagNameProperty = DependencyProperty.Register(
			nameof(TagName),
			typeof(string),
			typeof(TagSimpleInfoPopupView),
			new PropertyMetadata(null, OnTagNameChanged)
		);

		private static async void OnTagNameChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
			if (d is not TagSimpleInfoPopupView view) {
				return;
			}
			await view.ViewModel.Load((string)e.NewValue);
		}

		public TagSimpleInfoPopupView() {
			this.InitializeComponent();
		}
	}

	internal class TagSimpleInfoPopupViewModel : BindableBase {
		private bool isLoading = false;
		private E621Tag tag = null;
		private E621Wiki wiki = null;
		private Brush categoryBrush = "#6f6f6f".GetSolidColorBrush();
		private string tagName;
		private string body;

		public E621Tag Tag {
			get => tag;
			set => SetProperty(ref tag, value);
		}

		public E621Wiki Wiki {
			get => wiki;
			set => SetProperty(ref wiki, value);
		}

		public bool IsLoading {
			get => isLoading;
			set => SetProperty(ref isLoading, value);
		}

		public Brush CategoryBrush {
			get => categoryBrush;
			set => SetProperty(ref categoryBrush, value);
		}

		public string TagName {
			get => tagName;
			set => SetProperty(ref tagName, value);
		}

		public string Body {
			get => body;
			set => SetProperty(ref body, value);
		}

		public async Task Load(string tagName) {
			TagName = tagName;
			IsLoading = true;
			Tag = await E621API.GetE621TagAsync(tagName);
			CategoryBrush = new SolidColorBrush(E621Tag.GetCategoryColor(Tag.Category));
			Wiki = await E621API.GetE621WikiAsync(tagName);
			Body = Wiki?.Body.NotBlankCheck() ?? "No Wiki Found";
			IsLoading = false;
		}
	}
}
