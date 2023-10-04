using System;
using Windows.UI.Xaml.Markup;

namespace YiffBrowser.Extensions {
	[MarkupExtensionReturnType(ReturnType = typeof(Type))]
	public sealed class TypeExtension : MarkupExtension {
		public Type Type { get; set; }

		protected override object ProvideValue() => Type;
	}
}
