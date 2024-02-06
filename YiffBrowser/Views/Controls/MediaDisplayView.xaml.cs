using Prism.Mvvm;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.Devices.Input;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.Storage;
using Windows.UI.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using YiffBrowser.Helpers;
using YiffBrowser.Services.Locals;
using YiffBrowser.Views.Controls.Common;

namespace YiffBrowser.Views.Controls {
	public sealed partial class MediaDisplayView : UserControl {

		private bool showingControl = true;
		private MediaSource mediaSource = null;




		public SimpleMediaControl SimpleMediaControl {
			get => (SimpleMediaControl)GetValue(SimpleMediaControlProperty);
			set => SetValue(SimpleMediaControlProperty, value);
		}

		public static readonly DependencyProperty SimpleMediaControlProperty = DependencyProperty.Register(
			nameof(SimpleMediaControl),
			typeof(SimpleMediaControl),
			typeof(MediaDisplayView),
			new PropertyMetadata(null, OnSimpleMediaControlChanged)
		);

		private static void OnSimpleMediaControlChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
			((MediaDisplayView)d).UpdateMediaControl();
		}



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
			((MediaDisplayView)d).UpdateURL((string)e.NewValue);
		}

		private void UpdateURL(string url) {
			MediaSource = null;
			if (!url.IsBlank()) {
				MediaSource = MediaSource.CreateFromUri(new Uri(url));
			}
		}

		public IStorageFile File {
			get => (IStorageFile)GetValue(FileProperty);
			set => SetValue(FileProperty, value);
		}

		public static readonly DependencyProperty FileProperty = DependencyProperty.Register(
			nameof(File),
			typeof(IStorageFile),
			typeof(MediaDisplayView),
			new PropertyMetadata(null, OnFileChanged)
		);

		private static void OnFileChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
			((MediaDisplayView)d).UpdateFile((IStorageFile)e.NewValue);
		}

		private void UpdateFile(IStorageFile file) {
			MediaSource = null;
			if (file != null) {
				MediaSource = MediaSource.CreateFromStorageFile(file);
			}
		}

		public MediaSource MediaSource {
			get => mediaSource;
			set {
				if (mediaSource != null) {
					mediaSource.OpenOperationCompleted -= MediaSource_OpenOperationCompleted;
					mediaSource.StateChanged -= MediaSource_StateChanged;
				}
				mediaSource = value;
				if (mediaSource != null) {
					mediaSource.OpenOperationCompleted += MediaSource_OpenOperationCompleted;
					mediaSource.StateChanged += MediaSource_StateChanged;
				}
				MediaPlayer.Source = value;
			}
		}

		public MediaPlayerElement GetMediaPlayer() => MediaPlayer;

		public LocalSettings Settings { get; } = Local.Settings;

		public MediaDisplayView() {
			this.InitializeComponent();
			MediaPlayer.MediaPlayer.IsLoopingEnabled = Settings.AutoLooping;
			MediaPlayer.MediaPlayer.MediaOpened += MediaPlayer_MediaOpened;
			MediaPlayer.MediaPlayer.MediaFailed += MediaPlayer_MediaFailed;

			MediaPlayer.MediaPlayer.PlaybackSession.BufferingStarted += PlaybackSession_BufferingStarted;
			MediaPlayer.MediaPlayer.PlaybackSession.BufferingProgressChanged += PlaybackSession_BufferingProgressChanged;
			MediaPlayer.MediaPlayer.PlaybackSession.BufferingEnded += PlaybackSession_BufferingEnded;

			Settings.MediaControlTypeChanged += Settings_MediaControlTypeChanged;
		}

		private void PlaybackSession_BufferingStarted(MediaPlaybackSession sender, object args) {
			//BufferingGrid.Visibility = Visibility.Visible;
		}

		private void PlaybackSession_BufferingEnded(MediaPlaybackSession sender, object args) {
			//BufferingGrid.Visibility = Visibility.Collapsed;
		}

		private void PlaybackSession_BufferingProgressChanged(MediaPlaybackSession sender, object args) {

		}

		private void Settings_MediaControlTypeChanged(LocalSettings sender, MediaControlType args) {
			UpdateMediaControl(args);
		}

		private void UpdateMediaControl(MediaControlType? type = null) {
			if (SimpleMediaControl == null) {
				return;
			}
			type ??= Settings.MediaControlType;
			switch (type) {
				case MediaControlType.Full:
					MediaPlayer.AreTransportControlsEnabled = true;
					MediaControl.IsCompact = false;
					SimpleMediaControl.Visibility = Visibility.Collapsed;
					break;
				case MediaControlType.Compact:
					MediaPlayer.AreTransportControlsEnabled = true;
					MediaControl.IsCompact = true;
					SimpleMediaControl.Visibility = Visibility.Collapsed;
					break;
				case MediaControlType.Simple:
					MediaPlayer.AreTransportControlsEnabled = false;
					SimpleMediaControl.Visibility = Visibility.Visible;
					break;
				default:
					throw new NotImplementedException();
			}
		}

		private async void MediaSource_StateChanged(MediaSource sender, MediaSourceStateChangedEventArgs args) {
			await Task.Delay(500);
			//await Dispatcher.RunIdleAsync(e => {
			//	ShowingControl = false;
			//});
		}

		private void MediaSource_OpenOperationCompleted(MediaSource sender, MediaSourceOpenOperationCompletedEventArgs args) {
			//ShowingControl = false;
		}

		private async void MediaPlayer_MediaFailed(MediaPlayer sender, MediaPlayerFailedEventArgs args) {
			Debug.WriteLine($"{args.Error} - {args.ErrorMessage}");
			await Dispatcher.RunIdleAsync(e => {
				ShowingControl = false;
				ErrorTip.IsOpen = true;
				ErrorTip.Subtitle = args.ErrorMessage;
			});
		}

		private async void MediaPlayer_MediaOpened(MediaPlayer sender, object args) {
			await Dispatcher.RunIdleAsync(e => {
				ShowingControl = false;
			});
		}

		public void GetAudio() {

		}

		public void Initialize() {
			UpdateMediaControl();
			Play();
			ShowingControl = false;
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
			Step(-0.5);
		}

		private void Right_Invoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args) {
			Step(0.5);
		}

		private void Step(double seconds) {
			TimeSpan currentPosition = MediaPlayer.MediaPlayer.PlaybackSession.Position;
			TimeSpan newPosition = currentPosition.Add(TimeSpan.FromSeconds(seconds));
			MediaPlayer.MediaPlayer.PlaybackSession.Position = newPosition;
		}

		private void CtrlLeft_Invoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args) {
			ShowingControl = !ShowingControl;
		}

		private void CtrlRight_Invoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args) {
			ShowingControl = !ShowingControl;
		}

		private void Root_Loaded(object sender, RoutedEventArgs e) {
			UpdateMediaControl();
		}

		private void Root_Unloaded(object sender, RoutedEventArgs e) {

		}

		public bool ShowingControl {
			get => showingControl;
			set {
				showingControl = value;
				if (value) {
					MediaControl.Show();
				} else {
					MediaControl.Hide();
				}
			}
		}

		private void MediaPlayer_PointerPressed(object sender, PointerRoutedEventArgs e) {
			if (e.Pointer.PointerDeviceType == PointerDeviceType.Mouse) {
				PointerPointProperties properties = e.GetCurrentPoint(this).Properties;
				if (!properties.IsLeftButtonPressed) {
					// Left button pressed
					return;
				}
			}

			if (Settings.MediaControlType == MediaControlType.Simple) {
				SimpleMediaControl?.Focus(FocusState.Programmatic);
			} else {
				MediaControl.Focus(FocusState.Programmatic);
			}

			ShowingControl = !ShowingControl;
			e.Handled = true;
		}

	}


	//public class CustomMediaTransportControls : MediaTransportControls {
	//	protected override void OnApplyTemplate() {
	//		AppBarButton CastButton = GetTemplateChild("CastButton") as AppBarButton;

	//		CommandBar MediaControlsCommandBar = GetTemplateChild("MediaControlsCommandBar") as CommandBar;
	//		MediaControlsCommandBar.PrimaryCommands.Remove(CastButton);
	//		base.OnApplyTemplate();
	//	}
	//}
}
