using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using SlimDX;

namespace Direct3DLib
{
	public interface Shape2D	{	}

	public class Square2D : Shape, Shape2D
	{
		public Square2D()
			: base()
		{
			Regenerate();
		}

		protected virtual void Regenerate()
		{
			float t = 0.5f;
			float l = -0.5f;
			float b = -0.5f;
			float r = 0.5f;
			Vertex v0 = new Vertex(new Vector3(l, t, 0), new Vector3(0, 1, 0));
			v0.TextureCoordinates = new Vector2(1, 0);
			Vertex v1 = new Vertex(new Vector3(r, t, 0), new Vector3(0, 1, 0));
			v1.TextureCoordinates = new Vector2(0, 0);
			Vertex v2 = new Vertex(new Vector3(l, b, 0), new Vector3(0, 1, 0));
			v2.TextureCoordinates = new Vector2(1, 1);
			Vertex v3 = new Vertex(new Vector3(r, b, 0), new Vector3(0, 1, 0));
			v3.TextureCoordinates = new Vector2(0, 1);
			this.Vertices.AddRange(new Vertex[] { v2, v1, v0, v2, v3, v1 });
		}

		protected override void updateWorld()
		{
			Vector3 loc = this.Location;
			loc.Y = -loc.Y;
			Matrix m = Matrix.Scaling(mScale);
			m = m * RotationMatrix;
			m = m * Matrix.Translation(loc);
			mWorld = m;
		}
	}
}
