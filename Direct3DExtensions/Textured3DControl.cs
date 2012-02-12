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
		protected override void InitEffect()
		{
			Effect = new TexturedEffect();
			Effect.Init(D3DDevice);
		}
	}

	public class TexturedEffect : BasicEffect
	{
		public override void Init(D3DDevice device)
		{
			base.Init(device);

			D3D.Texture2D texture = D3D.Texture2D.FromFile(device.Device, @"C:\Users\adrianj\Documents\Visual Studio 2010\Projects\Direct3D-Testing\ImageTiler_Test\bin\Debug\Images\test_google.bmp");


			D3D.ShaderResourceView textureResourceView = new D3D.ShaderResourceView(device.Device, texture);
			D3D.EffectResourceVariable textureResource = effect.GetVariableByName("Texture_0").AsResource();
			device.Device.PixelShader.SetShaderResource(textureResourceView, 0);
			textureResource.SetResource(textureResourceView);
		}
	}
}
