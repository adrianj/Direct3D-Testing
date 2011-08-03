using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SlimDX;
using SlimDX.Direct3D10;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.IO;

namespace Direct3DLib
{
	public class ImageConverter
	{
		private static Texture2D NullTexture;

		public static Image ConvertTexture2DToImage(Device device,Texture2D texture)
		{
			throw new NotImplementedException();
		}

		public static Texture2D ConvertImageToTexture2D(Device device, Image image)
		{
			byte[] bytes = ConvertImageToBytes(image);
			return ConvertBytesToTexture2D(device,bytes);
		}

		public static byte[] ConvertImageToBytes(Image image)
		{
			int length = image.Height * image.Width * 4;
			byte[] bytes = new byte[length];
			using (MemoryStream s = new MemoryStream())
			{
				image.Save(s, System.Drawing.Imaging.ImageFormat.Png);
				s.Seek(0, SeekOrigin.Begin);
				s.Read(bytes, 0, length);
			}
			return bytes;
		}

		public static Texture2D ConvertBytesToTexture2D(Device device, byte[] bytes)
		{
			Texture2D texture = Texture2D.FromMemory(device, bytes);
			return texture;
		}

		private static string WriteImageToTempFile(Image image)
		{
			string filename = Path.GetRandomFileName();
			image.Save(filename);
			return filename;
		}

		private static void DeleteTempFile(string filename)
		{
			if (File.Exists(filename))
				File.Delete(filename);
		}

		public static Texture2D ConvertImageFileToTexture2D(Device device, string filename)
		{
			if (filename == null || filename.Length < 1 || !File.Exists(filename))
				return GetNullTexture(device);
			Texture2D texture = Texture2D.FromFile(device, filename);
			return texture;
		}

		public static Texture2D GetNullTexture(Device device) 
		{
			if (NullTexture == null)
			{
				if (File.Exists(ShaderHelper.DEFAULT_IMAGE_FILENAME))
					NullTexture = Texture2D.FromFile(device, ShaderHelper.DEFAULT_IMAGE_FILENAME);
				else
					NullTexture = CreateEmptyTexture(device, 1, 1);
			}
			return NullTexture;
		}

		public static Texture2D CreateEmptyTexture(Device device, int width, int height)
		{
			Texture2DDescription desc = new Texture2DDescription()
			{
				Width = width,
				Height = height,
				MipLevels = 1,
				ArraySize = 1,
				Usage = ResourceUsage.Dynamic,
				CpuAccessFlags = SlimDX.Direct3D10.CpuAccessFlags.Write,
				BindFlags = SlimDX.Direct3D10.BindFlags.ShaderResource,
				SampleDescription = new SlimDX.DXGI.SampleDescription(1, 0),
				Format = SlimDX.DXGI.Format.R8G8B8A8_UNorm,
			};
			Texture2D texture = new Texture2D(device, desc);
			return texture;
		}

		public static Image StitchImages(Image[] images, int tileWidth, int tileHeight)
		{
			Size imageSize = images[0].Size;
			Bitmap bmp = new Bitmap(imageSize.Width * tileWidth, imageSize.Height * tileHeight);
			bmp.SetResolution(bmp.HorizontalResolution * tileWidth, bmp.VerticalResolution * tileHeight);
			using (Graphics g = Graphics.FromImage(bmp))
			{
				for (int row = 0; row < tileHeight; row++)
					for (int col = 0; col < tileWidth; col++)
						g.DrawImage(images[row * tileWidth + col], col * imageSize.Width, row * imageSize.Height, imageSize.Width, imageSize.Height);
			}
			return bmp;
		}


		public static Image CropImage(Image image, RectangleF rect)
		{
			Bitmap bmp = new Bitmap(image);
			if (rect.Width > image.Width) rect.Width = image.Width;
			if (rect.X < 0) rect.X = 0;
			if (rect.Height > image.Height) rect.Height = image.Height;
			if (rect.Y < 0) rect.Y = 0;
			return bmp.Clone(rect, bmp.PixelFormat);
		}
	}
}
