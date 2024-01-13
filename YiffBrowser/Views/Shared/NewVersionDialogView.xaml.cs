using Prism.Mvvm;
using Windows.UI.Xaml.Controls;

namespace YiffBrowser.Views.Shared {
	public sealed partial class NewVersionDialogView : UserControl {
		public NewVersionDialogView() {
			this.InitializeComponent();
		}
	}


	internal class NewVersionDialogViewModel : BindableBase {
		private int selectedIndex = 0;

		public int SelectedIndex {
			get => selectedIndex;
			set => SetProperty(ref selectedIndex, value);
		}


	}

}
