using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using SlimDX;
using D3D9 = SlimDX.Direct3D9;

namespace Direct3DExtensions
{
	public class MeshFactory : DisposablePattern
	{
		MeshConverter converter;

		public Mesh CreateSphere(float radius, int slices, int stacks)
		{
			CreateConverter();
			D3D9.Mesh mesh9 = D3D9.Mesh.CreateSphere(converter.Device9, radius, slices, stacks);
			Mesh ret = converter.CreateMesh<VertexTypes.PositionNormal>(mesh9);
			mesh9.Dispose();
			return ret;
		}

		public Mesh CreateBox(float width, float height, float depth)
		{
			CreateConverter();
			D3D9.Mesh mesh9 = D3D9.Mesh.CreateBox(converter.Device9, width, height, depth);
			Mesh ret = converter.CreateMesh<VertexTypes.PositionNormal>(mesh9);
			mesh9.Dispose();
			return ret;
		}

		public Mesh CreateTeapot()
		{
			CreateConverter();
			D3D9.Mesh mesh9 = D3D9.Mesh.CreateTeapot(converter.Device9);
			Mesh ret = converter.CreateMesh<VertexTypes.PositionNormal>(mesh9);
			mesh9.Dispose();
			return ret;
		}

		public Mesh CreateTorus(float innerRadius, float outerRadius, int sides, int rings)
		{
			CreateConverter();
			D3D9.Mesh mesh9 = D3D9.Mesh.CreateTorus(converter.Device9, innerRadius, outerRadius, sides, rings);
			Mesh ret = converter.CreateMesh<VertexTypes.PositionNormal>(mesh9);
			mesh9.Dispose();
			return ret;
		}

		public Mesh CreateCylinder(float radius1, float radius2, float length, int slices, int stacks)
		{
			CreateConverter();
			D3D9.Mesh mesh9 = D3D9.Mesh.CreateCylinder(converter.Device9, radius1, radius2, length, slices, stacks);
			Mesh ret = converter.CreateMesh<VertexTypes.PositionNormal>(mesh9);
			mesh9.Dispose();
			return ret;
		}

		void CreateConverter()
		{
			if (converter == null)
				converter = new MeshConverter();
		}

		public Mesh CreateSquare(float width, float length)
		{
			Mesh ret = new BasicMesh();
			ret.Vertices = new Vertex[]{
				new VertexTypes.PositionNormalTextured(){ Pos = new Vector3(0, 0, 0), Normal = new Vector3(0,1,0), TexCoord = new Vector2(0,0)},
				new VertexTypes.PositionNormalTextured(){ Pos = new Vector3(0, 0, width), Normal = new Vector3(0,1,0), TexCoord = new Vector2(1,0)},
				new VertexTypes.PositionNormalTextured(){ Pos = new Vector3(length, 0, width), Normal = new Vector3(0,1,0), TexCoord = new Vector2(0,1)},
				new VertexTypes.PositionNormalTextured(){ Pos = new Vector3(length, 0, 0), Normal = new Vector3(0,1,0), TexCoord = new Vector2(1,1)}
			};
			ret.Indices = new int[] { 0, 1, 2, 0, 2, 3, 2, 1, 0, 3, 2, 0 };
			return ret;
		}

		public Mesh CreateGrid(float width, float length, int xTiles, int zTiles, bool originAtCentre)
		{
			Mesh mesh = new BasicMesh();
			mesh.Vertices = new Vertex[(xTiles+1) * (zTiles+1)];
			float xOffset = 0;
			float yOffset = 0;
			if (originAtCentre)
			{
				xOffset = -width / 2;
				yOffset = -length/2;
			}
			for (int x = 0; x < xTiles+1; x++)
			{
				float xf = (float)x / ((float)xTiles);
				for (int y = 0; y < zTiles+1; y++)
				{
					float yf = 0;
					float zf = (float)y / ((float)xTiles);
					VertexTypes.PositionNormalTextured vert = new VertexTypes.PositionNormalTextured();
					vert.Pos = new Vector3(xf * width + xOffset, yf, zf * length + yOffset);
					vert.TexCoord = new Vector2(xf, zf);
					vert.Normal = new Vector3(xf, yf / 10.0f, zf);
					mesh.Vertices[y * (xTiles+1) + x] = vert;
				}
			}
			
			mesh.Indices = new int[(xTiles) * (zTiles) * 6];
			int i = 0;
			for (int x = 0; x < xTiles; x++)
				for (int y = 0; y < zTiles; y++)
				{
					int v0 = IndexFromCoord(x, y, xTiles+1);
					int v1 = IndexFromCoord(x, y + 1, xTiles+1);
					int v2 = IndexFromCoord(x + 1, y + 1, xTiles+1);
					int v3 = IndexFromCoord(x + 1, y, xTiles+1);
					mesh.Indices[i++] = v0;
					mesh.Indices[i++] = v1;
					mesh.Indices[i++] = v2;
					mesh.Indices[i++] = v0;
					mesh.Indices[i++] = v2;
					mesh.Indices[i++] = v3;
				}
			return mesh;
		}

		public Mesh CreateDiamondGrid(int numColumns, int numRows)
		{
			CustomMesh ret = new CustomMesh();
			List<Vector3> vBuf = new List<Vector3>();
			List<int> iBuf = new List<int>();
			DiamondSquare sq = new DiamondSquare();

			for (int r = -numColumns; r < numColumns; r+=2)
			{
				for (int c = -numColumns; c < numColumns; c+=2)
				{
					sq.AppendTranslatedSquare(vBuf, iBuf, new Vector3(c, 0, r), DiamondSquare.SquareType.Normal);
				}
			}
			ret.SetVertexData<VertexTypes.Position>(vBuf, iBuf);
			return ret;
		}

		public static int IndexFromCoord(int x, int y, int width)
		{
			return y * width + x;
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
