using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;


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
			//if (dLat < 0)
			//	dLat++;
			//long lat = Convert.ToInt64(dLat);
			double ret = dLat * delta;
			return ret;
		}
	}
}
