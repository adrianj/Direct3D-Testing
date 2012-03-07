using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using NUnit.Framework;
using System.IO;
using System.Drawing;

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
		//Mesh plane = new ExpandableSquareGrid();
		Mesh plane = new TerrainMeshSet();
		int ScaleFactor = 1;
		ExTerrainEffect TerrainEffect;
		EquilateralTriangle tri;
		short[,] hiresTerrain;
		short[,] loresTerrain;
		float WorldUnitsPerDegree = 1024.0f;
		Vector2 initialCameraLocation = new Vector2(-0.2f, 0.2f);
		Vector2 initialTerrainLocation = new Vector2(175f, -37f);

		[SetUp]
		public void SetUp()
		{
			AppControl.SetUpApplication();
			form = new D3DHostForm();
			engine = new MultipleEffect3DEngine();
			TerrainEffect = new ExTerrainEffect();
			(engine as MultipleEffect3DEngine).AddEffect(TerrainEffect);

			form.SetEngine(engine);
		}

		private void FetchHiresTerrain()
		{
			TerrainHeightTextureFetcher fetcher = new Strm3TextureFetcher();
			hiresTerrain = fetcher.FetchTerrain(GetCentredSquareRectangle(initialTerrainLocation, DegreesToWorldUnits(0.125) / 1200.0f));
			TerrainEffect.WriteHeightDataToTexture(hiresTerrain);
			TerrainEffect.TerrainCentreLocation = DegreesToWorldUnits(initialCameraLocation);
		}

		Vector2 DegreesToWorldUnits(Vector2 degrees)
		{
			return new Vector2(DegreesToWorldUnits(degrees.X), DegreesToWorldUnits(degrees.Y));
		}

		float DegreesToWorldUnits(double degrees)
		{
			return (float)degrees * WorldUnitsPerDegree;
		}

		void FetchLoresTerrain()
		{

		}

		RectangleF GetCentredSquareRectangle(Vector2 centre, float width)
		{
			PointF point = new PointF(centre.X-width*0.5f, centre.Y-width*0.5f);
			SizeF size = new SizeF(width,width);
			RectangleF rect = new RectangleF(point, size);
			return rect;
		}

		[Test]
		public void TestTriangle()
		{
			tri = new EquilateralTriangle();
			engine.InitializationComplete += (o, e) =>
			{
				engine.BindMesh(tri, "ExTerrain");
				FetchHiresTerrain();
				FetchLoresTerrain();
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
			if(plane is ExpandedPlane)
				(plane as ExpandedPlane).Segments = new System.Drawing.Size(60, 100);
			plane.Scale = new Vector3(20, 0.05f, 20);
			engine.InitializationComplete += (o, e) => { OnInitComplete(); };


			engine.CameraChanged += (o, e) => { OnCameraChanged(e); };

			Application.Run(form);
		}

		void OnInitComplete()
		{
			engine.CameraInput.Camera.FarZ = 12000;
			Vector2 camPos = DegreesToWorldUnits(initialCameraLocation);
			engine.CameraInput.LookAt(new Vector3(camPos.X, 12, camPos.Y), new Vector3(camPos.X-10, 0, camPos.Y+10));
			CreateAndBindMarkerSpheres();
			engine.BindMesh(plane, "ExTerrain");
			FetchHiresTerrain();
			FetchLoresTerrain();
			SetTerrainScale();
			SetTerrainTranslation();
		}

		private void CreateAndBindMarkerSpheres()
		{
			using (MeshFactory fact = new MeshFactory())
			{
				float sphereHeight = 5;
				for(double y = -1; y <= 1; y+=0.5)
					for (double x = -1; x <= 1; x+=0.5)
					{
						Mesh sphere = fact.CreateSphere(10, 10, 10);
						sphere.Translation = new Vector3(DegreesToWorldUnits(x + initialCameraLocation.X), sphereHeight, DegreesToWorldUnits(y+initialCameraLocation.Y));
						engine.BindMesh(sphere, "P2");
					}
			}
		}


		void OnCameraChanged(CameraChangedEventArgs e)
		{
			if (e.PositionChanged)
			{
				SetTerrainScale();
				SetTerrainTranslation();

				(engine as Test3DEngine).additionalStatistics = "ScaleF: " + ScaleFactor + "\nScale: " + plane.Scale + 
					"\nPos: " + plane.Translation+"\n: var: "+TerrainEffect.InverseMapSize;

			}
		}

		private void SetTerrainTranslation()
		{
			Vector3 pos = engine.CameraInput.Camera.Position;
			Vector3 scale = plane.Scale;
			plane.Translation = new Vector3(pos.X, 0, pos.Z);
		}

		private void SetTerrainScale()
		{
			Vector3 pos = engine.CameraInput.Camera.Position;
			Vector3 scale = plane.Scale;
			plane.Scale = GetScaleFromHeight(pos.Y, scale.Y);
		}

		private Vector3 GetScaleFromHeight(double height, float originalYScale)
		{
			//double s = pos.Y;
			Vector3 scale = new Vector3(1, originalYScale, 1);
			height = Math.Pow(height, 2.0 / 3.0);
			height = height / 8;
			height = MathExtensions.PowerOfTwo(height, 1);
			scale.X = (float)height;
			scale.Z = (float)height;
			return scale;
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
