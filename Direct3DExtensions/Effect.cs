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
		void SetWorld(Matrix world);
		void SetCamera(Camera camera);
		D3D.EffectPass this[int index] { get; }
		int EffectCount { get; }
		int GetPassIndexByName(string passName);
		D3D.EffectVariable GetVariableByName(string name);
	}

	public class WorldViewProjEffect : DisposablePattern, Effect
	{
		protected int MaxTechniques = 4;
		protected int MaxPasses = 4;

		public static string DefaultShaderFilename = @"Effects\Main.fx";

		string filename = DefaultShaderFilename;
		public string ShaderFilename { get { return filename; } set { filename = value; } }

		protected D3D.Effect effect;
		protected List<D3D.EffectPass> passes;

		public D3D.EffectPass this[int index] { get { return passes[index]; } }
		public int EffectCount { get { return passes.Count; } }


		protected D3D.EffectMatrixVariable World;
		protected D3D.EffectMatrixVariable View;
		protected D3D.EffectMatrixVariable Proj;
		protected D3D.EffectMatrixVariable InvProj;
		protected D3D.EffectVectorVariable CameraPos;


		public WorldViewProjEffect()
		{
		}

		public int GetPassIndexByName(string name)
		{
			for (int i = 0; i < this.EffectCount; i++)
			{
				D3D.EffectPass pass = this[i];
				if (pass.Description.Name.Equals(name))
					return i;
			}
			return -1;
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

			World = effect.GetVariableByName("World").AsMatrix();
			View = effect.GetVariableByName("View").AsMatrix();
			Proj = effect.GetVariableByName("Proj").AsMatrix();
			InvProj = effect.GetVariableByName("InvProj").AsMatrix();
			CameraPos = effect.GetVariableByName("CameraPos").AsVector();
		}

		public D3D.EffectVariable GetVariableByName(string name)
		{
			return effect.GetVariableByName(name);
		}

		public virtual void SetWorld(Matrix world)
		{
			World.SetMatrix(world);
		}

		public virtual void SetCamera(Camera camera)
		{
			View.SetMatrix(camera.View);
			Proj.SetMatrix(camera.Projection);
			InvProj.SetMatrix(Matrix.Invert(camera.Projection));
			CameraPos.Set(camera.Position);
		}
		private bool disposed = false;
		protected override void Dispose(bool disposing)
		{
			if (!this.disposed)
			{
				if (disposing)
				{
					DisposeManaged();
				}
				this.disposed = true;
			}
			base.Dispose(disposing);
		}

		private void DisposeManaged()
		{
			if (effect != null)
				effect.Dispose();
			
		}

		// override object.Equals
		public override bool Equals(object obj)
		{
			if (obj == null || GetType() != obj.GetType())
			{
				return false;
			}
			Effect effect = obj as Effect;
			return effect.ShaderFilename.Equals(this.ShaderFilename);
		}

		// override object.GetHashCode
		public override int GetHashCode()
		{
			return ShaderFilename.GetHashCode();
		}
	}

}
