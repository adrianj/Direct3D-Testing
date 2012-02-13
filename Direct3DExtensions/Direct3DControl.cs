using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;

using SlimDX;
using D3D10 = SlimDX.Direct3D10;
using DXGI = SlimDX.DXGI;

namespace Direct3DExtensions
{
	public partial class Direct3DControl : UserControl, IDisposable
	{
		// Direct3D
		protected D3D10.Device device;
		protected D3D10.Viewport viewport;
		protected DXGI.SwapChain swapchain;

		// Buffers
		protected D3D10.RenderTargetView rendertarget;
		protected D3D10.Texture2D depthbuffer;
		protected D3D10.DepthStencilView depthbufferview;

		// Effects
		protected D3D10.Effect effect;
		protected D3D10.EffectTechnique technique;
		protected D3D10.EffectPass pass1;
		//protected D3D10.EffectPass pass2;
		//protected D3D10.EffectPass pass3;

		// Effect variables
		protected D3D10.EffectMatrixVariable WorldViewProj;
		protected D3D10.EffectScalarVariable TextureZoomLevel;

		// Geometry
		protected Geometry geometry;

		// Input / Control
		protected InputHelper input;
		protected Camera camera;
		protected FirstPersonCameraInput camerainput;
		public bool IsDesignMode { get; private set; }
		protected float zoomLevel = 1;


		public Direct3DControl()
		{
			
			if (LicenseManager.UsageMode == LicenseUsageMode.Designtime)
				IsDesignMode = true;
			InitializeComponent();
			this.Load += (o, e) => { this.InitializeDirect3D(); };

			errLabel.MaximumSize = this.Size;
			//InitializeDirect3D();
		}

		public bool InitializeDirect3D()
		{
			errLabel.Visible = true;
			if (!IsDesignMode)
			{

				bool success = SetupCameraInput();
				if (!success) return false;
				success = SetupDirect3D();
				if (!success) return false;


				Application.Idle += Application_Idle;

				success = LoadEffect();
				if (!success) return false;
				success = LoadGeometry();
				if (!success) return false;
				errLabel.Visible = false;
			}
			return true;
		}

		private bool SetupCameraInput()
		{

			camerainput = new FirstPersonCameraInput(this);
			camerainput.Camera.LookAt(new Vector3(0.5f, 1.5f, 0), new Vector3(0.5f, 0, 0.5f));
			camerainput.Camera.Persepective(45.0f * (float)Math.PI / 180.0f, ClientSize.Width / (float)ClientSize.Height, 0.025f, 1200.0f);
			return true;
		}


		bool SetupDirect3D()
		{
			try
			{
				CreateDevice();
				ResizeBuffers();
			}
			catch (Exception ex)
			{
				MessageBox.Show("" + ex);
				return false;
			}
			return true;
		}

		private void ResizeBuffers()
		{
			if (rendertarget != null) rendertarget.Dispose();
			if (depthbuffer != null) depthbuffer.Dispose();
			if (depthbufferview != null) depthbufferview.Dispose();

			swapchain.ResizeBuffers(1, ClientSize.Width, ClientSize.Height, DXGI.Format.R8G8B8A8_UNorm, DXGI.SwapChainFlags.None);

			camera.Persepective(45.0f * (float)Math.PI / 180.0f, ClientSize.Width / (float)ClientSize.Height, 0.025f, 1200.0f);
			using (D3D10.Texture2D backbuffer = D3D10.Texture2D.FromSwapChain<D3D10.Texture2D>(swapchain, 0))
			{
				rendertarget = new D3D10.RenderTargetView(device, backbuffer);
			}

			D3D10.Texture2DDescription depthbufferdesc = new D3D10.Texture2DDescription()
			{
				Width = ClientSize.Width,
				Height = ClientSize.Height,
				MipLevels = 1,
				ArraySize = 1,
				Format = SlimDX.DXGI.Format.D24_UNorm_S8_UInt,
				SampleDescription = new SlimDX.DXGI.SampleDescription(1, 0),
				Usage = D3D10.ResourceUsage.Default,
				BindFlags = D3D10.BindFlags.DepthStencil,
				CpuAccessFlags = D3D10.CpuAccessFlags.None,
				OptionFlags = D3D10.ResourceOptionFlags.None
			};

			depthbuffer = new D3D10.Texture2D(device, depthbufferdesc);
			depthbufferview = new D3D10.DepthStencilView(device, depthbuffer);

			viewport = new D3D10.Viewport()
			{
				X = 0,
				Y = 0,
				Width = ClientSize.Width,
				Height = ClientSize.Height,
				MinZ = 0.0f,
				MaxZ = 1.0f
			};

			device.Rasterizer.SetViewports(viewport);
			device.OutputMerger.SetTargets(depthbufferview, rendertarget);
			this.SizeChanged += (o, e) => { this.ResizeBuffers(); };
		}

