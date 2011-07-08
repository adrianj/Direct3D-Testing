using System;
using System.Collections.Generic;
using System.Text;
using SlimDX;
using SlimDX.DXGI;
using System.Runtime.InteropServices;
using SlimDX.Direct3D11;
using Buffer = SlimDX.Direct3D11.Buffer;
using Device = SlimDX.Direct3D11.Device;
using SlimDX.D3DCompiler;
using System.Drawing;

namespace MiniTri
{
    /// <summary>
    /// A class that consists of a number of ColoredVertices, and an index buffer specifying
    /// how it is made up of flat triangles.
    /// </summary>
    public class Shape : Object3D
    {
        public Vertex[] Vertices { get; set; }
        public short[] Indices { get; set; }
        public long VertexSize
        {
            get
            {
                if (Vertices == null) return 0;
                return Marshal.SizeOf(typeof(Vertex)) * Vertices.Length;
            }
        }
        private int nElements = 0;
        public int NumElements { get { return nElements; } }
        
        private SlimDX.Direct3D11.Buffer vertexBuffer;
        private SlimDX.Direct3D11.Buffer indexBuffer;
        private InputLayout vertexLayout;
        private Effect effect;
        private EffectTechnique effectTechnique;
        private EffectPass effectPass;
        private EffectMatrixVariable transformVariable;

        public Shape() { }
        public Shape(Vertex[] vertices)
        {
            Vertices = vertices;
            if (vertices != null && vertices.Length > 2)
            {
                // Auto-populate the Indices
                int iLen = (Vertices.Length - 2) * 3;
                Indices = new short[iLen];
                for (short i = 0; i < Vertices.Length-2; i++)
                {
                    Indices[i*3] = 0;
                    Indices[i * 3 + 1] = i;
                    Indices[i * 3 + 2] = (short)(i + 1);
                }
            }
        }

        

        public void Update(Device device)
        {
            // If there are less than 2 vertices then we can't make a line, let alone a shape!
            if (Vertices == null || Vertices.Length < 2) return;

            nElements = Vertices.Length;

            // Add Vertices to a datastream.
            DataStream dataStream = new DataStream(this.VertexSize, true, true);
            dataStream.WriteRange(this.Vertices);
            dataStream.Position = 0;
            // Create a new data buffer description and buffer
            BufferDescription desc = new BufferDescription()
            {
                BindFlags = BindFlags.VertexBuffer,
                CpuAccessFlags = CpuAccessFlags.None,
                OptionFlags = ResourceOptionFlags.None,
                //SizeInBytes = 3 * Marshal.SizeOf(typeof(ColoredVertex)),
                SizeInBytes = (int)this.VertexSize,
                Usage = ResourceUsage.Default
            };
            vertexBuffer = new SlimDX.Direct3D11.Buffer(device, dataStream, desc);
            dataStream.Close();

            // Get the shader effects.
            ShaderBytecode bytecode = ShaderBytecode.CompileFromFile("shaders/SimpleRender.fx", "fx_5_0", ShaderFlags.None, EffectFlags.None);
            effect = new Effect(device, bytecode);
            effectTechnique = effect.GetTechniqueByIndex(0);
            effectPass = effectTechnique.GetPassByIndex(0);
            transformVariable = effect.GetVariableByName("WorldViewProj").AsMatrix();

            // Set the input layout.
            InputElement[] inputElements = new InputElement[]
            {
                new InputElement("POSITION",0,Format.R32G32B32A32_Float,0,0),
                new InputElement("COLOR",0,Format.R32G32B32A32_Float,16,0)
            };
            vertexLayout = new InputLayout(device, effectPass.Description.Signature, inputElements);

            // Draw Indexed
            if (this.Indices != null)
            {
                DataStream iStream = new DataStream(sizeof(ushort) * this.Indices.Length, true, true);
                iStream.WriteRange(this.Indices);
                iStream.Position = 0;
                desc = new BufferDescription()
                {
                    Usage = ResourceUsage.Default,
                    SizeInBytes = sizeof(ushort) * this.Indices.Length,
                    BindFlags = BindFlags.IndexBuffer,
                    CpuAccessFlags = CpuAccessFlags.None,
                    OptionFlags = ResourceOptionFlags.None
                };
                indexBuffer = new Buffer(device, iStream, desc);
                iStream.Close();
                nElements = Indices.Length;
            }
        }

