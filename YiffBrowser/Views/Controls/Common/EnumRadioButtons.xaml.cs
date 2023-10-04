using Prism.Commands;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using YiffBrowser.Attributes;
using YiffBrowser.Helpers;

namespace YiffBrowser.Views.Controls.Common {
	public sealed partial class EnumRadioButtons : UserControl {

		public Type EnumType {
			get => (Type)GetValue(EnumTypeProperty);
			set => SetValue(EnumTypeProperty, value);
		}

		public object SelectedEnum {
			get => GetValue(SelectedEnumProperty);
			set => SetValue(SelectedEnumProperty, value);
		}

		public Orientation Orientation {
			get => (Orientation)GetValue(OrientationProperty);
			set => SetValue(OrientationProperty, value);
		}

		public double ItemWidth {
			get => (double)GetValue(ItemWidthProperty);
			set => SetValue(ItemWidthProperty, value);
		}


		public static readonly DependencyProperty SelectedEnumProperty = DependencyProperty.Register(
			nameof(SelectedEnum),
			typeof(object),
			typeof(EnumRadioButtons),
			new PropertyMetadata(null, OnSelectedEnumChanged)
		);

		public static readonly DependencyProperty EnumTypeProperty = DependencyProperty.Register(
			nameof(EnumType),
			typeof(Type),
			typeof(EnumRadioButtons),
			new PropertyMetadata(null, OnEnumTypeChanged)
		);

		public static readonly DependencyProperty OrientationProperty = DependencyProperty.Register(
			nameof(Orientation),
			typeof(Orientation),
			typeof(EnumRadioButtons),
			new PropertyMetadata(Orientation.Vertical)
		);

		public static readonly DependencyProperty ItemWidthProperty = DependencyProperty.Register(
			nameof(ItemWidth),
			typeof(double),
			typeof(EnumRadioButtons),
			new PropertyMetadata(double.NaN, OnItemWidthChanged)
		);

		private static void OnItemWidthChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
			if (d is EnumRadioButtons view) {
				view.UpdateItemMinWidth();
			}
		}

		private static void OnSelectedEnumChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
			if (d is EnumRadioButtons view) {
				view.UpdateSelection();
			}
		}

		private static void OnEnumTypeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
			if (d is EnumRadioButtons view) {
				view.RefreshItems();
			}
		}

		private void RefreshItems() {
			if (EnumType == null || !EnumType.IsEnum) {
				return;
			}
			Array values = Enum.GetValues(EnumType);
			List<EnumItem> items = new();
			foreach (object item in values) {
				string description = item.ToString();
				string toolTip = null;

				MemberInfo[] members = EnumType.GetMember(item.ToString());

				if (members.TryGetAttribute(out DescriptionAttribute descriptionAttribute)) {
					description = descriptionAttribute.Description;
				}

				if (members.TryGetAttribute(out ToolTipAttribute toolTipAttribute)) {
					toolTip = toolTipAttribute.ToolTip;
				}

				items.Add(new EnumItem(item.ToString(), item, (int)item, description, toolTip));
			}

			RootPanel.Children.Clear();

			foreach (EnumItem item in items) {
				RadioButton radioButton = new() {
					Content = item.Description,
					Tag = item,
					Command = new DelegateCommand(() => {
						object value = Enum.Parse(EnumType, item.Value.ToString());
						SelectedEnum = value;
					}),
				};
				if (item.ToolTip.IsNotBlank()) {
					ToolTipService.SetToolTip(radioButton, item.ToolTip);
				}
				RootPanel.Children.Add(radioButton);
			}

			UpdateItemMinWidth();

			UpdateSelection();
		}

		private void UpdateSelection() {
			if (SelectedEnum is Enum e) {
				int value = (int)(object)e;
				foreach (RadioButton item in RootPanel.Children.OfType<RadioButton>()) {
					if (item.Tag is EnumItem enumItem && enumItem.Value == value) {
						item.IsChecked = true;
						break;
					}
				}
			}
		}

		private void UpdateItemMinWidth() {
			foreach (RadioButton radioButton in RootPanel.Children.OfType<RadioButton>()) {
				if (double.IsNaN(ItemWidth) || ItemWidth <= 0) {
					radioButton.MinWidth = 0;
				} else {
					radioButton.MinWidth = ItemWidth;
				}

			}
		}


		public EnumRadioButtons() {
			InitializeComponent();
			RootPanel.Children.Clear();
		}


		private class EnumItem(string name, object @enum, int value, string description = null, string toolTip = null) {
			public string Name { get; set; } = name;
			public string Description { get; set; } = description;
			public string ToolTip { get; set; } = toolTip;
			public object Enum { get; set; } = @enum;
			public int Value { get; set; } = value;
		}

	}
}
