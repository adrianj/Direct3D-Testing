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
	public class CropTiler_Test
	{
		CropTiler tiler;

		[SetUp]
		public void SetUp()
		{
			tiler = new CropTiler();
			tiler.ImageFetchFunction = Tiler_Test.FetchFunction;
		}

		/*
		 * Basically does nothing at all, since input NumberOfTiles is 1.
		 */
		[Test]
		public void ConstructSaveAndCheckEqual()
		{
			tiler.MaxZoomLevel = 12;
			tiler.NumberOfTiles = 1;
			Size finalSize = new Size(32 / tiler.NumberOfTiles, 32 / tiler.NumberOfTiles);
			tiler.OffsetIntoLargerImage = new Point(0, 0);
			Image img = tiler.ConstructTiledImage();
			img.Save(Tiler_Test.ImageFolder + "test_crop.bmp");
			Assert.That(img.Size, Is.EqualTo(finalSize));
			// Saved image should be the bottom left corner of 12_0_0.
			
		}


		[Test]
		public void ConstructSaveAndCheck12()
		{
			tiler.MaxZoomLevel = 12;
			tiler.NumberOfTiles = 2;
			Size finalSize = new Size(32 / tiler.NumberOfTiles, 32 / tiler.NumberOfTiles);
			tiler.OffsetIntoLargerImage = new Point(0, 1);
			Image img = tiler.ConstructTiledImage();
			img.Save(Tiler_Test.ImageFolder + "test_crop12.bmp");
			Assert.That(img.Size, Is.EqualTo(finalSize));
			// Saved image should be the bottom left corner of 12_0_0.
		}

		[Test]
		public void ConstructSaveAndCheck11()
		{
			tiler.MaxZoomLevel = 11;
			tiler.NumberOfTiles = 4;
			Size finalSize = new Size(32 / tiler.NumberOfTiles, 32 / tiler.NumberOfTiles);
			tiler.OffsetIntoLargerImage = new Point(0, 3);
			Image img = tiler.ConstructTiledImage();
			img.Save(Tiler_Test.ImageFolder + "test_crop11.bmp");
			Assert.That(img.Size, Is.EqualTo(finalSize));
			// Saved image should be the bottom left corner of 11_0_0.
		}
	}
}
