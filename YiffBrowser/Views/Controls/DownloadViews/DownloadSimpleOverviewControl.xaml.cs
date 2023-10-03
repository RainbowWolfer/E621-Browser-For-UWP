using Prism.Mvvm;
using System;
using System.Linq;
using System.Windows.Input;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using YiffBrowser.Services.Downloads;

namespace YiffBrowser.Views.Controls.DownloadViews {
	public sealed partial class DownloadSimpleOverviewControl : UserControl {
		public event TypedEventHandler<DownloadSimpleOverviewControl, RoutedEventArgs> Click;

		public ICommand Command {
			get => (ICommand)GetValue(CommandProperty);
			set => SetValue(CommandProperty, value);
		}

		public static readonly DependencyProperty CommandProperty = DependencyProperty.Register(
			nameof(Command),
			typeof(ICommand),
			typeof(DownloadSimpleOverviewControl),
			new PropertyMetadata(null)
		);


		public DownloadSimpleOverviewControl() {
			this.InitializeComponent();
			PlayHide();
		}

		public void PlayShow() {
			ShowStoryboard.Begin();
		}

		public void PlayHide() {
			HideStoryboard.Begin();
		}

		private void MainButton_Click(object sender, RoutedEventArgs e) {
			Click?.Invoke(this, e);
			Command?.Execute(e);
		}
	}

	internal class DownloadSimpleOverviewControlViewModel : BindableBase {
		private bool hasDownloading = false;
		private string postID = null;
		private string postPreviewURL = null;
		private int completedCount = 0;
		private int allCount = 0;

		public bool HasDownloading {
			get => hasDownloading;
			set => SetProperty(ref hasDownloading, value);
		}

		public string PostID {
			get => postID;
			set => SetProperty(ref postID, value);
		}

		public string PostPreviewURL {
			get => postPreviewURL;
			set => SetProperty(ref postPreviewURL, value);
		}

		public int CompletedCount {
			get => completedCount;
			set => SetProperty(ref completedCount, value);
		}
		public int AllCount {
			get => allCount;
			set => SetProperty(ref allCount, value);
		}

		private DispatcherTimer LoopTimer { get; } = new DispatcherTimer();

		public DownloadSimpleOverviewControlViewModel() {
			LoopTimer.Interval = TimeSpan.FromMilliseconds(200);
			LoopTimer.Tick += LoopTimer_Tick;
			LoopTimer.Start();
		}

		private void LoopTimer_Tick(object sender, object e) {
			HasDownloading = DownloadManager.HasDownloading();
			DownloadInstance first = DownloadManager.downloadingPool.FirstOrDefault();
			if (first == null || !HasDownloading) {
				PostID = null;
				PostPreviewURL = null;
				AllCount = 0;
				CompletedCount = 0;
				return;
			}
			PostID = $"#{first.Post.ID}";
			PostPreviewURL = first.Post.Preview?.URL;
			AllCount = DownloadManager.downloadingPool.Count + DownloadManager.waitPool.Count + DownloadManager.completedPool.Count;
			CompletedCount = DownloadManager.completedPool.Count;
		}

	}
}
