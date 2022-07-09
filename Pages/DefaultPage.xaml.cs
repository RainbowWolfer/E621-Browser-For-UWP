using E621Downloader.Models;
using Microsoft.AppCenter.Crashes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using Windows.ApplicationModel.Core;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Provider;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace E621Downloader.Pages {
	public sealed partial class DefaultPage: Page {
		private object args;
		private bool expandStackTrace = false;
		private bool expandDevice = false;

		public bool ExpandStackTrace {
			get => expandStackTrace;
			set {
				expandStackTrace = value;
				StackTranceIcon.Glyph = value ? "\uE010" : "\uE011";
				StackTraceText.MaxHeight = value ? double.MaxValue : 20;
			}
		}

		public bool ExpandDevice {
			get => expandDevice;
			set {
				expandDevice = value;
				DeviceIcon.Glyph = value ? "\uE010" : "\uE011";
				DeviceAnimation.From = DeviceGrid.Height;
				DeviceAnimation.To = value ? 200 : 0;
				DeviceStoryboard.Begin();
			}
		}

		private ErrorReport report = null;

		public DefaultPage() {
			this.InitializeComponent();
			CoreApplicationViewTitleBar coreTitleBar = CoreApplication.GetCurrentView().TitleBar;
			coreTitleBar.ExtendViewIntoTitleBar = false;
			ErrorEntranceStoryboard.Begin();
		}

		protected override void OnNavigatedTo(NavigationEventArgs e) {
			base.OnNavigatedTo(e);
			args = e.Parameter;
		}

		private async void Page_Loaded(object sender, RoutedEventArgs e) {
			if(!await Crashes.HasCrashedInLastSessionAsync()) {
				NavigateToMainPage();
				//Crashes.GenerateTestCrash();
			} else {
				ErrorReport report = await Crashes.GetLastSessionCrashReportAsync();
				if(report == null) {
					NavigateToMainPage();
				} else {
					IDText.Text = report.Id;
					StartTimeText.Text = report.AppStartTime.ToString();
					CrashTimeText.Text = report.AppErrorTime.ToString();
					DeviceText.Text = report.Device.Model;
					AppVersionText.Text = report.Device.AppVersion;
					LocaleText.Text = report.Device.Locale;
					ModelText.Text = report.Device.Model;
					OEMNameText.Text = report.Device.OemName;
					OSBuildText.Text = report.Device.OsBuild;
					OSNameText.Text = report.Device.OsName;
					ScreenSizeText.Text = report.Device.ScreenSize;
					SDKNameText.Text = report.Device.SdkName;
					SDKVersionText.Text = report.Device.SdkVersion;
					StackTraceText.Text = report.StackTrace;
					this.report = report;
				}
			}
		}

		private void NavigateToMainPage() {
			if(Window.Current.Content is Frame rootFrame) {
				rootFrame.Navigate(typeof(MainPage), args);
			}
		}

		private void StackTraceButton_Click(object sender, RoutedEventArgs e) {
			ExpandStackTrace = !ExpandStackTrace;
		}

		private void DeviceButton_Click(object sender, RoutedEventArgs e) {
			ExpandDevice = !ExpandDevice;
		}

		private async void EmailButton_Click(object sender, RoutedEventArgs e) {
			string json = ReportToJson(report);
			await Methods.ComposeEmail("[E621 Browser For UWP] " + "Error Report".Language(), "Content" + "\n\n\nError Report Json:\n" + json);
		}

		private void IgnoreButton_Click(object sender, RoutedEventArgs e) {
			NavigateToMainPage();
		}

		private async void SaveButton_Click(object sender, RoutedEventArgs e) {
			FileSavePicker savePicker = new() {
				SuggestedStartLocation = PickerLocationId.DocumentsLibrary,
			};
			savePicker.FileTypeChoices.Add("Plain Text", new List<string>() { ".txt" });
			savePicker.FileTypeChoices.Add("Json File", new List<string>() { ".json" });
			savePicker.SuggestedFileName = "E621 UWP " + "Error Report".Language() + $" {report.AppErrorTime}";
			StorageFile file = await savePicker.PickSaveFileAsync();
			if(file != null) {
				CachedFileManager.DeferUpdates(file);
				await FileIO.WriteTextAsync(file, ReportToJson(report));
				FileUpdateStatus status = await CachedFileManager.CompleteUpdatesAsync(file);
				if(status == FileUpdateStatus.Complete) {
					SaveTip.Content = "File Saved".Language();
				} else {
					SaveTip.Content = "File Cannot Be Saved".Language();
				}
				SaveTip.IsOpen = true;
			}

		}

		private string ReportToJson(ErrorReport errorReport) {
			Report report = new(errorReport);
			var json = JsonConvert.SerializeObject(report, Formatting.Indented);
			return json;
		}

		private class Report {
			public string ID { get; set; }
			public string StartTime { get; set; }
			public string CrashTime { get; set; }
			public string AppVersion { get; set; }
			public string Locale { get; set; }
			public string Model { get; set; }
			public string OEMName { get; set; }
			public string OSBuild { get; set; }
			public string OSName { get; set; }
			public string ScreenSize { get; set; }
			public string SDKName { get; set; }
			public string SDKVersion { get; set; }
			public string StackTrace { get; set; }
			public Report(ErrorReport report) {
				ID = report.Id;
				StartTime = report.AppStartTime.ToString();
				CrashTime = report.AppErrorTime.ToString();
				AppVersion = report.Device.AppVersion;
				Locale = report.Device.Locale;
				Model = report.Device.Model;
				OEMName = report.Device.OemName;
				OSBuild = report.Device.OsBuild;
				OSName = report.Device.OsName;
				ScreenSize = report.Device.ScreenSize;
				SDKName = report.Device.SdkName;
				SDKVersion = report.Device.SdkVersion;
				StackTrace = report.StackTrace;
			}
		}
	}
}
