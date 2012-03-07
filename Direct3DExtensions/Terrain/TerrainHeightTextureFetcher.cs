using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Drawing;
using System.IO;

namespace Direct3DExtensions.Terrain
{
	public interface TerrainHeightTextureFetcher
	{
		short[,] FetchTerrain(RectangleF regionInLatLongNotation);
	}

	public class Strm3TextureFetcher : TerrainHeightTextureFetcher
	{
		public string Strm3DataFolder = @"C:\Users\adrianj\Documents\Work\CAD\WebGIS_SRTM3\";

		protected int FileMapHeight = 1201;
		protected int FileMapWidth = 1201;
		public int PixelsPerLatitude { get; protected set; }
		public int PixelsPerLongitude { get; protected set; }
		protected int LongDiffBetweenFiles = 1;
		protected int LatDiffBetweenFiles = 1;
		protected short NullValue = short.MinValue;
		protected Point sizeInPixels;
		protected Rectangle coarseRegion;

		public Strm3TextureFetcher()
		{
			PixelsPerLatitude = 1200;
			PixelsPerLongitude = 1200;
		}

		public short[,] FetchTerrain(RectangleF regionInLatLongNotation)
		{
			coarseRegion = CalculateCoarseRegion(regionInLatLongNotation);
			sizeInPixels = CalculateRegionSizeInPixels(regionInLatLongNotation);
			short[,] array = new short[sizeInPixels.Y,sizeInPixels.X];
			Point arrayOffset = new Point();
			Rectangle regionInPixels = new Rectangle();
			for (int y = 0; y <= coarseRegion.Height; y+=LatDiffBetweenFiles)
			{
				arrayOffset.X = 0;
				for (int x = 0; x <= coarseRegion.Width; x+=LongDiffBetweenFiles)
				{
					Point coarsePoint = UpdateCoarsePoint(y, x);
					string filename = GetFilenameFromLatLong(coarsePoint.X, coarsePoint.Y);
					regionInPixels = CalculateRegionInFile(regionInLatLongNotation, coarsePoint);
					ReadHgtFile(array, filename, arrayOffset, regionInPixels);
					arrayOffset.X += regionInPixels.Width;
				}
				arrayOffset.Y += regionInPixels.Height;
			}
			return array;
		}


		protected virtual Point UpdateCoarsePoint(int y, int x)
		{
			Point coarsePoint = new Point(coarseRegion.Left + x, coarseRegion.Bottom - y);
			return coarsePoint;
		}



		protected virtual void ReadHgtFile(short [,] destinationArray, string filename, Point arrayOffset, Rectangle regionInPixels)
		{
			Console.WriteLine("" + this + ": filename: " + filename);
			Console.WriteLine("offset: " + arrayOffset + ", region: " + regionInPixels +", coarseRegion: "+coarseRegion);

			if (regionInPixels.Left + regionInPixels.Width > FileMapWidth || regionInPixels.Top + regionInPixels.Height > FileMapHeight)
				throw new Exception("Requested offset and region outside bounds of file. " + regionInPixels);

			if (regionInPixels.Width < 0 || regionInPixels.Height < 0)
				throw new Exception("Can't access a negative region. " + regionInPixels);
			if (!File.Exists(filename))
			{
				Console.WriteLine("File not found: " + filename);
				return;
			}

			using (BinaryReader reader = new BinaryReader(new FileStream(filename, FileMode.Open)))
			{
				long seekOffset = regionInPixels.Top * (FileMapWidth) * 2;
				reader.BaseStream.Seek(seekOffset, SeekOrigin.Begin);
				for (int y = 0; y < regionInPixels.Height; y++)
				{
					seekOffset = regionInPixels.Left * 2;
					reader.BaseStream.Seek(seekOffset, SeekOrigin.Current);
					for (int x = 0; x < regionInPixels.Width; x++)
					{
						byte[] b = reader.ReadBytes(2);
						if (x > regionInPixels.Width) continue;
						if (b.Length != 2)
							throw new Exception("Unexpectedly reached EOF.");
						short s = GetValueFromBytes(b);
						destinationArray[y + arrayOffset.Y, x + arrayOffset.X] = s;
					}
					seekOffset = ((FileMapWidth) - regionInPixels.Left - regionInPixels.Width) * 2;
					reader.ReadBytes((int)seekOffset);
				}
			}
		}

