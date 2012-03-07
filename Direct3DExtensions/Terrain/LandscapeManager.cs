using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.IO;
using SlimDX;
using System.Drawing;

namespace Direct3DExtensions.Terrain
{
	public class LandscapeManager
	{
		short[,] heightMap;
		int MapSize = 64;
		int NumPatches = 8;
		float updatePeriod = 1.0f;
		float timeSinceLastUpdate = 0;
		float initialFrameVariance = 1;
		bool busy = false;

		Landscape landscape = new Landscape();
		public Landscape Landscape { get { return landscape; } }

		Direct3DEngine engine;

		public LandscapeManager(Direct3DEngine engine)
		{
			this.engine = engine;
			
			engine.InitializationComplete += (o, e) => { OnInitComplete(); };
			engine.PostRendering += (o, e) => { OnRender(); };
			engine.CameraChanged += (o, e) => { landscape.CameraPos = engine.CameraInput.Camera.Position; };
		}

		void OnInitComplete()
		{
			engine.CameraInput.Camera.FarZ = 10000;

			heightMap = ReadHgtFile();
			landscape.Scale = new SlimDX.Vector3(10, 1, 10);
			landscape.FrameVariance = initialFrameVariance;
			landscape.desiredTris = NumPatches*NumPatches*8;
			landscape.NUM_PATCHES_PER_SIDE = NumPatches;
			landscape.MAP_SIZE = MapSize;
			landscape.FetchFunction = MapFetchFunction;
			//landscape.VisibleFunction = IsTriVisible;
			landscape.SplitFunction = SplitByScreenArea;
			landscape.BindToPass(engine.D3DDevice, engine.Effect, "Terrain");
			engine.Geometry.Add(landscape);
		}

		void OnRender()
		{
			timeSinceLastUpdate += engine.CameraInput.TimeDelta / 10;
			if (timeSinceLastUpdate >= updatePeriod && !busy)
			{
				busy = true;
				landscape.Reset();
				landscape.Tessellate();
				(engine as Test3DEngine).additionalStatistics = "allocated: " + landscape.AllocatedTris + " / " + landscape.desiredTris + " : " + landscape.FrameVariance;
				timeSinceLastUpdate = 0;
				busy = false;
			}
		}

		short[,] ReadHgtFile()
		{
			string filename = @"C:\Users\adrianj\Documents\Work\CAD\WebGIS_SRTM3\S37E174.hgt";
			short [,] heightMap = new short[MapSize + 1, MapSize + 1];
			byte[][] hgtLarge = ReadHgtFile(filename);
			byte [] b = new byte[2];
			float xScale = 1200.0f / (float)MapSize;
			float yScale = 1200.0f / (float)MapSize;
			for (int y = 0; y < heightMap.GetLength(0); y++)
			{
				int row = (int)((float)y * yScale);
				for (int x = 0; x < heightMap.GetLength(1); x++)
				{
					int col = 2*(int)((float)x * xScale);
					Array.Copy(hgtLarge[row], col, b, 0, 2);
					b = b.Reverse().ToArray();
					short s = BitConverter.ToInt16(b, 0);
					if (s == short.MinValue) s = 0;
					heightMap[y, x] = s;
				}
			}
			return heightMap;
		}

		byte [][] ReadHgtFile(string filename)
		{
			byte[][] bytes = new byte[1201][];
			using (BinaryReader reader = new BinaryReader(new FileStream(filename, FileMode.Open)))
			{
				for (int y = 1200; y >= 0; y--)
				{
					bytes[y] = reader.ReadBytes(1201*2);
				}
			}
			return bytes;
		}

		bool IsTriVisible(int leftX, int leftY, int rightX, int rightY, int apexX, int apexY)
		{
			Vector3 v0 = new Vector3(leftX, MapFetchFunction(leftX,leftY), leftY);
			Vector3 v1 = new Vector3(apexX, MapFetchFunction(apexX,apexY), apexY);
			Vector3 v2 = new Vector3(rightX, MapFetchFunction(rightX,rightY), rightY);
			return MathExtensions.TriangleFullyOnScreen(v0, v1, v2, landscape, engine.CameraInput.Camera);
			
		}

