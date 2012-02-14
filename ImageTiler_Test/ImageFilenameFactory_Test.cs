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

		[Test]
		public void TestSimpleFactory()
		{
			ImageFilenameFactory fact = new SimpleFilenameFactory();
			string ret = fact.CreateFilename(14, 2, 1);
			Assert.That(ret, Is.EqualTo("14_2_1.png"));
		}

		
	}
}
