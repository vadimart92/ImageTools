using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Drawing;

namespace ImageTools {
	public static class Utils {
		public static BitmapSource GetBitmapImage(string path, Func<Bitmap,Bitmap> converter) {
			BitmapSource bitmap;
			using (var img = (Bitmap)Image.FromFile(path)) {
				using (var i = converter(img)) {
					var data = i.LockBits(new Rectangle(0, 0, i.Width, i.Height), System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
					IntPtr ptr = data.Scan0;
					int bytes = Math.Abs(data.Stride) * data.Height;
					byte[] rgbValues = new byte[bytes];
					System.Runtime.InteropServices.Marshal.Copy(ptr, rgbValues, 0, bytes);
					var width = i.Width;
					var height = i.Height;
					var dpiX = i.HorizontalResolution;
					var dpiY = i.VerticalResolution;
					var pixelFormat = PixelFormats.Bgr24;
					var bytesPerPixel = (pixelFormat.BitsPerPixel + 7) / 8;
					var stride = bytesPerPixel * width;
					bitmap = BitmapSource.Create(width, height, dpiX, dpiY,
													 pixelFormat, null, rgbValues, stride);
				}
			}
			return bitmap;
		}
		public static async Task CopyStreamAsync(StreamReader Source, StreamWriter Destination) {
			char[] buffer = new char[0x1000];
			int numRead;
			while ((numRead = await Source.ReadAsync(buffer, 0, buffer.Length)) != 0) {
				await Destination.WriteAsync(buffer, 0, numRead);
			}
		}
		public static async Task CopyFilesAsync(string source,string dest) {
			await Task.Run(() => {
				File.Copy(source, dest);
			});
		}
	}
}
