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
	}

	public class Float3TypeConverter : GenericTypeConverter
	{
		public Float3TypeConverter() { cType = typeof(Float3); }
		/*
		public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
		{
			if (sourceType.Equals(typeof(SlimDX.Vector3)))
				return true;
			return base.CanConvertFrom(context,sourceType);
		}

		public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
		{
			if (destinationType.Equals(typeof(SlimDX.Vector3)))
				return true;
			return base.CanConvertTo(context,destinationType);
		}

		public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
		{
			if(value is SlimDX.Vector3)
				return new Float3((SlimDX.Vector3)value);
			return base.ConvertFrom(context,culture,value);
		}

		public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
		{			
			Float3 thisVal = value as Float3;
			if (destinationType == typeof(SlimDX.Vector3))
			{
				return new SlimDX.Vector3(thisVal.X, thisVal.Y, thisVal.Z);
			}
			else
				return base.ConvertTo(context,culture,value,destinationType);
		}
		 */

	}
}
