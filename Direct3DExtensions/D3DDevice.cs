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
	public class D3DDevice : DisposablePattern, IDisposable
	{
		
		D3D.FillMode fillMode = D3D.FillMode.Solid;
		D3D.BlendStateDescription AlphaBlendState;

		public D3D.FillMode FillMode { get { return fillMode; } set { fillMode = value; SetRasterizer(fillMode); } }
		public D3D.Device Device { get; private set; }
		public D3D.Viewport Viewport { get; private set; }
		public DXGI.SwapChain SwapChain { get; private set; }

		public Texturing.RenderTarget RenderTarget { get; private set; }
		public D3D.Texture2D DepthBuffer { get; private set; }
		public D3D.DepthStencilView DepthBufferView { get; private set; }

		public int IndicesDrawn { get; private set; }
		

		protected D3DHostControl control;

		public D3DDevice() : base()
		{
		}

		public virtual void Init(D3DHostControl control)
		{
			DisposeManaged();
			

			this.control = control;
			DXGI.SampleDescription sampledesc = new SlimDX.DXGI.SampleDescription(1, 0);
			DXGI.ModeDescription modedesc = new SlimDX.DXGI.ModeDescription()
			{
				Format = DXGI.Format.R8G8B8A8_UNorm,
				RefreshRate = new Rational(60, 1),
				Width = control.Width,
				Height = control.Height
			};

			DXGI.SwapChainDescription swapchaindesc = new SlimDX.DXGI.SwapChainDescription()
			{ 
				ModeDescription = modedesc,
				SampleDescription = sampledesc,
				BufferCount = 1,
				Flags = DXGI.SwapChainFlags.None,
				IsWindowed = true,
				OutputHandle = control.Handle,
				SwapEffect = DXGI.SwapEffect.Discard,
				Usage = DXGI.Usage.RenderTargetOutput
			};

			D3D.DeviceCreationFlags deviceflags = D3D.DeviceCreationFlags.None;

			D3D.Device device = null;
			DXGI.SwapChain swapchain = null;
			D3D.Device.CreateWithSwapChain(null, D3D.DriverType.Hardware, deviceflags, swapchaindesc, out device, out swapchain);
			this.Device = device;
			this.SwapChain = swapchain;


			

			SetSize(control.Width, control.Height);
			SetBlendState(true);

		}

		public void SetBlendState(bool enableAlphaBlending)
		{
			AlphaBlendState = new D3D.BlendStateDescription()
			{
				BlendOperation = D3D.BlendOperation.Add,
				SourceBlend = D3D.BlendOption.SourceAlpha,
				DestinationBlend = D3D.BlendOption.InverseSourceAlpha,
				IsAlphaToCoverageEnabled = false,
				AlphaBlendOperation = D3D.BlendOperation.Add,
				SourceAlphaBlend = D3D.BlendOption.Zero,
				DestinationAlphaBlend = D3D.BlendOption.Zero,

			};
			AlphaBlendState.SetBlendEnable(0, enableAlphaBlending);
			AlphaBlendState.SetWriteMask(0, D3D.ColorWriteMaskFlags.All);
			Device.OutputMerger.BlendState = D3D.BlendState.FromDescription(Device, AlphaBlendState);
		}

		public void SetRasterizer(D3D.FillMode fillMode)
		{
			D3D.RasterizerStateDescription rsd = new D3D.RasterizerStateDescription()
			{
				CullMode = D3D.CullMode.Back,
				FillMode = fillMode
			};
			D3D.RasterizerState rsdState = D3D.RasterizerState.FromDescription(Device, rsd);
			Device.Rasterizer.State = rsdState;
		}


		public System.Drawing.Size GetSize()
		{
			if (RenderTarget == null) return new System.Drawing.Size();
			D3D.Texture2DDescription desc = RenderTarget.Texture.Description;
			return new System.Drawing.Size(desc.Width, desc.Height);
		}

		public void SetSize(int width, int height)
		{
			if (DepthBuffer != null) DepthBuffer.Dispose();
			if (DepthBufferView != null) DepthBufferView.Dispose();
			if (RenderTarget != null) RenderTarget.Dispose();
			
			
			SwapChain.ResizeBuffers(1, width, height, DXGI.Format.R8G8B8A8_UNorm, DXGI.SwapChainFlags.None);
			
			RenderTarget = new Texturing.RenderTarget(this, width, height);

			D3D.Texture2DDescription depthbufferdesc = new D3D.Texture2DDescription()
			{ 
				Width = width,
				Height = height,
				MipLevels = 1,
				ArraySize = 1,
				Format = SlimDX.DXGI.Format.D24_UNorm_S8_UInt,
				SampleDescription = new SlimDX.DXGI.SampleDescription(1, 0),
				Usage = D3D.ResourceUsage.Default,
				BindFlags = D3D.BindFlags.DepthStencil,
				CpuAccessFlags = D3D.CpuAccessFlags.None,
				OptionFlags = D3D.ResourceOptionFlags.None
			};

			DepthBuffer = new D3D.Texture2D(Device, depthbufferdesc);
			DepthBufferView = new D3D.DepthStencilView(Device, DepthBuffer);

			Viewport = new D3D.Viewport()
			{
				X = 0,
				Y = 0,
				Width = width,
				Height = height,
				MinZ = 0.0f,
				MaxZ = 1.0f
			};
			Device.Rasterizer.SetViewports(Viewport);
			SetOutputTargets();
		}

		protected virtual void SetOutputTargets()
		{
			SetRenderTargets(GetRenderTargets());
		}

		public void SetRenderTargets(D3D.RenderTargetView[] renderTargets)
		{
			Device.OutputMerger.SetTargets(DepthBufferView, renderTargets);
		}

		public virtual D3D.RenderTargetView[] GetRenderTargets()
		{
			return new D3D.RenderTargetView[] { RenderTarget.RenderView };
		}

		public virtual void Clear()
		{
			IndicesDrawn = 0;
			RenderTarget.Clear(control.BackColor);
			//Device.ClearRenderTargetView(RenderTargetView.RenderView, control.BackColor);
			Device.ClearDepthStencilView(DepthBufferView, D3D.DepthStencilClearFlags.Depth, 1.0f, 0);
			SetRasterizer(fillMode);
		}

		public virtual void DrawIndexed(int numIndices)
		{
			Device.DrawIndexed(numIndices, 0, 0);
			IndicesDrawn += numIndices;
		}

		public virtual void Present()
		{
			
			Result res = SwapChain.Present(0, DXGI.PresentFlags.None);
			
		}

		private bool disposed = false;
		protected override void Dispose(bool disposing)
		{
			if (!disposed)
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
			if (Device != null)
			{
				Device.ClearAllObjects();
			}
			if (RenderTarget != null) RenderTarget.Dispose(); RenderTarget = null;
			if (DepthBufferView != null) DepthBufferView.Dispose(); DepthBufferView = null;
			if (DepthBuffer != null) DepthBuffer.Dispose(); DepthBuffer = null;
			if (SwapChain != null) SwapChain.Dispose(); SwapChain = null;
			if (Device != null) Device.Dispose(); Device = null;
		}
	}
}
