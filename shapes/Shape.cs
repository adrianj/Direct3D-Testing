using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using SlimDX;
using SlimDX.DXGI;
using System.Runtime.InteropServices;
using SlimDX.Direct3D10;
using Buffer = SlimDX.Direct3D10.Buffer;
using Device = SlimDX.Direct3D10.Device;
using SlimDX.D3DCompiler;
using System.Drawing;

namespace Direct3DLib
{
    /// <summary>
    /// A class that consists of a number of ColoredVertices, and an index buffer specifying
    /// how it is made up of flat triangles.
    /// </summary>
    [TypeConverter(typeof(GenericTypeConverter<Shape>))]
    public class Shape : Object3D
    {
        
        private bool mPick = true;
        public virtual bool CanPick { get { return mPick; } set { mPick = value; } }
		private Vertex [] mSelectedVerts = new Vertex[3];
		public Vertex[] SelectedVertices { get { return mSelectedVerts; } set { mSelectedVerts = value; } }

        private VertexList mVList = new VertexList();
        public VertexList Vertices { get { return mVList; } set { mVList = value; } }
		public int SelectedVertexIndex { get; set; }

        private Device mDevice;
		private ShaderSignature mSignature;
		private Color mSolidColor = Color.Empty;
		public Color SolidColor { get { return mSolidColor; } set { SetUniformColor(value); mSolidColor = value; Update(); } }

        public virtual PrimitiveTopology Topology { get { return Vertices.Topology; } set { Vertices.Topology = value; AutoGenerateIndices(); Update(); } }

		private int textureIndex = -1;
		public int TextureIndex { get { return textureIndex; } set { textureIndex = value; } }

		private BoundingBox preWorldTransformBox;

        public void SetUniformColor(Color color)
        {
            for (int i = 0; i < Vertices.Count; i++)
            {
                Vertex v = Vertices[i];
				if (color == Color.Empty)
					v.Color = Vertex.FloatToColor(v.Position);
				else
	                v.Color = color;
                Vertices[i] = v;
            }
                
        }

        protected SlimDX.Direct3D10.Buffer vertexBuffer;
        protected SlimDX.Direct3D10.Buffer indexBuffer;
        protected InputLayout vertexLayout;


		public Shape(int initialVertexCapacity)
			: base()
		{
			Vertices = new VertexList(initialVertexCapacity);
		}
        public Shape()
            : base()
        {
            Vertices = new VertexList();
        }
        public Shape(Vertex[] vertices)
            : this()
        {
            Vertices = new VertexList(vertices);
        }

        public virtual void AutoGenerateIndices()
        {
            Vertices.Indices = null;
        }


		public void Update() { Update(mDevice, mSignature); }
		//public virtual void Update(Device device) { Update(device, null); }
        public virtual void Update(Device device, ShaderSignature effectSignature)
        {
            if (device == null || device.Disposed) return;
			
            mDevice = device;
			mSignature = effectSignature;
            // If there is less than 1 vertex then we can't make a point, let alone a shape!
            if (Vertices == null || Vertices.Count < 1) return;

			CalculatePreTransformBoundingBox();

            // Add Vertices to a datastream.
            DataStream dataStream = new DataStream(Vertices.NumBytes, true, true);
            dataStream.WriteRange(this.Vertices.ToArray());
            dataStream.Position = 0;

            
            // Create a new data buffer description and buffer
            BufferDescription desc = new BufferDescription()
            {
                BindFlags = BindFlags.VertexBuffer,
                CpuAccessFlags = CpuAccessFlags.None,
                OptionFlags = ResourceOptionFlags.None,
                //SizeInBytes = 3 * Marshal.SizeOf(typeof(ColoredVertex)),
                SizeInBytes = Vertices.NumBytes,
                Usage = ResourceUsage.Default
            };
            vertexBuffer = new SlimDX.Direct3D10.Buffer(device, dataStream, desc);
            dataStream.Close();

            // Get the shader effects signature
			//if(effect == null)
			//	effect = Effect.FromFile(device, "RenderWithLighting.fx","fx_4_0");
				//effect = Effect.FromString(device, Properties.Resources., "fx_4_0");
			//EffectPass effectPass = effect.GetTechniqueByIndex(0).GetPassByName("ColoredWithLighting");


            if (Vertices != null && Vertices.Count > 0)
            {
                // Set the input layout.
                InputElement[] inputElements = Vertices[0].GetInputElements();
                vertexLayout = new InputLayout(device, effectSignature, inputElements);

                // Draw Indexed
                if (Vertices.Indices != null && Vertices.Indices.Count > 0)
                {
                    DataStream iStream = new DataStream(sizeof(int) * Vertices.Indices.Count, true, true);
                    iStream.WriteRange(Vertices.Indices.ToArray());
                    iStream.Position = 0;
                    desc = new BufferDescription()
                    {
                        Usage = ResourceUsage.Default,
                        SizeInBytes = sizeof(int) * Vertices.Indices.Count,
                        BindFlags = BindFlags.IndexBuffer,
                        CpuAccessFlags = CpuAccessFlags.None,
                        OptionFlags = ResourceOptionFlags.None
                    };
                    indexBuffer = new Buffer(device, iStream, desc);
                    iStream.Close();

                    

                }
                else
                {
                    if (indexBuffer != null) indexBuffer.Dispose();
                    indexBuffer = null;
                }
            }
            else
            {
                if (vertexBuffer != null) vertexBuffer.Dispose();
                vertexBuffer = null;
            }
        }




