using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using SlimDX.Direct3D10;
using SlimDX;
using System.ComponentModel;

namespace Direct3DLib
{
	/// <summary>
	/// A 3D Pipe shape. basically these are cylinders with a varying number of "corners", ie, 4 corners = a cubic box.
	/// </summary>
	[ToolboxItem(true)]
	public class Pipe : Shape//, IComponent
	{
		/*
		#region IComponent Implementation
		public event EventHandler Disposed;
		private ISite _curISBNSite;
		public virtual new void Dispose()
		{
			if (Disposed != null)
			{
				Disposed(this, EventArgs.Empty);
			}
		}
		[Browsable(false)]
		public virtual ISite Site { get { return _curISBNSite; } set { _curISBNSite = value; } }
		#endregion
		 */

		protected int mCorners = 6;
		public int Corners
		{
			get { return mCorners; }
			set
			{
				if (value < 2) throw new ArgumentException("Pipe shape must have at least 2 corners");
				mCorners = value;
				Regenerate();
			}
		}

		public Pipe() : base() { Corners = 6; }
		public Pipe(Color col) : this(6, col) { }
		public Pipe(int corners, Color col) : this(corners) { SetUniformColor(col); }
		public Pipe(int corners)
			: base()
		{
			Corners = corners;
		}
		protected virtual void Regenerate()
		{
			Vertices.Clear();
			Vertices.Capacity = (mCorners * 6);
			for (int i = 0; i < mCorners; i++)
			{
				// Each face is a square with corners at (x0,-1,z0) and (x1,1,z1)

				float x0 = (float)(Math.Cos(Math.PI * 2 * i / mCorners + Math.PI / 4));
				float z0 = (float)(Math.Sin(Math.PI * 2 * i / mCorners + Math.PI / 4));
				float x1 = (float)(Math.Cos(Math.PI * 2 * (i + 1) / mCorners + Math.PI / 4));
				float z1 = (float)(Math.Sin(Math.PI * 2 * (i + 1) / mCorners + Math.PI / 4));
				float y0 = -0.5f;
				float y1 = -y0;
				Vector3 c0 = new Vector3(x0, y0, z0);
				Vector3 c1 = new Vector3(x0, y1, z0);
				Vector3 c2 = new Vector3(x1, y1, z1);
				Vector3 c3 = new Vector3(x1, y0, z1);
				Vector3 norm = new Plane(c0, c1, c2).Normal;
				Vertices.Add(new Vertex(c0, norm));
				Vertices.Add(new Vertex(c1, norm));
				Vertices.Add(new Vertex(c2, norm));
				Vertices.Add(new Vertex(c0, norm));
				Vertices.Add(new Vertex(c2, norm));
				Vertices.Add(new Vertex(c3, norm));
			}
		}

		public override void AutoGenerateIndices()
		{
			base.AutoGenerateIndices();
			if (Topology == PrimitiveTopology.TriangleStrip)
			{
				List<int> inds = Vertices.Indices;
				inds.Add(0);
				inds.Add(1);
				Vertices.Indices = inds;
			}
			if (Topology == PrimitiveTopology.LineList)
			{
				List<int> inds = new List<int>(mCorners * 6);
				for (int i = 0; i < mCorners; i++)
				{
					// Vertical lines.
					inds.Add(i * 2);
					inds.Add(i * 2 + 1);

					// Circular lines.
					inds.Add(i);
					inds.Add(i + 2);
					inds.Add((i + mCorners) % Vertices.Count);
					inds.Add((i + 2 + mCorners) % Vertices.Count);
				}
				Vertices.Indices = inds;
			}
		}

	}

	public class ClosedPipe : Pipe
	{
		public ClosedPipe() : base() { }
		public ClosedPipe(Color col) : this(6, col) { }
		public ClosedPipe(int corners, Color col) : this(corners) { SetUniformColor(col); }
		public ClosedPipe(int corners)
			: base(corners)
		{
		}

		protected override void Regenerate()
		{
			base.Regenerate();

			// Same as for a pipe, but just need to close off the top and bottom.
			Vector3 v0 = Vertices[4].Position;
			for (int i = 0; i < mCorners; i++)
			{
				Vector3 v1 = Vertices[i * 6 + 1].Position;
				Vector3 v2 = Vertices[i * 6 + 2].Position;
				Vector3 norm = new Plane(v2, v1, v0).Normal;
				Vertices.Add(new Vertex(v2, norm));
				Vertices.Add(new Vertex(v1, norm));
				Vertices.Add(new Vertex(v0, norm));
			}

			v0 = Vertices[0].Position;
			for (int i = 0; i < mCorners; i++)
			{
				Vector3 v1 = Vertices[i * 6 + 3].Position;
				Vector3 v2 = Vertices[i * 6 + 5].Position;
				Vector3 norm = new Plane(v0, v1, v2).Normal;
				Vertices.Add(new Vertex(v0, norm));
				Vertices.Add(new Vertex(v1, norm));
				Vertices.Add(new Vertex(v2, norm));
			}

		}

	}
}
