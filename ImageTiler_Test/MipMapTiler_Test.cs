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
	public class MipMapTiler_Test
	{
		Tiler tiler;
		int expectedReports;

		[SetUp]
		public void SetUp()
		{
			tiler = new MipMapTiler();
			tiler.MaxZoomLevel = 15;
			tiler.NumberOfTiles = 4;
			tiler.ImageFetchFunction = Tiler_Test.FetchFunction;
			expectedReports = 1;
			for (int i = 1; i <= tiler.NumberOfTiles; i *= 2)
			{
				expectedReports += i * i + 1;
			}
		}


		[Test]
		public void ConstructSaveAndCheck()
		{
			Image img = tiler.ConstructTiledImage();
			string filename = Tiler_Test.ImageFolder + "test_mipmap.bmp";
			img.Save(filename);
			Console.WriteLine("Saved to: " + filename);
			Image final = Bitmap.FromFile(Tiler_Test.ImageFolder + "final_mipmap.bmp");
			Assert.That(img.Size, Is.EqualTo(final.Size));
			// Do test and final look the same?
		}

		[Test]
		public void ConstructSmallerSaveAndCheck()
		{
			Size prefSize = new Size(256, 256);
			Size finalSize = new Size(prefSize.Width * tiler.NumberOfTiles *3/2, prefSize.Height * tiler.NumberOfTiles);
			tiler.PreferredTileSize = prefSize;
			Image img = tiler.ConstructTiledImage();
			string filename = Tiler_Test.ImageFolder + "test_mipmap_small.bmp";
			img.Save(filename);
			Console.WriteLine("Saved to: " + filename);
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
			reporter.ProgressChanged += (s, e) => { reports++; Console.WriteLine(e.ProgressPercentage); };
			reporter.RunWorkerCompleted += (s, e) => { workComplete = true; };
			reporter.RunWorkerAsync();
			for (int i = 0; i < 100; i++)
			{
				if (workComplete)
					break;
				System.Threading.Thread.Sleep(100);
			}
			Assert.That(workComplete, Is.True);
			Assert.That(reports, Is.EqualTo(expectedReports));
		}
	}
}
