using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using NUnit.Framework;

using Direct3DExtensions;
using D3D9 = SlimDX.Direct3D9;
using D3D10 = SlimDX.Direct3D10;

namespace Direct3DExtensions_Test
{
	[TestFixture]
	public class TestMeshConverter
	{
		[Test]
		public void CreateSquare()
		{
			using (MeshFactory factory = new MeshFactory())
			{
				Mesh mesh = factory.CreateSquare(2, 2);
				PrintMesh(mesh);
				mesh.Dispose();
			}
		}

		[Test]
		public void CreateSphereWithFactory()
		{
			using (MeshFactory factory = new MeshFactory())
			{
				Mesh mesh = factory.CreateSphere(1, 4, 4);
				PrintMesh(mesh);
				mesh.Dispose();
			}
		}

		[Test]
		public void CreateMassiveSphere()
		{
			using (MeshFactory factory = new MeshFactory())
			{
				Mesh mesh = factory.CreateSphere(3, 150, 100);
				Console.WriteLine("Sphere has: " + mesh.Vertices.Length + " vertices");
				mesh.Dispose();
			}
		}

		[Test]
		public void TestBoxCast()
		{
			Mesh mesh;
			using (MeshFactory factory = new MeshFactory())
			{
				mesh = factory.CreateBox(1,1,1);
				PrintMesh(mesh);
			}
			Vertex[] castVerts = mesh.Vertices.Select<Vertex, Vertex>(i => { return VertexTypes.Cast(i, typeof(VertexTypes.PositionNormalTextured)); }).ToArray();

			PrintArray(castVerts);
			mesh.Dispose();
		}

		public void PrintMesh(D3D9.Mesh mesh)
		{
			Console.WriteLine("" + mesh.VertexBuffer);

			Console.WriteLine("" + mesh.FaceCount+", "+mesh.VertexCount+": "+mesh.VertexFormat);
		}


		public void PrintMesh(Mesh mesh)
		{
			Console.WriteLine("" + mesh);

			Console.WriteLine("FaceVerts: " + mesh.Indices.Length + ", numVerts: " + mesh.Vertices.Length + ": " + mesh.Indices);
			PrintArray(mesh.Vertices);
			Console.WriteLine("FaceVertices: ");
			PrintArray(mesh.Indices);
		}

		public void PrintArray(Array a)
		{
			for (int i = 0; i < a.Length; i++)
				Console.WriteLine("" + a.GetValue(i));
		}
	}
}
