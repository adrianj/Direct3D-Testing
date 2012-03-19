using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using SlimDX;
using D3D10 = SlimDX.Direct3D10;

namespace Direct3DExtensions.VirtualTexture
{
	public class VirtualTextureController : DisposablePattern
	{
		VirtualTexture virtualtexture;
		FeedbackBuffer feedback;

		bool update = true;
		bool showprepass = false;

		Mesh geometry;
		D3D10.Device device;

		D3D10.Effect effect;
		D3D10.EffectTechnique technique;
		D3D10.EffectPass pass1;
		D3D10.EffectPass pass2;
		D3D10.EffectPass pass3;


		public VirtualTextureController(D3D10.Device device,Mesh geometry)
		{
			this.geometry = geometry;
			this.device = device;
			LoadEffect();
			LoadVirtualTexture( );
		}

		
		public void Draw(D3DDevice d3dDevice)
		{
			if (update)
			{
				// Process the last frame's data
				feedback.Download();
				virtualtexture.Update(feedback.Requests);

				// First Pass
				feedback.SetAsRenderTarget();
				feedback.Clear();
				{
					pass1.Apply();
					geometry.Draw();
				}

				// Start copying the frame results to the cpu
				feedback.Copy();
			}
		

			// Second Pass
			device.Rasterizer.SetViewports(d3dDevice.Viewport);
			device.OutputMerger.SetTargets(d3dDevice.DepthBufferView, d3dDevice.RenderTarget);
			{
				if (showprepass)
					pass1.Apply();
				else
					pass2.Apply();
				geometry.Draw();
			}
		}

		void LoadEffect()
		{

		}

		void LoadVirtualTexture()
		{

		}


		void DisposeManaged()
		{
			virtualtexture.Dispose();
			feedback.Dispose();
			effect.Dispose();
		}
		void DisposeUnmanaged() { }

		bool disposed = false;
		protected override void Dispose(bool disposing)
		{
			if (!disposed)
			{
				if (disposing)
				{
					DisposeManaged();
				}

				DisposeUnmanaged();
				disposed = true;
			}
			base.Dispose(disposing);
		}
    
	}
}
