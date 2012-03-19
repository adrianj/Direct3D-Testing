using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using SlimDX;
using D3D = SlimDX.Direct3D10;

namespace Direct3DExtensions.Texturing
{
	public class StagingTexture : ResourceView
	{

		protected D3D.Texture2D texture { get { return Resource as D3D.Texture2D; } }

		SlimDX.DXGI.Format format;

		public D3D.Texture2DDescription Description { get { return texture.Description; } }
		


		public StagingTexture(D3D.Device device, int width, int height, SlimDX.DXGI.Format format) : base(device)
		{
			this.format = format;
			RecreateTexture(width, height);
		}

		protected virtual void RecreateTexture(int width, int height)
		{
			D3D.Texture2DDescription description = CreateDescription(width, height);
			Resource = new D3D.Texture2D(device, description);
		}

		protected D3D.Texture2DDescription CreateDescription(int width, int height)
		{
			D3D.Texture2DDescription description = new D3D.Texture2DDescription()
			{
				ArraySize = 1,
				BindFlags = D3D.BindFlags.None,
				CpuAccessFlags = D3D.CpuAccessFlags.Read | D3D.CpuAccessFlags.Write,
				Format = format,
				Height = height,
				Width = width,
				MipLevels = 1,
				OptionFlags = D3D.ResourceOptionFlags.None,
				SampleDescription = new SlimDX.DXGI.SampleDescription(1, 0),
				Usage = D3D.ResourceUsage.Staging
			};
			return description;
		}

		public virtual void WriteTexture<T>(T[,] data) where T : IConvertible
		{
			int height = MathExtensions.PowerOfTwo(data.GetLength(0));
			int width = MathExtensions.PowerOfTwo(data.GetLength(1));
			if (texture == null || texture.Description.Width != width || texture.Description.Height != height)
			{
				RecreateTexture(width, height);
			}

			DataRectangle rect = texture.Map(0, D3D.MapMode.Write, D3D.MapFlags.None);

			using (DataStream stream = rect.Data)
			{
				for (int y = height - 1; y >= 0; y--)
					for (int x = 0; x < width; x++)
					{
						if (y < data.GetLength(0) && x < data.GetLength(1))
							stream.Write(Convert.ToSingle(data[y, x]));
						else
							stream.Write((float)0);
					}
			}

			texture.Unmap(0);
		}

		public virtual float[,] ReadTexture()
		{
			float[,] ret = new float[Description.Height, Description.Width];
			DataRectangle rect = texture.Map(0, D3D.MapMode.Read, D3D.MapFlags.None);
			using (DataStream stream = rect.Data)
			{
				for (int y = Description.Height - 1; y >= 0; y--)
					for (int x = 0; x < Description.Height; x++)
					{
						ret[y, x] = stream.Read<float>();
					}
			}
			texture.Unmap(0);
			return ret;
		}
	}
}
