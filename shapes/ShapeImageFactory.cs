using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using SlimDX;

namespace Direct3DLib
{
	public class ShapeImageFactory
	{
		private int width;
		private int height;
		private float shapeWidth = 1.0f;
		private float shapeHeight = 1.0f;
		public PointF ShapeSize { get { return new PointF(shapeWidth, shapeHeight); } set { shapeHeight = value.Y; shapeWidth = value.X; } }
		private Shape shape;

		public static Shape CreateFromFile(string filename) { return CreateFromFile(filename, new PointF(1.0f, 1.0f)); }
		public static Shape CreateFromFile(string filename, PointF outputShapeSize)
		{
			using (Image image = Bitmap.FromFile(filename))
			{
				return CreateFromImage(image, outputShapeSize);
			}
		}
		public static Shape CreateFromImage(Image image) { return CreateFromImage(image, new PointF(1.0f, 1.0f)); }
		public static Shape CreateFromImage(Image image, PointF outputShapeSize)
		{
			ShapeImageFactory factory = new ShapeImageFactory();
			factory.ShapeSize = outputShapeSize;
			return factory.ConvertImageToShape(image);
		}

		public Shape ConvertImageToShape(Image image)
		{
				Bitmap bmp = (Bitmap)image;
				width = image.Width;
				height = image.Height;
				shape = new Shape(width*height*6);
				int[] prevRow = ReadRow(bmp, 0);
				for (int y = 1; y < height; y++)
				{
					int[] nextRow = ReadRow(bmp, y);
					Vertex[] verts = GetRowOfVertices(nextRow,prevRow, y - 1);
					shape.Vertices.AddRange(verts);
					prevRow = nextRow;
				}
				return shape;
		}

		private Vertex [] GetRowOfVertices(int[] bottomRow, int[] topRow, int y)
		{
			List<Vertex> vertList = new List<Vertex>();
			for (int x = 0; x < bottomRow.Length-1; x++)
			{
				Vector3 topLeft = ConvertCoordinatesToVector(x, height-y+1, topRow[x]);
				Vector3 topRight = ConvertCoordinatesToVector(x + 1, height-y+1, topRow[x + 1]);
				Vector3 bottomLeft = ConvertCoordinatesToVector(x, height-y , bottomRow[x]);
				Vector3 bottomRight = ConvertCoordinatesToVector(x + 1, height-y, bottomRow[x + 1]);
				Vertex[] verts = CreateSquare(topLeft, topRight, bottomLeft, bottomRight);
				vertList.AddRange(verts);
			}
			return vertList.ToArray();
		}

		private int[] ReadRow(Bitmap bmp, int row)
		{
			int[] ret = new int[width];
			for (int x = 0; x < width; x++)
			{
				Color c = bmp.GetPixel(x, row);
				ret[x] = ((int)c.R << 8) + c.G;
			}
			return ret;
		}

		private Vector3 ConvertCoordinatesToVector(int x, int y, int elevation)
		{
			float xScale = shapeWidth / (float)width;
			float zScale = shapeHeight / (float)height;
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
			//Vector3 norm = new Vector3(0, 1, 0);
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
			//float bScale = 1.0f / (float)0x8000;
			//Color4 col = new Color4(1 - vertex.Position.X * rScale, vertex.Position.Z * gScale, vertex.Position.Y * bScale);
			Color4 col = new Color4(1 - vertex.Position.X * rScale, vertex.Position.Z * gScale, 1.0f);
			return col;
		}
	}
}
