using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using SlimDX;
using D3D = SlimDX.Direct3D10;

namespace Direct3DExtensions
{
	public class Texture : DisposablePattern
	{
		D3D.Texture2D texture;
		D3D.ShaderResourceView textureView;
		D3D.EffectResourceVariable textureResource;
		D3D.Device device;

		public D3D.Texture2DDescription Description { get { if (texture != null) return texture.Description; return new D3D.Texture2DDescription(); } }

		public int Width { get { if (texture == null) return 0; return texture.Description.Width; } }
		public int Height { get { if (texture == null) return 0; return texture.Description.Height; } }

		public D3D.Texture2D Texture2D { get { return texture; } }
		public D3D.ShaderResourceView TextureView { get { return textureView; } }

		public Texture(D3D.Device device)
		{
			this.device = device;
		}


		public Texture(D3D.Device device, D3D.Effect effect, string textureName) : this(device)
		{
			BindToEffect(effect, textureName);
		}

		public void BindToEffect(D3D.Effect effect, string textureName)
		{
			textureResource = effect.GetVariableByName(textureName).AsResource();
		}

		public void WriteTexture<T>(T[,] data) where T : IConvertible
		{
			int height = MathExtensions.PowerOfTwo(data.GetLength(0));
			int width = MathExtensions.PowerOfTwo(data.GetLength(1));
			if (!QueryLargeSize(width, height)) return;
			if (texture == null || texture.Description.Width != width || texture.Description.Height != height)
			{
				RecreateTexture(width, height);
			}
			WriteToTexture<T>(texture, data, height, width);
		}

		public static void WriteToTexture<T>(D3D.Texture2D texture, T[,] data, int height, int width) where T : IConvertible
		{
			DataRectangle rect = null;
			try
			{
				rect = texture.Map(0, D3D.MapMode.WriteDiscard, D3D.MapFlags.None);
			}
			catch (D3D.Direct3D10Exception)
			{
				rect = texture.Map(0, D3D.MapMode.Write, D3D.MapFlags.None);
			}
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

		public void RecreateTexture(D3D.Texture2DDescription tDesc)
		{
			DisposeUnmanaged();

			
			texture = new D3D.Texture2D(device, tDesc);
			if (tDesc.BindFlags == D3D.BindFlags.ShaderResource)
			{
				textureView = new D3D.ShaderResourceView(device, texture);

				this.device.VertexShader.SetShaderResource(textureView, 0);
				if (textureResource != null) textureResource.SetResource(textureView);
			}
		}

		public void RecreateTexture(int width, int height)
		{
			if(texture == null)
				RecreateTexture(CreateWritableTextureDescription(width, height, D3D.BindFlags.ShaderResource, SlimDX.DXGI.Format.R32_Float));
			else
				RecreateTexture(CreateWritableTextureDescription(width, height, Description.BindFlags, Description.Format));
			
		}

		public static Texture CreateStagingTexture(D3D.Device device, int width, int height, SlimDX.DXGI.Format PixelFormat)
		{
			Texture tex = new Texture(device);
			var tDesc = CreateWritableTextureDescription(width, height, D3D.BindFlags.None, PixelFormat);
			tDesc.Usage = D3D.ResourceUsage.Staging;
			tDesc.Format = PixelFormat;
			tDesc.CpuAccessFlags = D3D.CpuAccessFlags.Read | D3D.CpuAccessFlags.Write;
			tDesc.Format = SlimDX.DXGI.Format.R8G8B8A8_UNorm;
			tDesc.BindFlags = D3D.BindFlags.None;
			tex.RecreateTexture(tDesc);
			return tex;
		}

		public static Texture CreateWritableTexture(D3D.Device device, int width, int height, D3D.BindFlags ShaderBinding, SlimDX.DXGI.Format PixelFormat)
		{
			Texture tex = new Texture(device);
			var tDesc = CreateWritableTextureDescription(width, height, D3D.BindFlags.None, PixelFormat);
			tDesc.Format = PixelFormat;
			tDesc.BindFlags = ShaderBinding;
			tex.RecreateTexture(tDesc);
			return tex;
		}

		public static D3D.Texture2DDescription CreateWritableTextureDescription(int width, int height, D3D.BindFlags ShaderBinding, SlimDX.DXGI.Format PixelFormat)
		{
			D3D.Texture2DDescription tDesc = new D3D.Texture2DDescription()
			{
				ArraySize = 1,
				BindFlags = ShaderBinding,
				CpuAccessFlags = D3D.CpuAccessFlags.Write,
				Format = PixelFormat,
				Height = width,
				Width = height,
				MipLevels = 1,
				OptionFlags = D3D.ResourceOptionFlags.None,
				SampleDescription = new SlimDX.DXGI.SampleDescription(1, 0),
				Usage = D3D.ResourceUsage.Dynamic
			};
			return tDesc;
		}

		bool QueryLargeSize(int width, int height)
		{
			double sizeInMB = (width * height * 4) / 1024.0 / 1024.0;
			if (sizeInMB >= 256)
			{
				System.Windows.Forms.MessageBox.Show("Image size of " + sizeInMB + " MB is crazy. Not updating.");
				return false;
			}
			if (sizeInMB >= 64)
			{
				return System.Windows.Forms.MessageBox.Show("Image size of " + sizeInMB + " MB is really quite high. Continue?",
					"Large File Texture", System.Windows.Forms.MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes;
			}
			return true;
		}

		void DisposeManaged() { }
		void DisposeUnmanaged()
		{
			if (texture != null) texture.Dispose(); texture = null;
			if (textureView != null) textureView.Dispose(); textureView = null;
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
