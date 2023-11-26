using System;

namespace YiffBrowser.Attributes {
	[AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
	internal sealed class ToolTipAttribute(string toolTip) : Attribute {
		public string ToolTip { get; } = toolTip;
	}
}
