using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Runtime.InteropServices;
using SlimDX;
using SlimDX.Direct3D10;
using SlimDX.DXGI;
//using SlimDX.Windows;
using SlimDX.D3DCompiler;
using Device = SlimDX.Direct3D10.Device;
using Buffer = SlimDX.Direct3D10.Buffer;
//using System.Runtime.InteropServices;

namespace Direct3DControl
{
    public class Direct3DEngine : IDisposable
    {
        #region Parent Declaration and Constructor
        // To use within a Control instead of a Form, change the types here.
        private Control mParent;
        public Direct3DEngine(Control con)
        {
            mParent = con;
            mParent.Disposed += (o, e) => { this.Dispose(); };
            IsInitialized = InitializeDevice();
        }
        #endregion


        #region Dispose Method
        public void Dispose()
        {
            if(device != null) device.Dispose();
            if (swapChain != null) swapChain.Dispose();
        }
        #endregion

        #region Public Properties
        private Color mBackColor = Color.Beige;
        public Color BackColor { get { return mBackColor; } set { mBackColor = value; } }
        #endregion

        public bool IsInitialized { get; set; }

        private Device device;
        private SwapChain swapChain;
        private RenderTargetView renderView;
        private DepthStencilView renderDepth;
        private Viewport viewPort;

        private CameraControl camera;
        public CameraControl CameraView { get { return camera; } set { camera = value; } }

        
        private List<Shape> shapeList = new List<Shape>();

        public void AddShape(Shape s)
        {
            s.Update(device);
            shapeList.Add(s);
        }

        /// <summary>		
        /// Checks to see if a bounding box is beneath some screen coordinates		
        /// </summary>		
        /// <param name="device">The Device to be used</param>		
        /// <param name="bBox">The bounding box to check an intersection with</param>		
        /// <param name="coord">The X and Y coordinate of the screen (i.e. mouse coords)</param>		
        /// <param name="mViewProj">The view matrix * projection matrix</param>		
        /// <param name="distance">If intersection, then this contains the distance at which the ray intersected the plane</param>		
        /// <returns>true if successfull intersection</returns>		
        public bool CheckPicking(Device device, BoundingBox bBox, Vector2 coord, Matrix mViewProj, out float distance)		
        {			
            int width = (int)viewPort.Width;			
            int height = (int)viewPort.Height;			// Z -1000 to 1000 assumed			
            Vector3 ZNearPlane = Vector3.Unproject(new Vector3(coord, 0), 0, 0, width, height, -1000, 1000, mViewProj);			
            Vector3 ZFarPlane = Vector3.Unproject(new Vector3(coord, 1), 0, 0, width, height, -1000, 1000, mViewProj);			
            Vector3 direction = ZFarPlane - ZNearPlane;			
            direction.Normalize();			
            Ray ray = new Ray(ZNearPlane, direction);	
		    
            if ( Ray.Intersects(ray, bBox, out distance) )				
                return true;			
            return false;		
        }


        /*
        private void DoPicking(int mouseX, int mouseY)
        {
            // Clamp mouse coordinates to viewport 
            if (mouseX < 0) mouseX = 0;
            if (mouseY < 0) mouseY = 0;
            if (mouseX > viewPort.Width) mouseX = (int)viewPort.Width;
            if (mouseY > viewPort.Height) mouseY = (int)viewPort.Height;

            // Put mouse coordinates in screenspace Vector3's. These are the points 
            // defining our ray for picking, which we'll transform back to world space 
            Vector3 near = new Vector3(mouseX, mouseY, 0);
            Vector3 far = new Vector3(mouseX, mouseY, 1);

            // Transform points to world space 
            near.Unproject(this.sampleFramework.Device.Viewport, camera.ProjectionMatrix, camera.ViewMatrix, scannerWorldMatrix * Matrix.RotationY(scannerRotationTimer));
            far.Unproject(this.sampleFramework.Device.Viewport, camera.ProjectionMatrix, camera.ViewMatrix, scannerWorldMatrix * Matrix.RotationY(scannerRotationTimer));

            // Retrieve intersection information 
            IntersectInformation closestIntersection;
            bool intersects = scannerMesh.Intersect(near, far, out closestIntersection);

            if (intersects)
            {
                // If you only want to confirm intersection of the mesh (ie. whether it 
                // was clicked on), you can just use this boolean and you're done. 
                status = string.Format("Face={0}, tu={1}, tv={2}", closestIntersection.FaceIndex, closestIntersection.U, closestIntersection.V);

                // We'll continue here with showing how to obtain the intersected face 
                HighlightIntersectedFace(closestIntersection);
            }
            else
            {
                status = "Use mouse to pick a polygon";
            }
        }

         */

