using System;
using System.Diagnostics;
using Windows.Media.Playback;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;

namespace YiffBrowser.Views.Controls.Common {
	public sealed partial class SimpleMediaControl : UserControl {


		public MediaDisplayView MediaDisplayView {
			get => (MediaDisplayView)GetValue(MediaDisplayViewProperty);
			set => SetValue(MediaDisplayViewProperty, value);
		}

		public static readonly DependencyProperty MediaDisplayViewProperty = DependencyProperty.Register(
			nameof(MediaDisplayView),
			typeof(MediaDisplayView),
			typeof(SimpleMediaControl),
			new PropertyMetadata(null, OnMediaDisplayViewChanged)
		);
		private MediaPlayerElement player;
		private MediaPlaybackSession session;

		private static void OnMediaDisplayViewChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
			((SimpleMediaControl)d).Player = (e.NewValue as MediaDisplayView)?.GetMediaPlayer();
		}

		public MediaPlayerElement Player {
			get => player;
			private set {
				player = value;
				Session = value?.MediaPlayer.PlaybackSession;
			}
		}

		public MediaPlaybackSession Session {
			get => session;
			private set {
				if (session != null) {
					session.PositionChanged -= Session_PositionChanged;
					session.PlaybackStateChanged -= Session_PlaybackStateChanged;
				}
				session = value;
				if (session != null) {
					session.PositionChanged += Session_PositionChanged;
					session.PlaybackStateChanged += Session_PlaybackStateChanged;
					PositionSlider.ThumbToolTipValueConverter = new MediaTimeConverter(PositionSlider, value);
				}
			}
		}

		private async void Session_PlaybackStateChanged(MediaPlaybackSession sender, object args) {
			await Dispatcher.RunIdleAsync(e => {
				switch (sender.PlaybackState) {
					case MediaPlaybackState.Playing:
						PlayButton.Visibility = Visibility.Collapsed;
						PauseButton.Visibility = Visibility.Visible;
						PositionSlider.IsEnabled = true;
						break;
					case MediaPlaybackState.Paused:
						PlayButton.Visibility = Visibility.Visible;
						PauseButton.Visibility = Visibility.Collapsed;
						PositionSlider.IsEnabled = true;
						break;
					case MediaPlaybackState.None:
					case MediaPlaybackState.Opening:
					case MediaPlaybackState.Buffering:
					default:
						PlayButton.Visibility = Visibility.Collapsed;
						PauseButton.Visibility = Visibility.Collapsed;
						PositionSlider.IsEnabled = false;
						break;
				}
			});
		}

		private async void Session_PositionChanged(MediaPlaybackSession sender, object args) {
			await Dispatcher.RunIdleAsync(e => {
				double percentage = sender.Position / sender.NaturalDuration;
				if (!double.IsNaN(percentage)) {
					PositionSlider.Value = percentage * PositionSlider.Maximum;
				}
			});
		}

		public SimpleMediaControl() {
			this.InitializeComponent();
			PlayButton.Visibility = Visibility.Collapsed;
			PauseButton.Visibility = Visibility.Collapsed;
			PositionSlider.IsEnabled = false;
		}

		private void PositionSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e) {
			if (Session == null) {
				return;
			}

			TimeSpan duration = Session.NaturalDuration;
			double percentage = PositionSlider.Value / PositionSlider.Maximum;
			Session.Position = duration * percentage;
		}

		private void PlayButton_Click(object sender, RoutedEventArgs e) {
			Player.MediaPlayer.Play();
		}

		private void PauseButton_Click(object sender, RoutedEventArgs e) {
			Player.MediaPlayer.Pause();
		}

		public class MediaTimeConverter(Slider Slider, MediaPlaybackSession Session) : IValueConverter {

			public object Convert(object value, Type targetType, object parameter, string language) {
				if (value is double d) {
					double p = d / Slider.Maximum;
					TimeSpan t = Session.NaturalDuration * p;
					return t.ToString(@"hh\:mm\:ss");
				}
				return null;
			}

			public object ConvertBack(object value, Type targetType, object parameter, string language) {
				throw new NotSupportedException();
			}
		}
	}
}
