using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SlimDX;

namespace Direct3DLib.shapes
{
	public class Grid : Shape
	{
		private int xCount = 4;
		private int yCount = 4;
		private int zCount = 4;
		public int XCount { get { return xCount; } set { xCount = value; Regenerate(); } }
		public int YCount { get { return yCount; } set { yCount = value; Regenerate(); } }
		public int ZCount { get { return zCount; } set { zCount = value; Regenerate(); } }

		public Grid() : base() { Regenerate(); }
		protected virtual void Regenerate()
		{
			this.Topology = SlimDX.Direct3D10.PrimitiveTopology.LineList;
			this.Vertices.Clear();
			for(int x = 0; x < xCount; x++)
				for(int y = 0; y < yCount; y++)
					for (int z = 0; z < zCount; z++)
					{
						this.Vertices.Add(new Vertex(new Vector3(x, y, z), this.SolidColor, Vector3.UnitY));
					}

			AutoGenerateIndices();
			this.UpdateColor();
			this.Update();
		}

		protected override void AutoGenerateIndices()
		{
			this.Vertices.Indices = new List<int>();
			for (int x = 0; x < xCount; x++)
				for (int y = 0; y < yCount; y++)
				{
					this.Vertices.Indices.Add(GetIndexFromXYZ(x, y, 0));
					this.Vertices.Indices.Add(GetIndexFromXYZ(x, y, zCount - 1));
				}
			for (int x = 0; x < xCount; x++)
				for (int z = 0; z < zCount; z++)
				{
					this.Vertices.Indices.Add(GetIndexFromXYZ(x, 0, z));
					this.Vertices.Indices.Add(GetIndexFromXYZ(x, yCount - 1, z));
				}
			for (int z = 0; z < zCount; z++)
				for (int y = 0; y < yCount; y++)
				{
					this.Vertices.Indices.Add(GetIndexFromXYZ(0, y, z));
					this.Vertices.Indices.Add(GetIndexFromXYZ(xCount - 1, y, z));
				}
		}

		int GetIndexFromXYZ(int x, int y, int z)
		{
			int ret = z % zCount;
			ret += (y * zCount);
			ret += (x * yCount * zCount);
			return ret;
		}
	}
	
}
