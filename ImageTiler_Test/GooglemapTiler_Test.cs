using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using NUnit.Framework;
using ImageTiler;
using System.Drawing;

namespace ImageTiler_Test
{
	[TestFixture]
	public class GooglemapTiler_Test
	{
		GooglemapTiler tiler;
		int expectedReports;
		int zoom = 10;
		double lng = 174.6;
		double lat = -36.6;
		int numTiles = 4;

		[SetUp]
		public void SetUp()
		{
			tiler = new GooglemapTiler();
			tiler.ImageFolder = @"C:\Users\adrianj\Pictures\Mapping\GoogleTextures\";
			tiler.MaxZoomLevel = zoom;
			tiler.NumberOfTiles = numTiles;
			int minZoom = zoom;
			for (int i = tiler.NumberOfTiles; i > 1; i /= 2)
				minZoom--;
			GooglemapFilenameFactory fact = new GooglemapFilenameFactory();
			fact.ImageFolder = tiler.ImageFolder;

			tiler.BottomLatitude = fact.GetNearestCornerFromZoomLevel(lat, minZoom);
			tiler.LeftLongitude = fact.GetNearestCornerFromZoomLevel(lng, minZoom);
			fact.BottomLatitude = tiler.BottomLatitude;
			fact.LeftLongitude = tiler.LeftLongitude;
			Console.WriteLine("BottomLeft = " + tiler.LeftLongitude + "," + tiler.BottomLatitude);
			Console.WriteLine("BigImageCentre = " + fact.CreateFilename(minZoom, 0, 0));

			expectedReports = 1;
			for (int i = 1; i <= tiler.NumberOfTiles; i *= 2)
			{
				expectedReports += i * i + 1;
			}
		}

		[Test]
		public void TestGoogleTiler()
		{
			tiler.PreferredTileSize = new Size(512, 512);
			Image img = tiler.ConstructTiledImage();
			string filename = Tiler_Test.ImageFolder + "test_google.bmp";
			img.Save(filename);
			Console.WriteLine("Saved to: " + filename);
		}

		[Test]
		public void TestGoogleTilerSmalle()
		{
			tiler.MaxZoomLevel++;
			tiler.NumberOfTiles *= 2;
			tiler.PreferredTileSize = new Size(256, 256);
			Image img = tiler.ConstructTiledImage();
			string filename = Tiler_Test.ImageFolder + "test_google_small.bmp";
			img.Save(filename);
			Console.WriteLine("Saved to: " + filename);
		}

	}
}
