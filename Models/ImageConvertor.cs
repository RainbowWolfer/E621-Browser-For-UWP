using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Web.Http;

namespace E621Downloader {
	public class ImageConvertor {
		public static async Task<IBuffer> GetBufferAsync(string url) {
			return await new HttpClient().GetBufferAsync(new Uri(url));
		}
		public static async Task<WriteableBitmap> GetWriteableBitmapAsync(string url) {
			try {
				IBuffer buffer = await GetBufferAsync(url);
				if(buffer != null) {
					var bi = new BitmapImage();
					WriteableBitmap wb = null;
					Stream stream2Write;
					using var stream = new InMemoryRandomAccessStream();
					stream2Write = stream.AsStreamForWrite();
					await stream2Write.WriteAsync(buffer.ToArray(), 0, (int)buffer.Length);
					await stream2Write.FlushAsync();
					stream.Seek(0);
					await bi.SetSourceAsync(stream);
					wb = new WriteableBitmap(bi.PixelWidth, bi.PixelHeight);
					stream.Seek(0);
					await wb.SetSourceAsync(stream);
					return wb;
				} else {
					return null;
				}
			} catch {
				return null;
			}
		}
	}
}
