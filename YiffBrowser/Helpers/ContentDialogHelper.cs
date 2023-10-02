using System;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using YiffBrowser.Interfaces;

namespace YiffBrowser.Helpers {
	public static class ContentDialogHelper {
		public static ContentDialog CreateContentDialog(this object target, ContentDialogParameters parameters) {
			if (parameters is null) {
				throw new ArgumentNullException(nameof(parameters));
			}

			ContentDialog dialog = new() {
				Style = App.DialogStyle,
				Content = target,
				Title = parameters.Title,
				PrimaryButtonText = parameters.PrimaryText,
				SecondaryButtonText = parameters.SecondaryText,
				CloseButtonText = parameters.CloseText,
				DefaultButton = parameters.DefaultButton
			};

			if (target is IContentDialogView view) {
				view.Dialog = dialog;
			}

			if (parameters.MaxWidth.HasValue) {
				dialog.Resources["ContentDialogMaxWidth"] = parameters.MaxWidth.Value;
			}

			return dialog;
		}

		public static async Task<ContentDialogResult> ShowAsyncSafe(this ContentDialog dialog) {
			return await dialog.ShowAsyncSafe(ContentDialogResult.None);
		}

		public static async Task<ContentDialogResult> ShowAsyncSafe(this ContentDialog dialog, ContentDialogResult fallback) {
			try {
				return await dialog.ShowAsync();
			} catch {
				return fallback;
			}
		}

	}


	public class ContentDialogParameters {
		public const double DEFAULT_MAX_WIDTH = 1050;
		public double? MaxWidth { get; set; } = null;

		public object Title { get; set; } = string.Empty;
		public string PrimaryText { get; set; } = string.Empty;
		public string SecondaryText { get; set; } = string.Empty;
		public string CloseText { get; set; } = string.Empty;

		public ContentDialogButton DefaultButton { get; set; } = ContentDialogButton.None;

	}


	public class LoadingDialogControl {

		public ContentDialog Dialog { get; private set; }

		public LoadingDialogControl() {
			
		}

		public void Set(object content, ContentDialogParameters parameters = null) {
			Dialog = content.CreateContentDialog(parameters ?? new ContentDialogParameters());
		}

		public async Task Start(Func<Task> task) {
			if (Dialog is null) {
				throw new ArgumentNullException(nameof(Dialog));
			}

			if (task is null) {
				throw new ArgumentNullException(nameof(task));
			}

			bool finished = false;
			Dialog.Closing += (s, e) => {
				if (!finished) {
					e.Cancel = true;
				}
			};

			ShowDialog();
			await task();
			finished = true;
			HideDialog();
		}

		private async void ShowDialog() {
			if (Dialog is null) {
				throw new ArgumentNullException(nameof(Dialog));
			}
			await Dialog.ShowAsyncSafe();
		}

		private void HideDialog() {
			if (Dialog is null) {
				throw new ArgumentNullException(nameof(Dialog));
			}
			Dialog.Hide();
			Dialog = null;
		}
	}
}
