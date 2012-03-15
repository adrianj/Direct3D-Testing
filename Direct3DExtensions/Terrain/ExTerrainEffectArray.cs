using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using SlimDX;
using SlimDX.Direct3D10;
using SlimDX.DXGI;
using D3D = SlimDX.Direct3D10;

namespace Direct3DExtensions.Terrain
{
	public class ExTerrainEffectArray : ExTerrainEffect
	{

		public ExTerrainEffectArray()
			: base()
		{
			ShaderFilename = @"Effects\Terrain.fx";
			//ShaderFilename = @"Effects\Terrain_TexArray.fx";
		}

		protected override void InitTextures(D3DDevice device)
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

		public override void WriteHiresTexture<T>(T[,] data)
		{
			base.WriteHiresTexture<T>(data);
			InverseMapSize = 1.0f / (float)hiresTexture.Height;
			MapSize = (float)hiresTexture.Height;
		}

	}
}
