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
		D3D.Texture2D[] texArray = new D3D.Texture2D[4];

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
			/*
			D3D.Texture2DDescription tdesc = Texture.CreateWritableTextureDescription(256, 256, D3D.BindFlags.ShaderResource, SlimDX.DXGI.Format.R32_Float);
			tdesc.ArraySize = 1;
			texArray[0] = new D3D.Texture2D(device.Device, tdesc);
			D3D.EffectResourceVariable rVar = effect.GetVariableByName("HiresMap").AsResource();
			rVar.SetResourceArray();

			D3D.ShaderResourceView rView = new D3D.ShaderResourceView(device.Device, texArray[0]);
			DataBox src = new DataBox(4 * 256, 4 * 256 * 256, new DataStream(heightMap, true, true));
			device.Device.UpdateSubresource(src, rVar, 0, new D3D.ResourceRegion());
			*/
			WriteHiresTexture(heightMap);
			WriteLoresTexture(heightMap);
		}

		public override void WriteHiresTexture<T>(T[,] data)
		{
			base.WriteHiresTexture<T>(data);
			InverseMapSize = 1.0f / (float)hiresTexture.Height;
			MapSize = (float)hiresTexture.Height;
		}

		public void WriteHiresTexture<T>(T[,] data, int arrayIndex) where T : IConvertible
		{
			hiresTexture.WriteTexture<T>(data, arrayIndex);
			InverseMapSize = 1.0f / (float)hiresTexture.Height;
			MapSize = (float)hiresTexture.Height;
		}
	}
}