		private Vector3 GetMaxVector(Vector3[] vects)
		{
			Vector3 max = new Vector3(float.MinValue, float.MinValue, float.MinValue);
			foreach (Vector3 v in vects)
				max = Vector3.Maximize(max, v);
			return max;
		}
		private Vector3 GetMinVector(Vector3[] vects)
		{
			Vector3 min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
			foreach (Vector3 v in vects)
				min = Vector3.Minimize(min, v);
			return min;
		}

		public virtual void Render(Device device, ShaderHelper shaderHelper)
		{
			if (vertexBuffer != null && Topology != PrimitiveTopology.Undefined)
			{
				device.InputAssembler.SetInputLayout(vertexLayout);
				device.InputAssembler.SetPrimitiveTopology(Topology);

				device.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(vertexBuffer, Marshal.SizeOf(typeof(Vertex)), 0));
				if (indexBuffer != null)
					device.InputAssembler.SetIndexBuffer(indexBuffer, Format.R32_UInt, 0);


				shaderHelper.ConstantBufferSet.World = World;
				shaderHelper.ConstantBufferSet.LocalRotation = RotationMatrix;
				shaderHelper.ConstantBufferSet.TextureIndex = TextureIndex;
				shaderHelper.ApplyEffects();

				if (indexBuffer != null)
					device.DrawIndexed(Vertices.NumElements, 0, 0);
				else
				{
					device.Draw(Vertices.NumElements, 0);
				}
			}

		}

        public virtual bool RayIntersects(Ray ray, out float distance)
        {
            distance = float.MaxValue;
            if (Vertices == null || Vertices.Count < 1) return false;
			if (!this.CanPick) return false;
			if (!RayIntersectsBounds(ray, out distance)) return false;
			bool ints = RayIntersectsShape(ray, out distance);
            return ints;
        }

		private bool RayIntersectsShape(Ray ray, out float distance)
		{
			distance = float.MaxValue;
			bool ints = false;

			if (Vertices.Indices == null || Vertices.Indices.Count < 1)
			{
				if (Topology == PrimitiveTopology.TriangleList)
				{
					for (int i = 0; i < Vertices.Count - 2; i += 3)
					{
						Vector3 v1 = Vector3.TransformCoordinate(Vertices[i].Position, World);
						Vector3 v2 = Vector3.TransformCoordinate(Vertices[i + 1].Position, World);
						Vector3 v3 = Vector3.TransformCoordinate(Vertices[i + 2].Position, World);
						float d = float.MaxValue;
						bool ii = Ray.Intersects(ray, v1, v2, v3, out d);
						if (ii && d < distance)
						{
							distance = d;
							ints = true;
							SelectedVertices[0] = Vertices[i];
							SelectedVertices[0].Position = v1;
							SelectedVertices[1] = Vertices[i + 1];
							SelectedVertices[1].Position = v2;
							SelectedVertices[2] = Vertices[i + 2];
							SelectedVertices[2].Position = v3;
							SelectedVertexIndex = i;
						}
					}
				}
			}
			return ints;
		}

		private bool RayIntersectsBounds(Ray ray, out float distance)
		{
			distance = float.MaxValue;
			BoundingBox bb = Direct3DEngine.BoundingBoxMultiplyMatrix(this.MaxBoundingBox, World);
			return Ray.Intersects(ray, bb, out distance);
		}

		private void CalculatePreTransformBoundingBox()
		{
			Vector3[] vects = Vertices.GetVertexPositions();
			preWorldTransformBox = BoundingBox.FromPoints(vects);
		}

        public BoundingBox MaxBoundingBox
        {
			get { return preWorldTransformBox;}
        }


		private bool disposed = false;
		protected override void Dispose(bool disposing)
		{
			if (!this.disposed)
			{
				try
				{
					if (disposing)
						DisposeManaged();
					this.disposed = true;
				}
				finally
				{
					base.Dispose(disposing);
				}
			}
		}

        private void DisposeManaged()
        {
            if(vertexBuffer != null) vertexBuffer.Dispose();
            if(indexBuffer != null) indexBuffer.Dispose();
            if(vertexLayout != null) vertexLayout.Dispose();
        }
    }


}
