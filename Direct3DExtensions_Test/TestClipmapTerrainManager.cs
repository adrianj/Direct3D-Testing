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
	public class TestClipmapTerrainManager
	{
		D3DHostForm form;
		MultipleEffect3DEngine engine;
		ClipmapTerrainManager ctm;

		[SetUp]
		public void SetUp()
		{
			AppControl.SetUpApplication();
			engine = new MultipleEffect3DEngine();
			ctm = new ClipmapTerrainManager(engine as MultipleEffect3DEngine); 
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
			GetAndShowHiresTerrain("testClip_hires0.png");
			camera.Position += new Vector3(-200, 0, 200);
			GetAndShowHiresTerrain("testClip_hires1.png");
		}

		private void GetAndShowHiresTerrain(string filename)
		{
			short[,] texture = ctm.GetHiresTerrain();
			Image img = ImagingFunctions.CreateImageFromArray(texture);
			ImagingFunctions.SaveAndDisplayImage(img, filename);
		}

		[TearDown]
		public void TearDown()
		{
			ctm.Dispose();
		}
	}
}
