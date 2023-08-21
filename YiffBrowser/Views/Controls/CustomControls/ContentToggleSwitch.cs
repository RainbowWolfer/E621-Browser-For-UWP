using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace YiffBrowser.Views.Controls.CustomControls {
	internal class ContentToggleSwitch : Control {


		public string SwitchContent {
			get => (string)GetValue(SwitchContentProperty);
			set => SetValue(SwitchContentProperty, value);
		}

		public static readonly DependencyProperty SwitchContentProperty = DependencyProperty.Register(
			nameof(SwitchContent),
			typeof(string),
			typeof(ContentToggleSwitch),
			new PropertyMetadata(string.Empty)
		);



		public bool IsOn {
			get => (bool)GetValue(IsOnProperty);
			set => SetValue(IsOnProperty, value);
		}

		public static readonly DependencyProperty IsOnProperty = DependencyProperty.Register(
			nameof(IsOn),
			typeof(bool),
			typeof(ContentToggleSwitch),
			new PropertyMetadata(false)
		);




	}
}
