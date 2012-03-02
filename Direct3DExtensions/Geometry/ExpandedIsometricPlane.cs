using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using SlimDX;

namespace Direct3DExtensions
{
	public class ExpandedIsometricPlane : ExpandedPlane
	{
		protected override void CreateRegularGrid(int xTiles, int yTiles)
		{
			//xTiles = 16;
			int numTriExplosions = MathExtensions.Log2(xTiles);
			float scale = 2;
			float root3 = (float)Math.Sqrt(3)*scale;
			float w = scale;
			List<EquilateralTriangle> tris = new List<EquilateralTriangle>();
			tris.Add( new EquilateralTriangle(new Vector2(0, 0), new Vector2(w * -2, 0), new Vector2(-w, root3)));
			tris.Add( new EquilateralTriangle(new Vector2(0, 0), new Vector2(-w, root3), new Vector2(w, root3)));
			tris.Add( new EquilateralTriangle(new Vector2(0, 0), new Vector2(w, root3), new Vector2(w*2, 0)));
			//tris[3] = new EquilateralTriangle(new Vector2(0, 0), new Vector2(w * 2, 0), new Vector2(w, -root3));
			//tris[4] = new EquilateralTriangle(new Vector2(0, 0), new Vector2(w, -root3), new Vector2(-w, -root3));
			//tris[5] = new EquilateralTriangle(new Vector2(0, 0), new Vector2(-w, -root3), new Vector2(w * -2, 0));

			for (int i = 0; i < numTriExplosions; i++)
				foreach (EquilateralTriangle tri in tris)
					tri.Explode();

			List<Vector3> vBuf = new List<Vector3>();
			List<int> iBuf = new List<int>();
			foreach (EquilateralTriangle tri in tris)
				tri.RecurseSetVertexData(vBuf, iBuf);
			SetVertexData(vBuf.ToArray(), iBuf.ToArray());
		}

		int NumVerticesPerRing(int ring)
		{
			if (ring == 0) return 1;
			return ring * 6;
		}

		int NumIndicesPerRing(int ring)
		{
			if(ring == 0) return 0;
			int tris = (ring * 2 - 1) * 6;
			return tris * 3;
		}


		protected virtual void SetVertexData(Vector3[] vPos, int[] ind)
		{
			this.Indices = ind;
			this.Vertices = new Vertex[vPos.Length];
			for (int i = 0; i < vPos.Length; i++)
				Vertices[i] = new VertexTypes.Position() { Pos = vPos[i] };
		}

		protected override void ExpandMesh()
		{
			for (int i = 0; i < this.Vertices.Length; i++)
			{
				Vertex v = Vertices[i];

				double x = v.Pos.X;
				double y = v.Pos.Z;
				double a = Math.Atan2(y, x);
				double d = x * x + y * y;
				//if (d <= 1)
					d = Math.Sqrt(d);
				//else
				//	d = d * d;
				/*
				else if (d > 100)
				{
					double b = Math.Sqrt(d-100);
					d = d + b * b;
				}
				 */
					//d = MathExtensions.Clamp(d*1000, 1, 100000);
				//d = Math.Pow(2, d)-1;
				//else d = Math.Sqrt(d);
					
					//d = Math.Pow(2, d);
				//x = Math.Cos(a) * d;
				//y = Math.Sin(a) * d;
					y = y * Math.Abs(y);
					
						//x *= Math.Abs(y)*1.0;
					//x = x * Math.Abs(y);

				v.Pos = new SlimDX.Vector3((float)x, 0, (float)y);
				Vertices[i] = v;
			}
		}
		
	}
}
