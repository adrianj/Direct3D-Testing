using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Drawing;
using System.Windows.Forms;
using System.Text;
using SlimDX;

namespace Direct3DExtensions.Terrain
{
	public class ClipmapTerrainManager : DisposablePattern
	{
		ExTerrainEffect terrainEffect;
		Direct3DEngine engine;
		Mesh plane;
		short[,] hiresTerrain;
		short[,] loresTerrain;
		float WorldUnitsPerDegree = 1024.0f;
		float HiresResolution = 0.125f;
		float LoresResolution = 1.0f;
		double HiresWidth = 0.50000;
		double LoresWidth = 10;
		Vector2 initialTerrainLocation = new Vector2(175f, -37f);
		Vector2 currentCameraLocation;
		Vector2 currentTerrainLocation;

		public ExTerrainEffect TerrainEffect { get { return terrainEffect; } }

		public ClipmapTerrainManager(MultipleEffect3DEngine engine)
		{
			this.engine = engine;
			terrainEffect = new ExTerrainEffectArray();
			engine.AddEffect(terrainEffect);
			plane = new TerrainMeshSet();
			plane.Scale = new Vector3(20, 0.04f, 20);

			engine.InitializationComplete += (o, e) => { OnInitComplete(); };
			engine.PreRendering += (o, e) => { OnPreRender(); };
			engine.CameraChanged += (o, e) => { OnCameraChanged(e); };
		}

		public short[,] GetHiresTerrain() { return hiresTerrain; }

		public short[,] GetLoresTerrain() { return loresTerrain; }


		void OnPreRender()
		{
			if (engine.CameraInput.Input.IsKeyPressed(Keys.P))
			{
				terrainEffect.ZoomLevel = (terrainEffect.ZoomLevel + 1) % 3;
			}
		}

		void OnCameraChanged(CameraChangedEventArgs e)
		{
			if (e.PositionChanged)
			{
				SetTerrainScale();
				SetTerrainTranslation();
				SetHeightmapTranslation();

				(engine as Test3DEngine).additionalStatistics = "Scale: " + plane.Scale +
					"\nPos: " + plane.Translation + "\n: var: " + terrainEffect.InverseMapSize;

			}
		}

		void OnInitComplete()
		{
			engine.CameraInput.Camera.FarZ = 12000;
			engine.BindMesh(plane, "ExTerrain");
			ResetTerrain(initialTerrainLocation);
			SetTerrainScale();
			SetTerrainTranslation();
		}

		private void SetTerrainTranslation()
		{
			Vector3 pos = engine.CameraInput.Camera.Position;
			plane.Translation = new Vector3(pos.X, 0, pos.Z);
		}

		private void SetTerrainScale()
		{
			Vector3 pos = engine.CameraInput.Camera.Position;
			Vector3 scale = plane.Scale;
			plane.Scale = GetScaleFromHeight(pos.Y, scale.Y);
		}

		private void SetHeightmapTranslation()
		{
			Vector2 offset = CameraLocationInDegrees() - WorldUnitsToDegrees(currentCameraLocation);
			offset = MathExtensions.Round(offset, HiresResolution);
			if (offset != Vector2.Zero)
			{
				Console.WriteLine("\nBefore: ");
				Console.WriteLine("PrevTerrainLoc: " + currentTerrainLocation + ", offsetDeg: " + offset + ", initialCam: " + currentCameraLocation);
				currentTerrainLocation = currentTerrainLocation + offset;
				//currentCameraLocation = currentCameraLocation + DegreesToWorldUnits(offset);
				currentCameraLocation = currentCameraLocation + DegreesToWorldUnits(offset);
				//ResetHiresTerrain(currentTerrainLocation);
				//terrainEffect.WriteHiresTexture(new short[64, 256], 128, 128);
				Console.WriteLine("After: ");
				Console.WriteLine("PrevTerrainLoc: " + currentTerrainLocation + ", offsetDeg: " + offset + ", initialCam: " + currentCameraLocation);
			}
		}

