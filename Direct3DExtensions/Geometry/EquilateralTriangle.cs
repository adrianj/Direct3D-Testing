using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using SlimDX;

namespace Direct3DExtensions
{

	public class EquilateralTriangle : BasicMesh
	{
		public Vector2[] corners = new Vector2[3];
		EquilateralTriangle[] children;

		public EquilateralTriangle()
			: this(
				new Vector2(0, 0),
				new Vector2(1, (float)Math.Sqrt(3)),
				new Vector2(2, 0))
		{
		}

		public EquilateralTriangle(Vector2 v0, Vector2 v1, Vector2 v2)
		{
			corners[0] = v0;
			corners[1] = v1;
			corners[2] = v2;
			Recreate();
		}

		public void Explode()
		{
			if (children != null)
			{
				foreach (EquilateralTriangle tri in children)
					tri.Explode();
			}
			else
			{
				children = new EquilateralTriangle[4];
				Vector2 halfLeft = (corners[0]+corners[1])/2.0f;
				Vector2 halfRight = (corners[1]+corners[2])/2.0f;
				Vector2 halfBase = (corners[0]+corners[2])/2.0f;
				children[0] = new EquilateralTriangle(corners[0], halfLeft, halfBase);
				children[1] = new EquilateralTriangle(halfLeft, corners[1], halfRight);
				children[2] = new EquilateralTriangle(halfBase, halfRight, corners[2]);
				children[3] = new EquilateralTriangle(halfLeft, halfRight, halfBase);
			}
		}

		public virtual void RecurseSetVertexData(List<Vector3> vBuf, List<int> iBuf)
		{
			if (children == null)
			{
				int lastIndex = -1;
				if (iBuf.Count > 0)
					lastIndex = iBuf.Last();
				vBuf.Add(new Vector3(corners[0].X, 0, corners[0].Y));
				vBuf.Add(new Vector3(corners[1].X, 0, corners[1].Y));
				vBuf.Add(new Vector3(corners[2].X, 0, corners[2].Y));
				iBuf.AddRange(new int[] { lastIndex + 1, lastIndex + 2, lastIndex + 3 });
			}
			else
			{
				foreach (EquilateralTriangle tri in children)
					tri.RecurseSetVertexData(vBuf, iBuf);
			}
		}

		public virtual void Recreate()
		{
			List<Vector3> vBuf = new List<Vector3>();
			List<int> iBuf = new List<int>();
			RecurseSetVertexData(vBuf, iBuf);
			SetVertexPositionData<VertexTypes.Position>(vBuf.ToArray(), iBuf.ToArray());
			base.UploadToGpu();
		}

	}
}
