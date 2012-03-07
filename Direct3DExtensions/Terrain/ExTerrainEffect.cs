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
		D3D.EffectVectorVariable TerrainCentreLocationVar;

		Texture hiresTexture;


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


		public override void Init(D3DDevice device)
		{
			base.Init(device);

			float[,] heightMap = new float[4,4];

			hiresTexture = new Texture(device.Device, effect, "HeightMap");

			InverseMapSizeVar = effect.GetVariableByName("InverseMapSize").AsScalar();
			MapSizeVar = effect.GetVariableByName("MapSize").AsScalar();
			LoresInverseMapSizeVar = effect.GetVariableByName("LoresInverseMapSize").AsScalar();
			LoresMapSizeVar = effect.GetVariableByName("LoresMapSize").AsScalar();
			TerrainCentreLocationVar = effect.GetVariableByName("TerrainCentreLocation").AsVector();

			WriteHeightDataToTexture(heightMap);

		}

		public void WriteHeightDataToTexture<T>(T[,] data) where T : IConvertible
		{
			hiresTexture.WriteDataToTexture<T>(data);
			InverseMapSize = 1.0f / (float)hiresTexture.Height;
			MapSize = (float)hiresTexture.Height;
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
