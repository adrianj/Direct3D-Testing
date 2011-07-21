using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SlimDX;
using SlimDX.Direct3D10;

namespace Direct3DLib
{

	public class ShaderHelper : IDisposable
	{
		public const int MAX_TEXTURES = 4;

		private Device device;
		private Effect effect;
		public Effect ShaderEffect { get { return effect; } }
		private EffectTechnique effectTechnique;
		public EffectTechnique ShaderEffectTechnique { get { return effectTechnique; } }
		private EffectPass effectPass;
		public EffectPass ShaderEffectPass { get { return effectPass; } }

		private ConstantBufferHelper constantBuffer;
		public ConstantBufferHelper ConstantBufferSet { get { return constantBuffer; } }

		private TextureHelper [] textureHelper = new TextureHelper[MAX_TEXTURES];
		public TextureHelper [] TextureSet { get { return textureHelper; } }

		public string[] TextureImageFiles
		{
			get
			{
				string[] ret = new string[MAX_TEXTURES];
				for (int i = 0; i < MAX_TEXTURES; i++)
				{
					if (textureHelper[i] == null) ret[i] = "";
					else ret[i] = textureHelper[i].ImageFilename;
				}
				return ret;
			}
			set
			{
				if(value == null) return;
				for (int i = 0; i < MAX_TEXTURES; i++)
				{
					if (value.Length <= i) return;
					if (textureHelper[i] == null) continue;
					textureHelper[i].ImageFilename = value[i];
				}
			}
		}

		public ShaderHelper(Device device, Effect effect)
		{
			this.device = device;
			this.effect = effect;
			Update();
		}

		public void Update()
		{
			if (device != null)
			{
				// Get the shader effects.
				effectTechnique = effect.GetTechniqueByIndex(0);
				effectPass = effectTechnique.GetPassByIndex(0);
				constantBuffer = new ConstantBufferHelper(effect);
				for(int i = 0; i < textureHelper.Length; i++)
					textureHelper[i] = new TextureHelper(device, effect,i);
			}
		}

		public bool ApplyEffects()
		{
			for (int i = 0; i < MAX_TEXTURES; i++)
			{
				if (textureHelper[i] != null) 
					textureHelper[i].Apply();
			}
			bool doApply = constantBuffer.ApplyEffects();

			if (doApply)
				effectPass.Apply();
			return doApply;
		}

		public void Dispose()
		{
			for (int i = 0; i < textureHelper.Length; i++)
				textureHelper[i].Dispose();
		}
	}

	
}
