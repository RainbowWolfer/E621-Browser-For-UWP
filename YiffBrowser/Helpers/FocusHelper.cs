using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace YiffBrowser.Helpers {
	public static class FocusHelper {
		public static bool AcceptsKeyboardInput(this FrameworkElement element) {
			// Check if the element is a TextBox, RichEditBox, PasswordBox, or AutoSuggestBox
			if (element is TextBox || element is RichEditBox || element is PasswordBox || element is AutoSuggestBox) {
				return true;
			}

			return false;

			//// Check if the element has an InputScopeNameValue
			//var inputScope = InputScope.GetInputScope(element);
			//if (inputScope != null && inputScope.Names.Count > 0) {
			//	return true;
			//}

			//// Check if the element is not read-only, enabled, tab stop, and content editable
			//return !element.IsReadOnly && element.IsEnabled && element.IsTabStop && element.IsContentEditable;
		}

		public static bool IsCurrentFocusOnTextBox() {
			if (FocusManager.GetFocusedElement() is FrameworkElement frameworkElement) {
				return frameworkElement.AcceptsKeyboardInput();
			}
			return false;
		}
	}
}
