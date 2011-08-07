using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Drawing;
using NUnit.Framework;

namespace Direct3DLib.TestCode
{
	[TestFixture]
	public class TestShapeImageFactory
	{
		[Test]
		public void HGTToImage()
		{
			string filename = "C:\\Users\\adrianj\\Documents\\Work\\CAD\\WebGIS_SRTM3\\S37E174.hgt";
			string outFile = "C:\\Users\\adrianj\\Documents\\Work\\CAD\\WebGIS_SRTM3\\S37E174.bmp";
			Assert.That(File.Exists(filename));
			Image image = ShapeHGTFactory.ConvertHGTToImage(filename);
			Assert.That(image != null);
			image.Save(outFile, System.Drawing.Imaging.ImageFormat.Bmp);
		}

		[Test]
		public void ImageToHGT()
		{
			string inFile = "C:\\Users\\adrianj\\Documents\\Work\\CAD\\WebGIS_SRTM3\\S37E174_small.bmp";
			Assert.That(File.Exists(inFile));

			//Assert.Ignore();
			Shape shape = ShapeImageFactory.CreateFromFile(inFile);
			Assert.That(shape, Is.Not.Null);
			Assert.That(shape.Vertices.Count, Is.EqualTo(150 * 150 * 6),"Wrong number of vertices");
		}

		[Test]
		public void SmallerHGTToImage()
		{
			string filename = "C:\\Users\\adrianj\\Documents\\Work\\CAD\\WebGIS_SRTM3\\S37E174.hgt";
			string outFile = "C:\\Users\\adrianj\\Documents\\Work\\CAD\\WebGIS_SRTM3\\S37E174_small.bmp";

			Assert.That(File.Exists(filename));
			Image image = ShapeHGTFactory.ConvertHGTToImage(filename,150,150,151,151);
			Assert.That(image, Is.Not.Null);
			image.Save(outFile, System.Drawing.Imaging.ImageFormat.Bmp);
		}

		[Test]
		public void LatLongDeltaToShape()
		{
			ShapeHGTFactory.TerrainFolder = "C:\\Users\\adrianj\\Documents\\Work\\CAD\\WebGIS_SRTM3";
			double delta = 1;
			Shape shape = ShapeHGTFactory.CreateFromCoordinates(-36, 174, delta, delta);
			int nRows = (int)(1200.0 * delta);
			Assert.That(shape, Is.Not.Null);
			Assert.That(shape.Vertices.Count, Is.EqualTo(nRows * nRows * 6));
		}
	}
}
