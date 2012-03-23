using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using NUnit.Framework;

using Direct3DExtensions;
using SlimDX;
using Direct3DExtensions.Terrain;

namespace Direct3DExtensions_Test
{
	[TestFixture]
	public class TestClipmapTerrainManager
	{
		D3DHostForm form;
		MultipleEffect3DEngine engine;
		ClipmapTerrainManager hiresCtm;
		ClipmapTerrainManager loresCtm;
		ExTerrainManager etm;
		Effect effect;
		PointF startingLongLat = new PointF(174.5f, -37.0f);
		int widthOfTiles = 32;
		int widthInTiles = 32;

		[SetUp]
		public void SetUp()
		{
			AppControl.SetUpApplication();
			engine = new MultipleEffect3DEngine() { D3DDevice = new MultipleOutputDevice() { NumAdditionalTargets = 1 } };
			effect = new WorldViewProjEffect() { ShaderFilename = @"Effects\ClipmapTerrain.fx" };
			engine.AddEffect(effect);
			hiresCtm = new ClipmapTerrainManager(engine, effect)
			{
				WidthInTiles = widthInTiles/2,
				WidthOfTiles = widthOfTiles/2,
				StartingLongLat = startingLongLat
			};
			loresCtm = new ClipmapTerrainManager(engine, effect)
			{
				WidthInTiles = widthInTiles,
				WidthOfTiles = widthOfTiles,
				TextureVariableName = "LoresTexture",
				TerrainFetcher = new Srtm30TextureFetcher(),
				StartingLongLat = startingLongLat
			};
			etm = new ExTerrainManager(engine,effect);
			form = new D3DHostForm();

			//engine.InitializationComplete += (o, e) => 

			form.SetEngine(engine);
		}



		[Test]
		public void TestClipmapManager()
		{
			Application.Run(form);
		}

		[Test]
		public void TestClipmapManagerWithMultipleOutputs()
		{
			engine.PostRendering += (o, e) => CheckMouseClick();
			Application.Run(form);
		}

		void CheckMouseClick()
		{
			if (engine.CameraInput.Input.IsMousePressed(MouseButtons.Right))
			{
				Direct3DExtensions.Texturing.ScreenCapture sc = new Direct3DExtensions.Texturing.ScreenCapture(engine);
				Vector4 vec = sc.GetResultAtPoint(engine.CameraInput.Input.MousePosition,1);
				Console.WriteLine("Right click at: " + engine.CameraInput.Input.MousePosition + " = " + vec);
			}
		}


		[Test]
		public void TestClipmapMovement()
		{
			engine.InitializeDirect3D();
			Camera camera = engine.CameraInput.Camera;
			GetAndShowHiresTerrain("testClipmap_0.png");
			camera.Position += new Vector3(-200, 0, 200);
			GetAndShowHiresTerrain("testClipmap_1.png");
		}

		private void GetAndShowHiresTerrain(string filename)
		{
			float[,] texture = hiresCtm.GetTerrain();
			Image img = ImagingFunctions.CreateImageFromArray(texture);
			ImagingFunctions.SaveAndDisplayImage(img, filename);
		}

		[TearDown]
		public void TearDown()
		{
			hiresCtm.Dispose();
		}
	}
}

