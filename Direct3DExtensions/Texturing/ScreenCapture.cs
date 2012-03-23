using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
using SlimDX;
using D3D = SlimDX.Direct3D10;

namespace Direct3DExtensions.Texturing
{
	public class ScreenCapture
	{
		Direct3DEngine engine;

		public ScreenCapture(Direct3DEngine engine)
		{
			this.engine = engine;
		}

		public Vector4 GetResultAtPoint(Point p) { return GetResultAtPoint(p, 0); }

		public Vector4 GetResultAtPoint(Point p, int renderTargetIndex)
		{
			if (!(engine.D3DDevice is MultipleOutputDevice))
				renderTargetIndex = 0;
			StagingTexture staging = GetRenderTargets()[renderTargetIndex];
			Vector4 ret = (Vector4)staging.ReadPixelAsColour(p);

			return ret;
		}

		public void TakeScreenshotToClipboard()
		{
			if (!engine.InitSuccessful)
			{
				MessageBox.Show("Screenshot capture failed. Direct3D Engine not initialised");
				return;
			}
			if (System.Threading.Thread.CurrentThread.GetApartmentState() != System.Threading.ApartmentState.STA)
			{
				MessageBox.Show("Screenshot capture failed. Current thread apartment state not set to STA.");
			}
			D3D.Resource source = engine.D3DDevice.RenderTexture;
			//D3D.Resource source = (engine.D3DDevice as MultipleOutputDevice).RenderTextures[1];
			Image img = ConvertRenderTargetToImage(source);
			
			Clipboard.SetImage(img);
			MessageBox.Show("Screenshot copied to clipboard.");
		}

		public StagingTexture[] GetRenderTargets()
		{
			D3D.Texture2DDescription renderDesc = engine.D3DDevice.RenderTexture.Description;
			D3DDevice dev = engine.D3DDevice;
			StagingTexture[] ret;
			if (dev is MultipleOutputDevice)
			{
				MultipleOutputDevice mdev = dev as MultipleOutputDevice;
				ret = new StagingTexture[1 + mdev.NumAdditionalTargets];
				for (int i = 0; i < ret.Length; i++)
				{
					ret[i] = new StagingTexture(dev.Device, renderDesc.Width, renderDesc.Height, renderDesc.Format);
					dev.Device.CopySubresourceRegion(mdev.RenderTextures[i], 0, ret[i].Resource, 0, 0, 0, 0);
				}
				return ret;
			}
			ret = new StagingTexture[1];
			ret[0] = new StagingTexture(dev.Device, renderDesc.Width, renderDesc.Height, renderDesc.Format);
			dev.Device.CopySubresourceRegion(dev.RenderTexture, 0, ret[0].Resource, 0, 0, 0, 0);
			return ret;
		}

		private Image ConvertRenderTargetToImage(D3D.Resource source)
		{
			D3D.Texture2DDescription renderDesc = engine.D3DDevice.RenderTexture.Description;
			StagingTexture staging = new StagingTexture(engine.D3DDevice.Device, renderDesc.Width, renderDesc.Height, SlimDX.DXGI.Format.R8G8B8A8_UNorm);

			engine.D3DDevice.Device.CopySubresourceRegion(source, 0, staging.Resource, 0, 0, 0, 0);


			Image img = CopyTextureToBitmap(staging.Resource as D3D.Texture2D);
			return img;
		}

		Image CopyTextureToBitmap(D3D.Texture2D texture)
		{
			int width = texture.Description.Width;
			if (width % 16 != 0)
				width = MathExtensions.Round(width, 16) + 16;
			Bitmap bmp = new Bitmap(texture.Description.Width, texture.Description.Height, PixelFormat.Format32bppArgb);
			BitmapData bData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.WriteOnly, bmp.PixelFormat);
			using (DataStream stream = new DataStream(bData.Scan0, bData.Stride * bData.Height, false, true))
			{
				DataRectangle rect = texture.Map(0, D3D.MapMode.Read, D3D.MapFlags.None);
				using (DataStream texStream = rect.Data)
				{
					for (int y = 0; y < texture.Description.Height; y++)
						for (int x = 0; x < rect.Pitch; x+=4)
						{
							byte[] bytes = texStream.ReadRange<byte>(4);
							if (x < bmp.Width*4)
							{
								stream.Write<byte>(bytes[2]);	// DXGI format is BGRA, GDI format is RGBA.
								stream.Write<byte>(bytes[1]);
								stream.Write<byte>(bytes[0]);
								stream.Write<byte>(255);
							}
						}
				}
			}


			bmp.UnlockBits(bData);
			return bmp;
		}
	}
}
