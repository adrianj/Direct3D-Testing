using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Reflection;
using System.Windows.Forms;

namespace Direct3DLib
{
	public class GenericTypeConverter<T> : GenericTypeConverter
	{
		public override Type cType
		{
			get { return typeof(T); }
		}
	}

	public abstract class GenericTypeConverter : TypeConverter
	{
		public abstract Type cType { get; }

		public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
		{
			if (sourceType.Equals(typeof(string)))
				return true;
			return false;
		}

		public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
		{
			if(destinationType.Equals(typeof(string)))
				return true;
			return false;
		}

		public override object CreateInstance(ITypeDescriptorContext context, System.Collections.IDictionary propertyValues)
		{
			object value = Activator.CreateInstance(cType);
			foreach (KeyValuePair<object, object> pair in propertyValues)
			{
				MethodInfo setMethod = cType.GetProperty((string)pair.Key).GetSetMethod();
				if (setMethod != null)
					setMethod.Invoke(value, new object[] { pair.Value });
			}
			return value;
		}

		public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
		{
			object t = Activator.CreateInstance(cType);
			if (value.GetType() == typeof(string))
			{
				MethodInfo tryParse = GetTryParseMethod(cType);
				object[] paras = new object[] { value, t };
				bool parseSucceeded = (bool)tryParse.Invoke(t, paras);
				if (parseSucceeded)
				{
					return paras[1];
				}
			}
			throw new ArgumentException("Could not convert '" + value + "' to type '" + cType + "'");

		}

		public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
		{
			//LatLong thisVal = value as LatLong;
			if (destinationType == typeof(string))
				return "" + value;
			throw new FormatException("Could not convert from '" + value + "' to LatLong");
		}
		public override bool GetPropertiesSupported(ITypeDescriptorContext context)
		{
			return true;
		}

		public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object value, Attribute[] attributes)
		{
			return TypeDescriptor.GetProperties(cType);
		}

		public static MethodInfo GetTryParseMethod(Type typeWithMethods)
		{
			MethodInfo method = typeWithMethods.GetMethod("TryParse", BindingFlags.Static | BindingFlags.Public);
			if (method == null) return null;
			if (method.ReturnType != typeof(bool)) return null;
			return method;
		}
	}
}
