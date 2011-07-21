using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Runtime.InteropServices;
using System.ComponentModel;
using SlimDX;
using SlimDX.Direct3D10;
using SlimDX.DXGI;
//using SlimDX.DXGI;
//using SlimDX.Windows;
//using SlimDX.D3DCompiler;
using Device = SlimDX.Direct3D10.Device;
//using Buffer = SlimDX.Direct3D10.Buffer;
//using System.Runtime.InteropServices;

namespace Direct3DLib
{
	//[TypeConverter(typeof(EngineTypeConverter))]
    public class Direct3DEngine : IDisposable
    {
        #region Parent Declaration and Constructor
        // To use within a Control instead of a Form, change the types here.
        private Control mParent;
        public Direct3DEngine(Control con)
        {
            mParent = con;
            InitializeDevice();
            mParent.Disposed += (o, e) => { this.Dispose(); };
            mParent.SizeChanged += (o, e) => { this.ResizeBuffers(); };
        }
        #endregion


        #region Dispose Method
        public void Dispose()
        {
			foreach (Shape s in shapeList) s.Dispose();
			if (shaderEffect != null) shaderEffect.Dispose();
			if (shaderHelper != null) shaderHelper.Dispose();
            if(device != null) device.Dispose();
            if (swapChain != null) swapChain.Dispose();
        }
        #endregion

        #region Public Properties

		private bool isInitialized = false;
		public bool IsInitialized { get { return isInitialized; } }

		private CameraControl camera;
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
		public float LightAmbientIntensity
		{
			get { return shaderHelper.ConstantBufferSet.LightAmbientIntensity; }
			set { shaderHelper.ConstantBufferSet.LightAmbientIntensity = value; }
		}

		private List<IRenderable> shapeList = new List<IRenderable>();
		public List<IRenderable> ShapeList { get { return shapeList; }  }

		private long prevTick1 = 100;
		private long prevTick2 = 99;
		private double refreshRate = 100;
		public double RefreshRate { get { return refreshRate; } }

		public ShaderHelper Shader { get { return shaderHelper; } }

        #endregion

		#region Private Properties

		private Device device;
        private SwapChain swapChain;
        private RenderTargetView renderView;
        private DepthStencilView renderDepth;
        private Viewport viewPort;
		private ShaderHelper shaderHelper;
		private Effect shaderEffect;

		#endregion


		public void UpdateShapes()
		{
			foreach (IRenderable s in shapeList)
				s.Update(device,shaderEffect);
		}

        public IRenderable PickObjectAt(Point screenLocation)
        {
            Point p = screenLocation;
            
            Vector3 nearClick = new Vector3(p.X, p.Y, camera.ZClipNear);
            Vector3 farClick = new Vector3(p.X, p.Y, camera.ZClipFar);
            Vector3 v = new Vector3(0, 0, 0);
            float h = mParent.Height;
            float w = mParent.Width;
            Matrix iProj = camera.Proj;
            v.X = (((2.0f * p.X) / w) - 1) / iProj.M11;
            v.Y = -(((2.0f * p.Y) / h) - 1) / iProj.M22;
            v.Z = 1;
            //Plane pl = new Plane();
           
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
            Ray ray = new Ray(rayOrigin,rayDir);
            IRenderable ret = null;
            double minZ = float.MaxValue;
            foreach (IRenderable s in shapeList)
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

            Vector3 vv = Vector3.Project(new Vector3(0,0,0), 0, 0, mParent.Width, mParent.Height, 0.1f, 100, camera.World);
            return ret;
        }



        private void InitializeDevice()
        {
            isInitialized = false;
            try
            {
                camera = new CameraControl(mParent);
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

				shaderEffect = Effect.FromString(device, Properties.Resources.RenderWithLighting, "fx_4_0");
				shaderHelper = new ShaderHelper(device,shaderEffect);

                // Scale the buffers appropriately to the size of the parent control.
                isInitialized = true;
                ResizeBuffers();
            }
            catch (Direct3D10Exception ex) { MessageBox.Show("" + ex.Message + "\n\n" + ex.ResultCode.Code.ToString("X")
				+"\n\n"+ex.StackTrace); return; }
            this.Render();
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
                Camera.Pan = Camera.Pan;
                isInitialized = true;
            }
        }

        public void Render()
        {
			prevTick2 = prevTick1;
			prevTick1 = DateTime.Now.Ticks;
			double newRef = 0;
			if (prevTick1 == prevTick2) newRef = 100;
			else
			{
				long diff = prevTick1 - prevTick2;
				newRef = Math.Round(1000000.0 / (double)(diff),2);
			}
			refreshRate = (refreshRate * 0.7) + (0.3 * newRef);
            if (IsInitialized)
            {
                Device context = device;
                // Clear the view, resetting to the background colour.
                
                context.ClearRenderTargetView(renderView, new Color4(mParent.BackColor));
                context.ClearDepthStencilView(renderDepth, DepthStencilClearFlags.Depth, 1, 0);

				shaderHelper.ConstantBufferSet.ViewProj = Camera.World;

                // Call the Render method of each shape.
				foreach (Shape shape in shapeList)
				{
					shape.Render(context, shaderHelper);
				}

                // Present!
                swapChain.Present(0, PresentFlags.None);
            }
        }
    }
	
	public class EngineTypeConverter : System.ComponentModel.TypeConverter
	{
		public override bool GetPropertiesSupported(ITypeDescriptorContext context)
		{
			return true;
		}
		public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object value, Attribute[] attributes)
		{
			PropertyDescriptorCollection original = TypeDescriptor.GetProperties(typeof(Direct3DEngine));
			PropertyDescriptorCollection pdc = new PropertyDescriptorCollection(null, false);
			foreach (PropertyDescriptor pd in original)
			{
				if (pd.Name.Equals("ShapeList"))
					continue; 
				pdc.Add(pd);
			}
			return pdc;
		}
	}
	 
}




