using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Drawing;

namespace ImageTiler
{
	public class GooglemapTiler : MipMapTiler
	{
		GooglemapFilenameFactory filenameFactory = new GooglemapFilenameFactory();
		static double XScale = 256.0 / 360.0;

		public string ImageFolder { get { return filenameFactory.ImageFolder; } set { filenameFactory.ImageFolder = value; } }
		public double BottomLatitude { get { return filenameFactory.BottomLatitude; } set { filenameFactory.BottomLatitude = value; } }
		public double LeftLongitude { get { return filenameFactory.LeftLongitude; } set { filenameFactory.LeftLongitude = value; } }

		public GooglemapTiler()
			: base()
		{
			this.ImageFolder = "";
			this.InvertY = true;
			this.ImageFetchFunction = ImageFunction;
			this.ErrorReplacementImage = new Bitmap(512, 512, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
		}

		Image ImageFunction(int zoomLevel, int tileX, int tileY)
		{
			string filename = filenameFactory.CreateFilename(zoomLevel, tileX, tileY);
			double interval = filenameFactory.GetIntervalFromZoomLevel(zoomLevel);
			Image image = Bitmap.FromFile(filename);
			return ScaleImage(image, this.BottomLatitude+interval*tileY+interval/2);
		}

		Image ScaleImage(Image image, double latitude)
		{
			RectangleF bounds = CalculateImageBoundsAtLatitude(image.Width, image.Height, latitude);
			Image temp = CropImage(image, bounds);
			Image ret = new Bitmap(image);
			using (Graphics g = Graphics.FromImage(ret))
			{
				g.DrawImage(temp, 0, 0, image.Width, image.Height);
			}
			return ret;
		}

		public static RectangleF CalculateImageBoundsAtLatitude(int imageWidth, int imageHeight, double latitude)
		{
			double w = (double)(imageWidth);
			double h = (double)(imageHeight);
			double xs = GetXScaleAtLatitude(latitude);
			double ys = GetYScaleAtLatitude(latitude);
			double width = xs * w;
			double height = ys * (h);
			double left = (w - width) / 2.0;
			double top = Math.Floor((h - height) / 2.0);
			return new RectangleF((float)left, (float)top, (float)width, (float)height);
		}


		public static double GetYScaleAtLatitude(double latitude)
		{
			double sec = 1 / Math.Cos(Math.Abs(latitude) * Math.PI / 180.0);
			double ys = XScale * sec;
			return ys;
		}

		public static double GetXScaleAtLatitude(double latitude)
		{
			return XScale;
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
	}
}
