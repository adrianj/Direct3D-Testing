using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using SlimDX;

namespace Direct3DExtensions
{
	public class MeshOptimiser
	{
		public static void RemoveDuplicateVertices(Mesh mesh) { RemoveDuplicateVertices(mesh, 0.001f); }

		public static void RemoveDuplicateVertices(Mesh mesh, float tolerance)
		{
			Console.WriteLine("Vertices prior to duplicate removal: "+mesh.Vertices.Length);

			List<Vertex> vertices = mesh.Vertices.ToList();
			List<int> indicies = mesh.Indices.ToList();

			RemoveDuplicateVertices(ref vertices, indicies);
			mesh.Vertices = vertices.ToArray();
			mesh.Indices = indicies.ToArray();

			Console.WriteLine("Vertices after duplicate removal: " + mesh.Vertices.Length);
		}
		static List<Vertex> FindDuplicates(List<Vertex> vertices)
		{
			Dictionary<Vector3, Vertex> distinctValues = new Dictionary<Vector3, Vertex>();
			Dictionary<Vector3,Vertex> duplicates = new Dictionary<Vector3,Vertex>();
			foreach (Vertex v in vertices)
			{
				if (!distinctValues.ContainsKey(v.Pos))
					duplicates[v.Pos] = v;
			}
			return duplicates.Values.ToList();
		}

		public static void CombineIntoSingleMesh(Mesh hostMesh, List<Mesh> meshes)
		{
			foreach (Mesh mesh in meshes)
				AddMesh(hostMesh, mesh);
			{ }
		}

		/// <summary>
		/// Adds the vertex and index lists of two meshes together.
		/// Meshes must have the same Vertex structure, or an InvalidCastException will be thrown.
		/// </summary>
		/// <param name="hostMesh">The future combined mesh.</param>
		/// <param name="otherMesh">The other mesh to be added.</param>
		public static void AddMesh(Mesh hostMesh, Mesh otherMesh)
		{
			List<Vertex> hostVerts = hostMesh.Vertices.ToList();
			foreach (Vertex v in hostVerts)
				v.Pos = Vector3.TransformCoordinate(v.Pos, hostMesh.World);
			List<int> hostInds = hostMesh.Indices.ToList();
			List<Vertex> otherVerts = otherMesh.Vertices.ToList();
			foreach (Vertex v in otherVerts)
				v.Pos = Vector3.TransformCoordinate(v.Pos, otherMesh.World);
			List<int> otherInds = otherMesh.Indices.ToList();
			CombineVertexLists(hostVerts, hostInds, otherVerts, otherInds);
			hostMesh.Vertices = hostVerts.ToArray();
			hostMesh.Indices = hostInds.ToArray();
		}

		public static void CombineVertexLists(List<Vertex> hostVerts, List<int> hostInds, List<Vertex> otherVerts, List<int> otherInds)
		{
			Type vType;
			if (hostVerts.Count > 0)
				vType = hostVerts[0].GetType();
			else if (otherVerts.Count > 0)
				vType = otherVerts[0].GetType();
			else
				return;
			int IndexOffset = hostVerts.Count;
			for (int i = 0; i < otherInds.Count; i++)
			{
				otherInds[i] += IndexOffset;
				hostInds.Add(otherInds[i]);
			}

			hostVerts.AddRange(otherVerts);

			//RemoveDuplicateVertices(ref hostVerts, hostInds);
			/*
			for (int i = 0; i < otherVerts.Count; i++)
			{
				Vertex v = otherVerts[i];
				if (v.GetType() != vType)
					throw new InvalidCastException("Can only combine meshes where Verticies have the same type");
				AddVertexToHost(hostVerts, hostInds, v, i + IndexOffset, otherInds);
			}
			Console.WriteLine("maxInd: " + hostInds.Max());
			{ }
			 */
		}

		static void AddVertexToHost(List<Vertex> hostVerts, List<int> hostInds, Vertex vert, int newIndex, List<int> otherInds)
		{
			int duplicate = hostVerts.FindIndex(p=>p.Pos.Equals(vert.Pos));
			if (duplicate >= 0)
			{
				
				Console.WriteLine("A duplicate!!!" + vert+"ind: "+newIndex+" = "+hostVerts.Count);
				for (int i = 0; i < hostInds.Count; i++)
				{
					if (hostInds[i] == newIndex)
						hostInds[i] = duplicate;
					if (hostInds[i] > newIndex)
						hostInds[i]--;
				}
			}
			else
				hostVerts.Add(vert);
			
		}


		public static void RemoveDuplicateVertices(ref List<Vertex> vertices, List<int> indicies)
		{
			Console.WriteLine("Removing duplicates from Vertex List with " + vertices.Count + " elements");
			List<Vertex> uniqueVerticies = new List<Vertex>(vertices.Count);
			List<int> indexMap = new List<int>(indicies.Count);
			int duplicates = 0;

			for (int vertex = 0; vertex < vertices.Count; vertex++)
			{
				bool isDuplicate = false;
				int vertexToCheck;
				for (vertexToCheck = 0; vertexToCheck < vertex; vertexToCheck++)
				{
					Vertex v1 = vertices[vertexToCheck];
					Vertex v2 = vertices[vertex];
					if (VerticesEqual(v1,v2) && vertexToCheck != vertex)
					{
						isDuplicate = true;
						duplicates++;
						break;
					}
				}

				if (isDuplicate)
					indexMap.Add(vertexToCheck);
				else
				{
					indexMap.Add(vertex-duplicates);
					uniqueVerticies.Add(vertices[vertex]);
				}
				
			}
			for (int index = 0; index < indicies.Count; index++)
				indicies[index] = indexMap[indicies[index]];
			
			vertices.Clear();
			vertices.AddRange(uniqueVerticies);
		}

		static bool VerticesEqual(Vertex v1, Vertex v2)
		{
			if (v1.Pos.Equals(v2.Pos))
				return true;
			return false;
		}

	}
}
