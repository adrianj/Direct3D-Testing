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
	public class MultipleOutputDevice : D3DDevice
	{
		public Texturing.RenderTarget[] Targets { get; private set; }
		public D3D.RenderTargetView[] RenderTargets { get; private set; }
		public D3D.Texture2D[] RenderTextures { get; private set; }
		public int NumAdditionalTargets { get; set; }

		protected override void SetOutputTargets()
		{
			RenderTargets = new D3D.RenderTargetView[1 + NumAdditionalTargets];
			RenderTextures = new D3D.Texture2D[1 + NumAdditionalTargets];
			Targets = new Texturing.RenderTarget[1 + NumAdditionalTargets];
			RenderTargets[0] = RenderTarget.RenderView;
			RenderTextures[0] = RenderTarget.Texture;
			Targets[0] = RenderTarget;
			for (int i = 0; i < NumAdditionalTargets; i++)
			{
				RenderTextures[i + 1] = new D3D.Texture2D(Device, RenderTarget.Texture.Description);
				RenderTargets[i + 1] = new D3D.RenderTargetView(Device, RenderTextures[i + 1]);
				//Targets[i + 1] = new Texturing.RenderTarget(Device, RenderTargetView.Texture.Description.Width, RenderTargetView.Texture.Description.Height);
				Targets[i + 1] = new Texturing.RenderTarget(Device, RenderTarget.Texture.Description);
			}
			Device.OutputMerger.SetTargets(DepthBufferView, GetRenderTargets());
			//Device.OutputMerger.SetTargets(DepthBufferView, RenderTargets);
		}

		public override D3D.RenderTargetView[] GetRenderTargets()
		{
			D3D.RenderTargetView[] views = new D3D.RenderTargetView[Targets.Length];
			for (int i = 0; i < views.Length; i++)
				views[i] = Targets[i].RenderView;
			return views;
		}

	}
}