		protected virtual short GetValueFromBytes(byte[] b)
		{
			b = b.Reverse().ToArray();
			short s = BitConverter.ToInt16(b, 0);
			if (s == NullValue) s = 0;
			return s;
		}

		Point CalculateRegionSizeInPixels(RectangleF regionInLatLongNotation)
		{
			int width = 0;
			int height = 0;
			for (int y = 0; y <= coarseRegion.Height; y += LatDiffBetweenFiles)
			{
				width = 0;
				Rectangle regionInFile = new Rectangle();
				for (int x = 0; x <= coarseRegion.Width; x += LongDiffBetweenFiles)
				{
					Point coarsePoint = UpdateCoarsePoint(y, x);
					Console.WriteLine("coarsePoint: " + coarsePoint);
					regionInFile = CalculateRegionInFile(regionInLatLongNotation, coarsePoint);
					width += regionInFile.Width;

				}
				height += regionInFile.Height;
			}
			//width = width / PixelsPerLongitude;
			//height = height / PixelsPerLatitude;
			//width = (int)Math.Round((regionInLatLongNotation.Right - regionInLatLongNotation.Left) * (float)PixelsPerLongitude);
			//height = (int)Math.Round((regionInLatLongNotation.Bottom - regionInLatLongNotation.Top) * (float)PixelsPerLongitude);
			return new Point(width, height);
		}

		protected virtual Rectangle CalculateCoarseRegion(RectangleF regionInLatLongNotation)
		{
			int top = IncrementWhileLessThan(90, -LatDiffBetweenFiles, regionInLatLongNotation.Top);
			int bottom = IncrementWhileLessThan(90, -LatDiffBetweenFiles, regionInLatLongNotation.Bottom);
			int left = IncrementWhileLessThan(-180, LongDiffBetweenFiles,regionInLatLongNotation.Left);
			int right = IncrementWhileLessThan(-180, LongDiffBetweenFiles, regionInLatLongNotation.Right);
			int width = right - left;
			int height = bottom - top;
			Rectangle rect = new Rectangle(left, top, width, height);

			return rect;
		}



		int IncrementWhileLessThan(int start, int increment, float terminal)
		{
			int r = start;
			if (increment == 0)
				throw new Exception("Can't increment in steps of 0");
			if (increment > 0)
			{
				while ((float)r < terminal)
					r += increment;
				return r - increment;
			}
			else
			{
				while ((float)r > terminal)
					r += increment;
				return r - increment;
			}
		}

		protected virtual Rectangle CalculateRegionInFile(RectangleF regionInLatLongNotation, Point coarseLatLong)
		{
			int xOffset = 0;
			int yOffset = 0;
			int width = PixelsPerLongitude * LongDiffBetweenFiles;
			int height = PixelsPerLongitude * LatDiffBetweenFiles;
			Console.WriteLine("Calc Region: " + regionInLatLongNotation + ", " + coarseLatLong);
			int bdiff; int tdiff; int ldiff; int rdiff;
			GetPixelDistanceFromEdges(regionInLatLongNotation, coarseLatLong, out bdiff, out tdiff, out ldiff, out rdiff);

			Console.WriteLine("b: " + bdiff + ", t: " + tdiff + ", l: " + ldiff + ", r: " + rdiff);
			if (ldiff > 0)
			{
				xOffset = ldiff;
				width -= xOffset;
				Console.WriteLine("Left side: " + xOffset + "," + width);
			}
			if (rdiff > 0)
			{
				width -= rdiff;
				Console.WriteLine("Right side: " + xOffset + "," + width);
			}
			if (tdiff > 0)
			{
				yOffset = tdiff;
				height -= yOffset;
				Console.WriteLine("Top side: "+ yOffset + "," + height);
			}
			if (bdiff > 0)
			{
				height -= bdiff;
				Console.WriteLine("Bottom side: "  + yOffset + "," + height);
			}
			int lat = (int)regionInLatLongNotation.Top;
			int lng = (int)regionInLatLongNotation.Left;
			Point point = new Point(xOffset, yOffset);
			Size size = new Size(width, height);
			Rectangle rect = new Rectangle(point, size);
			return rect;
		}

