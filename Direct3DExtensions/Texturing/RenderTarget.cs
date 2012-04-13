using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using SlimDX;
using D3D = SlimDX.Direct3D10;

namespace Direct3DExtensions.Texturing
{
	public class RenderTarget : ResourceView
	{
		public D3D.Texture2D Texture { get { return Resource as D3D.Texture2D; } }
		public D3D.Texture2DDescription Description { get { return Texture.Description; } }
		public D3D.RenderTargetView RenderView { get; private set; }

		public RenderTarget(D3DDevice device, int width, int height)
			: base(device.Device)
		{
			Resource = D3D.Texture2D.FromSwapChain<D3D.Texture2D>(device.SwapChain, 0);
			RenderView = new D3D.RenderTargetView(device.Device, Resource);
		}

		public RenderTarget(D3D.Device device, D3D.Texture2DDescription rdesc) : base(device)
		{
			rdesc.BindFlags |= D3D.BindFlags.ShaderResource;
			D3D.Texture2D te = new D3D.Texture2D(device, rdesc);
			Resource = te;
			RenderView = new D3D.RenderTargetView(device, te);

			View = new D3D.ShaderResourceView(device, Resource);
		}

		public RenderTarget(D3D.Device device, int width, int height) : base(device) 
		{
			D3D.Texture2DDescription rdesc = new D3D.Texture2DDescription()
			{
				Width = width,
				Height = height,
				MipLevels = 1,
				ArraySize = 1,
				Format = SlimDX.DXGI.Format.R8G8B8A8_UNorm,
				SampleDescription = new SlimDX.DXGI.SampleDescription(1, 0),
				Usage = D3D.ResourceUsage.Default,
				BindFlags = D3D.BindFlags.RenderTarget | D3D.BindFlags.ShaderResource,
				CpuAccessFlags = D3D.CpuAccessFlags.None,
				OptionFlags = D3D.ResourceOptionFlags.None
			};
			Resource = new D3D.Texture2D(device, rdesc);
			RenderView = new D3D.RenderTargetView(device, Texture);
			View = new D3D.ShaderResourceView(device, Resource);
		}

		public System.Drawing.Size GetSize()
		{
			if (Texture == null) return new System.Drawing.Size();
			return new System.Drawing.Size(Description.Width, Description.Height);
		}

		public void Clear(Color4 colour)
		{
			device.ClearRenderTargetView(RenderView, colour);
		}


		#region Dispose
		void DisposeManaged() { if (RenderView != null) RenderView.Dispose(); View = null; }
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
		#endregion
    
	}
}