		private Vector3 GetScaleFromHeight(double height, float originalYScale)
		{
			Vector3 scale = new Vector3(1, originalYScale, 1);
			height = Math.Pow(height, 2.0 / 3.0);
			height = height / 8;
			height = MathExtensions.PowerOfTwo(height, 1);
			scale.X = (float)height;
			scale.Z = (float)height;
			return scale;
		}


		private void ResetTerrain(Vector2 terrainLoc)
		{
			currentTerrainLocation = terrainLoc;
			//currentCameraLocation = DegreesToWorldUnits(MathExtensions.Round(CameraLocationInDegrees(), HiresResolution));
			currentCameraLocation = CameraLocationInWorldUnits();
			ResetHiresTerrain(terrainLoc);
			ResetLoresTerrain(terrainLoc);
		}
		private void ResetHiresTerrain(Vector2 terrainLoc)
		{
			TerrainHeightTextureFetcher fetcher = new Strm3TextureFetcher();
			Vector2 terrainLocation = MathExtensions.Round(terrainLoc, HiresResolution);
			hiresTerrain = fetcher.FetchTerrain(GetCentredSquareRectangle(terrainLocation, DegreesToWorldUnits(HiresWidth) / 1200.0f));
			terrainEffect.WriteHiresTexture(hiresTerrain);
			Console.WriteLine("TerrainCentreLoc: " + currentCameraLocation);
			terrainEffect.TerrainCentreLocation = currentCameraLocation * 1200.0f/1024.0f;
		}

		private void ResetLoresTerrain(Vector2 terrainLoc)
		{
			TerrainHeightTextureFetcher fetcher = new Strm30TextureFetcher();
			Vector2 terrainLocation = MathExtensions.Round(terrainLoc, LoresResolution);

			loresTerrain = fetcher.FetchTerrain(GetCentredSquareRectangle(terrainLocation, DegreesToWorldUnits(LoresWidth) / 1200.0f));
			terrainEffect.WriteLoresTexture(loresTerrain);
			terrainEffect.LoresTerrainCentreLocation = currentCameraLocation;
		}

		Vector2 CameraLocationInDegrees()
		{
			Vector2 camPos = CameraLocationInWorldUnits();
			return WorldUnitsToDegrees(camPos);
		}

		private Vector2 CameraLocationInWorldUnits()
		{
			Vector2 camPos = MathExtensions.Vector3XZ(engine.CameraInput.Camera.Position);
			return camPos;
		}


		Vector2 DegreesToWorldUnits(Vector2 degrees)
		{
			return new Vector2(DegreesToWorldUnits(degrees.X), DegreesToWorldUnits(degrees.Y));
		}

		float DegreesToWorldUnits(double degrees)
		{
			return (float)degrees * WorldUnitsPerDegree;
		}

		Vector2 WorldUnitsToDegrees(Vector2 worldUnits)
		{
			return new Vector2(WorldUnitsToDegrees(worldUnits.X), WorldUnitsToDegrees(worldUnits.Y));
		}


		float WorldUnitsToDegrees(double worldUnits)
		{
			return (float)worldUnits / WorldUnitsPerDegree;
		}

		RectangleF GetCentredSquareRectangle(Vector2 centre, float width)
		{
			PointF point = new PointF(centre.X - width * 0.5f, centre.Y - width * 0.5f);
			SizeF size = new SizeF(width, width);
			RectangleF rect = new RectangleF(point, size);
			return rect;
		}


		void DisposeManaged() { }
		void DisposeUnmanaged()
		{
			if (plane != null) plane.Dispose(); plane = null;
			if (terrainEffect != null) terrainEffect.Dispose(); terrainEffect = null;
		}

		bool disposed = false;
		protected override void Dispose(bool disposing)
		{
			if (!disposed)
			{
				if (disposing)
				{
					DisposeManaged();
				}

				DisposeUnmanaged();
				disposed = true;
			}
			base.Dispose(disposing);
		}
    
	}
}
