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
		D3D.Texture2D hiresTexture;
		D3D.ShaderResourceView hiresTextureView;
		D3D.EffectResourceVariable hiresTextureResource;
		D3D.Device device;

		public int Width { get { if (hiresTexture == null) return 0; return hiresTexture.Description.Width; } }
		public int Height { get { if (hiresTexture == null) return 0; return hiresTexture.Description.Height; } }

		public Texture(D3D.Device device, D3D.Effect effect, string textureName)
		{
			this.device = device;
			hiresTextureResource = effect.GetVariableByName(textureName).AsResource();
		}

		public void WriteDataToTexture<T>(T[,] data) where T : IConvertible
		{
			int height = MathExtensions.PowerOfTwo(data.GetLength(0));
			int width = MathExtensions.PowerOfTwo(data.GetLength(1));
			if (!QueryLargeSize(width, height)) return;
			if (hiresTexture == null || hiresTexture.Description.Width != width || hiresTexture.Description.Height != height)
			{
				RecreateTexture(width, height);
			}
			DataRectangle rect = hiresTexture.Map(0, D3D.MapMode.WriteDiscard, D3D.MapFlags.None);
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
			hiresTexture.Unmap(0);
		}

		public void RecreateTexture(int width, int height)
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

			hiresTexture = new D3D.Texture2D(device, tDesc);
			hiresTextureView = new D3D.ShaderResourceView(this.device, hiresTexture);

			this.device.VertexShader.SetShaderResource(hiresTextureView, 0);
			hiresTextureResource.SetResource(hiresTextureView);
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
			if (hiresTexture != null) hiresTexture.Dispose(); hiresTexture = null;
			if (hiresTextureView != null) hiresTextureView.Dispose(); hiresTextureView = null;
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
