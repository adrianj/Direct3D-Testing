using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using SlimDX;
using D3D = SlimDX.Direct3D10;

namespace Direct3DExtensions
{
	public class TextureArray
	{
		D3D.Texture2D[] texArray;

		public TextureArray(D3D.Device device, D3D.Effect effect, string textureName, int arraySize)
		{
			
			D3D.Texture2DDescription tdesc = Texture.CreateWritableTextureDescription(256, 256, D3D.BindFlags.ShaderResource, SlimDX.DXGI.Format.R32_Float);
			tdesc.ArraySize = 1;
			texArray[0] = new D3D.Texture2D(device, tdesc);
			D3D.EffectResourceVariable rVar = effect.GetVariableByName("HiresMap").AsResource();
			//rVar.SetResourceArray(

			D3D.ShaderResourceView rView = new D3D.ShaderResourceView(device, texArray[0]);
		}
	}
}
