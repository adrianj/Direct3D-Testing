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
		D3D.EffectScalarVariable InverseMapSize;
		D3D.Texture2D texture;
		D3D.ShaderResourceView textureResourceView;
		D3D.EffectResourceVariable textureResource;
		D3D.Device device;

		public override void Init(D3DDevice device)
		{
			this.device = device.Device;
			base.Init(device);

			float[,] heightMap = new float[256,256];
			Random rand = new Random(10);
			for (int i = 0; i < heightMap.GetLength(0); i++)
				for (int x = 0; x < heightMap.GetLength(1); x++)
					heightMap[i,x] = (float)(rand.NextDouble()) * 10;




			textureResource = effect.GetVariableByName("HeightMap").AsResource();

			WriteHeightDataToTexture(heightMap);



			InverseMapSize = effect.GetVariableByName("InverseMapSize").AsScalar();
			InverseMapSize.Set(0.001f);

		}

		public void WriteHeightDataToTexture<T>(T[,] heightMap) where T : IConvertible
		{
			int h = MathExtensions.PowerOfTwo(heightMap.GetLength(0));
			int w = MathExtensions.PowerOfTwo(heightMap.GetLength(1));
			if (texture == null || texture.Description.Width != w || texture.Description.Height != h)
			{
				RecreateTexture(w, h);
			}
			DataRectangle rect = texture.Map(0, D3D.MapMode.WriteDiscard, D3D.MapFlags.None);
			using (DataStream stream = rect.Data)
			{
				for (int y = 0; y < h; y++)
					for (int x = 0; x < w; x++)
					{
						if (y < heightMap.GetLength(0) && x < heightMap.GetLength(1))
							stream.Write(Convert.ToSingle(heightMap[y, x]));
						else
							stream.Write((float)0);
					}
			}
			texture.Unmap(0);

		}

		private void RecreateTexture(int width, int height)
		{
			D3D.Texture2DDescription tDesc = new D3D.Texture2DDescription()
			{
				ArraySize = 1,
				BindFlags = D3D.BindFlags.ShaderResource,
				CpuAccessFlags = D3D.CpuAccessFlags.Write,
				Format = SlimDX.DXGI.Format.R32_Float,
				Height = width,
				Width = height,
				MipLevels = 1,
				OptionFlags = D3D.ResourceOptionFlags.None,
				SampleDescription = new SlimDX.DXGI.SampleDescription(1, 0),
				Usage = D3D.ResourceUsage.Dynamic
			};
			DisposeUnmanaged();

			texture = new D3D.Texture2D(device, tDesc);
			textureResourceView = new D3D.ShaderResourceView(this.device, texture);

			this.device.VertexShader.SetShaderResource(textureResourceView, 0);
			textureResource.SetResource(textureResourceView);
		}

		public ExTerrainEffect()
			: base()
		{
			ShaderFilename = @"Effects\Terrain.fx";
		}


		void DisposeManaged() { }
		void DisposeUnmanaged()
		{
			if (texture != null) texture.Dispose(); texture = null;
			if (textureResourceView != null) textureResourceView.Dispose(); textureResourceView = null;
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
