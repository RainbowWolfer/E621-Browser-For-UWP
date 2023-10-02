using Prism.Commands;
using Prism.Mvvm;
using System.Diagnostics;
using System.Windows.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using YiffBrowser.Helpers;
using YiffBrowser.Services.Locals;
using YiffBrowser.Services.Networks;
using YiffBrowser.Views.Controls;

namespace YiffBrowser.Views.Pages.E621 {
	public sealed partial class TestPage : Page {
		public TestPage() {
			this.InitializeComponent();
		}

		private async void Button_Click(object sender, RoutedEventArgs e) {
			bool followsOrBlocks = true;
			ListingsManager view = new(followsOrBlocks);
			await view.CreateContentDialog(new ContentDialogParameters() {
				Title = followsOrBlocks ? "Follows" : "Blocks",
				CloseText = "Back",
			}).ShowAsyncSafe();

			Local.Listing.Follows = view.GetResult();

			//save to local
			Listing.Write();
		}


	}

	public class TestPageViewModel : BindableBase {
		private string url;

		public string URL {
			get => url;
			set => SetProperty(ref url, value);
		}

		private DelegateCommand testURLCommand;
		public ICommand TestURLCommand => testURLCommand ??= new DelegateCommand(TestURL);

		private async void TestURL() {
			HttpResult<string> content = await NetCode.ReadURLAsync(url);
			Debug.WriteLine(content.Content);
		}

	}
}
