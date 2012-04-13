using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Drawing;
using System.Windows.Forms;
using System.Text;
using SlimDX;
using D3D = SlimDX.Direct3D10;

namespace Direct3DExtensions.Terrain
{
	public class ExTerrainManager : DisposablePattern
	{
		float heightAtCamera = 0;
		Texturing.StagingTexture minimapReadableCopy;
		bool planeSetup;
		protected Effect terrainEffect;
		protected MultipleEffect3DEngine engine;
		protected TerrainMeshSet plane;

		protected Vector2 cps;
		D3D.EffectVectorVariable cameraPosAtSetupVar;
		public Vector2 CameraPosAtSetup
		{
			get { return cps; }
			private set { cps = value; if (cameraPosAtSetupVar != null) cameraPosAtSetupVar.Set(cps); }
		}
		

		public string RenderPassName { get; set; }
		public string SetupPassName { get; set; }
		public Effect TerrainEffect { get { return terrainEffect; } }
		public int TerrainHeightOffset { get; set; }
		public bool AutoAdjustScaleBasedOnHeight { get; set; }


		public ExTerrainManager(MultipleEffect3DEngine engine, Effect effect)
		{
			this.terrainEffect = effect;
			this.RenderPassName = "ExTerrain";
			this.SetupPassName = "ExTerrainSetup";
			this.engine = engine;


			plane = new TerrainMeshSet();
			//MeshOptimiser.RemoveDuplicateVertices(plane);
			
			double yscale = EarthProjection.ConvertMetresToWorldUnits(1);
			plane.Scale = new Vector3(20, (float)yscale, 20);

			engine.InitializationComplete += (o, e) => { OnInitComplete(); };
			engine.PreRendering += (o, e) => { OnPreRender(); };
			engine.PostRendering += (o, e) => OnPostRender();
			engine.CameraChanged += (o, e) => { OnCameraChanged(e); };
		}


		void OnPostRender()
		{
			if (planeSetup)
			{
				CopyMinimap();
			}
		}


		void OnPreRender()
		{
			planeSetup = plane.doSetup;
			if(plane.doSetup)
			{
				Vector3 pos = engine.CameraInput.Camera.Position;
				CameraPosAtSetup = new Vector2(pos.X, pos.Z);
			}
		}

		private void CopyMinimap()
		{
			Size size = plane.MinimapSprite.Size;
			if (minimapReadableCopy == null)
				minimapReadableCopy = new Texturing.StagingTexture(engine.D3DDevice.Device, size.Width, size.Height, plane.MinimapSprite.Description.Format);
			else if (!size.Equals(minimapReadableCopy.Size))
			{
				minimapReadableCopy.Dispose();
				minimapReadableCopy = new Texturing.StagingTexture(engine.D3DDevice.Device, size.Width, size.Height, plane.MinimapSprite.Description.Format);
			}
			minimapReadableCopy.WriteTexture(plane.MinimapSprite.Texture);
		}

		Vector2 previousCameraLocation = new Vector2();
		float heightAGL = 1;

		void OnCameraChanged(CameraChangedEventArgs e)
		{
			if (e.PositionChanged)
			{
				SetTerrainScale();
				SetTerrainTranslation();
				Vector2 camLoc = new Vector2(e.Camera.Position.X, e.Camera.Position.Z);
				Vector2 planeLoc = new Vector2(plane.Translation.X, plane.Translation.Z);
				if (minimapReadableCopy == null) return;
				Size size = minimapReadableCopy.Size;
				
				Point centre = new Point(size.Width / 2, size.Height / 2);
				Vector2 camOffset = camLoc - planeLoc;
				centre = GetPointOnMinimap(camLoc);
				int i = minimapReadableCopy.ReadPixelAsInt(centre);
				i >>= 8;
				Vector4 v = (Vector4)minimapReadableCopy.ReadPixelAsColour(centre);
				Console.WriteLine(centre + ", Centre pixel: " + v + " = " + i);
				using (DataStream stream = minimapReadableCopy.GetReadStream())
				{
					
				}
				/*if (!camLoc.Equals(previousCameraLocation))
				{
					IEnumerable<VertexTypes.Pos3Norm3Tex3> gsOut = plane.ReadGSVertices();
					//float min = HeightAtPoint(gsOut, new Vector2(e.Camera.Position.X, e.Camera.Position.Z));
					float min = MeanHeight(gsOut);
					heightAtCamera = min;
					Console.WriteLine("" + heightAtCamera);
					previousCameraLocation = camLoc;
				}
				 */
			}

		}

		Point GetPointOnMinimap(Vector2 worldCoord)
		{
			float mapWidthInWorldUnits = 100 * plane.Scale.X;
			Vector2 minimapSize = new Vector2(minimapReadableCopy.Size.Width, minimapReadableCopy.Size.Height);
			Vector2 planeLoc = new Vector2(plane.Translation.X, plane.Translation.Z);

			// planeLoc actually doesn't move the minimap.
			Vector2 point = (CameraPosAtSetup);
			Console.WriteLine("plane: "+planeLoc+", camAtSetup: "+CameraPosAtSetup);
			point =  worldCoord - point;

			point = Vector2.Multiply(point, 1/mapWidthInWorldUnits);


			point = Vector2.Modulate(point, minimapSize);
			point = point + minimapSize * 0.5f;
			point = Vector2.Clamp(point, Vector2.Zero, minimapSize-new Vector2(1,1));
			return new Point((int)point.X, (int)point.Y);
		}


