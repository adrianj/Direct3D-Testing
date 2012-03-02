using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Direct3DExtensions;
using System.Windows.Forms;
using NUnit.Framework;

namespace Direct3DExtensions_Test
{
	[TestFixture]
	public class CustomFormTest
	{
		Form form;

		[SetUp]
		public void SetUp()
		{
			AppControl.SetUpApplication();
			form = new TestForm();
		}

		[Test]
		public void Test()
		{
			Application.Run(form);
		}
	}
}
