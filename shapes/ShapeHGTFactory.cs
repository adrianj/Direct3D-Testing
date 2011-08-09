using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Drawing;
using DTALib;
using SlimDX;
using Vector3 = SlimDX.Vector3;
using System.Windows.Forms;

namespace Direct3DLib
{
	public class ShapeHGTFactory
	{
		private const int ROWS_PER_FILE = 1200;
		private const int COLUMNS_PER_FILE = 1200;

		private int nRowsToRead = ROWS_PER_FILE;
		private int nColumnsToRead = COLUMNS_PER_FILE;
		private int startRow = 0;
		private int startCol = 0;

		private double latitudeStart = -37.7;
		public double BottomLeftLatitude { get { return latitudeStart; } set { latitudeStart = value; } }
		private double longitudeStart = 174.6;
		public double BottomLeftLongitude { get { return longitudeStart; } set { longitudeStart = value; } }
		private double latitudeDelta = 1;
		public double LatitudeDelta { get { return latitudeDelta; } set { latitudeDelta = value; } }
		private double longitudeDelta = 1;
		public double LongitudeDelta { get { return longitudeDelta; } set { longitudeDelta = value; } }


		private double unitsPerLatitude = 100000;	// Could be 111 km for example.
		public double UnitsPerDegreeLatitude { get { return unitsPerLatitude; } set { unitsPerLatitude = value; } }

		private double degreesLatitudePerPoint = 1/(double)(ROWS_PER_FILE-1);
		
		private float verticalScale = 1.0f;			// 1 m
		//public float VerticalScale { get { return verticalScale; } set { verticalScale = value; } }
		private float horizontalScale = 1.0f;
		//public float HorizontalScale { get { return horizontalScale; } set { horizontalScale = value; } }


		private string filename;
		public string Filename { get { return filename; } set { filename = value; } }
		private BinaryReaderBiEndian reader;
		private Shape shape;
		private int currentRow = 0;
		private int[] previousRow = null;
		private int previousInt = 0;
		private int currentColumn = 0;

		public static string TerrainFolder = null;

		public static Shape CreateFromFile(string filename)
		{
			ShapeHGTFactory factory = new ShapeHGTFactory(filename);
			
			Shape shape = factory.ReadShapeFromFile();
			return shape;
		}

		public static Shape CreateFromCoordinates(double latStart, double longStart, double latDelta, double longDelta)
		{
			string filename = CalculateFilenameFromLatLong(new LatLong(latStart, longStart));
			return CreateFromFile(filename, latStart, longStart, latDelta, longDelta);
		}

		public static Shape CreateFromFile(string filename, double latStart, double longStart, double latDelta, double longDelta)
		{
			ShapeHGTFactory factory = new ShapeHGTFactory(filename);
			factory.latitudeStart = latStart;
			factory.latitudeDelta = latDelta;
			factory.longitudeStart = longStart;
			factory.longitudeDelta = longDelta;
			Shape shape = factory.ReadShapeFromFile();
			return shape;
		}

		public static string CalculateFilenameFromLatLong(LatLong latLong)
		{
			int latitude = (int)latLong.Latitude;
			int longitude = (int)latLong.Longitude;
			if (latitude < 1 && latLong.Latitude % 1 != 0)
				latitude -= 1;
			if (longitude < 1 && latLong.Longitude % 1 != 0)
				longitude -= 1;
			string ret = LatLongToString(latitude, longitude);
			string folder = TerrainFolder;
			if (TerrainFolder == null) folder = Properties.Settings.Default.MapTerrainFolder;
			return folder + "\\" + ret + ".hgt";
		}

		private static string LatLongToString(int latitude, int longitude)
		{
			string ret = "";
			if (latitude >= 0)
				ret += "N" + latitude.ToString("D2");
			else
				ret = "S" + Math.Abs(latitude).ToString("D2");
			if (longitude >= 0)
				ret += "E" + longitude.ToString("D3");
			else
				ret = "W" + Math.Abs(longitude).ToString("D3");
			return ret;
		}

		public ShapeHGTFactory(string filename)
		{
			this.filename = filename;
		}

		public ShapeHGTFactory() { }

