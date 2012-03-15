using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using SlimDX;
using D3D = SlimDX.Direct3D10;

namespace Direct3DExtensions.Terrain
{
	public class ExTerrainEffect : WorldViewProjEffect
	{
		D3D.EffectScalarVariable LoresInverseMapSizeVar;
		D3D.EffectScalarVariable LoresMapSizeVar;
		D3D.EffectScalarVariable InverseMapSizeVar;
		D3D.EffectScalarVariable MapSizeVar;
		D3D.EffectScalarVariable ZoomLevelVar;
		D3D.EffectVectorVariable TerrainCentreLocationVar;
		D3D.EffectVectorVariable LoresTerrainCentreLocationVar;

		protected Texture hiresTexture;
		protected Texture loresTexture;


		public float InverseMapSize
		{
			get { if (InverseMapSizeVar != null) return InverseMapSizeVar.GetFloat(); return 0; }
			set { InverseMapSizeVar.Set(value); }
		}

		public float MapSize
		{
			get { if (MapSizeVar != null) return MapSizeVar.GetFloat(); return 0; }
			set { MapSizeVar.Set(value); }
		}

		public float LoresInverseMapSize
		{
			get { if (LoresInverseMapSizeVar != null) return LoresInverseMapSizeVar.GetFloat(); return 0; }
			set { LoresInverseMapSizeVar.Set(value); }
		}

		public float LoresMapSize
		{
			get { if (LoresMapSizeVar != null) return LoresMapSizeVar.GetFloat(); return 0; }
			set { LoresMapSizeVar.Set(value); }
		}

		Vector2 centreLoc = new Vector2();
		public Vector2 TerrainCentreLocation
		{
			get { return centreLoc; }
			set { centreLoc = value; TerrainCentreLocationVar.Set(centreLoc); }
		}

		Vector2 loresCentreLoc = new Vector2();
		public Vector2 LoresTerrainCentreLocation
		{
			get { return loresCentreLoc; }
			set { loresCentreLoc = value; LoresTerrainCentreLocationVar.Set(loresCentreLoc); }
		}

		public int ZoomLevel
		{
			get { if (ZoomLevelVar != null) return ZoomLevelVar.GetInt(); return 0; }
			set { ZoomLevelVar.Set(value); }
		}


		public override void Init(D3DDevice device)
		{
			base.Init(device);

			InverseMapSizeVar = effect.GetVariableByName("InverseMapSize").AsScalar();
			MapSizeVar = effect.GetVariableByName("MapSize").AsScalar();
			LoresInverseMapSizeVar = effect.GetVariableByName("LoresInverseMapSize").AsScalar();
			LoresMapSizeVar = effect.GetVariableByName("LoresMapSize").AsScalar();
			TerrainCentreLocationVar = effect.GetVariableByName("TerrainCentreLocation").AsVector();
			LoresTerrainCentreLocationVar = effect.GetVariableByName("LoresTerrainCentreLocation").AsVector();
			ZoomLevelVar = effect.GetVariableByName("ZoomLevel").AsScalar();

			InitTextures(device);

		}

		protected virtual void InitTextures(D3DDevice device)
		{
			float[,] heightMap = new float[256, 256];
			for (int i = 0; i < 256; i++)
				for (int k = 0; k < 256; k++)
					heightMap[i, k] = (i * 256 + k) * 0.02f;
			heightMap[128, 128] = 200;
			hiresTexture = new Texture(device.Device, effect, "HeightMap");
			loresTexture = new Texture(device.Device, effect, "LoresMap");
			WriteHiresTexture(heightMap);
			WriteLoresTexture(heightMap);
		}

		public virtual void WriteHiresTexture<T>(T[,] data) where T : IConvertible
		{
			hiresTexture.WriteTexture<T>(data);
			InverseMapSize = 1.0f / (float)hiresTexture.Height;
			MapSize = (float)hiresTexture.Height;
		}

		public void WriteLoresTexture<T>(T[,] data) where T : IConvertible
		{
			loresTexture.WriteTexture<T>(data);
			LoresInverseMapSize = 0.1f / (float)loresTexture.Height;
			LoresMapSize = (float)loresTexture.Height*10.0f;
		}


		public ExTerrainEffect()
			: base()
		{
			ShaderFilename = @"Effects\Terrain.fx";
		}


		void DisposeManaged() { }
		void DisposeUnmanaged()
		{
			if (hiresTexture != null) hiresTexture.Dispose(); hiresTexture = null;
			if (loresTexture != null) loresTexture.Dispose(); loresTexture = null;
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
