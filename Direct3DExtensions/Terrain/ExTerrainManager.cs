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
	public class ExTerrainManager : DisposablePattern
	{
		protected Effect terrainEffect;
		protected Direct3DEngine engine;
		protected Mesh plane;

		public string PassName { get; set; }
		public Effect TerrainEffect { get { return terrainEffect; } }
		public int TerrainHeightOffset { get; set; }
		public bool AutoAdjustScaleBasedOnHeight { get; set; }


		public ExTerrainManager(MultipleEffect3DEngine engine, Effect effect)
		{
			this.terrainEffect = effect;
			this.PassName = "ExTerrain";
			this.engine = engine;


			plane = new TerrainMeshSet();
			MeshOptimiser.RemoveDuplicateVertices(plane);
			
			double yscale = EarthProjection.ConvertMetresToWorldUnits(1);
			plane.Scale = new Vector3(20, (float)yscale, 20);

			engine.InitializationComplete += (o, e) => { OnInitComplete(); };
			engine.PreRendering += (o, e) => { OnPreRender(); };
			engine.CameraChanged += (o, e) => { OnCameraChanged(e); };
		}



		void OnPreRender()
		{

		}

		void OnCameraChanged(CameraChangedEventArgs e)
		{
			if (e.PositionChanged)
			{
				SetTerrainScale();
				SetTerrainTranslation();
			}
		}

		void OnInitComplete()
		{
			engine.CameraInput.Camera.FarZ = 12000;
			engine.BindMesh(plane, PassName);
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

		private Vector3 GetScaleFromHeight(double height, float originalYScale)
		{
			TerrainHeightOffset = 0;
			Vector3 scale = new Vector3(1, originalYScale, 1);
			if (!AutoAdjustScaleBasedOnHeight)
				return scale;
			height = Math.Max(height - TerrainHeightOffset, 1);
			height = Math.Pow(height, 2.0 / 3.0);
			height = height / 8;
			height = MathExtensions.PowerOfTwo(height, 1);
			scale.X = (float)height;
			scale.Z = (float)height;
			return scale;
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
