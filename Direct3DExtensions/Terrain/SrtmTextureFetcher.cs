using System.IO;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Drawing;

namespace Direct3DExtensions.Terrain
{

	public class Srtm3TextureFetcher : TerrainHeightTextureFetcher
	{
		public string SrtmDataFolder = @"C:\Public\WebGIS\WebGIS_SRTM3\";

		protected int FileMapHeight = 1201;
		protected int FileMapWidth = 1201;
		public int PixelsPerLatitude { get; protected set; }
		public int PixelsPerLongitude { get; protected set; }
		protected int LongDiffBetweenFiles = 1;
		protected int LatDiffBetweenFiles = 1;
		protected short NullValue = short.MinValue;
		protected Size sizeInPixels;
		protected Rectangle coarseRegion;

		protected MemorySet<TerrainDescriptor, short> TerrainMemory = new MemorySet<TerrainDescriptor, short>() { MaxSize = 2048*2048 };

		public Srtm3TextureFetcher()
		{
			PixelsPerLatitude = 1200;
			PixelsPerLongitude = 1200;
		}

		#region Specific to 'regionInPixels' method

		public virtual short[,] FetchTerrain(PointF startingLongLat, Rectangle regionInPixels)
		{
			TerrainDescriptor desc = new TerrainDescriptor() { longLat = startingLongLat, regionInPixels = regionInPixels };
			if (TerrainMemory.Contains(desc))
				return TerrainMemory.Get(desc);

			Point location = CalculatePixelLocationInCombinedFileMap(startingLongLat);
			location = Point.Add(location, new Size(regionInPixels.Left, -regionInPixels.Top));
			location = Point.Subtract(location, new Size(regionInPixels.Width / 2, regionInPixels.Height / 2));

			regionInPixels = new Rectangle(location, regionInPixels.Size);
			//Console.WriteLine("region: " + regionInPixels);
			coarseRegion = CalculateCoarseRegion(regionInPixels);
			//Console.WriteLine("regionPixels: " + regionInPixels+", coarseRegion: "+coarseRegion);
			sizeInPixels = regionInPixels.Size;
			short[,] array = new short[sizeInPixels.Height, sizeInPixels.Width];
			Point arrayOffset = new Point();
			Rectangle subregionInPixels = new Rectangle();
			for (int y = 0; y <= coarseRegion.Height; y += LatDiffBetweenFiles)
			{
				arrayOffset.X = 0;
				for (int x = 0; x <= coarseRegion.Width; x += LongDiffBetweenFiles)
				{
					Point cp = UpdateCoarsePoint(y, x);
					Point coarsePoint = CalculatePixelLocationInCombinedFileMap(cp);

					string filename = GetFilenameFromLongLat(cp.X, cp.Y);
					subregionInPixels = CalculateRegionInFile(regionInPixels, CalculatePixelLocationInCombinedFileMap(cp));
					//Console.WriteLine("cp: "+cp+", coarsePoint: " + coarsePoint + ", filename: " + filename + ", subregion: " + subregionInPixels);
					ReadHgtFile(array, filename, arrayOffset, subregionInPixels);
					arrayOffset.X += subregionInPixels.Width;
				}
				arrayOffset.Y += subregionInPixels.Height;
			}

			TerrainMemory.Add(desc, array);

			return array;
		}


		protected virtual Point CalculatePixelLocationInCombinedFileMap(PointF longLat)
		{
			int lng = (int)((double)(longLat.X + 180.0) * PixelsPerLongitude);
			int lat = (int)((double)(90.0 - longLat.Y) * PixelsPerLatitude);
			return new Point(lng, lat);
		}

		protected virtual Rectangle CalculateCoarseRegion(Rectangle regionInPixels)
		{
			float left = (float)regionInPixels.Left / (float)PixelsPerLongitude;
			float top = (float)regionInPixels.Bottom / (float)PixelsPerLatitude;
			float width = (float)regionInPixels.Width / (float)PixelsPerLongitude;
			float height = (float)regionInPixels.Height / (float)PixelsPerLatitude;
			RectangleF rect = new RectangleF(left - 180, 90 - top, width, height);
			return CalculateCoarseRegion(rect);
		}

