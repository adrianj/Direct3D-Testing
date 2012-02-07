using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Runtime.InteropServices;
using System.ComponentModel;
using SlimDX;
using SlimDX.Direct3D10;
using SlimDX.D3DCompiler;
using SlimDX.DXGI;
using Device = SlimDX.Direct3D10.Device;
using Vector3 = SlimDX.Vector3;

namespace Direct3DLib
{
	[TypeConverter(typeof(DTALib.BasicTypeConverter))]
    public class Direct3DEngine : IDisposable
    {
        #region Parent Declaration and Constructor
        // To use within a Control instead of a Form, change the types here.
        private Control mParent;
        public Direct3DEngine(Control con)
        {
            mParent = con;
            mParent.Disposed += (o, e) => { this.Dispose(); };
            mParent.SizeChanged += (o, e) => { this.ResizeBuffers(); };
        }
		~Direct3DEngine()
		{
			Dispose();
		}
        #endregion


        #region Dispose Method
		private bool disposed = false;
        public void Dispose()
        {
			if (!disposed)
			{
				foreach (Shape s in shapeList) s.Dispose();
				shapeList.Clear();
				if (shaderSignature != null) shaderSignature.Dispose();
				if (shaderEffect != null) shaderEffect.Dispose();
				if (shaderHelper != null) shaderHelper.Dispose();
				if (device != null) device.Dispose();
				if (swapChain != null) swapChain.Dispose();
				disposed = true;
			}
        }
        #endregion

        #region Public Properties

		private bool isInitialized = false;
		public bool IsInitialized { get { return isInitialized; } }

		private CameraControl camera = new CameraControl();
		public CameraControl Camera { get { return camera; } set { camera = value; } }

		public Vector3 LightDirection
		{
			get { return shaderHelper.ConstantBufferSet.LightDirection; }
			set { shaderHelper.ConstantBufferSet.LightDirection = value; }
		}

		public float LightDirectionalIntensity
		{
			get { return shaderHelper.ConstantBufferSet.LightDirectionalIntensity; }
			set { shaderHelper.ConstantBufferSet.LightDirectionalIntensity = value; }
		}
		private float ambientIntensity = 1.0f;
		public float LightAmbientIntensity
		{
			get { return ambientIntensity; }
			set { ambientIntensity = value;  }
		}

		private List<Shape> shapeList = new List<Shape>();
		public List<Shape> ShapeList { get { return shapeList; } set { shapeList = value; } }


		public ShaderHelper Shader { get { return shaderHelper; } }

		private Image [][] textureImages;
		public Image [][] TextureImages
		{
			get
			{
				if (textureImages == null)
				{
					textureImages = new Image[ShaderHelper.MAX_TEXTURES][];
					for (int i = 0; i < ShaderHelper.MAX_TEXTURES; i++)
						textureImages[i] = new Image[3];
				}
				return textureImages;
			}
			set
			{
				textureImages = value;
			}
		}

        #endregion

		#region Private Properties

		private Device device;
        private SwapChain swapChain;
        private RenderTargetView renderView;
        private DepthStencilView renderDepth;
        private Viewport viewPort;
		private ShaderHelper shaderHelper = new ShaderHelper();
		private ShaderSignature shaderSignature;
		private Effect shaderEffect;

		#endregion


		public void UpdateAllShapes()
		{
			foreach (Shape s in shapeList)
				UpdateShape(s);
		}

		public void UpdateShape(Shape s)
		{
			s.Update(device, shaderSignature);

		}

		public Shape PickObjectAt(Point screenLocation)
		{
			Shape shape = Pick2DObjectAt(screenLocation);
			if (shape == null)
				shape = Pick3DObjectAt(screenLocation);
			return shape;
		}

		Shape Pick2DObjectAt(Point screenLocation)
		{
			Ray ray = Get2DRayFromScreenPoint(screenLocation);
			Shape ret = null;
			double minZ = float.MaxValue;
			foreach (Shape s in shapeList.FindAll(s => (s is Shape2D)))
			{
				if (!s.CanPick) continue;
				float dist = 0;

				bool ints = s.RayIntersects(ray, out dist);
				if (ints && dist < minZ)
				{
					ret = s;
					minZ = dist;
					ret = s;
				}
			}
			return ret;
		}

		Shape Pick3DObjectAt(Point screenLocation)
        {
			Ray ray = Get3DRayFromScreenPoint(screenLocation); 
			Shape ret = null;
            double minZ = float.MaxValue;
			foreach (Shape s in shapeList.FindAll(s=>!(s is Shape2D)))
            {
                if (!s.CanPick) continue;
                float dist = 0;

                bool ints = s.RayIntersects(ray, out dist);
                if (ints && dist < minZ)
                {
                    ret = s;
                    minZ = dist;
                    ret = s;
                }
            }
            return ret;
        }