		float MeanHeight(IEnumerable<VertexTypes.Pos3Norm3Tex3> gsOut)
		{
			Dictionary<Vector2, float> uniquePoints = new Dictionary<Vector2, float>();
			float sum = 0;
			int i = 0;
			foreach (Vertex v in gsOut)
			{
				sum += v.Pos.Y;
				i++;
				if (i > 10000)
					break;
			}
			return sum / i;
		}

		class DVec : Tuple<Vector2, float>
		{
			public float Dist { get { return Item2; } }
			public Vector2 Loc { get { return Item1; } }
			public DVec(Vector2 loc, float dist) : base(loc,dist){}
			public static DVec MaxInList(IEnumerable<DVec> list)
			{
				DVec max = new DVec(Vector2.Zero, float.MinValue);
				foreach (DVec d in list)
					if (d.Dist > max.Dist)
						max = d;
				return max;
			}
			public static DVec MinInList(IEnumerable<DVec> list)
			{
				DVec min = new DVec(Vector2.Zero, float.MaxValue);
				foreach (DVec d in list)
					if (d.Dist < min.Dist)
						min = d;
				return min;
			}
		}

		float HeightAtPoint(IEnumerable<VertexTypes.Pos3Norm3Tex3> gsOut, Vector2 point)
		{

			Dictionary<DVec, Vertex> minList = new Dictionary<DVec, Vertex>();
			Dictionary<Vector2, float> uniquePoints = new Dictionary<Vector2, float>();
			DVec maxInList = new DVec(Vector2.Zero, float.MaxValue);

			foreach(Vertex v in gsOut)
			{
				Vector2 vNoHeight = new Vector2(v.Pos.X, v.Pos.Z);
				if (uniquePoints.ContainsKey(vNoHeight))
					continue;
				float d = Vector2.Subtract(point, vNoHeight).LengthSquared();
				uniquePoints[vNoHeight] = d;
				if (minList.Count < 4 || d < maxInList.Dist)
				{
					DVec dvec = new DVec(vNoHeight, d);
					if (minList.Count > 3)
						minList.Remove(maxInList);
					minList[dvec] = v;
					maxInList = DVec.MaxInList(minList.Keys);
				}
			}
			Vertex[] p = Sort(minList.Values);
			return BilinearInterp(p, point);
		}

		Vertex[] Sort(IEnumerable<Vertex> verts)
		{
			return verts.OrderBy(p => p.Pos.Z).ThenBy(p => p.Pos.X).Cast<Vertex>().ToArray();
		}

		float BilinearInterp(Vertex[] arrayOfFour, Vector2 point)
		{
			if (arrayOfFour.Length == 0)
				return 0;
			if (arrayOfFour.Length < 4)
				return arrayOfFour[0].Pos.Y;

			float x1 = (arrayOfFour[0].Pos.X + arrayOfFour[2].Pos.X) * 0.5f;
			float x2 = (arrayOfFour[1].Pos.X + arrayOfFour[3].Pos.X) * 0.5f;
			float y1 = (arrayOfFour[0].Pos.Z + arrayOfFour[1].Pos.Z) * 0.5f;
			float y2 = (arrayOfFour[2].Pos.Z + arrayOfFour[3].Pos.Z) * 0.5f;
			float x = point.X;
			float y = point.Y;

			float r1 = (x2 - x) / (x2 - x1) * arrayOfFour[0].Pos.Y + (x - x1) / (x2 - x1) * arrayOfFour[1].Pos.Y;
			if (float.IsNaN(r1))
				r1 = arrayOfFour[0].Pos.Y;
			float r2 = (x2 - x) / (x2 - x1) * arrayOfFour[2].Pos.Y + (x - x1) / (x2 - x1) * arrayOfFour[3].Pos.Y;
			if (float.IsNaN(r2))
				r2 = arrayOfFour[2].Pos.Y;

			float p = (y2 - y) / (y2 - y1) * r1 + (y - y1) / (y2 - y1) * r2;
			if (float.IsNaN(p))
				p = r1;

			return p;
		}

		void OnInitComplete()
		{
			engine.CameraInput.Camera.FarZ = 12000;
			engine.BindMesh(plane, SetupPassName);
			plane.BindToRenderPass(engine.D3DDevice, terrainEffect, terrainEffect.GetPassIndexByName(RenderPassName));
			cameraPosAtSetupVar = terrainEffect.GetVariableByName("CameraPosAtSetup").AsVector();
			SetTerrainScale();
			SetTerrainTranslation();
			if (engine is Sprite3DEngine)
			{
				Sprite3DEngine s = engine as Sprite3DEngine;
				plane.AddMinimapToSpriteEngine(s);
			}
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
			height = Math.Max(height - TerrainHeightOffset - heightAtCamera, 1);
			height = Math.Pow(height, 2.0 / 3.0);
			height = height / 8;
			height = MathExtensions.PowerOfTwo(height, 1);
			scale.X = (float)height;
			scale.Z = (float)height;
			return scale;
		}

		#region dispose

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
		#endregion

	}
}
