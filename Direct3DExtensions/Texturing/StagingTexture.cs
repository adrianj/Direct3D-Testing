using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
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

		public StagingTexture(D3D.Device device, string filename)
			: base(device)
		{
			D3D.ImageLoadInformation iml = new D3D.ImageLoadInformation()
			{
				BindFlags = D3D.BindFlags.None,
				CpuAccessFlags = D3D.CpuAccessFlags.Read | D3D.CpuAccessFlags.Write,
				Depth = 1,
				FilterFlags = D3D.FilterFlags.None,
				FirstMipLevel = 0,
				Format = SlimDX.DXGI.Format.R8G8B8A8_UNorm,
				MipFilterFlags = D3D.FilterFlags.None,
				MipLevels = 1,
				OptionFlags = D3D.ResourceOptionFlags.None,
				Usage = D3D.ResourceUsage.Staging
			};
			Resource = D3D.Texture2D.FromFile(device, filename, iml);
		}

		protected virtual void RecreateTexture(int width, int height)
		{
			if (Resource != null)
				Resource.Dispose();
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

		public virtual void WriteTexture<T>(T[] data, int width) where T : IConvertible
		{
			int height = data.Length / width;
			this.format = SlimDX.DXGI.Format.R32_Float;
			CheckSizeFormatAndRecreate(height, width);
			DataRectangle rect = texture.Map(0, D3D.MapMode.Write, D3D.MapFlags.None);

			using (DataStream stream = rect.Data)
			{
				stream.WriteRange<float>(data.Cast<float>().ToArray());
			}

			texture.Unmap(0);
		}

		public virtual void WriteTexture<T>(T[,] data) where T : IConvertible
		{
			int height = data.GetLength(0);
			int width = data.GetLength(1);
			this.format = SlimDX.DXGI.Format.R32_Float;
			CheckSizeFormatAndRecreate(height, width);

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

		public virtual void WriteTexture(byte[] argbColours, int width)
		{
			int height = argbColours.Length / width / 4;
			this.format = SlimDX.DXGI.Format.R8G8B8A8_UNorm;
			CheckSizeFormatAndRecreate(height, width);

			DataRectangle rect = texture.Map(0, D3D.MapMode.Write, D3D.MapFlags.None);

			using (DataStream stream = rect.Data)
			{
				for (int i = 0; i < argbColours.Length; i += 4)
				{
					stream.Write<byte>(argbColours[i+2]);  // DXGI format is BGRA, GDI format is RGBA.
					stream.Write<byte>(argbColours[i+1]);
					stream.Write<byte>(argbColours[i]);
					stream.Write<byte>(argbColours[i+3]); 
				}
			}

			texture.Unmap(0);
		}

		public virtual void WriteTexture(Image image)
		{
			BitmapData bData = (image as Bitmap).LockBits(new Rectangle(0, 0, image.Width, image.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
			using(DataStream stream = new DataStream(bData.Scan0, bData.Stride * bData.Height, true, false))
			{
				WriteTexture(stream.ReadRange<byte>((int)stream.Length), image.Width);
			}
			(image as Bitmap).UnlockBits(bData);
		}

		private void CheckSizeFormatAndRecreate(int height, int width)
		{
			if (texture == null || texture.Description.Format != this.format || texture.Description.Width != width || texture.Description.Height != height)
			{
				RecreateTexture(width, height);
			}
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
