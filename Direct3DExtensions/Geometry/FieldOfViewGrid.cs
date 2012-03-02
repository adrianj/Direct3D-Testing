using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using SlimDX;

namespace Direct3DExtensions
{
	public class FieldOfViewGrid : ExpandedPlane
	{
		protected override void CreateRegularGrid(int xTiles, int yTiles)
		{
			float[] x = new float[xTiles+1];
			float[] y = new float[xTiles+1];
			for (int i = 0; i < xTiles+1; i++)
			{
				x[i] = -(float)Math.Cos(2*Math.PI * i / (float)(xTiles));
				y[i] = (float)Math.Sin(2*Math.PI * i / (float)(xTiles));
			}
			Vector3[] vPos = new Vector3[1 + (xTiles) * (yTiles)];
			vPos[0] = new Vector3();
			for (int ring = 1; ring < yTiles+1; ring++)
			{
				float d = (float)ring / (float)yTiles;
				d = (float)(ring*ring)/1.0f;
				for (int segment = 0; segment < xTiles; segment++)
				{
					Vector3 v = new Vector3(x[segment] * d, 0, y[segment] * d);
					vPos[(ring-1)*(xTiles) + segment + 1] = v;
				}
			}
			List<int> ind = new List<int>();
			for (int segment = 1; segment < xTiles + 1; segment++)
			{
				ind.Add(0);
				ind.Add(segment);
				ind.Add(segment + 1);
			}
			for (int ring = 1; ring < yTiles; ring++)
			{
				for (int segment = 0; segment < xTiles; segment++)
				{
					ind.Add((ring-1)*xTiles+segment+1);
					ind.Add((ring) * xTiles + segment);
					ind.Add((ring) * xTiles + segment+1);

					ind.Add((ring - 1) * xTiles + segment+1);
					ind.Add((ring) * xTiles + segment + 1);
					ind.Add((ring - 1) * xTiles + segment+2);
				}
			}

			SetVertexData(vPos, ind.ToArray());
		}

		protected virtual void SetVertexData(Vector3[] vPos, int [] ind)
		{
			this.Indices = ind;
			this.Vertices = new Vertex[vPos.Length];
			for (int i = 0; i < vPos.Length; i++)
				Vertices[i] = new VertexTypes.Position() { Pos = vPos[i] };
		}

		protected override void ExpandMesh()
		{
			//base.ExpandMesh();
		}
	}
}
