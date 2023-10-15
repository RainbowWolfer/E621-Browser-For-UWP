using Prism.Mvvm;
using System.Windows.Input;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace YiffBrowser.Views.Controls.Common {
	public sealed partial class PostDetailControlView : UserControl {
		public event TypedEventHandler<PostDetailControlView, RoutedEventArgs> BackClick;

		public PostDetailControlViewModel ViewModel {
			get => (PostDetailControlViewModel)GetValue(ViewModelProperty);
			set => SetValue(ViewModelProperty, value);
		}

		public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(
			nameof(ViewModel),
			typeof(PostDetailControlViewModel),
			typeof(PostDetailControlView),
			new PropertyMetadata(new PostDetailControlViewModel())
		);

		public PostDetailControlView() {
			this.InitializeComponent();
		}

		private void BackButton_Click(object sender, RoutedEventArgs e) {
			BackClick?.Invoke(this, e);
		}
	}

	public class PostDetailControlViewModel : BindableBase {
		public event TypedEventHandler<PostDetailControlViewModel, bool> ShowImageListChanged;

		private ICommand backCommand;
		private ICommand goPreviousCommand;
		private ICommand goNextCommand;
		private bool showImageList;
		private bool isImageListLocked;

		public ICommand BackCommand {
			get => backCommand;
			set => SetProperty(ref backCommand, value);
		}

		public ICommand GoPreviousCommand {
			get => goPreviousCommand;
			set => SetProperty(ref goPreviousCommand, value);
		}

		public ICommand GoNextCommand {
			get => goNextCommand;
			set => SetProperty(ref goNextCommand, value);
		}

		public bool ShowImageList {
			get => showImageList;
			set => SetProperty(ref showImageList, value, () => {
				ShowImageListChanged?.Invoke(this, value);
			});
		}

		public bool IsImageListLocked {
			get => isImageListLocked;
			set => SetProperty(ref isImageListLocked, value);
		}

	}
}
