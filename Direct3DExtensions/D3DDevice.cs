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
	public class D3DDevice : IDisposable
	{
		public D3D.Device Device { get; private set; }
		public D3D.Viewport Viewport { get; private set; }
		public DXGI.SwapChain SwapChain { get; private set; }

		public D3D.RenderTargetView RenderTarget { get; private set; }
		public D3D.Texture2D DepthBuffer { get; private set; }
		public D3D.DepthStencilView DepthBufferView { get; private set; }

		protected Basic3DControl control;

		public void Init(Basic3DControl control)
		{
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
		}

		public void SetSize(int width, int height)
		{
			if (RenderTarget != null) RenderTarget.Dispose();
			if (DepthBuffer != null) DepthBuffer.Dispose();
			if (DepthBufferView != null) DepthBufferView.Dispose();

			SwapChain.ResizeBuffers(1, width, height, DXGI.Format.R8G8B8A8_UNorm, DXGI.SwapChainFlags.None);

			using (D3D.Texture2D backbuffer = D3D.Texture2D.FromSwapChain<D3D.Texture2D>(SwapChain, 0))
			{
				RenderTarget = new D3D.RenderTargetView(Device, backbuffer);
			}

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
			Device.OutputMerger.SetTargets(DepthBufferView, RenderTarget);
		}

		public virtual void Clear()
		{
			Device.ClearRenderTargetView(RenderTarget, control.BackColor);
			Device.ClearDepthStencilView(DepthBufferView, D3D.DepthStencilClearFlags.Depth, 1.0f, 0);
		}

		public virtual void Present()
		{
			SwapChain.Present(0, DXGI.PresentFlags.None);
		}

		public virtual void Dispose()
		{
			if(RenderTarget != null) RenderTarget.Dispose();
			if(DepthBuffer != null) DepthBuffer.Dispose();
			if(DepthBufferView != null) DepthBufferView.Dispose();
			if(Device != null) Device.Dispose();
			if(SwapChain != null) SwapChain.Dispose();

		}
	}
}