		public Shape ReadShapeFromFile()
		{
			try
			{
				shape = new Shape();
				using (reader = new BinaryReaderBiEndian(filename, true))
				{
					Initialize();
					SkipToStartRow();
					for (int r = 0; r < nRowsToRead-1; r++)
					{
						Vertex[] row = ReadNextRowOfTriangles();
						shape.Vertices.AddRange(row);
					}
				}

				return shape;
			}
			catch (Exception) { throw; }
			 
		}

		public Shape ReadAndReduceShapeFromFile(int logFactorToDecimate)
		{
			if (LatitudeDelta > 1 || LongitudeDelta > 1)
				return GenerateNullShape();
			Initialize();
			short[,] data = ShapeHGTFactory.ReadHGT(filename, startCol, startRow, nColumnsToRead, nRowsToRead);
			int width = data.GetLength(1);
			int height = data.GetLength(0);
			int factor = (int)Math.Pow(2.0, logFactorToDecimate);
			if (factor > 1)
			{
				Size newSize = new Size(width / factor, height / factor);
				data = Decimate(data, factor);
			}
			float latScale = (float)(UnitsPerDegreeLatitude * LatitudeDelta);
			float longScale = (float)(UnitsPerDegreeLatitude * LongitudeDelta);
			shape = ShapeArrayFactory.CreateFromArray(data, new PointF(latScale, longScale));
			return shape;
		}

		private void Initialize()
		{
			horizontalScale = CalculateHorizontalScale();
			CalculateStartingRowsAndColumns();
			CalculateNumRowsAndColumnsToRead();
			previousRow = null;
			currentRow = 0;
		}

		private void CalculateStartingRowsAndColumns()
		{
			double latOffset = (latitudeStart+latitudeDelta) % 1;
			if (latOffset < 0)
				latOffset = -latOffset;
			double longOffset = longitudeStart % 1;
			if (longOffset < 0)
				longOffset = -longOffset;
			startRow = (int)(latOffset * (double)ROWS_PER_FILE);
			startCol = (int)(longOffset * (double)COLUMNS_PER_FILE);
		}

		private void CalculateNumRowsAndColumnsToRead()
		{
			nRowsToRead = (int)(latitudeDelta * (double)ROWS_PER_FILE)+1 ;
			if (nRowsToRead + startRow > ROWS_PER_FILE+1)
				nRowsToRead = ROWS_PER_FILE - startRow;
			nColumnsToRead = (int)(longitudeDelta * (double)COLUMNS_PER_FILE)+1;
			if (nColumnsToRead + startCol > COLUMNS_PER_FILE+1)
				nColumnsToRead = COLUMNS_PER_FILE - startCol;
		}


		private float CalculateHorizontalScale()
		{
			return (float)(unitsPerLatitude * degreesLatitudePerPoint);
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
			nColumnsToRead = (int)rows;
			nRowsToRead = (int)(nInts / rows);
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
			int nTriangles = 6 * (nColumnsToRead-1);
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
			currentRow++;
			int[] row = new int[nColumnsToRead];
			int[] read = new int[COLUMNS_PER_FILE + 1];
			for (int i = 0; i < COLUMNS_PER_FILE + 1; i++)
			{
				read[i] = ReadAndCheckInt();
			}
			Array.Copy(read, startCol, row, 0, nColumnsToRead);
			return row;
		}

		private int ReadAndCheckInt()
		{
			int read = reader.ReadInt16();
			if (read == -0x8000)
				read = previousInt;
			previousInt = read;
			return read;
		}


		private void SkipToStartRow()
		{
			int bytesToSkip = 2 * startRow * (COLUMNS_PER_FILE+1);
			reader.BaseStream.Seek(bytesToSkip, SeekOrigin.Begin);
		}

