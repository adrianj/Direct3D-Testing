using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.ComponentModel;
using SlimDX.Direct3D10;
using SlimDX;

namespace Direct3DLib
{
	[ToolboxItem(true)]
    public class Sphere : Shape
    {
		// It's easier to internally store nLatLines as LatLines + 1.
		private int nLatLines = 6;
		private int nLongLines = 12;
		public int LatLines { get { return nLatLines-1; } set { nLatLines = value+1; Regenerate(); } }
		public int LongLines { get { return nLongLines; } set { nLongLines = value; Regenerate(); } }
		private int mCorners = 6;
		public int Corners
		{
			get { return mCorners; }
			set
			{
				if (value < 4) throw new ArgumentException("Sphere shape must have at least 4 corners");
				mCorners = value; 
				Regenerate();
			}
		}
        public Sphere() : this(6) { }
        public Sphere(Color col) : this(6, col) { }
		public Sphere(int corners, Color col) : this(corners) { SolidColor = col; }
        public Sphere(int corners)
            : base()
        {
            
			Corners = corners;

        }

		private void Regenerate()
		{
			checkParams();
			AutoGenerateVertices();
			AutoGenerateIndices();
			Update();
		}

		private void checkParams()
		{
			// Corners should be an even number.
			mCorners = 2 * (Corners / 2);
			// Corners should be evenly divisible by LongLines and LatLines
			// be sure not to assign to LatLines or LongLines directly, as it will cause a StackOverflow.
			if (nLatLines > Corners / 2)
				nLatLines = Corners / 2;
			int i = Corners/2;
			while (Corners % nLatLines != 0)
			{
				nLatLines = Corners / i++;
			}
			if (nLongLines > Corners)
				nLongLines = Corners;
			if (Corners % nLongLines != 0)
			{
				i = Corners / nLongLines;
				nLongLines = Corners / i;
			}
		}


		public void AutoGenerateVertices()
		{
			PrimitiveTopology top = Topology;
			List<Vertex> verts = new List<Vertex>(Corners * (Corners - 1) + 2);
			// Figure out all the points.
			int height = Corners / 2 + 1;
			int circ = Corners;
			float[,] x = new float[height, circ];
			float[,] y = new float[height, circ];
			float[,] z = new float[height, circ];

			for (int k = 0; k < height; k++)
				for (int i = 0; i < circ; i++)
				{
					float s = (float)Math.Sin(Math.PI * k / (height-1));
					float yy = (float)Math.Cos(Math.PI * k / (height-1));
					float xx = (float)(Math.Sin(Math.PI * 2 * i / (circ)) * s);
					float zz = (float)(Math.Cos(Math.PI * 2 * i / (circ)) * s);
					x[k, i] = xx;
					y[k, i] = yy;
					z[k, i] = zz;
				}
			// Draw triangles in belts
			for (int k = 1; k < height; k++)
			{
				for (int i = 0; i < circ; i++)
				{
					// Offset each new row by -1 (aka, height-k)
					// This ensures that when shown as a LineStrip it looks like a nice spherical mesh.
					int m = (i + height - k) % (Corners);
					Vector3 c0 = new Vector3(x[k - 1, m], y[k - 1, m], z[k - 1, m]);
					Vector3 c3 = new Vector3(x[k, m], y[k, m], z[k, m]);
					m = (i + height - k - 1) % (Corners);
					Vector3 c1 = new Vector3(x[k - 1, m], y[k - 1, m], z[k - 1, m]);
					Vector3 c2 = new Vector3(x[k, m], y[k, m], z[k, m]);
					Vector3 norm = new Plane(c0, c1, c2).Normal;

					verts.Add(new Vertex(c0, norm));
					verts.Add(new Vertex(c1, norm));
					verts.Add(new Vertex(c2, norm));
					norm = new Plane(c2, c3, c0).Normal;
					verts.Add(new Vertex(c0, norm));
					verts.Add(new Vertex(c2, norm));
					verts.Add(new Vertex(c3, norm));

				}
			}
			Vertices = new VertexList(verts);
			Topology = top;
			SetSolidColor(SolidColor);
		}


		public override void AutoGenerateIndices()
		{
			if (Topology == PrimitiveTopology.LineList)
			{
				// Draw lines of longitude
				int height = Corners / 2 + 1;
				int circ = Corners;
				List<int> inds = new List<int>();
				int sLat = (int)Math.Ceiling((decimal)(Corners/2) / (nLatLines));
				int sLong = (int)Math.Ceiling((decimal)(Corners) / (nLongLines));
				int offset = Corners;
				for (int i = 0; i < Corners; i++)
				{
					for (int k = 0; k < LatLines + 1; k++)
					{
						int p = (i + k * Corners * sLat - offset) * 6;
						inds.Add(p+4);
						inds.Add(p+5);
					}
				}
				for (int i = 0; i < Corners/2; i++)
				{
					for (int k = 0; k < Corners; k+=sLong)
					{
						// This offset account for the Vertices being offset by 1 with each row.
						int off = k + (i % sLong);
						int z = i * Corners + off;

						inds.Add(z*6 + 3);
						inds.Add(z*6 + 5);
					}
				}

				Vertices.Indices = inds;
			}
			else if( Topology == PrimitiveTopology.LineStrip)
			{
				List<int> inds = new List<int>(); 
				for (int i = 3; i < Corners*6; i+=6)
				{
					inds.Add(i);
					inds.Add(i+1);
					inds.Add(i+2);
				}
				for (int i = 21; i < Vertices.Count; i++ )
				{
					inds.Add(i);
				}
				Vertices.Indices = inds;
			}
			else
				Vertices.Indices = new List<int>();
		}
    }
}
