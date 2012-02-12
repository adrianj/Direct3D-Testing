using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SlimDX;

namespace Direct3DLib
{
	public class Triangle : Shape
	{
		private Float3 c0 = new Float3(0,1,0);
		private Float3 c1 = new Float3(-0.5f, 0, 0);
		private Float3 c2 = new Float3(0.5f, 0, 0);
		public Float3 Corner0 { get { return c0; } set { c0 = value; Regenerate(); } }
		public Float3 Corner1 { get { return c1; } set { c1 = value; Regenerate(); } }
		public Float3 Corner2 { get { return c2; } set { c2 = value; Regenerate(); } }
		public Triangle()
			: base()
		{
			Regenerate();
		}

		protected virtual void Regenerate()
		{
			Vector3 norm = new Plane(c0.AsVector3(), c1.AsVector3(), c2.AsVector3()).Normal;
			Vertex[] vertices = { new Vertex(c0.AsVector3(), SolidColor, norm), 
									new Vertex(c2.AsVector3(), SolidColor, norm), 
									new Vertex(c1.AsVector3(), SolidColor, norm) };
			this.Vertices.Clear();
			this.Vertices.AddRange(vertices);
			this.Update();
		}
	}
}
