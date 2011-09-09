using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using SlimDX;

namespace Direct3DLib
{
	[TypeConverter(typeof(Float3TypeConverter))]
	public class Float3
	{
		private SlimDX.Vector3 vec;// = new SlimDX.Vector3();
		public Float3() { }
		public Float3(double x, double y, double z) : this((float)x, (float)y, (float)z) { }
		public Float3(float x, float y, float z) { vec.X = x; vec.Y = y; vec.Z = z; }
		public Float3(SlimDX.Vector3 original) { vec = original; }
		public Float3(Float3 original) : this(original.AsVector3()) { }
		public float X { get { return vec.X; } set { vec = new SlimDX.Vector3(value, Y, Z); } }
		public float Y { get { return vec.Y; } set { vec = new SlimDX.Vector3(X, value, Z); } }
		public float Z { get { return vec.Z; } set { vec = new SlimDX.Vector3(X, Y, value); } }
		public SlimDX.Vector3 AsVector3() { return vec; }
		public override string ToString()
		{
			return "" + vec.X + ", " + vec.Y + ", " + vec.Z;
		}
		public static bool TryParse(string s, out Float3 result)
		{
			result = new Float3();
			string [] split = s.Split(new char[]{','},StringSplitOptions.RemoveEmptyEntries);
			if(split.Length != 3) return false;
			float f = 0;
			if(!float.TryParse(split[0],out f)) return false;
			result.X = f;
			if (!float.TryParse(split[1], out f)) return false;
			result.Y = f;
			if (!float.TryParse(split[2], out f)) return false;
			result.Z = f;
			return true;
		}

		public override bool Equals(object obj)
		{
			if (obj is SlimDX.Vector3)
				return this.AsVector3().Equals(obj);
			if (obj is Float3)
			{
				Float3 f3 = (Float3)obj;
				return this.vec.Equals(f3.AsVector3());
			}
			return false;
		}

		public override int GetHashCode()
		{
			return vec.GetHashCode();
		}

		public static Float3 Subtract(Float3 a, Vector3 b) { return Subtract(a, new Float3(b)); }
		public static Float3 Subtract(Vector3 a, Float3 b) { return Subtract(new Float3(a), b); }
		public static Float3 Subtract(Vector3 a, Vector3 b) { return Subtract(new Float3(a), new Float3(b)); }
		public static Float3 Subtract(Float3 a, Float3 b)
		{
			return new Float3(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
		}
		public static Float3 Add(Float3 a, Vector3 b) { return Add(a, new Float3(b)); }
		public static Float3 Add(Vector3 a, Vector3 b) { return Add(new Float3(a), new Float3(b)); }
		public static Float3 Add(Float3 a, Float3 b)
		{
			return new Float3(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
		}


		public static Float3 ConvertYawPitchRollToCartesian(Float3 yawPitchRoll)
		{
			return ConvertYawPitchRollToCartesian(yawPitchRoll.X, yawPitchRoll.Y, yawPitchRoll.Z);
		}

		public static Float3 ConvertYawPitchRollToCartesian(double yaw, double pitch, double roll)
		{
			Matrix m = Matrix.RotationX(-(float)pitch);
			m = m * Matrix.RotationZ((float)roll);
			m = m * Matrix.RotationY((float)yaw);
			Vector3 vec = Vector3.TransformCoordinate(Vector3.UnitZ, m);
			return new Float3(vec);
		}

		public static Float3 ConvertYawPitchRollToPolar(Float3 yawPitchRoll)
		{
			Float3 cart = ConvertYawPitchRollToCartesian(yawPitchRoll);
			return ConvertCartesianToPolar(cart);
		}

		public static Float3 ConvertCartesianToPolar(Float3 cartesian)
		{
			Vector3 vec = cartesian.AsVector3();
			double radius = vec.Length();
			double azimuth = Math.Atan2(cartesian.X, cartesian.Z);
			if (azimuth < 0) azimuth += Math.PI * 2;
			double elevation = Math.Asin(cartesian.Y / radius);
			return new Float3(radius, azimuth, elevation);
		}

		public static Float3 ConvertPolarToCartesian(Float3 polar)
		{
			double radius = polar.X;
			double azimuth = polar.Y;
			double elevation = polar.Z;
			double x = radius * Math.Sin(azimuth) * Math.Cos(elevation);
			double y = radius * Math.Sin(elevation);
			double z = radius * Math.Cos(azimuth) * Math.Cos(elevation);
			return new Float3(x, y, z);
		}

		public static Float3 Average(Float3[] values)
		{
			if (values.Length == 0) return new Float3();
			Float3 sum = new Float3();
			foreach (Float3 val in values)
				sum = Float3.Add(sum, val);
			return new Float3(sum.X / values.Length, sum.Y / values.Length, sum.Z / values.Length);
		}

		public static Float3[] UnwrapPhase(Float3[] values) { return UnwrapPhase(values, (float)Math.PI); }
		public static Float3[] UnwrapPhase(Float3[] values, float tolerance)
		{
			if (values.Length == 0) return values;
			Float3[] ret = new Float3[values.Length];
			ret[0] = values[0];
			for (int i = 1; i < values.Length; i++)
			{
				ret[i] = UnwrapPhase(ret[i - 1], values[i], tolerance);
			}
			return ret;
		}

		public static Float3 UnwrapPhase(Float3 previous, Float3 current, double tolerance)
		{
			float x = UnwrapPhase(previous.X, current.X, tolerance);
			float y = UnwrapPhase(previous.Y, current.Y, tolerance);
			float z = UnwrapPhase(previous.Z, current.Z, tolerance);
			return new Float3(x, y, z);
		}

		public static float UnwrapPhase(double previous, double current, double tolerance)
		{
			while (current > previous + tolerance)
				current -= 2*tolerance;
			while (current < previous - tolerance)
				current += 2*tolerance;
			return (float)current;
		}

		public static float UnwrapPhase(double phase, bool AllowNegative)
		{
			float pi = (float)Math.PI;
			if (AllowNegative)
			{
				while (phase > pi) phase -= 2 * pi;
				while (phase < -pi) phase += 2 * pi;
			}
			else
			{
				while (phase > 2 * pi) phase -= 2 * pi;
				while (phase < 0) phase += 2 * pi;
			}
			return (float)phase;
		}
	}

	public class Float3TypeConverter : GenericTypeConverter
	{
		public Float3TypeConverter() { cType = typeof(Float3); }


	}
}
