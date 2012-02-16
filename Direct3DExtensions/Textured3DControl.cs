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
	public class Textured3DControl : Basic3DControl
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
			CameraInput.CameraChanged += new EventHandler(CameraInput_CameraChanged);
			SetZoomLevelFromHeight(CameraInput.Camera.Position.Y);
		}

		protected override void InitGeometry()
		{
			base.InitGeometry();
			using (MeshFactory factory = new MeshFactory())
			{
				//Mesh mesh = factory.
			}
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

	public class TexturedEffect : BasicEffect
	{
		int zoomLevel = 0;
		D3D.EffectScalarVariable zoomEffect;

		public int TextureZoomLevel
		{
			get { return zoomLevel; }
			set { if (zoomLevel != value) { zoomLevel = value; zoomEffect.Set(zoomLevel); } }
		}

		public override void Init(D3DDevice device)
		{
			base.Init(device);
			
			D3D.Texture2D texture = D3D.Texture2D.FromFile(device.Device, @"C:\Users\adrianj\Documents\Visual Studio 2010\Projects\Direct3D-Testing\ImageTiler_Test\bin\Debug\Images\test_google.bmp");


			D3D.ShaderResourceView textureResourceView = new D3D.ShaderResourceView(device.Device, texture);
			D3D.EffectResourceVariable textureResource = effect.GetVariableByName("Texture_0").AsResource();
			device.Device.PixelShader.SetShaderResource(textureResourceView, 0);
			textureResource.SetResource(textureResourceView);
			zoomEffect = effect.GetVariableByName("TextureZoomLevel").AsScalar();
		}
	}
}
