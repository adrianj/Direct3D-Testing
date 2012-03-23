using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Direct3DExtensions;
using Direct3DExtensions.Terrain;
using System.Drawing;
using SlimDX;
using System.Windows.Forms;

namespace Direct3DExtensions_Test
{
	[TestFixture]
	public class TestExTerrainManager
	{
		D3DHostForm form;
		MultipleEffect3DEngine engine;
		ExTerrainManager ctm;
		Effect effect;

		[SetUp]
		public void SetUp()
		{
			AppControl.SetUpApplication();
			engine = new MultipleEffect3DEngine();
			effect = new ExTerrainEffect();
			ctm = new ExTerrainManager(engine as MultipleEffect3DEngine, effect); 
			form = new D3DHostForm();

			form.SetEngine(engine);
		}



		[Test]
		public void TestClipmapManager()
		{
			Application.Run(form);
		}

		[Test]
		public void TestClipmapMovement()
		{
			engine.InitializeDirect3D();
			Camera camera = engine.CameraInput.Camera;
			System.Threading.Thread.Sleep(200);
			camera.Position += new Vector3(-200, 0, 200);
			System.Threading.Thread.Sleep(200);
			camera.Position += new Vector3(-200, 0, 200);
			System.Threading.Thread.Sleep(200);
			camera.Position += new Vector3(-200, 0, 200);
		}


		[TearDown]
		public void TearDown()
		{
			ctm.Dispose();
		}
	}
}
