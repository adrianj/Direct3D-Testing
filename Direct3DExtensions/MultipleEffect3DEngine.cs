using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Direct3DExtensions
{
	public class MultipleEffect3DEngine : Test3DEngine
	{
		List<Effect> effectList = new List<Effect>();

		public override Effect Effect
		{
			get
			{
				if (effectList.Count < 1)
					AddDefaultEffect();
				return effectList[0];
			}
			set
			{
				AddEffect(value);
			}
		}

		public void AddEffect(Direct3DExtensions.Effect value)
		{
			if (!effectList.Contains(value))
				effectList.Add(value);
		}

		protected override void InitEffect()
		{
			AddDefaultEffect();
			foreach (Effect effect in effectList)
			{
				effect.Init(D3DDevice);
			}
		}

		protected override void ApplyCameraToEffect()
		{
			foreach (Effect effect in effectList)
				effect.SetCamera(this.CameraInput.Camera);
		}

		public override void BindMesh(Mesh mesh, string passName)
		{
			int index = -1;
			Effect effect = effectList[0];
			foreach (Effect e in effectList)
			{
				index = e.GetPassIndexByName(passName);
				if (index >= 0)
				{
					effect = e;
					break;
				}
			}
			if (index < 0)
				throw new Exception("No Effect pass with name '"+passName+"' was found.");

			mesh.BindToPass(D3DDevice, effect, index);
			Geometry.Add(mesh);
		}

		void AddDefaultEffect()
		{
			Effect effect = effectList.Find(e => e.ShaderFilename.Equals(WorldViewProjEffect.DefaultShaderFilename));
			if (effect == null)
				effectList.Add(new WorldViewProjEffect());
		}
	}
}
