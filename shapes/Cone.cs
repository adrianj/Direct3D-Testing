using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SlimDX;
using System.ComponentModel;
using System.Drawing;

namespace Direct3DLib
{

	[ToolboxItem(true)]
	public class Cone : Shape
	{
		public Float3 NarrowLocation { get { return new Float3(this.Location); } set { this.Location = value.AsVector3(); updateWorld(); } }
		private Float3 broadLocation = new Float3(0, 1, 0);
		public Float3 BroadLocation { get { return broadLocation; } set { broadLocation = value; updateWorld(); } }
		private float width = 1;
		public float Width { get { return width; } set { width = value; updateWorld(); } }

		protected int mCorners = 6;
		public int Corners { get{return mCorners;} 
			set
			{
				if (value < 2) throw new ArgumentException("Cone shape must have at least 2 corners");
				mCorners = value;
				Regenerate();
			}
		}
		public Cone() : base() { }


		private void Regenerate()
		{
			Vertices.Clear();
			Vector3 zero = Vector3.Zero;
			for (int i = 0; i < Corners; i++)
			{
				float x0 = (float)Math.Sin(2 * Math.PI * i / mCorners);
				float z0 = (float)Math.Cos(2 * Math.PI * i / mCorners);
				float x1 = (float)Math.Sin(2 * Math.PI * (i+1) / mCorners);
				float z1 = (float)Math.Cos(2 * Math.PI * (i+1) / mCorners);
				Vector3 c0 = new Vector3(x0, 1, z0);
				Vector3 c1 = new Vector3(x1, 1, z1);
				Vector3 norm = new Plane(c1, c0, zero).Normal;
				Vertices.Add(new Vertex(zero, norm));
				Vertices.Add(new Vertex(c1, norm));
				Vertices.Add(new Vertex(c0, norm));
				/*
				norm = new Plane(zero, c0, c1).Normal;
				Vertices.Add(new Vertex(c1, norm));
				Vertices.Add(new Vertex(zero, norm));
				Vertices.Add(new Vertex(c0, norm));
				 */
			}
			base.Update();
		}

		public float rotX { get; set; }
		public float rotY { get; set; }
		public Vector3 diff { get; set; }
		public double distance { get; set; }
		


		protected override void updateWorld()
		{
			Vector3 broad = broadLocation.AsVector3();
			distance = Vector3.Distance(broad, this.Location);
			this.mScale = new Vector3(width, (float)distance, width);
			diff = Vector3.Subtract(broad, this.Location);
			double y = diff.Y;
			double x = diff.X;
			double z = diff.Z;
			rotX = (float)(Math.PI / 2 + Math.Asin(-y / distance));
			rotY = (float)(Math.Atan2(x,z));
			base.updateWorld();
		}

		public override Matrix RotationMatrix
		{
			get
			{
				return Matrix.RotationX(rotX) * Matrix.RotationY(rotY);
			}
		}
	}
}
