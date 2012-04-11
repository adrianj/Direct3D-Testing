using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using SlimDX;
using D3D = SlimDX.Direct3D10;

namespace Direct3DExtensions
{
	public class GeometryOutputStream<T> : DisposablePattern, IEnumerable<T> where T : struct
	{
		D3D.Device device;
		D3D.Buffer gsOutput;
		D3D.Buffer staging;
		bool dataCopied = false;

		public D3D.Buffer GSOutputBuffer { get { return gsOutput; } }

		public GeometryOutputStream(D3D.Device device, D3D.EffectPass pass, int maximumNumber)
		{
			this.device = device;
			int size = Marshal.SizeOf(typeof(T)) * maximumNumber;
			D3D.BufferDescription bdesc = new D3D.BufferDescription()
			{
				BindFlags =  D3D.BindFlags.StreamOutput | D3D.BindFlags.VertexBuffer,
				CpuAccessFlags = D3D.CpuAccessFlags.None,
				OptionFlags = D3D.ResourceOptionFlags.None,
				SizeInBytes = size,
				Usage = D3D.ResourceUsage.Default
			};
			gsOutput = new D3D.Buffer(device, bdesc);
			D3D.BufferDescription sbdesc = new D3D.BufferDescription()
			{
				BindFlags = D3D.BindFlags.None,
				CpuAccessFlags = D3D.CpuAccessFlags.Read,
				OptionFlags = D3D.ResourceOptionFlags.None,
				SizeInBytes = bdesc.SizeInBytes,
				Usage = D3D.ResourceUsage.Staging
			};
			staging = new D3D.Buffer(device, sbdesc);
		}

		public void BindTarget()
		{
			D3D.StreamOutputBufferBinding[] bindings = { new D3D.StreamOutputBufferBinding(gsOutput,0) };
			device.StreamOutput.SetTargets(bindings);
			dataCopied = false;
		}

		public void UnbindTarget()
		{
			device.StreamOutput.SetTargets(new D3D.StreamOutputBufferBinding[0]);
		}

		public void CopyStream()
		{
			Console.WriteLine("Gs copy");
			device.CopyResource(gsOutput, staging);
			dataCopied = true;
		}

		long NumTriangles(long len) { return len / 3; }

		IEnumerator<T> IEnumerable<T>.GetEnumerator()
		{
			if (staging == null)
				yield break;
			if (!dataCopied)
				CopyStream();
			using (DataStream stream = staging.Map(D3D.MapMode.Read, D3D.MapFlags.None))
			{
				long numVertices = NumTriangles(stream.Length / Marshal.SizeOf(typeof(T))) * 3;
				for (int i = 0; i < numVertices; i++)
				{
					T d = stream.Read<T>();
					yield return d;
				}
			}
			staging.Unmap();
		}

		public System.Collections.IEnumerator GetEnumerator()
		{
			return GetEnumerator();
		}

		#region Dispose
		void DisposeManaged()
		{
			if (staging != null) staging.Dispose(); staging = null;
			if (gsOutput != null) gsOutput.Dispose(); gsOutput = null;
		}
		void DisposeUnmanaged() { }

		bool disposed = false;
		protected override void Dispose(bool disposing)
		{
			if (!disposed)
			{
				if (disposing)
				{
					DisposeManaged();
				}

				DisposeUnmanaged();
				disposed = true;
			}
			base.Dispose(disposing);
		}
		#endregion
    
	}
}
