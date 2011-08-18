using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Reflection;

namespace Direct3DLib
{
	public struct LatLong
	{
		private double lat;
		private double lng;
		public double Latitude { get { return lat; } set { lat = value; } }
		public double Longitude { get { return lng; } set { lng = value; } }
		//public LatLong() { }
		public LatLong(double lat, double lng) :this() { this.lat = lat; this.lng = lng; }
		public LatLong(LatLong latLong) : this(latLong.Latitude, latLong.Longitude) { }

		public override bool Equals(object obj)
		{
			if (!(obj is LatLong)) return false;
			LatLong other = (LatLong)obj;
			if (other.Latitude != this.Latitude) return false;
			if (other.Longitude != this.Longitude) return false;
			return true;
		}

		public override int GetHashCode()
		{
			return Latitude.GetHashCode() ^ Longitude.GetHashCode();
		}

		public override string ToString()
		{
			return Latitude.ToString("F5") + ","+Longitude.ToString("F5");
		}

		public static bool TryParse(string s, out LatLong value)
		{
			value = new LatLong();
			try
			{
				value = LatLong.Parse(s);
			}
			catch (ArgumentException) { return false; }
			return true;
		}

		public static LatLong Parse(string s)
		{
			LatLong value = new LatLong();
			string[] split = s.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
			if (split.Length != 2) throw new ArgumentException("LatLong should have a comma seperated pair of numbers.");
			double d = 0;
			if (!double.TryParse(split[0], out d)) throw new ArgumentException("LatLong should have a comma seperated pair of numbers.");
			value.Latitude = d;
			if (!double.TryParse(split[1], out d)) throw new ArgumentException("LatLong should have a comma seperated pair of numbers.");
			value.Longitude = d;
			return value;
		}

		public static LatLong Min(LatLong a, LatLong b)
		{
			return new LatLong(Math.Min(a.Latitude, b.Latitude), Math.Min(a.Longitude, b.Longitude));
		}
		public static LatLong Subtract(LatLong a, LatLong b)
		{
			return new LatLong(a.Latitude - b.Latitude, a.Longitude - b.Longitude);
		}
		public static LatLong Add(LatLong a, LatLong b)
		{
			return new LatLong(a.Latitude + b.Latitude, a.Longitude + b.Longitude);
		}

		public static LatLong MinValue = new LatLong(-90, -180);
		public static LatLong MaxValue = new LatLong(90, 180);
	}

	public class LatLongTypeConverter : GenericTypeConverter
	{
		public LatLongTypeConverter() { cType = typeof(LatLong); }
	}

}
