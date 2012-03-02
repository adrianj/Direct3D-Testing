using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Drawing;

namespace Direct3DExtensions
{
	public class ExpandedPlane : BasicMesh
	{
		int xTiles = 1;
		int yTiles = 1;

		public Size Segments
		{
			get { return new Size(xTiles, yTiles); }
			set { Recreate(value.Width, value.Height); }
		}

		public ExpandedPlane()
			: base()
		{
			Recreate(2,2);
		}

		public void Recreate(int xTiles, int yTiles)
		{
			this.xTiles = xTiles;
			this.yTiles = yTiles;
			CreateRegularGrid(xTiles, yTiles);
			ExpandMesh();
			this.UploadToGpu();
		}

		protected virtual void ExpandMesh()
		{
			for (int i = 0; i < this.Vertices.Length; i++)
			{
				Vertex v = Vertices[i];
				
				double x = v.Pos.X;
				double y = v.Pos.Z;
				double a = Math.Atan2(y, x);
				double d = x * x + y * y;
				if (d <= 1) 
					d = Math.Sqrt(d);
				//d = Math.Pow(2, d)-1;
				//else d = Math.Sqrt(d);
				x = Math.Cos(a) * d;
				//y = Math.Sin(a) * d;

				v.Pos = new SlimDX.Vector3((float)x, 0, (float)y);
				Vertices[i] = v;
			}
		}


		protected virtual void CreateRegularGrid(int xTiles, int yTiles)
		{
			using (MeshFactory fact = new MeshFactory())
			{
				Mesh grid = fact.CreateGrid(15, 15, xTiles, yTiles, true);
				CopyFromMesh(grid);
			}
		}

		protected void CopyFromMesh(Mesh grid)
		{
			this.Vertices = new Vertex[grid.Vertices.Length];
			grid.Vertices.CopyTo(this.Vertices, 0);
			this.Indices = new int[grid.Indices.Length];
			grid.Indices.CopyTo(this.Indices, 0);
		}
	}
}