		protected virtual Rectangle CalculateRegionInFile(Rectangle regionInPixels, Point coarseLongLat)
		{
			Rectangle orig = CalculateRegionInFile((RectangleF)regionInPixels, coarseLongLat);

			int xOffset = 0;
			int yOffset = 0;
			int width = PixelsPerLongitude * LongDiffBetweenFiles;
			int height = PixelsPerLongitude * LatDiffBetweenFiles;

			int ldiff = regionInPixels.Left - coarseLongLat.X;
			int rdiff = coarseLongLat.X - regionInPixels.Right + PixelsPerLongitude * LongDiffBetweenFiles;
			int bdiff = coarseLongLat.Y - regionInPixels.Bottom + PixelsPerLatitude * LatDiffBetweenFiles;
			int tdiff = regionInPixels.Top - coarseLongLat.Y;

			if (ldiff > 0)
			{
				xOffset = ldiff;
				width -= xOffset;
			}
			if (rdiff > 0)
			{
				width -= rdiff;
			}
			if (tdiff > 0)
			{
				yOffset = tdiff;
				height -= yOffset;
			}
			if (bdiff > 0)
			{
				height -= bdiff;
			}
			Rectangle ret = new Rectangle(xOffset, yOffset, width, height);
			return ret;
		}

		#endregion

		#region Specific to 'regionInLatLongNotation' method
		public short[,] FetchTerrain(RectangleF regionInLatLongNotation)
		{
			coarseRegion = CalculateCoarseRegion(regionInLatLongNotation);
			sizeInPixels = CalculateRegionSizeInPixels(regionInLatLongNotation);
			short[,] array = new short[sizeInPixels.Height, sizeInPixels.Width];
			Point arrayOffset = new Point();
			Rectangle subregionInPixels = new Rectangle();
			for (int y = 0; y <= coarseRegion.Height; y += LatDiffBetweenFiles)
			{
				arrayOffset.X = 0;
				for (int x = 0; x <= coarseRegion.Width; x += LongDiffBetweenFiles)
				{
					Point coarsePoint = UpdateCoarsePoint(y, x);
					string filename = GetFilenameFromLongLat(coarsePoint.X, coarsePoint.Y);
					subregionInPixels = CalculateRegionInFile(regionInLatLongNotation, coarsePoint);
					ReadHgtFile(array, filename, arrayOffset, subregionInPixels);
					arrayOffset.X += subregionInPixels.Width;
				}
				arrayOffset.Y += subregionInPixels.Height;
			}
			return array;
		}


		protected virtual Point UpdateCoarsePoint(int y, int x)
		{
			Point coarsePoint = new Point(coarseRegion.Left + x, coarseRegion.Bottom - y);
			return coarsePoint;
		}