		private DXGI.SampleDescription CreateDevice()
		{
			//DXGI.SampleDescription sampledesc = new SlimDX.DXGI.SampleDescription(info.Multisamples, 0);
			DXGI.SampleDescription sampledesc = new SlimDX.DXGI.SampleDescription(1, 0);
			DXGI.ModeDescription modedesc = new SlimDX.DXGI.ModeDescription()
			{

				Format = DXGI.Format.R8G8B8A8_UNorm,
				RefreshRate = new Rational(60, 1),
				//Scaling = DXGI.DisplayModeScaling.Unspecified,
				//ScanlineOrdering = DXGI.DisplayModeScanlineOrdering.Unspecified,
				Width = ClientSize.Width,
				Height = ClientSize.Height
			};

			DXGI.SwapChainDescription swapchaindesc = new SlimDX.DXGI.SwapChainDescription()
			{
				ModeDescription = modedesc,
				SampleDescription = sampledesc,
				BufferCount = 1,
				Flags = DXGI.SwapChainFlags.None,
				IsWindowed = true,
				OutputHandle = this.Handle,
				SwapEffect = DXGI.SwapEffect.Discard,
				Usage = DXGI.Usage.RenderTargetOutput
			};

			D3D10.DeviceCreationFlags deviceflags = D3D10.DeviceCreationFlags.None;


			D3D10.Device.CreateWithSwapChain(null, D3D10.DriverType.Hardware, deviceflags, swapchaindesc, out device, out swapchain);
			return sampledesc;
		}

		bool LoadEffect()
		{
			string errors = string.Empty;
			try
			{
			SlimDX.D3DCompiler.ShaderFlags shaderflags = SlimDX.D3DCompiler.ShaderFlags.None;//.EnableStrictness;
#if DEBUG
			shaderflags |= SlimDX.D3DCompiler.ShaderFlags.Debug | SlimDX.D3DCompiler.ShaderFlags.SkipOptimization;
#endif // DEBUG

				//effect = D3D10.Effect.FromFile(device, "Effects\\Default.fx", "fx_4_0");
				//effect = D3D10.Effect.FromFile(device, "Effects\\Default.fx", "fx_4_0", shaderflags, SlimDX.D3DCompiler.EffectFlags.None, null, null, null, out errors);
				effect = D3D10.Effect.FromFile(device, "Effects\\Basic.fx", "fx_4_0", shaderflags, SlimDX.D3DCompiler.EffectFlags.None, null, null, null, out errors);
			

			technique = effect.GetTechniqueByIndex(0);
			pass1 = technique.GetPassByIndex(0);
			//pass2 = technique.GetPassByIndex(1);
			//pass3 = technique.GetPassByIndex(2);

			// Setup shader variables
			D3D10.Texture2D texture = D3D10.Texture2D.FromFile(device, @"C:\Users\adrianj\Documents\Visual Studio 2010\Projects\Direct3D-Testing\ImageTiler_Test\bin\Debug\Images\test_google.bmp");


			D3D10.ShaderResourceView textureResourceView = new D3D10.ShaderResourceView(device, texture);
			D3D10.EffectResourceVariable textureResource = effect.GetVariableByName("Texture_0").AsResource();
			device.PixelShader.SetShaderResource(textureResourceView, 0);
			textureResource.SetResource(textureResourceView);
			WorldViewProj = effect.GetVariableByName("WorldViewProj").AsMatrix();
			TextureZoomLevel = effect.GetVariableByName("TextureZoomLevel").AsScalar();
			TextureZoomLevel.Set(zoomLevel);
			}

			catch (Exception ex)
			{
				if (!string.IsNullOrEmpty(errors))
				{
					MessageBox.Show(errors);
					return false;
				}
				else
				{
					MessageBox.Show("" + ex);
					return false;
				}
			}
			return true;
		}


