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
	public class Square : Shape
	{
		public Square() : this(new RectangleF(-0.5f, -0.5f, 1.0f, 1.0f)) { }
		public Square(RectangleF bounds)
			: base()
		{
			float t = bounds.Top;
			float l = bounds.Left;
			float b = bounds.Bottom;
			float r = bounds.Right;
			Vertex v0 = new Vertex(new Vector3(l, 0, t), new Vector3(0, 1, 0));
			v0.TextureCoordinates = new Vector2(1, 0);
			Vertex v1 = new Vertex(new Vector3(r, 0, t), new Vector3(0, 1, 0));
			v1.TextureCoordinates = new Vector2(0, 0);
			Vertex v2 = new Vertex(new Vector3(l, 0, b), new Vector3(0, 1, 0));
			v2.TextureCoordinates = new Vector2(1, 1);
			Vertex v3 = new Vertex(new Vector3(r, 0, b), new Vector3(0, 1, 0));
			v3.TextureCoordinates = new Vector2(0, 1);
			this.Vertices.AddRange(new Vertex[] { v2, v1, v0, v2, v3, v1 });
		}


	}
}
