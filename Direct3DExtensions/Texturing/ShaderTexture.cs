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
		public const string VariableTextureSuffix = "Texture";
		public const string VariableInverseSizeSuffix = "ISize";

		protected Effect effect;
		protected string variableName;
		

		public ShaderTexture(D3D.Device device, int width, int height, SlimDX.DXGI.Format format) : base(device, width, height, format) { }

		public virtual void BindToEffect(Effect effect, string variableName)
		{
			this.effect = effect;
			if (!variableName.EndsWith(VariableTextureSuffix))
				variableName += VariableTextureSuffix;
			this.variableName = variableName;
			BindToEffect();
		}

		protected virtual void BindToEffect()
		{
			if (effect == null) return;
			D3D.EffectResourceVariable var = this.effect.GetVariableByName(this.variableName).AsResource();
			var.SetResource(View);

			string iSizeName = this.variableName.Substring(0, variableName.Length - VariableTextureSuffix.Length);
			iSizeName += VariableInverseSizeSuffix;
			D3D.EffectVectorVariable vec = this.effect.GetVariableByName(iSizeName).AsVector();
			if(vec != null)
				vec.Set(new Vector2(1.0f/(float)Description.Width, 1.0f/(float)Description.Height));
		}

		protected override void RecreateTexture(int width, int height)
		{
			D3D.Texture2DDescription description = CreateDescription(width, height);
			description.Usage = D3D.ResourceUsage.Default;
			description.CpuAccessFlags = D3D.CpuAccessFlags.None;
			description.BindFlags = D3D.BindFlags.ShaderResource;
			if (Resource != null) Resource.Dispose();
			if (View != null) View.Dispose();
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
			throw new InvalidOperationException("Cannot read directly from a ShaderTexture. Use a staging texture intermediate");
		}

		public void WriteTexture(StagingTexture staging, int xoffset, int yoffset)
		{
			if (staging.Description.Width + xoffset > this.Description.Width
				|| staging.Description.Height + yoffset > this.Description.Height)
				throw new ArgumentOutOfRangeException("Staging texture with added offset exceeds bounds of this texture.");
			device.CopySubresourceRegion(staging.Resource, 0, Resource, 0, xoffset, yoffset, 0);
			if (device.DeviceRemovedReason.IsFailure)
				throw new SlimDXException("Could not copy device subresource. Double check you haven't exceeded the resource region.");
		}


    
	}
}
