using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;

using NUnit.Framework;
using Direct3DExtensions;
using Direct3DExtensions.Terrain;
using SlimDX;

namespace Direct3DExtensions_Test
{
	[TestFixture]
	public class TestLandscapeManager
	{
		D3DHostForm form;
		Direct3DEngine engine;
		LandscapeManager manager;


		[SetUp]
		public void SetUp()
		{
			AppControl.SetUpApplication();
			form = new D3DHostForm();
			engine = new Terrain3DEngine();
			manager = new LandscapeManager(engine);

			form.SetEngine(engine);
		}


		[Test]
		public void Test()
		{
			Application.Run(form);
		}

		[TearDown]
		public void TearDown()
		{
			if (form != null && !form.IsDisposed)
				form.Dispose();
			form = null;
			GC.Collect();
		}
	}
}
