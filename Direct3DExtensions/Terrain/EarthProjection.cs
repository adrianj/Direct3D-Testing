using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Direct3DExtensions.Terrain
{
	public class EarthProjection
	{
		public static double EarthDiameterInMetres = 12756000;
		public static double WorldUnitsPerDegree = 1200;

		public static double ConvertWorldUnitsToMetres(double worldUnits)
		{
			return worldUnits * EarthDiameterInMetres / WorldUnitsPerDegree / 360.0;
		}

		public static double ConvertMetresToWorldUnits(double metres)
		{
			return metres * WorldUnitsPerDegree * 360.0 / EarthDiameterInMetres;
		}

		public static double ConvertWorldUnitsToDegrees(double worldUnits)
		{
			return worldUnits / WorldUnitsPerDegree;
		}

		public static double ConvertDegreesToWorldUnits(double degrees)
		{
			return degrees * WorldUnitsPerDegree;
		}
	}
}
