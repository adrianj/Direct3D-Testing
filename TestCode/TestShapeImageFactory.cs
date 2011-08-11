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
