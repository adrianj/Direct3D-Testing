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
		Vector3 Rotation { get; set; }
		Vector3 Scale { get; set; }
		Vector3 Translation { get; set; }
		Matrix World { get; }
		Vertex[] Vertices { get; set; }
		int[] Indices { get; set; }
		int EffectPassIndex { get; }
		D3D.PrimitiveTopology Topology { get; set; }
		void BindToPass(D3DDevice device, Effect effect, int passIndex);
		void BindToPass(D3DDevice device, Effect effect, string passName);
		void Draw();
	}


	public class BasicMesh : DisposablePattern, Mesh
	{
		public int EffectPassIndex { get; private set; }
		public virtual Vertex[] Vertices { get; set; }
		public virtual int[] Indices { get; set; }
		public D3D.PrimitiveTopology Topology { get; set; }
		protected D3DDevice Device;
		protected Effect Effect;
		protected D3D.Buffer vertexbuffer;
		protected D3D.Buffer indexbuffer;
		protected D3D.InputLayout layout;

		public virtual Vector3 Rotation { get; set; }
		public virtual Vector3 Scale {get;set;}
		public virtual Vector3 Translation { get; set; }
		public Matrix World { get { return CalculateWorld(); } }
		//protected Matrix World = Matrix.Identity;

		long memoryPressure = 0;
		void AddMemoryPressure(long mem)
		{
			memoryPressure += mem;
			GC.AddMemoryPressure(mem);
		}

		void RemoveMemoryPressure(long mem)
		{
			if (mem > memoryPressure)
				throw new Exception("Removing move memory than we know about. What happened?!");
			memoryPressure -= mem;
			if (memoryPressure < 0)
				throw new Exception("Memory pressure has gone negative.  What?");
			GC.RemoveMemoryPressure(mem);
		}

		void EmptyMemoryPressure()
		{
			if(memoryPressure > 0)
				RemoveMemoryPressure(memoryPressure);

		}

		public BasicMesh()
		{
			Topology = D3D.PrimitiveTopology.TriangleList;
			Vertices = new Vertex[0];
			Indices = new int[0];
			Scale = new Vector3(1,1,1);
		}

		public void BindToPass(D3DDevice device, Effect effect, string passName)
		{
			int passIndex = effect.GetPassIndexByName(passName);
			this.BindToPass(device, effect, passIndex);
		}

		public virtual void BindToPass(D3DDevice device, Effect effect, int passIndex)
		{
			this.Device = device;
			this.Effect = effect;
			EffectPassIndex = passIndex;
			UploadToGpu();
		}


		protected virtual void SetVertexPositionData<T>(Vector3[] vPos, int[] ind) where T : Vertex
		{
			this.Indices = ind;
			this.Vertices = new Vertex[vPos.Length];
			for (int i = 0; i < vPos.Length; i++)
			{
				T v = default(T);
				v.Pos = vPos[i];
				Vertices[i] = v;
			}
		}

		protected void UploadToGpu()
		{
			if (this.Effect == null || this.Device == null) return;
			D3D.EffectPass effectPass = Effect[EffectPassIndex];
			if (vertexbuffer != null || indexbuffer != null || layout != null)
				this.DisposeUnmanaged();
			if (Indices.Length < 1 || Vertices.Length < 1) return;
			UploadVertices();
			UploadIndices();

			layout = VertexTypes.GetInputLayout(this.Device.Device, effectPass, Vertices[0].GetType());
		}


		protected virtual void UploadIndices()
		{
			long size = sizeof(uint) * Indices.Length;
			AddMemoryPressure(size);
			DataStream iStream = new DataStream(size, true, true);
			
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
				indexbuffer = new D3D.Buffer(this.Device.Device, iStream, desc);
				iStream.Close();
				iStream.Dispose();
		}

		protected virtual void UploadVertices()
		{
			int vertexcount = Vertices.Length;
			int vsize = VertexTypes.SizeOf(Vertices[0].GetType());
			long size = vertexcount * vsize;
			AddMemoryPressure(size);
			SlimDX.DataStream stream = new SlimDX.DataStream(size, false, true);
			
				for (int i = 0; i < Vertices.Length; i++)
				{
					int face = i;
					Vertex vertex = Vertices[i];

					byte[] buffer = VertexTypes.GetBytes(vertex);
					stream.Write(buffer, 0, buffer.Length);
				}

				stream.Position = 0;

				D3D.BufferDescription bufferDescription = new D3D.BufferDescription();
				bufferDescription.BindFlags = D3D.BindFlags.VertexBuffer;
				bufferDescription.CpuAccessFlags = D3D.CpuAccessFlags.None;
				bufferDescription.OptionFlags = D3D.ResourceOptionFlags.None;
				bufferDescription.SizeInBytes = vertexcount * vsize;
				bufferDescription.Usage = D3D.ResourceUsage.Default;

				

				vertexbuffer = new D3D.Buffer(Device.Device, stream, bufferDescription);
				stream.Close();
				stream.Dispose();
		}


		public virtual void Draw()
		{
			if (Vertices.Length < 1 || Indices.Length < 1 || vertexbuffer == null || layout == null) return;
			DoDrawConstants();
			DoDrawVertices();
			DoDrawFinal();
		}

		protected void DoDrawFinal()
		{
			Device.DrawIndexed(Indices.Length);
		}

		protected void DoDrawVertices()
		{
			Device.Device.InputAssembler.SetInputLayout(layout);
			Device.Device.InputAssembler.SetPrimitiveTopology(Topology);
			int vsize = VertexTypes.SizeOf(Vertices[0].GetType());
			Device.Device.InputAssembler.SetVertexBuffers(0, new D3D.VertexBufferBinding(vertexbuffer, vsize, 0));
			Device.Device.InputAssembler.SetIndexBuffer(indexbuffer, SlimDX.DXGI.Format.R32_UInt, 0);
		}

		protected void DoDrawConstants()
		{
			Matrix World = CalculateWorld();
			Effect.SetWorld(World);
			Effect[EffectPassIndex].Apply();
		}

		protected virtual Matrix CalculateWorld()
		{
			Matrix World = Matrix.Multiply(Matrix.Identity, Matrix.Scaling(Scale));
			World = Matrix.Multiply(World, Matrix.RotationYawPitchRoll(Rotation.Y, Rotation.X, Rotation.Z));
			World = Matrix.Multiply(World, Matrix.Translation(Translation));
			return World;
		}

		private bool disposed = false;
		protected override void Dispose(bool disposing)
		{
			if (!this.disposed)
			{
				if (disposing)
				{
					DisposeManaged();
				}
				DisposeUnmanaged();
				this.disposed = true;
			}
			base.Dispose(disposing);
		}
		void DisposeManaged() { }
		private void DisposeUnmanaged()
		{
			if (vertexbuffer != null) vertexbuffer.Dispose();
			vertexbuffer = null;
			if (indexbuffer != null) indexbuffer.Dispose();
			indexbuffer = null;
			if (layout != null) layout.Dispose();
			layout = null;
			EmptyMemoryPressure();
		}
	}
}
