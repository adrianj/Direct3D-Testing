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
		private static NullImage nullImage = new NullImage(new Size(256,256),Color.WhiteSmoke);

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


		public static Texture2D ConvertImageFileToTexture2D(Device device, string filename)
		{
			if (filename == null || filename.Length < 1 || !File.Exists(filename))
				return GetErrorTexture(device,"File Not Found:\n"+filename);
			Texture2D texture = Texture2D.FromFile(device, filename);
			return texture;
		}

		public static Texture2D GetNullTexture(Device device)
		{
			if (NullTexture == null)
			{
				NullTexture = GetErrorTexture(device, "Null Texture");
			}
			return NullTexture;
		}

		public static Texture2D GetErrorTexture(Device device, string errorMessage)
		{
			nullImage.Text = errorMessage;
			return ConvertImageToTexture2D(device, nullImage.ImageClone);
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
			if (image.Width < 1 || image.Height < 1) throw new ArgumentException("Cannot crop image to less than 1x1 pixels");
			Bitmap bmp = (Bitmap)image;
			if (rect.Width > image.Width) rect.Width = image.Width;
			if (rect.Height > image.Height) rect.Height = image.Height;
			if (rect.Width < 1) rect.Width = 1;
			if (rect.Height < 1) rect.Width = 1;
			if (rect.X < 0) rect.X = 0;
			if (rect.X + rect.Width > bmp.Width) rect.X = bmp.Width - rect.Width;
			if (rect.Y < 0) rect.Y = 0;
			if (rect.Y + rect.Height > bmp.Height) rect.Y = bmp.Height - rect.Height;
			return bmp.Clone(rect, bmp.PixelFormat);
		}

		public static Image ResizeImage(Image image, Size newSize)
		{
			Bitmap bmp = new Bitmap(newSize.Width, newSize.Height);
			using (Graphics g = Graphics.FromImage(bmp))
			{
				g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBilinear;
				g.DrawImage(image, 0, 0, newSize.Width, newSize.Height);
			}
			return bmp;
		}
	}
}
