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
		Vertex[] Vertices { get; set; }
		uint[] Indices { get; set; }
		int EffectPassIndex { get; }
		D3D.PrimitiveTopology Topology { get; set; }
		void BindToPass(D3DDevice device, Effect effect, int passIndex);
		void Draw();
	}


	public class BasicMesh : DisposablePattern, Mesh
	{
		public int EffectPassIndex { get; private set; }
		public virtual Vertex[] Vertices { get; set; }
		public virtual uint[] Indices { get; set; }
		public D3D.PrimitiveTopology Topology { get; set; }
		protected D3D.Device Device;
		protected D3D.Buffer vertexbuffer;
		protected D3D.Buffer indexbuffer;
		protected D3D.InputLayout layout;


		public BasicMesh()
		{
			Topology = D3D.PrimitiveTopology.TriangleList;
			Vertices = new Vertex[0];
			Indices = new uint[0];
		}


		public virtual void BindToPass(D3DDevice device, Effect effect, int passIndex)
		{
			this.Device = device.Device;
			if (vertexbuffer != null)
				this.Dispose();
			if (Indices.Length < 1 || Vertices.Length < 1) return;
			UploadVertices();
			UploadIndices();
			layout = VertexTypes.GetInputLayout(Device, effect[passIndex], Vertices[0].GetType());
			EffectPassIndex = passIndex;
		}

		protected virtual void UploadIndices()
		{
			using (DataStream iStream = new DataStream(sizeof(uint) * Indices.Length, true, true))
			{
				iStream.WriteRange(Indices);
				iStream.Position = 0;
				D3D.BufferDescription desc = new D3D.BufferDescription()
				{
					Usage = D3D.ResourceUsage.Default,
					SizeInBytes = sizeof(uint) * Indices.Length,
					BindFlags = D3D.BindFlags.IndexBuffer,
					CpuAccessFlags = D3D.CpuAccessFlags.None,
					OptionFlags = D3D.ResourceOptionFlags.None
				};
				if (indexbuffer != null)
					indexbuffer.Dispose();
				indexbuffer = new D3D.Buffer(this.Device, iStream, desc);
			}
		}

		protected virtual void UploadVertices()
		{
			int vertexcount = Vertices.Length * 3;
			int vsize = VertexTypes.SizeOf(Vertices[0].GetType());
			using (SlimDX.DataStream stream = new SlimDX.DataStream(vertexcount * vsize, false, true))
			{
				for (int i = 0; i < Vertices.Length; i++)
				{
					int face = i;
					Vertex vertex = Vertices[i];

					byte[] buffer = VertexTypes.GetBytes(vertex);
					stream.Write(buffer, 0, buffer.Length);
				}
				Console.WriteLine("StreamPos: " + stream.Position);

				stream.Position = 0;

				D3D.BufferDescription bufferDescription = new D3D.BufferDescription();
				bufferDescription.BindFlags = D3D.BindFlags.VertexBuffer;
				bufferDescription.CpuAccessFlags = D3D.CpuAccessFlags.None;
				bufferDescription.OptionFlags = D3D.ResourceOptionFlags.None;
				bufferDescription.SizeInBytes = vertexcount * vsize;
				bufferDescription.Usage = D3D.ResourceUsage.Default;

				vertexbuffer = new D3D.Buffer(Device, stream, bufferDescription);
			}
		}


		public virtual void Draw()
		{
			if (Vertices.Length < 1 || Indices.Length < 1 || vertexbuffer == null || layout == null) return;
			Device.InputAssembler.SetInputLayout(layout);
			Device.InputAssembler.SetPrimitiveTopology(Topology);
			int vsize = VertexTypes.SizeOf(Vertices[0].GetType());
			Device.InputAssembler.SetVertexBuffers(0, new D3D.VertexBufferBinding(vertexbuffer, vsize, 0));
			Device.InputAssembler.SetIndexBuffer(indexbuffer, SlimDX.DXGI.Format.R32_UInt, 0);
			Device.DrawIndexed(Indices.Length, 0, 0);
		}

		private bool disposed = false;
		protected override void Dispose(bool disposing)
		{
			if (!this.disposed)
			{
				if (disposing)
				{
					if (vertexbuffer != null) vertexbuffer.Dispose();
					vertexbuffer = null;
					if (indexbuffer != null) indexbuffer.Dispose();
					indexbuffer = null;
					if (layout != null) layout.Dispose();
					layout = null;
				}
				this.disposed = true;
			}
			base.Dispose(disposing);
		}
	}
}