		protected virtual void GetPixelDistanceFromEdges(RectangleF regionInLatLongNotation, Point coarseLatLong, out int bdiff, out int tdiff, out int ldiff, out int rdiff)
		{
			int top = (int)((float)PixelsPerLatitude * regionInLatLongNotation.Bottom);
			int bottom = (int)((float)PixelsPerLatitude * regionInLatLongNotation.Top);
			int left = (int)((float)PixelsPerLongitude * regionInLatLongNotation.Left);
			int right = (int)((float)PixelsPerLatitude * regionInLatLongNotation.Right);

			bdiff = bottom - PixelsPerLatitude * (coarseLatLong.Y -LatDiffBetweenFiles);
			
			tdiff = PixelsPerLatitude * coarseLatLong.Y - top;
			ldiff = left - PixelsPerLongitude * coarseLatLong.X;
			rdiff = PixelsPerLongitude * (coarseLatLong.X + LongDiffBetweenFiles) - right;
		}

		protected virtual string GetFilenameFromLatLong(int longitude, int latitude)
		{
			if (longitude >= 180)
				longitude -= 360;
			latitude -= 1;
			string lat = GetLatitudeString(latitude);
			string lng = GetLongitudeString(longitude);
			string filename = lat + lng + ".hgt";
			return Strm3DataFolder + filename;
		}

		protected static string GetLongitudeString(int longitude)
		{
			string lng = "";
			if (longitude < 0)
			{
				longitude = Math.Abs(longitude);
				lng = "W";
			}
			else
				lng = "E";
			lng += longitude.ToString("D3");
			return lng;
		}

		protected static string GetLatitudeString(int latitude)
		{
			string lat = "";
			if (latitude < 0)
			{
				latitude = Math.Abs(latitude);
				lat = "S";
			}
			else
				lat = "N";
			lat += latitude.ToString("D2");
			return lat;
		}
    
	}

	public class Strm30TextureFetcher : Strm3TextureFetcher
	{

		public Strm30TextureFetcher()
		{
			this.FileMapHeight = 6000;
			this.FileMapWidth = 4800;
			this.LatDiffBetweenFiles = 50;
			this.LongDiffBetweenFiles = 40;
			this.PixelsPerLatitude = this.FileMapHeight / LatDiffBetweenFiles;
			this.PixelsPerLongitude = this.FileMapWidth / LongDiffBetweenFiles;
			Strm3DataFolder = @"C:\Users\adrianj\Documents\Work\CAD\WebGIS_SRTM30\";
		}

		protected override string GetFilenameFromLatLong(int longitude, int latitude)
		{
			if (longitude >= 180)
				longitude -= 360;
			string lat = GetLatitudeString(latitude);
			string lng = GetLongitudeString(longitude);
			string filename = lng + lat + ".dem";
			return Strm3DataFolder + filename;
		}


		/*
		protected override void GetPixelDistanceFromEdges(RectangleF regionInLatLongNotation, Point coarseLatLong, out int bdiff, out int tdiff, out int ldiff, out int rdiff)
		{
			int top = (int)((float)PixelsPerLatitude * regionInLatLongNotation.Bottom);
			int bottom = (int)((float)PixelsPerLatitude * regionInLatLongNotation.Top);
			int left = (int)((float)PixelsPerLongitude * regionInLatLongNotation.Left);
			int right = (int)((float)PixelsPerLatitude * regionInLatLongNotation.Right);

			bdiff = bottom - PixelsPerLatitude * coarseLatLong.Y + FileMapHeight;
			tdiff = PixelsPerLatitude * coarseLatLong.Y - top;
			ldiff = left - PixelsPerLongitude * coarseLatLong.X;
			rdiff = PixelsPerLongitude * coarseLatLong.X - right + FileMapWidth;
		}
		 */


	}

}
