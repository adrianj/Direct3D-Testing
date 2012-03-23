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
		public D3D.RenderTargetView[] RenderTargets { get; private set; }
		public D3D.Texture2D[] RenderTextures { get; private set; }
		public int NumAdditionalTargets { get; set; }

		protected override void SetOutputTargets()
		{
			RenderTargets = new D3D.RenderTargetView[1 + NumAdditionalTargets];
			RenderTextures = new D3D.Texture2D[1 + NumAdditionalTargets];
			RenderTargets[0] = RenderTarget;
			RenderTextures[0] = RenderTexture;
			for (int i = 0; i < NumAdditionalTargets; i++)
			{
				RenderTextures[i + 1] = new D3D.Texture2D(Device, RenderTexture.Description);
				RenderTargets[i + 1] = new D3D.RenderTargetView(Device, RenderTextures[i + 1]);
			}
			Device.OutputMerger.SetTargets(DepthBufferView, RenderTargets);
		}
	}
}
