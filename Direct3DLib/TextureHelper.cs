using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
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

		private Texture2D image;
		public Texture2D TextureImage
		{
			get { return image; }
			set
			{
				if (value != null && !value.Equals(image))
				{
					image = value;
					updateRequired = true;
				}
			}
		}
		private EffectResourceVariable textureResource;
		private ShaderResourceView textureResourceView;

		public TextureHelper(int textureIndex)
		{
			this.textureIndex = textureIndex;
		}

		public void Initialize(Device device, Effect effect)
		{
			this.device = device;
			this.effect = effect;
			image = ImageConverter.GetNullTexture(device);
			Update();
		}

		public void Update()
		{
			updateRequired = false;
			if (device == null || effect == null)
			{
				updateRequired = true;
				return;
			}
			Texture2D texture = image;
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
