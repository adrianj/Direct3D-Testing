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
	public interface Effect : IDisposable
	{
		string ShaderFilename { get; set; }
		void Init(D3DDevice device);
		void ApplyAll(Camera camera);
		D3D.EffectPass this[int index] { get; }
		int EffectCount { get; }
	}

	public class BasicEffect : Effect
	{
		protected int MaxTechniques = 4;
		protected int MaxPasses = 4;

		public string ShaderFilename { get; set; }

		protected D3D.Effect effect;
		protected List<D3D.EffectPass> passes;

		public D3D.EffectPass this[int index] { get { return passes[index]; } }
		public int EffectCount { get { return passes.Count; } }

		protected D3D.EffectMatrixVariable WorldViewProj;

		public BasicEffect()
		{
			ShaderFilename = @"Effects\Basic.fx";
		}

		public virtual void Init(D3DDevice device)
		{
			SlimDX.D3DCompiler.ShaderFlags shaderflags = SlimDX.D3DCompiler.ShaderFlags.None;
			effect = D3D.Effect.FromFile(device.Device, ShaderFilename, "fx_4_0", shaderflags, SlimDX.D3DCompiler.EffectFlags.None, null, null, null);

			passes = new List<D3D.EffectPass>();
			for (int i = 0; i < MaxTechniques; i++)
			{
				D3D.EffectTechnique technique = effect.GetTechniqueByIndex(i);
				if (technique == null || !technique.IsValid) break;
				for (int p = 0; p < MaxPasses; p++)
				{
					D3D.EffectPass pass = technique.GetPassByIndex(p);
					if (pass == null || !pass.IsValid) break;
					passes.Add(pass);
				}
			}

			WorldViewProj = effect.GetVariableByName("WorldViewProj").AsMatrix();
		}

		public virtual void ApplyAll(Camera camera)
		{
			WorldViewProj.SetMatrix(camera.View * camera.Projection);
			foreach (D3D.EffectPass pass in passes)
				pass.Apply();
		}

		public virtual void Dispose()
		{
			if (effect != null)
				effect.Dispose();
		}
	}
}
