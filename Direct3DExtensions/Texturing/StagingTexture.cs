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
		protected SlimDX.DXGI.Format format;

		public D3D.Texture2D Texture { get { return Resource as D3D.Texture2D; } }
		public D3D.Texture2DDescription Description { get { return Texture.Description; } }
		public Size Size { get { return GetSize(); } }

		public StagingTexture(D3D.Device device) : base(device) { }

		public StagingTexture(D3D.Device device, int width, int height) : this(device, width, height, SlimDX.DXGI.Format.R8G8B8A8_UNorm) { }

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

		public Size GetSize()
		{
			if (Texture == null) return new Size();
			return new Size(Description.Width, Description.Height);
		}

		protected virtual void RecreateTexture(int width, int height)
		{
			if (Resource != null)
				Resource.Dispose();
			D3D.Texture2DDescription description = CreateDescription(width, height);
			Resource = new D3D.Texture2D(device, description);
		}

		protected virtual D3D.Texture2DDescription CreateDescription(int width, int height)
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

		public virtual void WriteTexture(D3D.Texture2D existingTexture)
		{
			device.CopyResource(existingTexture, Resource);
		}

		public virtual void WriteTexture<T>(T[] data, int width) where T : IConvertible
		{
			int height = data.Length / width;
			this.format = SlimDX.DXGI.Format.R32_Float;
			CheckSizeFormatAndRecreate(height, width);

			using (DataStream stream = GetWriteStream())
			{
				stream.WriteRange<float>(data.Cast<float>().ToArray());
			}

			Texture.Unmap(0);
		}

		public virtual void WriteTexture<T>(T[,] data) where T : IConvertible
		{
			int height = data.GetLength(0);
			int width = data.GetLength(1);
			this.format = SlimDX.DXGI.Format.R32_Float;
			CheckSizeFormatAndRecreate(height, width);

			using (DataStream stream = GetWriteStream())
				for (int y = height - 1; y >= 0; y--)
					for (int x = 0; x < width; x++)
					{
						stream.Write(Convert.ToSingle(data[y, x]));
					}



			Texture.Unmap(0);
		}

		public virtual void WriteTexture(byte[] argbColours, int width, bool convertRGBAtoBGRA = true)
		{
			int height = argbColours.Length / width / 4;
			this.format = SlimDX.DXGI.Format.R8G8B8A8_UNorm;
			CheckSizeFormatAndRecreate(height, width);

			using (DataStream stream = GetWriteStream())
			{
				for (int i = 0; i < argbColours.Length; i += 4)
				{
					byte[] components = new byte[] { argbColours[i], argbColours[i + 1], argbColours[i + 2], argbColours[i + 3] };
					WriteColourToStream(stream, components,convertRGBAtoBGRA);
				}
			}

			Texture.Unmap(0);
		}

		public static void WriteColourToStream(DataStream stream, byte[] components, bool convertRGBAtoBGRA)
		{
			byte alpha = components[3];
			if (convertRGBAtoBGRA)
			{
				stream.Write<byte>(components[2]);  // DXGI format is BGRA, GDI format is RGBA.
				stream.Write<byte>(components[1]);
				stream.Write<byte>(components[0]);
			}
			else
			{
				stream.Write<byte>(components[0]);  // DXGI format is BGRA, GDI format is RGBA.
				stream.Write<byte>(components[1]);
				stream.Write<byte>(components[2]);
			}
			stream.Write<byte>(alpha);
		}

		public virtual void WriteTexture(Image image, bool convertGRBAtoBGRA = true)
		{
			BitmapData bData = (image as Bitmap).LockBits(new Rectangle(0, 0, image.Width, image.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
			using(DataStream stream = new DataStream(bData.Scan0, bData.Stride * bData.Height, true, false))
			{
				WriteTexture(stream.ReadRange<byte>((int)stream.Length), image.Width, convertGRBAtoBGRA);
			}
			(image as Bitmap).UnlockBits(bData);
		}

		private void CheckSizeFormatAndRecreate(int height, int width)
		{
			if (Texture == null || Texture.Description.Format != this.format || Texture.Description.Width != width || Texture.Description.Height != height)
			{
				Console.WriteLine("Recreating staging texture, "+width+","+height);
				RecreateTexture(width, height);
			}
		}

		public virtual Image ConvertToImage()
		{
			Bitmap bmp = new Bitmap(this.Description.Width, this.Description.Height, PixelFormat.Format32bppArgb);
			BitmapData bData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.WriteOnly, bmp.PixelFormat);
			using (DataStream stream = new DataStream(bData.Scan0, bData.Stride * bData.Height, false, true))
			{
				using (DataStream texStream = GetReadStream())
				{
					byte[] bytes = texStream.ReadRange<byte>((int)stream.Length);
					stream.WriteRange<byte>(bytes);
				}
			}
			bmp.UnlockBits(bData);
			return bmp;
		}

		public virtual DataStream GetWriteStream()
		{
			DataRectangle rect;
			if (Description.Usage == D3D.ResourceUsage.Staging)
				rect = Texture.Map(0, D3D.MapMode.Write, D3D.MapFlags.None);
			else
				rect = Texture.Map(0, D3D.MapMode.WriteDiscard, D3D.MapFlags.None);
			return rect.Data;
		}

		public DataStream GetReadStream() { int i; return GetReadStream(out i); }
		public virtual DataStream GetReadStream(out int dataPitch)
		{
			if ((Description.CpuAccessFlags & D3D.CpuAccessFlags.Read) != D3D.CpuAccessFlags.Read)
				throw new InvalidOperationException("Texture was not created with read access.  Using a staging texture intermediate");
			DataRectangle rect = Texture.Map(0, D3D.MapMode.Read, D3D.MapFlags.None);
			dataPitch = rect.Pitch;
			return rect.Data;
		}

		public virtual float[,] ReadTexture()
		{
			float[,] ret = new float[Description.Height, Description.Width];
			DataRectangle rect = Texture.Map(0, D3D.MapMode.Read, D3D.MapFlags.None);
			using (DataStream stream = rect.Data)
			{
				for (int y = Description.Height - 1; y >= 0; y--)
					for (int x = 0; x < Description.Height; x++)
					{
						ret[y, x] = stream.Read<float>();
					}
			}
			Texture.Unmap(0);
			return ret;
		}

		public virtual Color4 ReadPixelAsColour(Point p)
		{
			int pitch;
			using (DataStream stream = GetReadStream(out pitch))
			{
				int formatSize = pitch / Description.Width;
				stream.Seek(p.X * formatSize + p.Y * pitch, System.IO.SeekOrigin.Begin);
				Color4 ret = new Color4();
				ret.Alpha = stream.Read<byte>();
				ret.Blue = stream.Read<byte>();
				ret.Green = stream.Read<byte>();
				ret.Red = stream.Read<byte>();
				return ret;
			}
		}

		public virtual float ReadPixelAsFloat(Point p)
		{
			int pitch;
			using (DataStream stream = GetReadStream(out pitch))
			{
				int formatSize = pitch / Description.Width;
				stream.Seek(p.X * formatSize + p.Y * pitch, System.IO.SeekOrigin.Begin);
				byte[] bytes = stream.ReadRange<byte>(4);
				bytes = bytes.Reverse().ToArray();
				return BitConverter.ToSingle(bytes, 0);
			}
		}

		public virtual int ReadPixelAsInt(Point p)
		{
			int pitch;
			using (DataStream stream = GetReadStream(out pitch))
			{
				int formatSize = pitch / Description.Width;
				stream.Seek(p.X * formatSize + p.Y * pitch, System.IO.SeekOrigin.Begin);
				byte[] bytes = stream.ReadRange<byte>(4);
				bytes = bytes.Reverse().ToArray();
				return BitConverter.ToInt32(bytes, 0);
			}
		}
	}
}
