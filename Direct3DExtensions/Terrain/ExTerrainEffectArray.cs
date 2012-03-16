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
		TextureArray texArray;

		public ExTerrainEffectArray()
			: base()
		{
			//ShaderFilename = @"Effects\Terrain.fx";
			ShaderFilename = @"Effects\Terrain_TexArray.fx";
		}

		protected override void InitTextures(D3DDevice device)
		{
			int w = 64;
			float[,] heightMap = new float[w, w];
			for (int y = 0; y < w; y++)
				for (int x = 0; x < w; x++)
					heightMap[y, x] = (y * w + x) * 0.02f;
			heightMap[w/2, w/2] = 200;
			hiresTexture = new Texture(device.Device, effect, "HeightMap");
			loresTexture = new Texture(device.Device, effect, "LoresMap");

			texArray = new TextureArray(device.Device, effect, "HiresMap", 4);

			WriteHiresTexture(heightMap, 0);
			texArray.WriteTexture(heightMap, 0);
			texArray.WriteTexture(heightMap, 1);
			texArray.WriteTexture(heightMap, 2);
			texArray.WriteTexture(heightMap, 3);

			WriteHiresTexture(heightMap);
			WriteLoresTexture(heightMap);
		}

		public override void WriteHiresTexture<T>(T[,] data)
		{
			WriteHiresTexture(data, 0);
		}

		public void WriteHiresTexture<T>(T[,] data, int arrayIndex) where T : IConvertible
		{
			texArray.WriteTexture(data,arrayIndex);
			InverseMapSize = 1.0f / (float)texArray.tex2DArray.Description.Width;
			MapSize = (float)texArray.tex2DArray.Description.Width;
		}

	}
}