		public Ray Get2DRayFromScreenPoint(Point screenLocation)
		{
			Point p = screenLocation;
			float h = mParent.Height;
			float w = mParent.Width;
			Vector3 rayDir = Vector3.UnitZ;
			Vector3 rayOrigin = new Vector3()
			{
				X = (((2.0f * p.X) / w) - 1) / 1,
				Y = -(((2.0f * p.Y) / h) - 1) / 1,
				Z = 0
			};

			Matrix trans = Matrix.Invert(Get2DTransform(false));
			//rayDir = Vector3.TransformCoordinate(rayDir, trans);
			rayOrigin = Vector3.TransformCoordinate(rayOrigin, trans);
			Ray ray = new Ray(rayOrigin, rayDir);
			return ray;
		}
		public Ray Get3DRayFromScreenPoint(Point screenLocation)
		{
			Point p = screenLocation;

			Vector3 v = new Vector3(0, 0, 0);
			float h = mParent.Height;
			float w = mParent.Width;
			Matrix iProj = camera.Proj;
			v.X = (((2.0f * p.X) / w) - 1) / iProj.M11;
			v.Y = -(((2.0f * p.Y) / h) - 1) / iProj.M22;
			v.Z = 1;

			Matrix m = Matrix.Invert(camera.View);
			Vector3 rayDir = new Vector3();
			rayDir.X = v.X * m.M11 + v.Y * m.M21 + v.Z * m.M31;
			rayDir.Y = v.X * m.M12 + v.Y * m.M22 + v.Z * m.M32;
			rayDir.Z = v.X * m.M13 + v.Y * m.M23 + v.Z * m.M33;
			rayDir = Vector3.Normalize(rayDir);
			Vector3 rayOrigin = new Vector3();
			rayOrigin.X = m.M41;
			rayOrigin.Y = m.M42;
			rayOrigin.Z = m.M43;
			Ray ray = new Ray(rayOrigin, rayDir);
			return ray;
		}



		public void InitializeDevice()
        {
            isInitialized = false;
			try
			{
				// Declare and create the Device and SwapChain.
				var desc = new SwapChainDescription()
				{
					BufferCount = 1,
					ModeDescription = new ModeDescription(mParent.ClientSize.Width, mParent.ClientSize.Height, new Rational(60, 1), Format.R8G8B8A8_UNorm),
					IsWindowed = true,
					OutputHandle = mParent.Handle,
					SampleDescription = new SampleDescription(1, 0),
					SwapEffect = SwapEffect.Discard,
					Usage = Usage.RenderTargetOutput
				};
				Factory fact = new Factory();
				Device.CreateWithSwapChain(fact.GetAdapter(0), DriverType.Hardware, DeviceCreationFlags.None, desc, out device, out swapChain);
				Device context = device;

				shaderEffect = ShaderHelper.GetEffect(device);
				shaderHelper.Initialize(device, shaderEffect);
				shaderSignature = shaderEffect.GetTechniqueByIndex(0).GetPassByIndex(0).Description.Signature;

				// Scale the buffers appropriately to the size of the parent control.
				isInitialized = true;
				ResizeBuffers();
				EnableAlphaBlending();

				UpdateTextures();

				UpdateAllShapes();
			}
			catch (Direct3D10Exception ex)
			{
				MessageBox.Show("" + ex.Message + "\n\n" + ex.ResultCode.Code.ToString("X")
					+ "\n\n" + ex.StackTrace); return;
			}
        }

		public void UpdateTextures()
		{
			if (isInitialized && textureImages != null)
			{
				for (int i = 0; i < Math.Min(textureImages.Length, ShaderHelper.MAX_TEXTURES); i++)
				{
					if(textureImages[i] != null)
						shaderHelper.TextureSet[i].TextureImage = ImageConverter.ConvertImagesToTexture2D(device, textureImages[i]);
				}
			}
		}

        /// <summary>
        /// Rescale the buffers to the size of the parent control.
        /// </summary>
        private void ResizeBuffers()
        {
            if (IsInitialized && mParent.Width > 0 && mParent.Height > 0)
            {
                isInitialized = false;
                if(renderView != null) renderView.Dispose();
                if(renderDepth != null) renderDepth.Dispose();
                swapChain.ResizeBuffers(1, mParent.Width, mParent.Height, Format.R8G8B8A8_UNorm, SwapChainFlags.None);

                // Set the view port
                viewPort = new Viewport(0, 0, mParent.Width, mParent.Height, 0.0f, 1.0f);
                device.Rasterizer.SetViewports(viewPort);
                // Set the render target.

                using(Texture2D backBuffer = Texture2D.FromSwapChain<Texture2D>(swapChain, 0))
                    renderView = new RenderTargetView(device, backBuffer);

                // Create the Depth Buffer.
                // Without this, farther objects could draw on top of nearer objects.
                Texture2DDescription depthDesc = new Texture2DDescription()
                {
                    Width = mParent.Width,
                    Height = mParent.Height,
                    MipLevels = 1,
                    ArraySize = 1,
                    Format = Format.D32_Float,
                    Usage = ResourceUsage.Default,
                    SampleDescription = new SampleDescription(1, 0),
                    BindFlags = BindFlags.DepthStencil,
                    CpuAccessFlags = CpuAccessFlags.None,
                    OptionFlags = ResourceOptionFlags.None
                };
                Texture2D dBuf = new Texture2D(device, depthDesc);
                renderDepth = new DepthStencilView(device, dBuf);

                device.OutputMerger.SetTargets(renderDepth, renderView);
				Camera.ViewHeight = mParent.Height;
				Camera.ViewWidth = mParent.Width;
                isInitialized = true;
            }
        }

