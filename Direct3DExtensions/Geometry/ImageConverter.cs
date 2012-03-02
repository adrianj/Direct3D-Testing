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

namespace Direct3DExtensions
{
	public class ImageConverter
	{

		public static Image ConvertTexture2DToImage(Device device,Texture2D texture)
		{
			throw new NotImplementedException();
		}


		public static Texture2D ConvertImagesToTexture2D(Device device, Image [] image)
		{
			Texture2DDescription td = new Texture2DDescription();
			td.Format = SlimDX.DXGI.Format.R32G32B32A32_Float;
			td.Width = image[0].Width;
			td.Height = image[0].Height;
			td.MipLevels = 1;
			td.ArraySize = 1;
			td.SampleDescription = new SlimDX.DXGI.SampleDescription(1, 0);
			td.Usage = ResourceUsage.Default;
			td.BindFlags = BindFlags.ShaderResource;
			td.CpuAccessFlags = CpuAccessFlags.None;
			td.OptionFlags = ResourceOptionFlags.None;

			

			 
			byte[] b = ConvertImagesToBytes(image);
			byte[] bytes = ConvertImageToBytes(image[0]);

			Texture2D texture = ConvertBytesToTexture2D(device, b);

			return texture;
		}

		public static Texture2D ConvertImageToTexture2D(Device device, Image image)
		{
			
			byte[] bytes = ConvertImageToBytes(image);
			return ConvertBytesToTexture2D(device,bytes);
			 
		}

		public static byte[] ConvertImagesToBytes(Image[] image)
		{
			int length = 0;
			for (int i = 0; i < image.Length; i++)
				length += image[i].Height * image[i].Width * 4;
			byte[] bytes = new byte[length];
			int r = 0;
			for (int i = 0; i < image.Length; i++)
			{
				using (MemoryStream s = new MemoryStream())
				{
					image[i].Save(s, System.Drawing.Imaging.ImageFormat.Png);
					s.Seek(0, SeekOrigin.Begin);
					byte[] b = new byte[s.Length];
					int rr = s.Read(b, 0, (int)s.Length);
					b.CopyTo(bytes, r);
					r = r + rr;
				}
			}
			return bytes;
		}

		public static byte[] ConvertImageToBytes(Image image)
		{
			return ConvertImagesToBytes(new Image[] { image });
		}

		public static Texture2D ConvertBytesToTexture2D(Device device, byte[] bytes)
		{

			ImageLoadInformation mli = new ImageLoadInformation()
			{
				CpuAccessFlags = CpuAccessFlags.Read,
				BindFlags = BindFlags.None,
				MipLevels = 1,
				Format = SlimDX.DXGI.Format.R8G8B8A8_UNorm,
				FilterFlags = SlimDX.Direct3D10.FilterFlags.None,
				OptionFlags = ResourceOptionFlags.None,
				Usage = ResourceUsage.Default,
				Height = 256,
				Width = 256,
				Depth = 256 * 256
			};

			Texture2D texture = Texture2D.FromMemory(device, bytes, mli);
			return texture;
			 

		}


		public static Texture2D ConvertImageFileToTexture2D(Device device, string filename)
		{
			Texture2D texture = Texture2D.FromFile(device, filename);
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
			Bitmap bmp = new Bitmap(newSize.Width, newSize.Height, image.PixelFormat);
			using (Graphics g = Graphics.FromImage(bmp))
			{
				g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
				g.DrawImage(image, 0, 0, newSize.Width, newSize.Height);
			}
			return bmp;
		}

		public static void SaveAndDisplayImage(Image image)
		{
			string filename = Path.GetTempPath();
			filename += Path.GetRandomFileName();
			filename += ".png";
			SaveAndDisplayImage(image, filename);
		}
		public static void SaveAndDisplayImage(Image image, string filename)
		{
			image.Save(filename);
			System.Diagnostics.Process.Start(filename);
		}
	}
}