		Size CalculateRegionSizeInPixels(RectangleF regionInLatLongNotation)
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
					//Console.WriteLine("coarsePoint: " + coarsePoint);
					regionInFile = CalculateRegionInFile(regionInLatLongNotation, coarsePoint);
					width += regionInFile.Width;

				}
				height += regionInFile.Height;
			}
			return new Size(width, height);
		}

		protected virtual Rectangle CalculateCoarseRegion(RectangleF regionInLatLongNotation)
		{
			int top = IncrementWhileLessThan(90, -LatDiffBetweenFiles, regionInLatLongNotation.Top);
			int bottom = IncrementWhileLessThan(90, -LatDiffBetweenFiles, regionInLatLongNotation.Bottom);
			int left = IncrementWhileLessThan(-180, LongDiffBetweenFiles, regionInLatLongNotation.Left);
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

		protected virtual Rectangle CalculateRegionInFile(RectangleF regionInLatLongNotation, Point coarseLongLat)
		{
			int xOffset = 0;
			int yOffset = 0;
			int width = PixelsPerLongitude * LongDiffBetweenFiles;
			int height = PixelsPerLongitude * LatDiffBetweenFiles;
			int bdiff; int tdiff; int ldiff; int rdiff;
			GetPixelDistanceFromEdges(regionInLatLongNotation, coarseLongLat, out bdiff, out tdiff, out ldiff, out rdiff);

			if (ldiff > 0)
			{
				xOffset = ldiff;
				width -= xOffset;
			}
			if (rdiff > 0)
			{
				width -= rdiff;
			}
			if (tdiff > 0)
			{
				yOffset = tdiff;
				height -= yOffset;
			}
			if (bdiff > 0)
			{
				height -= bdiff;
			}
			int lat = (int)regionInLatLongNotation.Top;
			int lng = (int)regionInLatLongNotation.Left;
			Point point = new Point(xOffset, yOffset);
			Size size = new Size(width, height);
			Rectangle rect = new Rectangle(point, size);
			return rect;
		}

		protected virtual void GetPixelDistanceFromEdges(RectangleF regionInLatLongNotation, Point coarseLongLat, out int bdiff, out int tdiff, out int ldiff, out int rdiff)
		{
			int top = (int)((float)PixelsPerLatitude * regionInLatLongNotation.Bottom);
			int bottom = (int)((float)PixelsPerLatitude * regionInLatLongNotation.Top);
			int left = (int)((float)PixelsPerLongitude * regionInLatLongNotation.Left);
			int right = (int)((float)PixelsPerLatitude * regionInLatLongNotation.Right);

			bdiff = bottom - PixelsPerLatitude * (coarseLongLat.Y - LatDiffBetweenFiles);

			tdiff = PixelsPerLatitude * coarseLongLat.Y - top;
			ldiff = left - PixelsPerLongitude * coarseLongLat.X;
			rdiff = PixelsPerLongitude * (coarseLongLat.X + LongDiffBetweenFiles) - right;
		}

		#endregion

		#region Common Methods
		protected virtual void ReadHgtFile(short[,] destinationArray, string filename, Point arrayOffset, Rectangle regionInPixels)
		{
			//Console.WriteLine("" + this + ": filename: " + filename);
			//Console.WriteLine("offset: " + arrayOffset + ", region: " + regionInPixels +", coarseRegion: "+coarseRegion);

			if (regionInPixels.Left + regionInPixels.Width > FileMapWidth || regionInPixels.Top + regionInPixels.Height > FileMapHeight)
				throw new Exception("Requested offset and region outside bounds of file. " + regionInPixels);

			if (regionInPixels.Width < 0 || regionInPixels.Height < 0)
				throw new Exception("Can't access a negative region. " + regionInPixels);
			if (!File.Exists(filename))
			{
				//Console.WriteLine("File not found: " + filename);
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

		protected virtual string GetFilenameFromLongLat(int longitude, int latitude)
		{
			if (longitude >= 180)
				longitude -= 360;
			latitude -= 1;
			string lat = GetLatitudeString(latitude);
			string lng = GetLongitudeString(longitude);
			string filename = lat + lng + ".hgt";
			return SrtmDataFolder + filename;
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
		#endregion
	}

	public class Srtm30TextureFetcher : Srtm3TextureFetcher
	{

		public Srtm30TextureFetcher()
		{
			this.FileMapHeight = 6000;
			this.FileMapWidth = 4800;
			this.LatDiffBetweenFiles = 50;
			this.LongDiffBetweenFiles = 40;
			this.PixelsPerLatitude = this.FileMapHeight / LatDiffBetweenFiles;
			this.PixelsPerLongitude = this.FileMapWidth / LongDiffBetweenFiles;
			SrtmDataFolder = @"C:\Public\WebGIS\WebGIS_SRTM30\";
		}

		protected override string GetFilenameFromLongLat(int longitude, int latitude)
		{
			if (longitude >= 180)
				longitude -= 360;
			string lat = GetLatitudeString(latitude);
			string lng = GetLongitudeString(longitude);
			string filename = lng + lat + ".dem";
			return SrtmDataFolder + filename;
		}



	}
}
