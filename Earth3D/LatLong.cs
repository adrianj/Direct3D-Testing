using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace Direct3DLib
{
	[TypeConverter(typeof(GenericTypeConverter<LatLong>))]
	public struct LatLong
	{
		public double latitude;
		public double longitude;
		public LatLong(double lat, double lng) { latitude = lat; longitude = lng; }

		public override bool Equals(object obj)
		{
			if (!(obj is LatLong)) return false;
			LatLong other = (LatLong)obj;
			if (other.latitude != this.latitude) return false;
			if (other.longitude != this.longitude) return false;
			return true;
		}

		public override int GetHashCode()
		{
			return latitude.GetHashCode() ^ longitude.GetHashCode();
		}

		public override string ToString()
		{
			return latitude.ToString("F5") + ","+longitude.ToString("F5");
		}

		public static bool TryParse(string s, out LatLong value)
		{
			value = new LatLong();
			string[] split = s.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
			if (split.Length != 2) return false;
			double d = 0;
			if (!double.TryParse(split[0], out d)) return false;
			value.latitude = d;
			if(!double.TryParse(split[1],out d)) return false;
			value.longitude = d;
			return true;
		}
	}
}
