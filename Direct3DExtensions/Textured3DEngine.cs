using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

using SlimDX;
using D3D = SlimDX.Direct3D10;
using DXGI = SlimDX.DXGI;

namespace Direct3DExtensions
{
	public class Textured3DEngine : Test3DEngine
	{
		float previousCameraHeight = 0;


		protected override void InitEffect()
		{
			Effect = new TexturedEffect();
			Effect.Init(D3DDevice);
		}

		protected override void InitCameraInput()
		{
			base.InitCameraInput();
			CameraInput.CameraChanged += new CameraChangedEventHandler(CameraInput_CameraChanged);
			SetZoomLevelFromHeight(CameraInput.Camera.Position.Y);
		}

		void CameraInput_CameraChanged(object sender, EventArgs e)
		{
			float newHeight = CameraInput.Camera.Position.Y;
			if (newHeight != previousCameraHeight)
			{
				SetZoomLevelFromHeight(newHeight);
			}
			previousCameraHeight = newHeight;
		}

		void SetZoomLevelFromHeight(float height)
		{
			int zoom = 0;
			height = Math.Abs(height);
			for (double res = 0.5; res < height; res *= 2)
			{
				zoom++;
			}
			zoom = zoom.Clamp(0, 6);
			(this.Effect as TexturedEffect).TextureZoomLevel = zoom;
		}
	}

	public class TexturedEffect : WorldViewProjEffect
	{
		int zoomLevel = 0;
		D3D.EffectScalarVariable zoomEffect;
		D3D.Texture2D texture;

		public int TextureZoomLevel
		{
			get { return zoomLevel; }
			set { if (zoomLevel != value) { zoomLevel = value; zoomEffect.Set(zoomLevel); } }
		}

		public override void Init(D3DDevice device)
		{
			base.Init(device);
			
			texture = D3D.Texture2D.FromFile(device.Device, @"C:\Users\adrianj\Documents\Visual Studio 2010\Projects\Direct3D-Testing\ImageTiler_Test\bin\Debug\Images\test_google.bmp");
			D3D.ShaderResourceView textureResourceView = new D3D.ShaderResourceView(device.Device, texture);
			D3D.EffectResourceVariable textureResource = effect.GetVariableByName("Texture_0").AsResource();
			device.Device.PixelShader.SetShaderResource(textureResourceView, 0);
			textureResource.SetResource(textureResourceView);
			texture.Dispose();
			textureResourceView.Dispose();

			

			zoomEffect = effect.GetVariableByName("TextureZoomLevel").AsScalar(); 
			
		}


		void DisposeManaged() {  }
		void DisposeUnmanaged() { if (texture != null) texture.Dispose(); texture = null; }

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
