using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Drawing;

namespace ImageTiler
{
	public class GooglemapFilenameFactory : SimpleFilenameFactory
	{
		public double BottomLatitude { get; set; }
		public double LeftLongitude { get; set; }


		public override string CreateFilename(int zoomLevel, int tileX, int tileY)
		{
			double centreLat = GetNearestCentreFromZoomLevel(BottomLatitude, zoomLevel);
			double centreLong = GetNearestCentreFromZoomLevel(LeftLongitude, zoomLevel);
			double interval = GetIntervalFromZoomLevel(zoomLevel);
			centreLat += tileY * interval;
			centreLong += tileX * interval;
			string ret = "zoom="+zoomLevel+@"\googlemap_&zoom="+zoomLevel+"center=" + centreLat.ToString("F8") + "," + centreLong.ToString("F8") + ".png";
			return this.ImageFolder + ret;
		}

		public double GetIntervalFromZoomLevel(int zoomLevel)
		{
			double d = 256;
			for (int i = 1; i < zoomLevel; i++)
				d /= 2;
			return d;
		}

		public double GetNearestCentreFromZoomLevel(double offset, int zoomLevel)
		{
			double interval = GetIntervalFromZoomLevel(zoomLevel);
			double corner = CalculateNearestLatLongAtDelta(offset, interval, false);
			return corner + interval/2;
		}

		public double GetNearestCornerFromZoomLevel(double offset, int zoomLevel)
		{
			double interval = GetIntervalFromZoomLevel(zoomLevel);
			double cn = CalculateNearestLatLongAtDelta(offset, interval, false);
			return cn;
		}

		public double CalculateNearestLatLongAtDelta(double latLong, double delta, bool roundToNearest)
		{
			double dLat = latLong / delta;
			if (!roundToNearest)
			{
				dLat = Math.Floor(dLat);
			}
			else
			{
				dLat = Math.Round(dLat, MidpointRounding.AwayFromZero);
			}
			double ret = dLat * delta;
			return ret;
		}

		public Point FindCoordinateInLargerMap(int largerMapZoom, int thisMapZoom)
		{
			return this.FindCoordinateInLargerMap(largerMapZoom, thisMapZoom, this.BottomLatitude, this.LeftLongitude);
		}

		public Point FindCoordinateInLargerMap(int largerMapZoom, int thisMapZoom, double latitude, double longitude)
		{
			Point ret = new Point();
			double delta = this.GetIntervalFromZoomLevel(thisMapZoom);
			double leftThis = this.GetNearestCornerFromZoomLevel(longitude, thisMapZoom);
			double leftLrg = this.GetNearestCornerFromZoomLevel(longitude, largerMapZoom);
			int diffX = Convert.ToInt32((leftThis - leftLrg) / delta);
			double bottomThis = this.GetNearestCornerFromZoomLevel(latitude, thisMapZoom);
			double bottomLrg = this.GetNearestCornerFromZoomLevel(latitude, largerMapZoom);
			int diffY = Convert.ToInt32((bottomThis - bottomLrg) / delta);
			ret.X = diffX;
			ret.Y = diffY;
			Console.WriteLine("delta: " + delta + ", leftT: " + leftThis + ", leftL: " + leftLrg + ", bottT: " + bottomThis + ", bottL: " + bottomLrg+", ret: "+ret);
			return ret;
		}
	}
}
