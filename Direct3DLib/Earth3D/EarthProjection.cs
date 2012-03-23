using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace Direct3DLib
{
	public class EarthProjection
	{
		public static double EarthDiameterInMetres = 12756000;

		private static double XScale = 256.0 / 360.0;
		public static double UnitsPerDegreeLatitude = 110;
		public static double UnitsPerMetreElevation = 0.001;
		public static float ShapeScale = (float)UnitsPerMetreElevation;

		public static double GetYScaleAtLatitude(double latitude)
		{
			double sec = 1 / Math.Cos(Math.Abs(latitude) * Math.PI / 180.0);
			double ys = XScale * sec;
			if (ys > 1) Console.WriteLine("Extreme Latitude: " + latitude + ", " + ys);
			return ys;
		}

		public static double GetXScaleAtLatitude(double latitude)
		{
			return XScale;
		}

		public static RectangleF CalculateImageBoundsAtLatitude(int imageWidth, int imageHeight, double latitude)
		{
			double w = (double)(imageWidth);
			double h = (double)(imageHeight);
			double xs = GetXScaleAtLatitude(latitude);
			double ys = GetYScaleAtLatitude(latitude);
			double width = xs * w;
			double height = ys * h;
			double left = (w - width) / 2.0;
			double top = Math.Floor((h - height) / 2.0);
			return new RectangleF((float)left, (float)top, (float)width, (float)height);
		}

		public static LatLong CalculateNearestLatLongAtDelta(LatLong latLong, double delta)
		{
			return CalculateNearestLatLongAtDelta(latLong, delta, true);
		}

		public static LatLong CalculateNearestLatLongAtDelta(LatLong latLong, double delta, bool roundToNearest)
		{
			double dLat = latLong.Latitude / delta;
			double dLng = latLong.Longitude / delta;
			if (!roundToNearest)
			{
				dLat = Math.Floor(dLat);
				dLng = Math.Floor(dLng);
			}
			long lat = Convert.ToInt64(dLat);
			long lng = Convert.ToInt64(dLng);
			LatLong ret = new LatLong((double)lat * delta, (double)lng * delta);
			return ret;
		}

		public static int GetZoomFromElevation(double elevation)
		{
			if (elevation <= 0)
				return EarthTiles.MaxGoogleZoom;
			double zoom = 25.0 - Math.Log(elevation, 2.0);
			int z = (int)zoom;
			if (z > EarthTiles.MaxGoogleZoom)
				z = EarthTiles.MaxGoogleZoom;
			return z;
		}

		public static LatLong ConvertCameraLocationToLatLong(Float3 cameraLocation)
		{
			float units = (float)UnitsPerDegreeLatitude;
			return new LatLong(cameraLocation.Z / units, cameraLocation.X / units);
		}

		public static double ConvertCameraLocationToElevation(Float3 cameraLocation)
		{
			return cameraLocation.Y / UnitsPerMetreElevation;
		}

		public static Float3 ConvertLatLongElevationToCameraLocation(LatLong latLong, double elevation)
		{
			float units = (float)UnitsPerDegreeLatitude;
			return new Float3((float)latLong.Longitude * units, (float)(elevation * UnitsPerMetreElevation), (float)latLong.Latitude * units);
		}
	}
}
