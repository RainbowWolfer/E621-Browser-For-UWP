using E621Downloader.Models.Locals;
using E621Downloader.Models.Posts;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Shapes;

namespace E621Downloader.Views {
	public sealed partial class ListingToggleSplitButton: UserControl {
		public event Action<Dictionary<string, bool>> OnComboBoxChecked;
		public event Action OnToggled;
		public event Action OnSettingsClick;

		private readonly List<ComboBoxLine> lists = new();
		private List<SingleListing> listings;
		private SingleListing defaultListing;
		private string tag;

		private bool isChecked;

		public bool IsChecked {
			get => isChecked;
			set {
				isChecked = value;
				MainToggle.IsChecked = isChecked;
				SideToggle.IsChecked = isChecked;
				MainText.Text = isChecked ? OnText : OffText;
				MainIcon.Glyph = isChecked ? OnIcon : OffIcon;
			}
		}

		public string OnText { get; set; }
		public string OffText { get; set; }
		public string OnIcon { get; set; }
		public string OffIcon { get; set; }

		public ListingToggleSplitButton() {
			this.InitializeComponent();
		}

		public void Initialize(string tag, IEnumerable<SingleListing> listings) {
			this.tag = tag;
			this.listings = listings.ToList();
			defaultListing = this.listings.Find(x => x.IsDefault);
			foreach(SingleListing item in listings) {
				lists.Add(new ComboBoxLine(item.Name, item.Tags.Contains(tag), item.IsCloud, item.IsDefault));
			}
			UpdateIsChecked();
		}

		public void UncheckDefault() {
			if(!IsChecked) {
				return;
			}
			if(!defaultListing.Tags.Contains(tag)) {
				return;
			}
			defaultListing.Tags.Remove(tag);
			UpdateIsChecked();
			ComboBoxLine foundDefault = lists.Find(l => l.Name == defaultListing.Name);
			if(foundDefault != null) {
				foundDefault.IsChecked = IsChecked;
			}
		}

		private void UpdateIsChecked() {
			IsChecked = defaultListing.Tags.Contains(tag);
		}

		private void MainToggle_Click(object sender, RoutedEventArgs e) {
			if(IsChecked) {
				defaultListing.Tags.Remove(tag);
			} else {
				defaultListing.Tags.Add(tag);
			}
			UpdateIsChecked();
			ComboBoxLine foundDefault = lists.Find(l => l.Name == defaultListing.Name);
			if(foundDefault != null) {
				foundDefault.IsChecked = IsChecked;
			}
			OnToggled?.Invoke();
		}

		private void SideToggle_Click(object sender, RoutedEventArgs e) {
			StackPanel panel = new();
			Flyout flyout = new() {
				Placement = FlyoutPlacementMode.Bottom,
				Content = panel,
			};
			ComboBoxLine cloud = lists.Find(l => l.IsCloud);
			if(cloud != null) {
				StackPanel cloudLine = new() {
					Orientation = Orientation.Horizontal
				};
				var text = new TextBlock() {
					Text = cloud.Name,
					Margin = new Thickness(0, 0, 0, 0),
				};
				var icon = new FontIcon() {
					FontFamily = new FontFamily("Segoe Fluent Icons"),
					Glyph = "\uEBC3",
					Margin = new Thickness(10, 0, 0, 0)
				};
				cloudLine.Children.Add(text);
				cloudLine.Children.Add(icon);
				CheckBox box = new() {
					Content = cloudLine,
					IsChecked = cloud.IsChecked,
					Tag = cloud.Name,
				};
				box.Checked += Box_Checked;
				box.Unchecked += Box_Checked;
				panel.Children.Add(box);

				string tooltip;
				if(E621User.Current == null) {
					box.IsEnabled = false;
					tooltip = "Not Logged In";
				} else {
					tooltip = $"Using ({E621User.Current.name}) Account";
				}
				ToolTipService.SetToolTip(box, tooltip);

				panel.Children.Add(new Rectangle() {
					Width = 120,
					Height = 3,
					RadiusX = 2,
					RadiusY = 10,
					Margin = new Thickness(5),
					Fill = new SolidColorBrush(new Color() {
						A = 100,
						R = 100,
						G = 100,
						B = 100,
					}),
				});
			}
			foreach(ComboBoxLine item in lists.Where(l => !l.IsCloud).OrderByDescending(l => l.IsDefault)) {
				string name;
				if(item.IsDefault) {
					name = $"(Default) {item.Name}";
				} else {
					name = item.Name;
				}
				CheckBox box = new() {
					Content = name,
					IsChecked = item.IsChecked,
					Tag = item.Name,
				};
				box.Checked += Box_Checked;
				box.Unchecked += Box_Checked;
				panel.Children.Add(box);
			}
			StackPanel buttonPanel = new() {
				Orientation = Orientation.Horizontal,
			};
			var gotoSettingsIcon = new FontIcon {
				Glyph = "\uE115",
				Margin = new Thickness(0, 0, 10, 0),
				VerticalAlignment = VerticalAlignment.Center,
			};
			var gotoSettingsText = new TextBlock() {
				Text = "Settings",
				VerticalAlignment = VerticalAlignment.Center,
			};
			buttonPanel.Children.Add(gotoSettingsIcon);
			buttonPanel.Children.Add(gotoSettingsText);
			Button gotoSettingsButton = new() {
				HorizontalAlignment = HorizontalAlignment.Stretch,
				HorizontalContentAlignment = HorizontalAlignment.Center,
				Margin = new Thickness(5, 10, 5, 0),
				Content = buttonPanel,
			};
			gotoSettingsButton.Click += async (s, args) => {
				flyout.Hide();
				await Task.Delay(10);
				OnSettingsClick?.Invoke();
				await Task.Delay(10);
				MainPage.NavigateTo(PageTag.Settings);
			};
			panel.Children.Add(gotoSettingsButton);

			flyout.ShowAt(SideToggle);
		}

		private void Box_Checked(object sender, RoutedEventArgs e) {
			CheckBox box = (CheckBox)sender;
			bool boxIsChecked = box.IsChecked.Value;
			string boxTag = (string)box.Tag;
			foreach(ComboBoxLine item in lists) {
				if(item.Name == boxTag) {
					item.IsChecked = boxIsChecked;
					if(item.IsDefault) {
						if(!boxIsChecked) {
							defaultListing.Tags.Remove(this.tag);
						} else {
							defaultListing.Tags.Add(this.tag);
						}
						UpdateIsChecked();
					}
					break;
				}
			}
			var result = new Dictionary<string, bool>();
			lists.ForEach(l => result.Add(l.Name, l.IsChecked));
			OnComboBoxChecked?.Invoke(result);
		}


		private class ComboBoxLine {
			public string Name { get; set; }
			public bool IsChecked { get; set; }
			public bool IsCloud { get; set; }
			public bool IsDefault { get; set; }

			public ComboBoxLine(string name, bool isChecked, bool isCloud, bool isDefault) {
				Name = name;
				IsChecked = isChecked;
				IsCloud = isCloud;
				IsDefault = isDefault;
			}
		}
	}
}
