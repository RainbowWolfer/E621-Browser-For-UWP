using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.ApplicationModel.DataTransfer;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using YiffBrowser.Helpers;
using YiffBrowser.Models.E621;
using YiffBrowser.Services.Locals;
using YiffBrowser.Services.Networks;

namespace YiffBrowser.Views.Controls.Users {
	public sealed partial class LoginView : UserControl {
		public ContentDialog Dialog { get; set; }

		public LoginView() {
			this.InitializeComponent();
			ViewModel.RegainFocus += ViewModel_RegainFocus;
		}

		private void ViewModel_RegainFocus() {
			UsernameText.Focus(FocusState.Programmatic);
		}

		private void UserControl_Loaded(object sender, RoutedEventArgs e) {
			UsernameText.Focus(FocusState.Programmatic);
		}

		private void UsernameTextEnter_Invoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args) {
			ApiText.Focus(FocusState.Programmatic);
		}

		private void ApiTextEnter_Invoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args) {
			//Dialog.
		}

		public async void Submit() {
			Dialog.IsPrimaryButtonEnabled = false;
			bool success = await ViewModel.Submit();
			Dialog.IsPrimaryButtonEnabled = true;
			if (success) {
				Dialog.Hide();
			}
		}

		public (E621User user, E621Post avatarPost) GetUserResult() => ViewModel.Result;

		public bool IsLoading => ViewModel.IsLoading;

	}

	public class LoginViewModel : BindableBase {
		public event Action<bool> IsLoadingChanged;
		public event Action RegainFocus;

		private string userName;
		private string api;
		private bool isLoading;
		private string errorHint;

		public string UserName {
			get => userName;
			set => SetProperty(ref userName, value, OnInputChanged);
		}

		public string API {
			get => api;
			set => SetProperty(ref api, value, OnInputChanged);
		}

		public bool IsLoading {
			get => isLoading;
			set => SetProperty(ref isLoading, value, () => IsLoadingChanged?.Invoke(IsLoading));
		}

		public string ErrorHint {
			get => errorHint;
			set => SetProperty(ref errorHint, value);
		}

		public (E621User, E621Post) Result { get; private set; } = (null, null);

		private void OnInputChanged() {
			ErrorHint = "";
		}


		public async Task<bool> Submit() {
			if (UserName.IsBlank() || API.IsBlank()) {
				RegainFocus?.Invoke();
				return false;
			}

			ErrorHint = "";
			IsLoading = true;
			bool success = false;

			HttpResult<string> result = await NetCode.ReadURLAsync($"https://{E621API.GetHost()}/favorites.json", null, UserName, API);

			if (result.Result == HttpResultType.Success) {
				success = true;
				Local.Settings.SetLocalUser(UserName, API);
			} else {
				Local.Settings.ClearLocalUser();
				ErrorHint = "Sign in failed";

				IsLoading = false;
				RegainFocus?.Invoke();
				return false;
			}

			E621User user = await E621API.GetUserAsync(UserName);
			E621Post avatarPost = await E621API.GetPostAsync(user.avatar_id);
			Result = (user, avatarPost);

			IsLoading = false;
			RegainFocus?.Invoke();
			return success;
		}

		public ICommand PasteCommand => new DelegateCommand(Paste);
		public ICommand HelpInBrowserCommand => new DelegateCommand<string>(HelpInBrowser);

		private void HelpInBrowser(string address) {
			address.OpenInBrowser();
		}

		private async void Paste() {
			DataPackageView dataPackageView = Clipboard.GetContent();
			if (dataPackageView.Contains(StandardDataFormats.Text)) {
				string text = await dataPackageView.GetTextAsync();
				if (text.IsNotBlank()) {
					API = text;
				}
			}
		}

	}
}
