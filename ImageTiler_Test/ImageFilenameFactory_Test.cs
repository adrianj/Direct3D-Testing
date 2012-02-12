using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using NUnit.Framework;

using ImageTiler;

namespace ImageTiler_Test
{
	[TestFixture]
	public class ImageFilenameFactory_Test
	{

		int zoom = 13;
		double lng = 174.8;
		double lat = -36.7;
		string ImageFolder = @"C:\Users\adrianj\Pictures\Mapping\GoogleTextures\";
		GooglemapFilenameFactory fact;

		[Test]
		public void TestSimpleFactory()
		{
			ImageFilenameFactory fact = new SimpleFilenameFactory();
			string ret = fact.CreateFilename(14, 2, 1);
			Assert.That(ret, Is.EqualTo("14_2_1.png"));
		}

		[Test]
		public void TestGoogleFactorySetup()
		{
			fact = new GooglemapFilenameFactory();
			fact.ImageFolder = ImageFolder;

			double interval = fact.GetIntervalFromZoomLevel(zoom);
			Assert.AreEqual(0.0625, interval, 0.0001);

			fact.LeftLongitude = fact.GetNearestCornerFromZoomLevel(lng, zoom);
			fact.BottomLatitude = fact.GetNearestCornerFromZoomLevel(lat, zoom);
			Assert.AreEqual(174.75, fact.LeftLongitude, 0.0000001, "longCorner");
			Assert.AreEqual(-36.75, fact.BottomLatitude, 0.0000001, "latCorner");


			double centreLong = fact.GetNearestCentreFromZoomLevel(lng, zoom);
			double centreLat = fact.GetNearestCentreFromZoomLevel(lat, zoom);
			Assert.AreEqual(174.7812500, centreLong, 0.0000001, "longCentre");
			Assert.AreEqual(-36.7187500, centreLat, 0.0000001, "latCentre");


			string act = fact.CreateFilename(11, 0, 0);
			string exp = ImageFolder+@"zoom=11\googlemap_&zoom=11center=-36.62500000,174.87500000.png";
			Assert.That(act, Is.EqualTo(exp));
		}

		[Test]
		public void TestGoogleFactory()
		{
			fact = new GooglemapFilenameFactory();
			fact.ImageFolder = ImageFolder;

			fact.BottomLatitude = lat;
			fact.LeftLongitude = lng;
			int tiles = 4;
			string[] filenames = new string[tiles * tiles];
			for (int y = 0; y < tiles; y++)
			for(int x = 0; x < tiles; x++)
				{
					string filename = fact.CreateFilename(zoom, x, y);
					Console.WriteLine("" + System.IO.File.Exists(filename) + ", " + filename);
					filenames[y * tiles + x] = filename;
				}
			foreach (string filename in filenames)
				Assert.That(System.IO.File.Exists(filename), filename + " does not exist.");

		}
	}
}
