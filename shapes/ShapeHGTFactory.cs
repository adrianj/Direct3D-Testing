using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using DTALib;
using SlimDX;

namespace Direct3DLib
{
	public class ShapeHGTFactory
	{
		private int nRows = 1201;
		private int nColumns = 1201;

		private double bottomLeftLatitude = -37.7;
		public double BottomLeftLatitude { get { return bottomLeftLatitude; } set { bottomLeftLatitude = value; } }
		private double bottomLeftLongitude = 174.6;
		public double BottomLeftLongitude { get { return bottomLeftLongitude; } set { bottomLeftLongitude = value; } }
		private double metresPerLatitude = 111000;	// 111 km
		public double metresPerDegreeLatitude { get { return metresPerLatitude; } set { metresPerLatitude = value; } }

		private double degreesLatitudePerPoint = 1/1200.0;
		
		private float verticalScale = 1.0f;			// 1 m
		public float VerticalScale { get { return verticalScale; } set { verticalScale = value; } }
		private float horizontalScale;
		public float HorizontalScale { get { return horizontalScale; } set { horizontalScale = value; } }

		private string filename;
		private BinaryReaderBiEndian reader;
		private Shape shape;
		private int currentRow = 0;
		private int[] previousRow = null;
		private int currentColumn = 0;

		public static Shape CreateFromFile(string filename)
		{
			ShapeHGTFactory factory = new ShapeHGTFactory(filename);
			Shape shape = factory.ReadShapeFromFile();
			return shape;
		}


		private ShapeHGTFactory(string filename)
		{
			this.filename = filename;
		}

		private Shape ReadShapeFromFile()
		{
			horizontalScale = CalculateHorizontalScale();
			shape = new Shape();
			InferRowsAndColumnsFromFileSize();
			previousRow = null;
			using (reader = new BinaryReaderBiEndian(filename, true))
			{
				for (int r = 0; r < nRows-1; r++)
				{
					currentRow = r;
					Vertex[] row = ReadNextRowOfTriangles();
					shape.Vertices.AddRange(row);
					Console.WriteLine("Done row: " + r);
				}
			}
			return shape;
		}

		private float CalculateHorizontalScale()
		{
			return (float)(metresPerLatitude * degreesLatitudePerPoint);
		}

		private void InferRowsAndColumnsFromFileSize()
		{
			FileInfo info = new FileInfo(filename);
			long nInts = info.Length / 2;
			long maxDim = (int)Math.Sqrt(nInts);
			long rows = 1;
			for (int i = 1; i <= maxDim; i++)
			{
				if (nInts % i == 0)
					rows = nInts / i;
			}
			nRows = (int)rows;
			nColumns = (int)(nInts / nRows);
			Console.WriteLine("ShapeHGTFactory: nRows: " + nRows + ", nColumns: " + nColumns);
		}

		private Vertex[] ReadNextRowOfTriangles()
		{
			if (previousRow == null)
				previousRow = ReadNextRowOfInts();
			int [] newRow = ReadNextRowOfInts();
			Vertex[] rowTriangles = CreateTrianglesFromIntRows(previousRow, newRow);
			previousRow = newRow;
			return rowTriangles;
		}

		private Vertex[] CreateTrianglesFromIntRows(int[] topRow, int[] bottomRow)
		{
			int nTriangles = 6 * (nColumns-1);
			List<Vertex> triangleRow = new List<Vertex>(nTriangles);
			for (int c = 0; c < topRow.Length - 1; c++)
			{
				currentColumn = c;
				triangleRow.AddRange(CreateSquare(topRow[c], topRow[c + 1], bottomRow[c], bottomRow[c + 1]));
			}
			return triangleRow.ToArray();
		}

		private int[] ReadNextRowOfInts()
		{
			int[] row = new int[nColumns];
			for (int i = 0; i < nColumns; i++)
				row[i] = reader.ReadInt16();
			return row;
		}

		private Vertex[] CreateSquare(int topLeftHeight, int topRightHeight, int bottomLeftHeight, int bottomRightHeight)
		{
			float[] x = new float[] { (float)currentRow * horizontalScale, (float)(currentRow + 1) * horizontalScale };
			float[] z = new float[] { (float)currentColumn * horizontalScale, (float)(currentColumn + 1) * horizontalScale };
			Vertex topLeft = new Vertex(x[0], (float)topLeftHeight*verticalScale, z[0]);
			Vertex topRight = new Vertex(x[0], (float)topRightHeight * verticalScale, z[1]);
			Vertex bottomLeft = new Vertex(x[1], (float)bottomLeftHeight * verticalScale, z[0]);
			Vertex bottomRight = new Vertex(x[1], (float)bottomRightHeight * verticalScale, z[1]);
			List<Vertex> square = new List<Vertex>(6);
			square.AddRange(CreateTriangle(new Vertex[] { topLeft, topRight, bottomLeft }));
			square.AddRange(CreateTriangle(new Vertex[] { topRight, bottomRight, bottomLeft }));
			return square.ToArray();
		}

		private Vertex[] CreateTriangle(Vertex [] corners)
		{
			Vertex[] triangle = new Vertex[3];
			Vector3 norm = new Plane(corners[2].Position, corners[1].Position, corners[0].Position).Normal;
			for (int i = 0; i < 3; i++)
			{
				Color4 col = GetColorFromVertex(corners[i]);
				Vertex v = new Vertex(new Vector4(corners[i].Position, 1.0f), col, new Vector4(norm, 1.0f));
				triangle[i] = v;
			}
			return triangle;
		}

		private Color4 GetColorFromVertex(Vertex vertex)
		{
			float textureWidthInMetres = 39300.0f;
			float textureLengthInMetres = 31600.0f;
			float xScale = 1 / textureLengthInMetres;
			float yScale = 1 / 1000.0f;
			float zScale = 1 / textureWidthInMetres;
			Color4 col = new Color4(1-vertex.Position.Z * zScale, 1-vertex.Position.X * xScale, vertex.Position.Y*yScale);
			return col;
		}

	}
}