        public void Render(DeviceContext context, Matrix worldViewProj)
        {
            context.InputAssembler.InputLayout = vertexLayout;
            context.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;
            context.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(vertexBuffer, Marshal.SizeOf(typeof(Vertex)), 0));
            if(this.Indices != null)
                context.InputAssembler.SetIndexBuffer(indexBuffer, Format.R16_UInt, 0);
            Matrix wvp = World * worldViewProj;
            transformVariable.SetMatrix(wvp);
            effectPass.Apply(context);
            
            if (this.Indices != null)
                context.DrawIndexed(this.NumElements, 0, 0);
            else
                context.Draw(this.NumElements, 0);
        }


    }

    public class Cube : Shape
    {
        public Cube()
            : base()
        {
            Vertices = new Vertex[]
            {
                new Vertex(new Vector4(-1, -1, -1,1), Color.Yellow),
                new Vertex(new Vector4(1, -1, -1,1), Color.Yellow),
                new Vertex(new Vector4(1, -1, 1,1), Color.Red),
                new Vertex(new Vector4(-1, -1, 1,1), Color.Red),
                new Vertex(new Vector4(-1, 1, -1,1), Color.Blue),
                new Vertex(new Vector4(1,1,-1,1), Color.Blue),
                new Vertex(new Vector4(1,1,1,1), Color.Green),
                new Vertex(new Vector4(-1, 1,1,1), Color.Green)
            };
            Indices = new short[]
            {
                4,1,0,4,5,1,
                5,2,1,5,6,2,
                6,3,2,6,7,3,
                7,0,3,7,4,0,
                7,5,4,7,6,5,
                2,3,0,2,0,1
            };
        }
        public Cube(Color4 color)
            : this()
        {
            for(int i = 0; i < Vertices.Length; i++)
                Vertices[i].Color = color;
        }
    }

    public class Triangle : Shape
    {
        public Triangle()
            : base(new Vertex[]{
                new Vertex( new Vector4(0.0f, 0.5f, 0.5f,1), Color.Red ),
                new Vertex( new Vector4(0.5f, -0.5f, 0.5f,1), Color.Green),
                new Vertex( new Vector4(-0.5f, -0.5f, 0.5f,1), Color.Blue )})
        { }
        public Triangle(Color4 color)
            : this()
        {
            for (int i = 0; i < Vertices.Length; i++)
                Vertices[i].Color = color;
        }
    }

    /// <summary>
    /// A flat (Y = 0) square.
    /// </summary>
    public class Square : Shape
    {
        public Square()
            : base()
        {
            Vertices = new[]{
                new Vertex( new Vector4(-1f, 0f, -1f,1), Color.Red ),
                new Vertex( new Vector4(-1f, 0f, 1f,1), Color.Green),
                new Vertex( new Vector4(1f, 0f, 1f,1), Color.Yellow),
                new Vertex( new Vector4(1f, 0f, -1f,1), Color.Blue )};
            Indices = new short[] { 0, 1, 2, 2, 3, 0 };
        }
        public Square(Color4 color)
            : this()
        {
            for (int i = 0; i < Vertices.Length; i++)
                Vertices[i].Color = color;

        }
    }

    public class XAxis : Shape
    {
        public XAxis()
            : base()
        {
            Vertices = new[]{
                new Vertex(new Vector4(-1000f,0,0,1),Color.Black),
                new Vertex(new Vector4(1000f,0,0,1),Color.Black)}; 
        }
    }
}