        public Shape PickObjectAt(Point screenLocation)
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
            Shape ret = null;
            double minZ = float.MaxValue;
            foreach (Shape s in shapeList)
            {
                bool ints = false;
                float dist = 0;
                //Console.WriteLine("test shape: " + s + ", " + s.Name + ", Loc: " + s.Location + "\n\t"
                //    + Vector3.TransformCoordinate(s.Location, camera.World) + "\n");
                if (s.Indices != null && s.Indices.Length > 2 && s.Indices.Length % 3 == 0)
                {
                    for (int i = 0; i < s.Indices.Length; i += 3)
                    {
                        Vector3 v1 = Vector3.TransformCoordinate(s.Vertices[s.Indices[i + 2]].Position, s.World);
                        Vector3 v2 = Vector3.TransformCoordinate(s.Vertices[s.Indices[i + 1]].Position, s.World);
                        Vector3 v3 = Vector3.TransformCoordinate(s.Vertices[s.Indices[i]].Position, s.World);

                        dist = 0;
                        ints = Ray.Intersects(ray, v1, v2, v3, out dist);
                        if (ints && dist < minZ)
                        {
                            ret = s;
                            minZ = dist;
                        }
                    }
                }
                else
                {
                    BoundingBox bb = GetSurroundingBox(s);
                    dist = 0;
                    ints = Ray.Intersects(ray, bb, out dist);
                }

                if (ints && dist < minZ)
                {
                    ret = s;
                    minZ = dist;
                }

            }

            Vector3 vv = Vector3.Project(new Vector3(0,0,0), 0, 0, mParent.Width, mParent.Height, 0.1f, 100, camera.World);
            return ret;
        }

        private BoundingBox GetSurroundingBox(Shape s)
        {
            float maxX = float.MinValue;
            float maxY = float.MinValue;
            float maxZ = float.MinValue;
            float minX = float.MaxValue;
            float minY = float.MaxValue;
            float minZ = float.MaxValue;
            for (int i = 0; i < s.Vertices.Length; i++)
            {
                Vector3 v = s.Vertices[i].Position;
                v = Vector3.TransformCoordinate(v, s.World);
                if (v.X < minX) minX = v.X;
                if (v.Y < minY) minY = v.Y;
                if (v.Z < minZ) minZ = v.Z;
                if (v.X > maxX) maxX = v.X;
                if (v.Y > maxY) maxY = v.Y;
                if (v.Z > maxZ) maxZ = v.Z;
            }
            return new BoundingBox(new Vector3(minX, minY, minZ), new Vector3(maxX, maxY, maxZ));
        }

        private bool InitializeDevice()
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
            try
            {
                Device.CreateWithSwapChain(null,DriverType.Hardware,DeviceCreationFlags.None,desc,out device, out swapChain);
                //device = new Device(DriverType.Hardware, DeviceCreationFlags.None);
                
                //Device.CreateWithSwapChain(DriverType.Hardware, DeviceCreationFlags.Debug, desc, out device, out swapChain);
            }
            catch (Direct3D10Exception ex) { MessageBox.Show("" + ex.Message + "\n\n" + ex.ResultCode.Code.ToString("X")); return false; }
            Device context = device;
            //DeviceContext context = device.ImmediateContext;
            // Set the view port
            viewPort = new Viewport(0, 0, mParent.Width, mParent.Height, 0.0f, 1.0f);
            context.Rasterizer.SetViewports(viewPort);
            // Set the render target.
            
            Texture2D backBuffer = Texture2D.FromSwapChain<Texture2D>(swapChain, 0);
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

            context.OutputMerger.SetTargets(renderDepth,renderView);
            return true;
        }

        public void Render()
        {
            Device context = device;
            //DeviceContext context = device.ImmediateContext;
            // Clear the view, resetting to the background colour.
            context.ClearRenderTargetView(renderView, new Color4(this.BackColor));
            context.ClearDepthStencilView(renderDepth, DepthStencilClearFlags.Depth, 1, 0);
            // Call the Render method of each shape.
            foreach(Shape shape in shapeList)
                shape.Render(context, camera.World);
            // Present!
            swapChain.Present(0, PresentFlags.None);
        }
    }
    [StructLayout(LayoutKind.Sequential)]
    public struct Vertex
    {
        public Vector3 Position;
        public Color4 Color;
        public Vertex(Vector4 pos, Color col) {
            Vector3 pp = new Vector3(pos.X, pos.Y, pos.Z);
            Position = pp; Color = new Color4(col);
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct WorldView
    {
        public Matrix World;
        public Matrix View;
        public Matrix Projection;
    }
}




