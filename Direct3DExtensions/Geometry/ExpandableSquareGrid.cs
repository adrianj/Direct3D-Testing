using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using SlimDX;

namespace Direct3DExtensions
{
	public class DiamondSquare
	{
		public enum SquareType { Normal, TopRow, Right, TopRight};


		public DiamondSquare()
		{
		}

		public void AppendTranslatedSquare(List<Vector3> vBuf, List<int> iBuf, Vector3 trans)
		{
			AppendTranslatedSquare(vBuf, iBuf, trans, SquareType.Normal);
		}

		public void AppendTranslatedSquare(List<Vector3> vBuf, List<int> iBuf, Vector3 trans, SquareType st)
		{
			List<Vector3> tempBuf = new List<Vector3>(9);
			List<int> tempiBuf = new List<int>(24);
			tempBuf.Add(new Vector3(0, 0, 0) + trans);
			tempBuf.Add(new Vector3(0, 0, 1) + trans);
			tempBuf.Add(new Vector3(0, 0, 2) + trans);
			tempBuf.Add(new Vector3(1, 0, 1) + trans);
			tempBuf.Add(new Vector3(2, 0, 2) + trans);
			tempiBuf.AddRange(new int[] { 0, 1, 3, 1, 2, 3 });

			if (st == SquareType.TopRight || st == SquareType.TopRow)
			{
				tempiBuf.AddRange(new int[] { 4, 3, 2 });
				if(st == SquareType.TopRow)
				{
					tempBuf.Add(new Vector3(1, 0, 0) + trans);
					tempBuf.Add(new Vector3(2, 0, 0) + trans);
					tempBuf.Add(new Vector3(2, 0, 1) + trans);
					tempiBuf.AddRange(new int[] { 0, 3, 5, 5, 3, 6, 6, 3, 7, 7, 3, 4 });
				}
			}
			else
			{
				tempBuf.Add(new Vector3(1, 0, 2) + trans);
				tempiBuf.AddRange(new int[] { 5, 3, 2, 3, 5, 4 });
				if (st == SquareType.Normal)
				{
					tempBuf.Add(new Vector3(1, 0, 0) + trans);
					tempBuf.Add(new Vector3(2, 0, 0) + trans);
					tempBuf.Add(new Vector3(2, 0, 1) + trans);
					tempiBuf.AddRange(new int[] { 0, 3, 6, 6, 3, 7, 7, 3, 8, 8, 3, 4 });
				}
			}

			MathExtensions.AppendVertices(vBuf, iBuf, tempBuf, tempiBuf);
		}
	}

	public class ExpandableSquareGrid : BasicMesh
	{
		public ExpandableSquareGrid()
		{
			Recreate(16,16);
		}

		public ExpandableSquareGrid(int numColumns, int numRows)
		{
			Recreate(numColumns, numRows);
		}

		void Recreate(int numColumns,int numRows)
		{

			List<Vector3> vBuf = new List<Vector3>();
			List<int> iBuf = new List<int>();

			List<Vector3> tempBuf = new List<Vector3>();
			List<int> tempiBuf = new List<int>();

			DiamondSquare sq = new DiamondSquare();

			for (int row = 0; row < numRows; row += 2)
			{

				for (int column = 0; column <= numColumns + row; column += 2)
				{
					DiamondSquare.SquareType st = DiamondSquare.SquareType.Normal;
					if (row == numRows-2 && column == numColumns + row)
						st = DiamondSquare.SquareType.TopRight;
					else if (row == numRows-2)
						st = DiamondSquare.SquareType.TopRow;
					else if (column == numColumns + row)
						st = DiamondSquare.SquareType.Right;
					sq.AppendTranslatedSquare(tempBuf, tempiBuf, new Vector3(column, 0, row + numColumns), st);
				}
			}

			List<Vector3> rev;
			List<int> revi;
			MathExtensions.ReflectVerticesThroughXAxis(tempBuf, tempiBuf, out rev, out revi);
			MathExtensions.AppendVertices(tempBuf, tempiBuf, rev, revi);

			MathExtensions.AppendVertices(vBuf, iBuf, tempBuf, tempiBuf);

			Matrix m = Matrix.RotationY((float)(Math.PI / 2));
			MathExtensions.TransformVertices(tempBuf, out rev, m);
			MathExtensions.AppendVertices(vBuf, iBuf, rev, tempiBuf);

			m = Matrix.RotationY((float)(Math.PI));
			MathExtensions.TransformVertices(tempBuf, out rev, m);
			MathExtensions.AppendVertices(vBuf, iBuf, rev, tempiBuf);

			m = Matrix.RotationY((float)(3 * Math.PI / 2));
			MathExtensions.TransformVertices(tempBuf, out rev, m);
			MathExtensions.AppendVertices(vBuf, iBuf, rev, tempiBuf);

			/*
			tempBuf = new List<Vector3>(vBuf);
			tempiBuf = new List<int>(iBuf);

			for (int s = 1; s < overlappingGrids; s++)
			{
				m = Matrix.Scaling(new Vector3(1<<s, 1, 1<<s));
				MathExtensions.TransformVertices(tempBuf, out rev, m);
				MathExtensions.AppendVertices(vBuf, iBuf, rev, tempiBuf);
			}
			 */
			/*
			for (int r = -numColumns; r < numColumns; r+=2)
			{
				for (int c = -numColumns; c < numColumns; c+=2)
				{
					sq.AppendTranslatedSquare(vBuf, iBuf, new Vector3(c, 0, r), Square.SquareType.Normal);
				}
			}
			 */


			this.SetVertexPositionData<VertexTypes.Position>(vBuf.ToArray(), iBuf.ToArray());
			
		}

		void AppendVertices(IEnumerable<Vector3> srcVBuf, IEnumerable<Vector3> destVBuf, IEnumerable<int> srcIBuf, IEnumerable<int> destIBuf)
		{

		}
	}
}
