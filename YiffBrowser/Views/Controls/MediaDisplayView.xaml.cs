using Prism.Mvvm;
using System;
using System.Diagnostics;
using System.Windows.Input;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using YiffBrowser.Helpers;

namespace YiffBrowser.Views.Controls {
	public sealed partial class MediaDisplayView : UserControl {



		public ICommand MediaLoadedCommand {
			get => (ICommand)GetValue(MediaLoadedCommandProperty);
			set => SetValue(MediaLoadedCommandProperty, value);
		}

		public static readonly DependencyProperty MediaLoadedCommandProperty = DependencyProperty.Register(
			nameof(MediaLoadedCommand),
			typeof(ICommand),
			typeof(MediaDisplayView),
			new PropertyMetadata(null)
		);



		public string URL {
			get => (string)GetValue(URLProperty);
			set => SetValue(URLProperty, value);
		}

		public static readonly DependencyProperty URLProperty = DependencyProperty.Register(
			nameof(URL),
			typeof(string),
			typeof(MediaDisplayView),
			new PropertyMetadata(string.Empty, OnURLChanged)
		);

		private static void OnURLChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
			if (d is MediaDisplayView view) {
				view.ViewModel.Initialize((string)e.NewValue);
			}
		}

		public MediaDisplayView() {
			this.InitializeComponent();
			MediaPlayer.MediaPlayer.IsLoopingEnabled = true;
			MediaPlayer.MediaPlayer.MediaOpened += MediaPlayer_MediaOpened;
			MediaPlayer.MediaPlayer.MediaFailed += MediaPlayer_MediaFailed;
		}

		private void MediaPlayer_MediaFailed(MediaPlayer sender, MediaPlayerFailedEventArgs args) {

		}

		private void MediaPlayer_MediaOpened(MediaPlayer sender, object args) {

		}

		public void GetAudio() {

		}

		public void Play() {
			MediaPlayer.MediaPlayer.Play();
		}

		public void Pause() {
			MediaPlayer.MediaPlayer.Pause();
		}

		private void Space_Invoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args) {
			if (MediaPlayer.MediaPlayer.PlaybackSession.PlaybackState == MediaPlaybackState.Paused) {
				MediaPlayer.MediaPlayer.Play();
			} else if (MediaPlayer.MediaPlayer.PlaybackSession.PlaybackState == MediaPlaybackState.Playing) {
				MediaPlayer.MediaPlayer.Pause();
			} else {

			}
		}

		private void Left_Invoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args) {
			Step(-1);
		}

		private void Right_Invoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args) {
			Step(1);
		}

		private void Step(double seconds) {
			TimeSpan currentPosition = MediaPlayer.MediaPlayer.PlaybackSession.Position;
			TimeSpan newPosition = currentPosition.Add(TimeSpan.FromSeconds(seconds));
			MediaPlayer.MediaPlayer.PlaybackSession.Position = newPosition;
		}

		private void CtrlLeft_Invoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args) {
			MediaPlayer.MediaPlayer.StepBackwardOneFrame();
		}

		private void CtrlRight_Invoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args) {
			MediaPlayer.MediaPlayer.StepForwardOneFrame();
			//MediaControl.Show();
		}

		private void Root_Loaded(object sender, RoutedEventArgs e) {
			MediaControl.Hide();
			MediaPlayer.AreTransportControlsEnabled = true;
			//MediaPlayer.SetMediaPlayer(new MediaPlayer() { });
		}

	}

	public class MediaDisplayViewModel : BindableBase {
		private MediaSource mediaSource;

		public MediaSource MediaSource {
			get => mediaSource;
			set => SetProperty(ref mediaSource, value);
		}

		public void Initialize(string url) {
			MediaSource = null;
			if (!url.IsBlank()) {
				MediaSource = MediaSource.CreateFromUri(new Uri(url));

				MediaSource.OpenOperationCompleted += MediaSource_OpenOperationCompleted;
				MediaSource.StateChanged += MediaSource_StateChanged;

			}
		}

		private void MediaSource_StateChanged(MediaSource sender, MediaSourceStateChangedEventArgs args) {
			Debug.WriteLine(args.NewState);
		}

		private void MediaSource_OpenOperationCompleted(MediaSource sender, MediaSourceOpenOperationCompletedEventArgs args) {
			Debug.WriteLine(args.Error);
		}
	}
}
