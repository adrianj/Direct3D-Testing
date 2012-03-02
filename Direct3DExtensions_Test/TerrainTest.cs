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
	public class TerrainTest
	{
		D3DHostForm form;
		Direct3DEngine engine;
		Landscape landscape;
		Mesh person;
		Vector3 previousCameraPosition = new Vector3();
		short[,] heightMap;
		public static string hgtFile = @"C:\Users\adrianj\Documents\Work\CAD\WebGIS_SRTM3\S37E174.hgt";
		int MapSize = 1024;
		int MaxHeight = 1000;
		int NumPatches = 32;
		Matrix wvp;

		[SetUp]
		public void SetUp()
		{
			AppControl.SetUpApplication();
			form = new D3DHostForm();
			engine = new Terrain3DEngine();
			
			form.SetEngine(engine);
		}




		[Test]
		public void Test()
		{
			engine.InitializationComplete += (o, e) =>
			{
				OnInitComplete();
			};
			engine.PostRendering += (o, e) =>
			{
				MovePerson();
				DoManualSplit();
			};
			
			Application.Run(form);
		}

		float timeSinceLastSplit = 0;

		[Test]
		public void AutoSplitTest()
		{
			engine.InitializationComplete += (o, e) =>
			{
				OnInitComplete();
			};
			engine.PostRendering += (o, e) =>
			{
				MovePerson();
				timeSinceLastSplit += engine.CameraInput.TimeDelta;
				DoAutoSplit();
			};
			engine.CameraChanged += (o, e) =>
				{
					wvp = landscape.World;
					wvp = Matrix.Multiply(wvp, engine.CameraInput.Camera.View);
					wvp = Matrix.Multiply(wvp, engine.CameraInput.Camera.Projection);
				};

			Application.Run(form);
		}

		bool busy = false;
		void DoAutoSplit()
		{
			if (timeSinceLastSplit >= 1 && !busy)
			{
				busy = true;
				landscape.Reset();
				landscape.Tessellate();
				timeSinceLastSplit = 0;
				previousCameraPosition = engine.CameraInput.Camera.Position;
				busy = false;
			}
		}

		void DoManualSplit()
		{
			if (engine.CameraInput.Input.IsKeyPressed(Keys.L))
			{
				landscape.Reset();
				landscape.Tessellate();
			}
		}

		void OnInitComplete()
		{
			engine.CameraInput.Camera.FarZ = 12000;

			heightMap = ReadHgtFile(hgtFile, MapSize);

			landscape = new Landscape();
			//landscape.SplitFunction = SplitFromHeight;
			landscape.VisibleFunction = IsTriVisible;
			landscape.FetchFunction = MapFetchFunction;
			landscape.MAP_SIZE = heightMap.GetLength(0)-1;
			landscape.NUM_PATCHES_PER_SIDE = NumPatches;
			landscape.Init();
			landscape.Scale = new Vector3(1, 0.0005f, 1);
			landscape.BindToPass(engine.D3DDevice, engine.Effect, "Terrain");
			engine.Geometry.Add(landscape);


			using (MeshFactory fact = new MeshFactory())
				person = fact.CreateSphere(0.5f, 20, 20);

			float h = GetHeightAtPoint(2.5f,2.5f);

			person.Translation = new Vector3(2.5f, h+1, 2.5f);
			person.BindToPass(engine.D3DDevice, engine.Effect, 2);
			engine.Geometry.Add(person);

			engine.D3DDevice.FillMode = SlimDX.Direct3D10.FillMode.Wireframe;
			
			engine.CameraInput.LookAt(new Vector3(MapSize/2, 100, 0), new Vector3(MapSize/2, 0, MapSize/2));
		}


		float GetHeightAtPoint(float x, float y)
		{
			int ix = (int)(x * landscape.Scale.X);
			int iy = (int)(y * landscape.Scale.Z);
			short h = MapFetchFunction(ix, iy);
			return h * landscape.Scale.Y;
		}

		short MapFetchFunction(int worldX, int worldY)
		{
			worldX = MathExtensions.Clamp(worldX, 0, heightMap.GetLength(1));
			worldY = MathExtensions.Clamp(worldY, 0, heightMap.GetLength(1));
			return heightMap[worldY, worldX];
		}

		bool SplitFromHeight(int leftX, int leftY, int rightX, int rightY, int apexX, int apexY)
		{
			int centerX = (leftX + rightX) >> 1;		// Compute X coordinate of center of Hypotenuse
			int centerY = (leftY + rightY) >> 1;		// Compute Y coord...
			int variance = 0;

			// Get the height value at the middle of the Hypotenuse
			int centerZ = heightMap[centerY, centerX];
			int leftZ = heightMap[leftY, leftX];
			int rightZ = heightMap[rightY, rightX];

			// Variance of this triangle is the actual height at it's hypotenuse midpoint minus the interpolated height.
			variance = Math.Abs(centerZ - ((leftZ + rightZ) >> 1));

			// Also consider distance to camera.
			// If further from camera, increase variance threshold
			float threshold = landscape.FrameVariance;

			Vector3 CameraPos = engine.CameraInput.Camera.Position;
			Vector3 center = new Vector3(centerX, centerZ, centerY);
			float distance = (center - CameraPos).Length();

			float triVar = ((float)variance * MapSize * 2) / distance;

			bool b = triVar > threshold;// Land.FrameVariance;
			//if(b) Console.WriteLine("splitting: " + b + ", " + variance + " > " + threshold);
			return b;
		}

		bool SplitFromArea(int leftX, int leftY, int rightX, int rightY, int apexX, int apexY)
		{
			Vector3 leftVt;
			Vector3 rightVt;
			Vector3 apexVt;
			Vector3 leftV;
			Vector3 rightV;
			Vector3 apexV;
			GetTriVertices(leftX, leftY, rightX, rightY, apexX, apexY, out leftV, out rightV, out apexV);
			TransformTriVertices(leftV, rightV, apexV, wvp, out leftVt, out rightVt, out apexVt);

			Vector3 ab = (leftVt - rightVt);
			Vector3 ac = (leftVt - apexVt);
			Vector3 cross = Vector3.Cross(ab, ac);
			float area = cross.Length() * 0.5f;

			bool b = area > 0.01f;

			return b;
		}

		bool SplitCombination(int leftX, int leftY, int rightX, int rightY, int apexX, int apexY)
		{
			return true;
		}

		int [,] CreateHeightMap()
		{
			int[,] hMap = new int[MapSize + 1, MapSize + 1];
			for (int y = 0; y < MapSize + 1; y++)
			{
				for (int x = 0; x < MapSize + 1; x++)
				{
					double s = (-Math.Cos(Math.PI * 2 * x / (double)MapSize) + 1) * Math.Sin(Math.PI * 2 * y / (double)MapSize) * MaxHeight;
					hMap[y, x] = (int)s;
				}
			}
			return hMap;
		}

		public static short[,] ReadHgtFile(string filename, int MapSize)
		{
			short [,] hMap = new short[MapSize+1,MapSize+1];
			using (BinaryReader reader = new BinaryReader(new FileStream(filename, FileMode.Open)))
			{
				for(int y = 1201; y > 0; y--)
					for (int x = 0; x < 1201; x++)
					{
						byte [] b = reader.ReadBytes(2);
						b = b.Reverse().ToArray();
						short s = BitConverter.ToInt16(b, 0);
						if (s == short.MinValue) s = 0;
						if (y < hMap.GetLength(0) && x < hMap.GetLength(1))
							hMap[y,x] = s;
					}

			}
			return hMap;
		}

		void MovePerson()
		{
			InputHelper ih = engine.CameraInput.Input;
			Vector3 move = new Vector3();
			if (ih.IsKeyDown(Keys.Y))
				move.Z += 0.01f;
			if (ih.IsKeyDown(Keys.H))
				move.Z -= 0.01f;
			if (ih.IsKeyDown(Keys.G))
				move.X -= 0.01f;
			if (ih.IsKeyDown(Keys.J))
				move.X += 0.01f;
			move += person.Translation;

			float h = GetHeightAtPoint(move.X, move.Z);
			move.Y = h + 1;
			person.Translation = move;
		}



		bool IsTriVisible(int leftX, int leftY, int rightX, int rightY, int apexX, int apexY)
		{
			Vector3 leftVt;
			Vector3 rightVt;
			Vector3 apexVt;
			Vector3 leftV;
			Vector3 rightV;
			Vector3 apexV;
			GetTriVertices(leftX, leftY, rightX, rightY, apexX, apexY, out leftV, out rightV, out apexV);
			TransformTriVertices(leftV, rightV, apexV, wvp, out leftVt, out rightVt, out apexVt);
			bool b = (IsVertexVisible(leftVt)
			|| IsVertexVisible(rightVt)
			|| IsVertexVisible(apexVt));
			b &= TriOrientation(leftVt, apexVt, rightVt);
			return b;
		}

		private void TransformTriVertices(Vector3 leftV, Vector3 rightV, Vector3 apexV, Matrix w, out Vector3 leftVt, out Vector3 rightVt, out Vector3 apexVt)
		{
			leftVt = Vector3.TransformCoordinate(leftV, w);
			rightVt = Vector3.TransformCoordinate(rightV, w);
			apexVt = Vector3.TransformCoordinate(apexV, w);
		}

		private void GetTriVertices(int leftX, int leftY, int rightX, int rightY, int apexX, int apexY, out Vector3 leftV, out Vector3 rightV, out Vector3 apexV)
		{
			int leftH = heightMap[leftY, leftX];
			int rightH = heightMap[rightY, rightX];
			int apexH = heightMap[apexY, apexX];
			leftV = new Vector3(leftX, leftH, leftY);
			rightV = new Vector3(rightX, rightH, rightY);
			apexV = new Vector3(apexX, apexH, apexY);
		}

		bool IsVertexVisible(Vector3 v)
		{
			if (v.X < -1 || v.X > 1) return false;
			if (v.Y < -1 || v.Y > 1) return false;
			if (v.Z < 0 || v.Z > 1) return false;
			return true;
		}


		bool TriOrientation(Vector3 leftV, Vector3 apexV, Vector3 rightV)
		{
			Plane p = new Plane(leftV, apexV, rightV);
			Vector3 norm = p.Normal;
			bool b = norm.Z > 0;
			return b;
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
