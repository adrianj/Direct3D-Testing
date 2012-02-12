using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SlimDX;
using SlimDX.Direct3D10;
using System.Drawing;

namespace Direct3DLib
{

	public class ShaderHelper : IDisposable
	{
		public const int MAX_TEXTURES = 16;
		public const string SHADER_RESOURCE = "Direct3DLib.shaders.RenderWithLighting.fx";

		private Device device;
		private Effect effect;
		public Effect ShaderEffect { get { return effect; } }
		private EffectTechnique effectTechnique;
		public EffectTechnique ShaderEffectTechnique { get { return effectTechnique; } }
		private EffectPass effectPass;
		public EffectPass ShaderEffectPass { get { return effectPass; } }

		private ConstantBufferHelper constantBuffer = new ConstantBufferHelper();
		public ConstantBufferHelper ConstantBufferSet { get { return constantBuffer; } }

		private TextureHelper [] textureHelper = new TextureHelper[MAX_TEXTURES];
		public TextureHelper [] TextureSet { get { return textureHelper; } }

		public Texture2D[] TextureImages
		{
			get
			{
				Texture2D[] ret = new Texture2D[MAX_TEXTURES];
				for (int i = 0; i < MAX_TEXTURES; i++)
				{
					if (textureHelper[i] == null) ret[i] = null;
					else ret[i] = textureHelper[i].TextureImage;
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
					textureHelper[i].TextureImage = value[i];
				}
			}
		}

		public ShaderHelper()
		{
			for (int i = 0; i < textureHelper.Length; i++)
				textureHelper[i] = new TextureHelper(i);
		}

		public void Initialize(Device device, Effect effect)
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
				constantBuffer.Initialize(effect);
				for(int i = 0; i < textureHelper.Length; i++)
					textureHelper[i].Initialize(device, effect);
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

		public static Effect GetEffect(Device device)
		{
			try
			{
				System.IO.Stream stream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(SHADER_RESOURCE);
				System.IO.StreamReader reader = new System.IO.StreamReader(stream);
				string shaderString = reader.ReadToEnd();
				Effect e = Effect.FromString(device, shaderString, "fx_4_0");
				//Effect e = Effect.FromStream(device, stream, "fx_4_0");
				//Effect e = Effect.FromFile(device, "shaders\\RenderWithLighting.fx", "fx_4_0");
				return e;
			}
			catch (CompilationException cx)
			{
				DisplayEffectException(cx); throw;
			}
			catch (Exception) { throw; }
		}

		private static void DisplayEffectException(Exception ex)
		{
			System.Windows.Forms.MessageBox.Show("" + ex, "Error loading shader file",
				System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
		}
	}

	
}
