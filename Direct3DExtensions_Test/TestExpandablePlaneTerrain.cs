using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using NUnit.Framework;
using System.IO;

using Direct3DExtensions;
using Direct3DExtensions.Terrain;
using SlimDX;


namespace Direct3DExtensions_Test
{

	[TestFixture]
	public class TestExpandablePlaneTerrain
	{
		D3DHostForm form;
		Direct3DEngine engine;
		ExpandedPlane plane = new ExpandedIsometricPlane();
		Mesh sphere;
		float timeSinceLastUpdate = 0;
		int ScaleFactor = 1;
		ExTerrainEffect TerrainEffect;
		EquilateralTriangle tri;
		short[,] terrain;

		[SetUp]
		public void SetUp()
		{
			AppControl.SetUpApplication();
			form = new D3DHostForm();
			engine = new MultipleEffect3DEngine();
			TerrainEffect = new ExTerrainEffect();
			(engine as MultipleEffect3DEngine).AddEffect(TerrainEffect);
			using (MeshFactory fact = new MeshFactory())
			{
				sphere = fact.CreateSphere(1, 16, 16);
				sphere.Translation = new Vector3(0, 2, 0);
			}

			terrain = TerrainTest.ReadHgtFile(TerrainTest.hgtFile, 1200);
			form.SetEngine(engine);
		}

		[Test]
		public void TestTriangle()
		{
			tri = new EquilateralTriangle();
			//tri.Scale = new Vector3(100, 100, 100);
			engine.InitializationComplete += (o, e) =>
			{
				engine.BindMesh(tri, "ExTerrain");
				engine.BindMesh(sphere, "P2");
				TerrainEffect.WriteHeightDataToTexture(terrain);
			};
			engine.PreRendering += (o, e) =>
			{
				if (engine.CameraInput.Input.IsKeyPressed(Keys.X))
				{
					tri.Explode();
					tri.Recreate();
				}
			MoveMesh(tri);
			};

			Application.Run(form);
		}

		[Test]
		public void TestExpandablePlane()
		{
			
			plane.Segments = new System.Drawing.Size(60, 100);
			plane.Scale = new Vector3(20, 0.01f, 20);
			engine.InitializationComplete += (o, e) => { OnInitComplete(); };


			engine.CameraChanged += (o, e) => { OnCameraChanged(e); };

			Application.Run(form);
		}

		void OnInitComplete()
		{
			engine.BindMesh(sphere, "P2");
			engine.BindMesh(plane, "ExTerrain");
			TerrainEffect.WriteHeightDataToTexture(terrain);
			plane.Translation = new Vector3(-18, 0, -400);
			engine.CameraInput.LookAt(new Vector3(-18,12,-400), new Vector3(0, 0, 0));
		}


		void OnCameraChanged(CameraChangedEventArgs e)
		{
			if (e.ViewChanged)
			{
				Vector3 pos = engine.CameraInput.Camera.Position;
				Vector3 scale = plane.Scale;
				//float s = MathExtensions.Clamp(pos.Y, 0, 100) / 100;
				double s = MathExtensions.Clamp(pos.Y-10, 1, 200000);
				s = pos.Y;
				s = MathExtensions.Clamp(s, 1, 10000);
				//s = Math.Floor(s);
				ScaleFactor = (int)s;

				scale.X = (float)s;
				scale.Z = (float)s;
				plane.Scale = scale;

				double pitch = engine.CameraInput.Camera.YawPitchRoll.Y;
				pitch = Math.PI/2.0+MathExtensions.Clamp(pitch, -90.0 / 180.0 * Math.PI, -30.0/180.0*Math.PI);
				double yaw = engine.CameraInput.Camera.YawPitchRoll.X;
				double x = pos.X;
				double z = pos.Z;
				double h = pos.Y;
				double d = h * Math.Tan(pitch);
				
				//x += Math.Sin(yaw) * d;
				//z +=Math.Cos(yaw) * d;
				pos.Y = 0;
				plane.Translation = new Vector3((float)x,0,(float)z);
				plane.Rotation = new Vector3(0, (float)yaw, 0);

				(engine as Test3DEngine).additionalStatistics = "ScaleF: " + ScaleFactor + "\nScale: " + plane.Scale + 
					"\nPos: " + plane.Translation+"\nPitch: "+pitch+"\nYaw: "+yaw;

			}
		}

		void MoveMesh(Mesh mesh)
		{
			float speed = engine.CameraInput.Speed;
			InputHelper ih = engine.CameraInput.Input;
			Vector3 move = new Vector3();
			if (ih.IsKeyDown(Keys.Y))
				move.Z += 10;
			if (ih.IsKeyDown(Keys.H))
				move.Z -= 10;
			if (ih.IsKeyDown(Keys.G))
				move.X -= 10;
			if (ih.IsKeyDown(Keys.J))
				move.X += 10;
			move *= speed;
			move += mesh.Translation;

			mesh.Translation = move;
		}

	}
}
