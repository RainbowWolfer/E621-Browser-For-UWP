using Newtonsoft.Json.Linq;
using Prism.Mvvm;
using System;
using System.Linq;
using System.Numerics;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using YiffBrowser.Models.E621;

namespace YiffBrowser.Views.Controls.Common {
	public sealed partial class TypeHintView : UserControl {

		public E621Post Post {
			get => (E621Post)GetValue(PostProperty);
			set => SetValue(PostProperty, value);
		}

		public static readonly DependencyProperty PostProperty = DependencyProperty.Register(
			nameof(Post),
			typeof(E621Post),
			typeof(TypeHintView),
			new PropertyMetadata(null, OnPostChanged)
		);

		private static void OnPostChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
			if (d is not TypeHintView view) {
				return;
			}
			E621Post post = (E621Post)e.NewValue;
			if (new string[] { "gif", "webm", "swf" }.Contains(post.File.Ext.ToLower())) {
				string value = post.File.Ext.ToUpper();
				view.ViewModel.TypeHint = value;
			} else {
				view.ViewModel.TypeHint = string.Empty;
			}
		}

		public TypeHintView() {
			this.InitializeComponent();
			TypeHintBorder.Translation += new Vector3(0, 0, 32);
		}
	}

	internal class TypeHintViewModel : BindableBase {
		private string typeHint;

		public string TypeHint {
			get => typeHint;
			set => SetProperty(ref typeHint, value);
		}
	}

}
