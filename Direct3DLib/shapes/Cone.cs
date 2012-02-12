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
		public Float3 NarrowLocation { get { return new Float3(this.Location); } set { this.Location = value.AsVector3(); } }

		public virtual Float3 BroadLocation { get { return GetBroadLocation(); } set { SetBroadLocation(value); } }

		public float Azimuth { get { return Rotation.X - (float)Math.PI; ; } set { Rotation = new Vector3(value + (float)Math.PI, mRotation.Y, mRotation.Z); } }

		public float Elevation { get { return Rotation.Y-(float)Math.PI/2; } set { Rotation = new Vector3(mRotation.X, value+(float)Math.PI/2, mRotation.Z); } }

		public float Length { get { return mScale.Y; } set { Scale = new Vector3(mScale.X, value, mScale.Z); } }

		public float Width { get { return mScale.X; } set { Scale = new Vector3(value,mScale.Y,value); } }

		protected int mCorners = 12;
		public int Corners { get{return mCorners;} 
			set
			{
				if (value < 2) throw new ArgumentException("Cone shape must have at least 2 corners");
				mCorners = value;
				Regenerate();
			}
		}
		public Cone() : base() {
			Corners = 12;
		}


		private void Regenerate()
		{
			Vertices.Clear();
			Vector3 zero = Vector3.Zero;
			Vector3 one = new Vector3(0, -1, 0);
			for (int i = 0; i < Corners; i++)
			{
				float x0 = (float)Math.Sin(2 * Math.PI * i / mCorners);
				float z0 = (float)Math.Cos(2 * Math.PI * i / mCorners);
				float x1 = (float)Math.Sin(2 * Math.PI * (i+1) / mCorners);
				float z1 = (float)Math.Cos(2 * Math.PI * (i+1) / mCorners);
				Vector3 c0 = new Vector3(x0, -1, z0);
				Vector3 c1 = new Vector3(x1, -1, z1);
				Vector3 norm = new Plane(c0,c1, zero).Normal;
				Vertices.Add(new Vertex(zero, norm));
				Vertices.Add(new Vertex(c0, norm));
				Vertices.Add(new Vertex(c1, norm));
				norm = new Plane(c0, one, c1).Normal;
				Vertices.Add(new Vertex(c0, norm));
				Vertices.Add(new Vertex(one, norm));
				Vertices.Add(new Vertex(c1, norm));
			}
			UpdateColor();
			base.Update();
		}


		private Float3 GetBroadLocation()
		{
			Vector3 broad = new Vector3(0,-1,0);
			Matrix transform = Matrix.Scaling(new Vector3(1,mScale.Y,1));
			transform = transform * RotationMatrix;
			transform = transform * Matrix.Translation(Location);
			broad = Vector3.TransformCoordinate(broad,transform);
			return new Float3(broad);
		}

		private void SetBroadLocation(Float3 value)
		{
			Vector3 broad = value.AsVector3();
			float dist = Vector3.Distance(broad, Location);
			mScale = new Vector3(mScale.X, dist, mScale.Z);
			Vector3 diff = Vector3.Subtract(broad, this.Location);
			double y = diff.Y;
			double x = diff.X;
			double z = diff.Z;
			Elevation = (float)(Math.Asin(y / dist));
			Azimuth = (float)(Math.Atan2(x, z));
		}
	}
}
