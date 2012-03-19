using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using SlimDX;
using D3D = SlimDX.Direct3D10;

namespace Direct3DExtensions.Texturing
{
	public class ShaderTexture : StagingTexture
	{
		protected Effect effect;
		protected string variableName;
		

		public ShaderTexture(D3D.Device device, int width, int height, SlimDX.DXGI.Format format) : base(device, width, height, format) { }

		public void BindToEffect(Effect effect, string variableName)
		{
			this.effect = effect;
			this.variableName = variableName;
			BindToEffect();
		}

		private void BindToEffect()
		{
			if (effect == null) return;
			D3D.EffectResourceVariable var = this.effect.GetVariableByName(this.variableName).AsResource();
			var.SetResource(View);
		}

		protected override void RecreateTexture(int width, int height)
		{
			D3D.Texture2DDescription description = CreateDescription(width, height);
			description.Usage = D3D.ResourceUsage.Default;
			description.CpuAccessFlags = D3D.CpuAccessFlags.None;
			description.BindFlags = D3D.BindFlags.ShaderResource;
			Resource = new D3D.Texture2D(device, description);
			View = new D3D.ShaderResourceView(device, Resource);
			BindToEffect();
		}

		public override void WriteTexture<T>(T[,] data)
		{
			throw new InvalidOperationException("Cannot write directly to a ShaderTexture. Use a staging texture intermediate.");
		}

		public override float[,] ReadTexture()
		{
			throw new InvalidOperationException("Cannot read directly from a ShaderTexture.");
		}

		public void WriteTexture(StagingTexture staging, int xoffset, int yoffset)
		{
			device.CopySubresourceRegion(staging.Resource, 0, Resource, 0, xoffset, yoffset, 0);
		}
	}
}
