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
	public interface Mesh : IDisposable
	{
		int FaceCount { get; set; }
		Vector3[] Vertices { get; set; }
		uint[] FaceVertices { get; set; }
		D3D.PrimitiveTopology Topology { get; set; }
		void BindToPass(D3DDevice device, D3D.EffectPass pass);
		void Draw();
	}

	public class BasicMesh : Mesh
	{
		public int FaceCount { get; set; }
		public Vector3[] Vertices { get; set; }
		public uint[] FaceVertices { get; set; }
		public D3D.PrimitiveTopology Topology { get; set; }
		protected D3D.Device Device;
		protected D3D.Buffer vertexbuffer;
		protected D3D.InputLayout layout;

		public BasicMesh()
		{
			Topology = D3D.PrimitiveTopology.TriangleList;
		}

		public virtual void BindToPass(D3DDevice device, D3D.EffectPass pass)
		{
			this.Device = device.Device;
			if (vertexbuffer != null)
				this.Dispose();
			if (FaceCount < 1) return;
			UploadVertices();
			layout = IGameVertex.GetInputLayout(Device, pass);
		}

		protected virtual void UploadVertices()
		{
			int vertexcount = FaceCount * 3;

			SlimDX.DataStream stream = new SlimDX.DataStream(vertexcount * IGameVertex.Size, false, true);


			for (int i = 0; i < FaceCount; i++)
			{
				for (int j = 0; j < 3; ++j)
				{
					IGameVertex vertex = new IGameVertex();

					int face = 3 * i + j;


					vertex.Position = Vertices[FaceVertices[face]];
					vertex.TexCoord = new Vector2(vertex.Position.X, vertex.Position.Z);

					stream.Write<IGameVertex>(vertex);
				}
			}

			stream.Position = 0;

			D3D.BufferDescription bufferDescription = new D3D.BufferDescription();
			bufferDescription.BindFlags = D3D.BindFlags.VertexBuffer;
			bufferDescription.CpuAccessFlags = D3D.CpuAccessFlags.None;
			bufferDescription.OptionFlags = D3D.ResourceOptionFlags.None;
			bufferDescription.SizeInBytes = vertexcount * IGameVertex.Size;
			bufferDescription.Usage = D3D.ResourceUsage.Default;

			vertexbuffer = new D3D.Buffer(Device, stream, bufferDescription);
			stream.Close();
		}

		public virtual void Draw()
		{
			if (FaceCount < 1 || vertexbuffer == null || layout == null) return;
			Device.InputAssembler.SetInputLayout(layout);
			Device.InputAssembler.SetPrimitiveTopology(Topology);
			Device.InputAssembler.SetVertexBuffers(0, new D3D.VertexBufferBinding(vertexbuffer, IGameVertex.Size, 0));
			Device.Draw(FaceCount * 3, 0);
		}

		public virtual void Dispose()
		{
			if(vertexbuffer != null) vertexbuffer.Dispose();
			vertexbuffer = null;

			if(layout != null) layout.Dispose();
			layout = null;
		}
	}

	public struct IGameVertex
	{
		public Vector3 Position;
		//public Vector3 Normal;
		public Vector2 TexCoord;

		public static D3D.InputLayout GetInputLayout(D3D.Device device, D3D.EffectPass pass)
		{
			D3D.InputElement[] inputElements = 
			{
				new D3D.InputElement( "POSITION", 0, DXGI.Format.R32G32B32_Float,  0, 0 ),
				//new D3D10.InputElement( "NORMAL",   0, DXGI.Format.R32G32B32_Float, 12, 0 ),
				new D3D.InputElement( "TEXCOORD", 0, DXGI.Format.R32G32_Float,    12, 0 )
			};

			return new D3D.InputLayout(device, pass.Description.Signature, inputElements);
		}

		public static int Size
		{
			get { return System.Runtime.InteropServices.Marshal.SizeOf(typeof(IGameVertex)); }
		}
	}
}