		protected virtual bool LoadGeometry()
		{
			try
			{
				Console.Write("Loading Geometry...");
				Mesh mesh = CreateSimpleMesh();

				geometry = new BasicGeometry();
				geometry.Meshes.Add(mesh);

				//geometry.Init(pass1);
				Console.WriteLine("done.");
			}
			catch (Exception ex)
			{
				MessageBox.Show("" + ex);
				return false;
			}
			return true;
		}

		private Mesh CreateSimpleMesh()
		{
			Mesh mesh = new BasicMesh();
			mesh.Vertices = new Vector3[12];
			mesh.Vertices[0] = new Vector3(0, 0, 0);
			mesh.Vertices[1] = new Vector3(0, 0, 1);
			mesh.Vertices[2] = new Vector3(1, 0, 1);
			mesh.Vertices[3] = new Vector3(1, 0, 0);
			mesh.Vertices[4] = new Vector3(0, 0, 2);
			mesh.Vertices[5] = new Vector3(1, 0, 2);
			mesh.Vertices[6] = new Vector3(2, 0, 1);
			mesh.Vertices[7] = new Vector3(2, 0, 0);
			mesh.Vertices[8] = new Vector3(1, 0, -1);
			mesh.Vertices[9] = new Vector3(0, 0, -1);
			mesh.Vertices[10] = new Vector3(-1, 0, 0);
			mesh.Vertices[11] = new Vector3(-1, 0, 1);
			
			mesh.FaceVertices = new uint[] { 
				0,1,2,
				0,2,3,
				1,4,2,
				4,5,2,
				2,6,7,
				2,7,3,
				3,8,0,
				0,8,9,
				0,10,11,
				0,11,1
			};
			mesh.FaceCount = mesh.FaceVertices.Length / 3;
			/*
			mesh.MapChannelCount = 1;
			MapChannel mc = new MapChannel();
			mc.MapFaces = mesh.FaceVertices;
			mc.MapVertices = mesh.Vertices;

			mesh.MapChannels = new MapChannel[] { mc };
			 */
			return mesh;
		}


		public virtual new void Dispose()
		{
			Application.Idle -= Application_Idle;

			device.InputAssembler.SetInputLayout(null);

			device.ClearState();

			rendertarget.Dispose();
			depthbuffer.Dispose();
			depthbufferview.Dispose();

			device.Dispose();
			swapchain.Dispose();
			
			geometry.Dispose();
			effect.Dispose();
			base.Dispose();
		}

		void Application_Idle(object sender, System.EventArgs e)
		{
			while (SlimDX.Windows.MessagePump.IsApplicationIdle)
			{
				Render();
			}
		}

		private void Render()
		{
			//long time = stopwatch.ElapsedMilliseconds;
			//frametime = time - lasttime;
			//lasttime = time;

			input.Update();

			UpdateInput();

			float hgt = camera.Position.Y;

			camerainput.OnRender();

			if (hgt != camera.Position.Y)
			{
				int zoom = 0;
				hgt = Math.Abs(camera.Position.Y);
				for (double res = 0.5; res < hgt; res *= 2)
				{
					zoom++;
				}
				if (zoom != zoomLevel)
				{
					zoomLevel = zoom;
					TextureZoomLevel.Set(zoomLevel);
					Console.WriteLine("zoomlevel: " + zoomLevel);
				}
			}

			device.ClearRenderTargetView(rendertarget, this.BackColor);
			device.ClearDepthStencilView(depthbufferview, D3D10.DepthStencilClearFlags.Depth, 1.0f, 0);

			Draw();

			swapchain.Present(0, DXGI.PresentFlags.None);
		}

		protected virtual void UpdateInput() 
		{

			if (input.IsKeyPressed(Keys.Escape))
				Application.Exit();

		}

		protected virtual void Draw()
		{
			if (!errLabel.Visible)
			{
				try
				{
					WorldViewProj.SetMatrix(camera.View * camera.Projection);
					
					/*
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
					 */

					// Second Pass
					device.Rasterizer.SetViewports(viewport);
					device.OutputMerger.SetTargets(depthbufferview, rendertarget);
					{
						//if (showprepass)
						//	pass1.Apply();
						//else
						pass1.Apply();
						geometry.Draw();
					}
				}
				catch (Exception ex)
				{
					MessageBox.Show("" + ex); 
					errLabel.Visible = true;
				}
			}
		}

		protected override bool ProcessDialogKey(Keys keyData)
		{
			input.SetKeyPressed(keyData);
			return base.ProcessDialogKey(keyData);
		}
	}
}
