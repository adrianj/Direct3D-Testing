using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using SlimDX;

namespace Direct3DLib
{
	public class ShapeArrayFactory
	{
		private int width;
		private int height;
		private float shapeWidth = 1.0f;
		private float shapeHeight = 1.0f;
		public PointF ShapeSize { get { return new PointF(shapeWidth, shapeHeight); } set { shapeHeight = value.Y; shapeWidth = value.X; } }
		private Shape shape;

		private double verticalScale = 1.0;
		public double VerticalScale { get { return verticalScale; } set { verticalScale = value; } }

		public static Shape CreateFromArray(Array data) { return CreateFromArray(data, new PointF(1.0f, 1.0f)); }
		public static Shape CreateFromArray(Array data, PointF outputShapeSize) { return CreateFromArray(data, outputShapeSize, 1.0); }

		public static Shape CreateFromArray(Array data, PointF outputShapeSize, double verticalScale)
		{
			ShapeArrayFactory factory = new ShapeArrayFactory();
			factory.VerticalScale = verticalScale;
			factory.ShapeSize = outputShapeSize;
			return factory.ConvertArrayToShape(data);
		}

		public Shape ConvertArrayToShape(Array data)
		{
			width = data.GetLength(1);
			height = data.GetLength(0);
			shape = new Shape();
			double[] prevRow = ReadRow(data, 0);
			for (int y = 1; y < height; y++)
			{
				double[] nextRow = ReadRow(data, y);
				Vertex[] verts = GetRowOfVertices(nextRow, prevRow, y - 1);
				shape.Vertices.AddRange(verts);
				prevRow = nextRow;
			}
			return shape;
		}

		private Vertex[] GetRowOfVertices(double[] bottomRow, double[] topRow, int y)
		{
			List<Vertex> vertList = new List<Vertex>();
			for (int x = 0; x < bottomRow.Length - 1; x++)
			{
				Vector3 topLeft = ConvertCoordinatesToVector(x, height - y - 1, topRow[x]);
				Vector3 topRight = ConvertCoordinatesToVector(x + 1, height - y - 1, topRow[x + 1]);
				Vector3 bottomLeft = ConvertCoordinatesToVector(x, height - y - 2, bottomRow[x]);
				Vector3 bottomRight = ConvertCoordinatesToVector(x + 1, height - y - 2, bottomRow[x + 1]);
				Vertex[] verts = CreateSquare(topLeft, topRight, bottomLeft, bottomRight);
				vertList.AddRange(verts);
			}
			return vertList.ToArray();
		}

		private double[] ReadRow(Array data, int row)
		{
			double[] ret = new double[width];
			for (int x = 0; x < width; x++)
			{
				object o = data.GetValue(x, row);
				double elevation = Convert.ToDouble(o);
				ret[x] = elevation * verticalScale;
			}
			return ret;
		}

		private Vector3 ConvertCoordinatesToVector(int x, int y, double elevation)
		{
			float xScale = shapeWidth / (float)(width - 1);
			float zScale = shapeHeight / (float)(height - 1);
			Vector3 vect = new Vector3((float)x * xScale, (float)elevation, (float)y * zScale);
			return vect;
		}

		private Vertex[] CreateSquare(Vector3 topLeftCorner, Vector3 topRightCorner, Vector3 bottomLeftCorner, Vector3 bottomRightCorner)
		{
			Vertex topLeft = new Vertex(topLeftCorner);
			Vertex topRight = new Vertex(topRightCorner);
			Vertex bottomLeft = new Vertex(bottomLeftCorner);
			Vertex bottomRight = new Vertex(bottomRightCorner);
			List<Vertex> square = new List<Vertex>(6);
			Vertex[] sq1 = new Vertex[] { topLeft, topRight, bottomLeft };
			Vertex[] sq2 = new Vertex[] { topRight, bottomRight, bottomLeft };
			square.AddRange(CreateTriangle(sq1));
			square.AddRange(CreateTriangle(sq2));
			return square.ToArray();
		}

		private Vertex[] CreateTriangle(Vertex[] corners)
		{
			Vertex[] triangle = new Vertex[3];
			Vector3 norm = new Plane(corners[0].Position, corners[1].Position, corners[2].Position).Normal;
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
			float rScale = 1.0f / (shapeWidth);
			float gScale = 1.0f / (shapeHeight);
			Color4 col = new Color4(1 - vertex.Position.X * rScale, vertex.Position.Z * gScale, 1.0f);
			return col;
		}
	}
}
