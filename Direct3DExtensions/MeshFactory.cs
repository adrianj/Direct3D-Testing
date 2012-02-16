using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using D3D9 = SlimDX.Direct3D9;

namespace Direct3DExtensions
{
	public class MeshFactory : DisposablePattern
	{
		MeshConverter converter = new MeshConverter();

		public Mesh CreateSphere(float radius, int slices, int stacks)
		{
			D3D9.Mesh mesh9 = D3D9.Mesh.CreateSphere(converter.Device9, radius, slices, stacks);
			return converter.CreateMesh<VertexTypes.PositionNormal>(mesh9);
		}

		public Mesh CreateBox(float width, float height, float depth)
		{
			D3D9.Mesh mesh9 = D3D9.Mesh.CreateBox(converter.Device9, width, height, depth);
			return converter.CreateMesh<VertexTypes.PositionNormal>(mesh9);
		}

		public Mesh CreateTeapot()
		{
			D3D9.Mesh mesh9 = D3D9.Mesh.CreateTeapot(converter.Device9);
			return converter.CreateMesh<VertexTypes.PositionNormal>(mesh9);
		}

		public Mesh CreateTorus(float innerRadius, float outerRadius, int sides, int rings)
		{
			D3D9.Mesh mesh9 = D3D9.Mesh.CreateTorus(converter.Device9, innerRadius, outerRadius, sides, rings);
			return converter.CreateMesh<VertexTypes.PositionNormal>(mesh9);
		}

		public Mesh CreateCylinder(float radius1, float radius2, float length, int slices, int stacks)
		{
			D3D9.Mesh mesh9 = D3D9.Mesh.CreateCylinder(converter.Device9, radius1, radius2, length,  slices, stacks);
			return converter.CreateMesh<VertexTypes.PositionNormal>(mesh9);
		}

		private bool disposed = false;
		protected override void Dispose(bool disposing)
		{
			if (!disposed)
			{
				if (disposing)
				{
					if (converter != null) converter.Dispose();
					converter = null;
				}
				this.disposed = true;
			}
			base.Dispose(disposing);
		}
	}
}
