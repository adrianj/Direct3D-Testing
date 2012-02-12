using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SlimDX;
using System.Drawing;

namespace Direct3DLib
{
	public class Compass : Square2D
	{
		public Compass()
			: base()
		{

		}

		protected override void Regenerate()
		{
			this.Vertices.Clear();
			Vector3 c = new Vector3(0,0,0);
			Vector3 n = new Vector3(0,2,0);
			Vector3 s = new Vector3(0, -1, 0);
			Vector3 e = new Vector3(1, 0, 0);
			Vector3 w = new Vector3(-1, 0, 0);
			Vector3 ne = new Vector3(0.25f, 0.25f, 0);
			Vector3 nw = new Vector3(-0.25f, 0.25f, 0);
			Vector3 se = new Vector3(0.25f, -0.25f, 0);
			Vector3 sw = new Vector3(-0.25f, -0.25f, 0);
			this.Vertices.AddRange(Vertex.CreateTriangle(n, c, nw, Color.Blue));
			this.Vertices.AddRange(Vertex.CreateTriangle(e, c, ne, Color.Blue));
			this.Vertices.AddRange(Vertex.CreateTriangle(s, c, se, Color.Blue));
			this.Vertices.AddRange(Vertex.CreateTriangle(w, c, sw, Color.Blue));
			this.Vertices.AddRange(Vertex.CreateTriangle(c, n, ne, Color.Red));
			this.Vertices.AddRange(Vertex.CreateTriangle(c, e, se, Color.Red));
			this.Vertices.AddRange(Vertex.CreateTriangle(c, s, sw, Color.Red));
			this.Vertices.AddRange(Vertex.CreateTriangle(c, w, nw, Color.Red));
			this.Update();
		}
	}
}
