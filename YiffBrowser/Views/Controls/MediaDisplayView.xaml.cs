using Prism.Mvvm;
using System;
using System.Diagnostics;
using System.Windows.Input;
using Windows.Devices.Input;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.UI.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using YiffBrowser.Helpers;
using YiffBrowser.Services.Locals;

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
		private bool showingControl = true;

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
			//MediaPlayer.MediaPlayer.StepBackwardOneFrame();
			ShowingControl = !ShowingControl;
		}

		private void CtrlRight_Invoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args) {
			//MediaPlayer.MediaPlayer.StepForwardOneFrame();
			ShowingControl = !ShowingControl;
		}

		private void Root_Loaded(object sender, RoutedEventArgs e) {
			MediaPlayer.AreTransportControlsEnabled = true;
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
			//MediaControl.Focus(FocusState.Keyboard);
			ShowingControl = !ShowingControl;
			e.Handled = true;
		}

		private void MediaControl_Loaded(object sender, RoutedEventArgs e) {
			ShowingControl = true;
		}

	}

	public class MediaDisplayViewModel : BindableBase {
		private MediaSource mediaSource;

		public MediaSource MediaSource {
			get => mediaSource;
			set => SetProperty(ref mediaSource, value);
		}

		public LocalSettings Settings { get; } = Local.Settings;

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


	//public class CustomMediaTransportControls : MediaTransportControls {
	//	protected override void OnApplyTemplate() {
	//		AppBarButton CastButton = GetTemplateChild("CastButton") as AppBarButton;

	//		CommandBar MediaControlsCommandBar = GetTemplateChild("MediaControlsCommandBar") as CommandBar;
	//		MediaControlsCommandBar.PrimaryCommands.Remove(CastButton);
	//		base.OnApplyTemplate();
	//	}
	//}
}
