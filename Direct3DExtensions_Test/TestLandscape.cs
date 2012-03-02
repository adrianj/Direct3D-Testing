using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using NUnit.Framework;

using Direct3DExtensions;
using Direct3DExtensions.Terrain;

namespace Direct3DExtensions_Test
{
	[TestFixture]
	public class TestLandscape
	{
		Landscape landscape;

		[SetUp]
		public void SetUp()
		{
			landscape = new Landscape();
		}


	}
}