		private Vertex[] CreateSquare(int topLeftHeight, int topRightHeight, int bottomLeftHeight, int bottomRightHeight)
		{
			float row = (float)(nRowsToRead - currentRow + 1);
			float col = (float)currentColumn;
			float[] z = new float[] { (row) * horizontalScale, (row+1) * horizontalScale };
			float[] x = new float[] { (col) * horizontalScale, (col+1) * horizontalScale };
			Vertex topLeft = new Vertex(x[0], (float)topLeftHeight*verticalScale, z[1]);
			Vertex topRight = new Vertex(x[1], (float)topRightHeight * verticalScale, z[1]);
			Vertex bottomLeft = new Vertex(x[0], (float)bottomLeftHeight * verticalScale, z[0]);
			Vertex bottomRight = new Vertex(x[1], (float)bottomRightHeight * verticalScale, z[0]);
			List<Vertex> square = new List<Vertex>(6);
			Vertex[] sq1 = new Vertex[] { topLeft, topRight, bottomLeft };
			Vertex[] sq2 = new Vertex[] { topRight, bottomRight, bottomLeft };
			square.AddRange(CreateTriangle(sq1));
			square.AddRange(CreateTriangle(sq2));
			return square.ToArray();
		}

		private Vertex[] CreateTriangle(Vertex [] corners)
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
			float xScale = 1 / ((nColumnsToRead+1) * horizontalScale);
			float yScale = 1 / 1000.0f;
			float zScale = 1 / ((nRowsToRead+1) * horizontalScale);
			Color4 col = new Color4(1-vertex.Position.X * xScale, vertex.Position.Z * zScale, vertex.Position.Y*yScale);
			return col;
		}

		public Shape GenerateNullShape()
		{
			float latExtent = (float)(unitsPerLatitude * latitudeDelta);
			float longExtent = (float)(unitsPerLatitude * longitudeDelta);
			Shape shape = new PictureTile(new System.Drawing.RectangleF(0,0,longExtent,latExtent));
			return shape;
		}

		public static Image ConvertHGTToImage(string hgtFilename)
		{
			return ConvertHGTToImage(hgtFilename, 0, 0, COLUMNS_PER_FILE, ROWS_PER_FILE);
		}
		public static Image ConvertHGTToImage(string hgtFilename, int xOffset, int yOffset, int width, int height)
		{
			short[,] data = ReadHGT(hgtFilename, xOffset, yOffset, width, height);
			Bitmap image = new Bitmap(data.GetLength(1), data.GetLength(0), System.Drawing.Imaging.PixelFormat.Format32bppArgb);
			for(int y = 0; y < data.GetLength(1); y++)
				for (int x = 0; x < data.GetLength(0); x++)
				{
					short color = data[x, y];
					image.SetPixel(x, y, Color.FromArgb(color));
				}
			return image;
		}

		public static short[,] ReadHGT(string hgtFilename, int xOffset, int yOffset, int width, int height)
		{
			short[,] data = new short[width, height];
			short prevColor = 0;
			using (BinaryReaderBiEndian reader = new BinaryReaderBiEndian(new FileStream(hgtFilename, FileMode.Open)))
			{
				reader.IsBigEndian = true;
				for (int y = 0; y < ROWS_PER_FILE + 1; y++)
				{
					int finalY = y - yOffset;
					for (int x = 0; x < COLUMNS_PER_FILE + 1; x++)
					{
						int finalX = x - xOffset;
						short color = reader.ReadInt16();
						if (color < -30000) color = prevColor;
						if (finalY >= 0 && finalY < height && finalX >= 0 && finalX < width)
						{
							data[finalX, finalY] = color;
						}
						prevColor = color;
					}
				}
			}
			return data;
		}

		public static short[,] Decimate(short[,] data, int factor)
		{
			int width = data.GetLength(1) / factor;
			int height = data.GetLength(0) / factor;
			short[,] ret = new short[width+1, height+1];
			for(int x = 0; x < width+1; x++)
				for (int y = 0; y < height+1; y++)
				{
					if (x >= width)
						ret[x, y] = data[data.GetLength(1) - 1, y * factor];
					else if (y >= height)
						ret[x, y] = data[x * factor, data.GetLength(0) - 1];
					else if (x == 0)
						ret[x, y] = data[0, y * factor];
					else if (y == 0)
						ret[x, y] = data[x * factor, 0];
					else
						ret[x, y] = AveragePixelsInRange(data, x * factor, y * factor, factor, factor);
				}
			return ret;
		}

		public static short AveragePixelsInRange(short[,] data, int xOffset, int yOffset, int width, int height)
		{
			long sum = 0;
			long n = width * height;
			for(int x = xOffset; x < xOffset + width; x++)
				for(int y = yOffset; y < yOffset + height; y++)
					sum += data[x,y];
			sum = sum / n;
			return (short)sum;
		}
	}
}
