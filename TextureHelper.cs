using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using SlimDX;
using SlimDX.Direct3D10;

namespace Direct3DLib
{
	public class TextureHelper : IDisposable
	{

		private Device device;
		private Effect effect;
		private int textureIndex = 0;
		public int TextureIndex { get { return textureIndex; } }

		private bool updateRequired = true;

		private string imageFilename = "textures\\streettexture.dds";
		public string ImageFilename
		{
			get { return imageFilename; }
			set
			{
				if (value != null && !value.Equals(imageFilename))
				{
					imageFilename = value;
					updateRequired = true;
				}
			}
		}
		private EffectResourceVariable textureResource;
		private ShaderResourceView textureResourceView;

		public TextureHelper(Device device, Effect effect, int textureIndex)
		{
			this.textureIndex = textureIndex;
			this.device = device;
			this.effect = effect;
			Update();
		}

		public void Update()
		{
			if (device == null || effect == null)
			{
				updateRequired = true;
				return;
			}
			if (!System.IO.File.Exists(ImageFilename))
				return;
			Texture2D texture = Texture2D.FromFile(device, ImageFilename);
			textureResourceView = new ShaderResourceView(device, texture);

			textureResource = effect.GetVariableByName("Texture_" + textureIndex).AsResource();
			device.PixelShader.SetShaderResource(textureResourceView, 0);
			textureResource.SetResource(textureResourceView);
			setSampler();
		}


		private void setSampler()
		{
			SamplerDescription a = new SamplerDescription();
			a.AddressU = TextureAddressMode.Wrap;
			a.AddressV = TextureAddressMode.Wrap;
			a.AddressW = TextureAddressMode.Wrap;
			a.Filter = Filter.MinPointMagMipLinear;
			SamplerState b = SamplerState.FromDescription(device, a);
			device.PixelShader.SetSampler(b, 0);
		}

		public bool Apply()
		{
			if (updateRequired)
			{
				updateRequired = false;
				Update();
				return true;
			}
			return false;
		}

		public void Dispose()
		{
			if (textureResourceView != null) textureResourceView.Dispose();
		}

	}
}
