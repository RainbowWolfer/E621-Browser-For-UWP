using E621Downloader.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using static E621Downloader.Models.Data;

namespace E621Downloader {
	public sealed partial class MainPage: Page {
		private string url = "https://e621.net/posts?tags=skyleesfm+order%3Ascore";
		public MainPage() {
			this.InitializeComponent();
		}

		private async void Page_Loaded(object sender, RoutedEventArgs e) {
			//Debug.WriteLine("Start 1");
			//Debug.WriteLine(DateTime.Now.Second + "_" + DateTime.Now.Millisecond);
			//string result = await Read1();
			//Paragraph[] gs = GetCompleteParagraphs(result);
			//Debug.WriteLine(DateTime.Now.Second + "_" + DateTime.Now.Millisecond);




			//Debug.WriteLine("Start 2");
			//Debug.WriteLine(DateTime.Now.Second + "_" + DateTime.Now.Millisecond);
			string result = await ReadFromTestFile();
			result = result.Replace("<!doctype html>\n", "");
			Paragraph[] gs = GetCompleteParagraphs(result);
			//Debug.WriteLine(DateTime.Now.Second + "_" + DateTime.Now.Millisecond);

			Debug.WriteLine("\n\n\n Hello" + gs.Length + "\nHello");
		}

		private async Task<string> Read1() {
			HttpResponseMessage responseMessage = await new HttpClient().GetAsync(new Uri(url));
			string result = await responseMessage.Content.ReadAsStringAsync();
			return result;
		}
		private async Task<string> ReadFromTestFile() {
			StorageFolder InstallationFolder = Package.Current.InstalledLocation;
			StorageFile file = await InstallationFolder.GetFileAsync(@"Assets\TestText_Copy.txt");
			return File.ReadAllText(file.Path);
		}
		private string Read2() {
			var request = (HttpWebRequest)WebRequest.Create(url);
			request.UserAgent = "RainbowWolferE621TestApp";
			HttpWebResponse response = null;

			response = (HttpWebResponse)request.GetResponse();

			Stream dataStream = response.GetResponseStream();
			StreamReader reader = new StreamReader(dataStream);
			string data = reader.ReadToEnd();
			return data;
		}
	}
}

