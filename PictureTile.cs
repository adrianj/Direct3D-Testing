using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SlimDX.Direct3D10;
using SlimDX;

namespace Direct3DLib
{
	public class PictureTile : Shape
	{
		public PictureTile()
			: base()
		{
			Vertex v0 = new Vertex(new Vector3(-0.5f, 0, -0.5f), new Vector3(0, 1, 0));
			v0.TextureCoordinates = new Vector2(1, 1);
			Vertex v1 = new Vertex(new Vector3(-0.5f, 0, 0.5f), new Vector3(0, 1, 0));
			v1.TextureCoordinates = new Vector2(0, 1);
			Vertex v2 = new Vertex(new Vector3(0.5f, 0, 0.5f), new Vector3(0, 1, 0));
			v2.TextureCoordinates = new Vector2(0, 0);
			Vertex v3 = new Vertex(new Vector3(0.5f, 0, -0.5f), new Vector3(0, 1, 0));
			v3.TextureCoordinates = new Vector2(1, 0);
			this.Vertices.AddRange(new Vertex[] { v0, v1, v2, v2, v3, v0 });
		}


	}
}
