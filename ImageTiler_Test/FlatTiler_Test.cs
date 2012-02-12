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
	public class FlatTiler_Test
	{
		Tiler tiler;

		[SetUp]
		public void SetUp()
		{
			tiler = new FlatTiler();
			tiler.MaxZoomLevel = 15;
			tiler.NumberOfTiles = 4;
			tiler.ImageFetchFunction = Tiler_Test.FetchFunction;
		}


		[Test]
		public void ConstructSaveAndCheck()
		{
			Image img = tiler.ConstructTiledImage();
			img.Save(Tiler_Test.ImageFolder + "test.bmp");
			Image final = Bitmap.FromFile(Tiler_Test.ImageFolder + "final_flat.bmp");
			Assert.That(img.Size, Is.EqualTo(final.Size));
			// Do test and final look the same?
		}

		[Test]
		public void ConstructSmallerSaveAndCheck()
		{
			Size prefSize = new Size(256, 256);
			Size finalSize = new Size(prefSize.Width * tiler.NumberOfTiles, prefSize.Height * tiler.NumberOfTiles);
			tiler.PreferredTileSize = prefSize;
			Image img = tiler.ConstructTiledImage();
			img.Save(Tiler_Test.ImageFolder + "test_small.bmp");
			Assert.That(img.Size, Is.EqualTo(finalSize));
			// Do test and final look the same?
		}

		volatile bool workComplete = false;

		[Test]
		public void ConstructInThreadAndMonitorProgress()
		{
			BackgroundWorker reporter = new BackgroundWorker();
			reporter.WorkerReportsProgress = true;
			int reports = 0;
			workComplete = false;
			reporter.DoWork += (s, e) => { tiler.ConstructTiledImage(reporter); };
			reporter.ProgressChanged += (s, e) => { reports++;  Console.WriteLine(e.ProgressPercentage); };
			reporter.RunWorkerCompleted += (s, e) => { workComplete = true; };
			reporter.RunWorkerAsync();
			for (int i = 0; i < 100; i++)
			{
				if (workComplete)
					break;
				System.Threading.Thread.Sleep(100);
			}
			Assert.That(workComplete, Is.True);
			Assert.That(reports, Is.EqualTo(tiler.NumberOfTiles * tiler.NumberOfTiles + 1));
		}
	}
}
