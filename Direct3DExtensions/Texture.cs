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


		public D3D.BindFlags ShaderBinding { get; set; }
		public SlimDX.DXGI.Format PixelFormat { get; set; }

		public int Width { get { if (texture == null) return 0; return texture.Description.Width; } }
		public int Height { get { if (texture == null) return 0; return texture.Description.Height; } }

		public Texture(D3D.Device device, D3D.Effect effect, string textureName)
		{
			ShaderBinding = D3D.BindFlags.ShaderResource;
			PixelFormat = SlimDX.DXGI.Format.R32_Float;
			this.device = device;
			textureResource = effect.GetVariableByName(textureName).AsResource();
		}

		public void WriteTexture<T>(T[,] data) where T : IConvertible
		{ WriteTexture(data, 0); }

		public void WriteTexture<T>(T[,] data, int arrayIndex) where T : IConvertible
		{
			int height = MathExtensions.PowerOfTwo(data.GetLength(0));
			int width = MathExtensions.PowerOfTwo(data.GetLength(1));
			if (!QueryLargeSize(width, height)) return;
			if (texture == null || texture.Description.Width != width || texture.Description.Height != height)
			{
				RecreateTexture(width, height);
			}
			DataRectangle rect = texture.Map(0, D3D.MapMode.WriteDiscard, D3D.MapFlags.None);
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


		public void RecreateTexture(int width, int height)
		{
			D3D.Texture2DDescription tDesc = CreateWritableTextureDescription(width, height, ShaderBinding, PixelFormat);

			DisposeUnmanaged();
			
			texture = new D3D.Texture2D(device, tDesc);
			textureView = new D3D.ShaderResourceView(device, texture);

			this.device.VertexShader.SetShaderResource(textureView, 0);
			textureResource.SetResource(textureView);
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
