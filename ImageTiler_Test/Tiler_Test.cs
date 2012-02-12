using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using ImageTiler;
using NUnit.Framework;
using System.Drawing;

namespace ImageTiler_Test
{

	[TestFixture]
	public class Tiler_Test
	{
		public static string ImageFolder = @"Images\";

		[Test]
		public void EmptyTest()
		{
			Exception ex = Assert.Catch(() =>
			{
				Tiler t = new FlatTiler();
				t.ConstructTiledImage();
			});
			Assert.That(ex, Is.InstanceOf(typeof(ArgumentNullException)));
		}

		[Test]
		public void FetchError()
		{
			Tiler t = new FlatTiler();
			t.ImageFetchFunction = (z, x, y) => { return Bitmap.FromFile("nofile.png"); };
			Exception ex = Assert.Catch(() =>
				{
					t.ConstructTiledImage();
				});
		}

		[Test]
		public void FetchesAllFiles()
		{
			Tiler t = new FlatTiler();
			t.MaxZoomLevel = 15;
			t.NumberOfTiles = 4;
			t.ImageFetchFunction = Tiler_Test.FetchFunction;
			Image img = t.ConstructTiledImage();
		}


		public static Image FetchFunction(int z, int x, int y)
		{
			string filename = Tiler_Test.ImageFolder + "zoom="+z+@"\"+z + "_" + x + "_" + y + ".png";
			Console.WriteLine("Fetching: " + filename);
			return Bitmap.FromFile(filename);
		}
	}
}