		private void EnableAlphaBlending()
		{
			BlendStateDescription blendState = new BlendStateDescription()
			{
				BlendOperation = BlendOperation.Add,
				SourceBlend = BlendOption.SourceAlpha,
				DestinationBlend = BlendOption.InverseSourceAlpha,
				IsAlphaToCoverageEnabled = false,
				AlphaBlendOperation = BlendOperation.Add,
				SourceAlphaBlend = BlendOption.Zero,
				DestinationAlphaBlend = BlendOption.Zero,
				
			};
			blendState.SetBlendEnable(0, true);
			blendState.SetWriteMask(0, ColorWriteMaskFlags.All);
			device.OutputMerger.BlendState = BlendState.FromDescription(device, blendState);
		}

        public void Render()
        {
			
			//UpdateRefreshRate();
            if (IsInitialized)
            {
				try
				{
					// Clear the view, resetting to the background colour.
					Color4 back = new Color4(mParent.BackColor);
					//back.Alpha = 0;
					device.ClearRenderTargetView(renderView, back);
					device.ClearDepthStencilView(renderDepth, DepthStencilClearFlags.Depth, 1, 0);
					RemoveDisposedShapes();
					RenderAll3DShapes();
					RenderAll2DShapes();
					// Present!
					swapChain.Present(0, PresentFlags.None);
				}
				catch (Direct3D10Exception dex) { MessageBox.Show("" + dex); throw; }
            }
        }

		void RemoveDisposedShapes()
		{
			List<Shape> disposedShapes = new List<Shape>();
			foreach (Shape s in shapeList)
			{
				if (s.IsDisposed)
					disposedShapes.Add(s);
			}
			foreach (Shape s in disposedShapes)
				shapeList.Remove(s);
		}

		private void RenderAll3DShapes()
		{
			shaderHelper.ConstantBufferSet.LightAmbientIntensity = ambientIntensity;
			shaderHelper.ConstantBufferSet.ViewProj = Camera.World;
			foreach (Shape shape in shapeList.FindAll(s => !(s is Shape2D)))
			{
				BoundingBox bbInWorld = Direct3DEngine.BoundingBoxMultiplyMatrix(shape.MaxBoundingBox, shape.World);
				bool onScreen = BoundingBoxOnScreenFine(bbInWorld);
				if (!onScreen)
					onScreen = BoundingBoxOnScreenCoarse(bbInWorld);
				if (onScreen)
				{
					shape.Render(device, shaderHelper);
				}

			}
		}


		void RenderAll2DShapes()
		{
			shaderHelper.ConstantBufferSet.LightAmbientIntensity = 1;
			shaderHelper.ConstantBufferSet.ViewProj = Get2DTransform(false);
			//shaderHelper.ConstantBufferSet.ViewProj = Camera.Proj;
			foreach (Shape shape in shapeList.FindAll(s => s is Shape2D))
			{
				shape.Render(device, shaderHelper);
			}
		}


		Matrix Get2DTransform(bool reverse)
		{
			float w = (float)mParent.Width;
			float h = (float)mParent.Height;
			Matrix scale = Matrix.Scaling(new Vector3(2 / w, 2 / h, 1));
			Matrix trans = Matrix.Translation(new Vector3(-1, 1, 0));
			if (!reverse)
				return scale * trans;
			else
				return trans * scale;
			//return scale;
		}

		private bool BoundingBoxOnScreenCoarse(BoundingBox bb)
		{
			int s = 3;
			for (int i = 0; i < s; i++)
				for (int k = 0; k < s; k++)
				{
					Point p = new Point(i * mParent.Width / (s-1), k * mParent.Height / (s - 1));
					Ray ray = Get3DRayFromScreenPoint(p);
					float f = 0;
					if (Ray.Intersects(ray, bb, out f))
						return true;
				}
			return false;
		}

		private bool BoundingBoxOnScreenFine(BoundingBox bb)
		{
			BoundingBox newBB = Direct3DEngine.BoundingBoxMultiplyMatrix(bb, Camera.World); 
			BoundingBox bbCam = new BoundingBox(new Vector3(-1.0f, -1.0f, 0), new Vector3(1.0f, 1.0f, 1.0f));
			return BoundingBox.Intersects(newBB, bbCam);
		}
		

		public static BoundingBox BoundingBoxMultiplyMatrix(BoundingBox bb, Matrix m)
		{
			Vector3 []corners = bb.GetCorners();
			for (int i = 0; i < corners.Length; i++)
			{
				corners[i] = Vector3.TransformCoordinate(corners[i], m);
			}

			return BoundingBox.FromPoints(corners);
		}
    }
	 
}