		bool SplitByDistance(int leftX, int leftY, int rightX, int rightY, int apexX, int apexY)
		{
			int centerX = (leftX + rightX) >> 1;
			int centerY = (leftY + rightY) >> 1;
			int centerZ = MapFetchFunction(centerX, centerY);
			Vector3 CameraPos = engine.CameraInput.Camera.Position;
			Vector3 center = new Vector3(centerX, centerZ, centerY);
			float distance = MapSize-(center - CameraPos).Length();
			distance = MathExtensions.Clamp(distance, 0, MapSize);
			float threshold = landscape.FrameVariance;
			return distance > threshold;
		}


		bool SplitByHeightVariance(int leftX, int leftY, int rightX, int rightY, int apexX, int apexY)
		{
			int centerX = (leftX + rightX) >> 1;		// Compute X coordinate of center of Hypotenuse
			int centerY = (leftY + rightY) >> 1;		// Compute Y coord...
			int variance = 0;

			// Get the height value at the middle of the Hypotenuse
			int centerZ = MapFetchFunction(centerX, centerY);
			int leftZ = MapFetchFunction(leftX, leftY);
			int rightZ = MapFetchFunction(rightX, rightY);

			// Variance of this triangle is the actual height at it's hypotenuse midpoint minus the interpolated height.
			variance = Math.Abs(centerZ - ((leftZ + rightZ) >> 1));
			// Also consider distance to camera.
			// If further from camera, increase variance threshold
			

			Vector3 CameraPos = engine.CameraInput.Camera.Position;
			Vector3 center = new Vector3(centerX, centerZ, centerY);
			float distance = (center - CameraPos).Length();
			float distanceFactor = MathExtensions.Clamp(MapSize - distance, 1, MapSize);

			float triVar = (float)variance * distanceFactor;
			if (triVar > 0)
			{
				Console.WriteLine("distance: " + distance + ", variance: " + variance + ", trivar: " + triVar);
			}
			float threshold = landscape.FrameVariance;

			bool b = triVar > threshold;// Land.FrameVariance;
			//if(b) Console.WriteLine("splitting: " + b + ", " + variance + " > " + threshold);
			return b;
		}

		bool SplitByScreenArea(int leftX, int leftY, int rightX, int rightY, int apexX, int apexY)
		{
			Vector3 v0 = new Vector3(leftX, MapFetchFunction(leftX, leftY), leftY);
			Vector3 v1 = new Vector3(apexX, MapFetchFunction(apexX, apexY), apexY);
			Vector3 v2 = new Vector3(rightX, MapFetchFunction(rightX, rightY), rightY);
			Vector3 v0wvp;
			Vector3 v1wvp;
			Vector3 v2wvp;
			MathExtensions.OnScreen os = MathExtensions.TriangleOnScreen(v0, v1, v2, landscape, engine.CameraInput.Camera, out v0wvp, out v1wvp, out v2wvp);
			
			v0wvp.Z = 0;
			v1wvp.Z = 0;
			v2wvp.Z = 0;
			float area = MathExtensions.TriangleArea(v0wvp, v1wvp, v2wvp);

			if (area < 0.002f) return false;
			return SplitByHeightVariance(leftX, leftY, rightX, rightY, apexX, apexY);
			
		}
		

		short MapFetchFunction(int worldX, int worldY)
		{
			return MapFetchFunction(worldX, worldY, heightMap);
		}

		public static short MapFetchFunction(int worldX, int worldY, short[,] hMap)
		{
			worldX = MathExtensions.Clamp(worldX, 0, hMap.GetLength(1));
			worldY = MathExtensions.Clamp(worldY, 0, hMap.GetLength(0));
			return hMap[worldY, worldX];
		}
	}
}
