using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.ComponentModel.Design.Serialization;
using System.Windows.Forms;

namespace Direct3DLib
{
	public class GenericTypeConverter<T> : GenericTypeConverter
	{
		public GenericTypeConverter()
			: base()
		{
			cType = typeof(T);
		}
	}

	public abstract class GenericTypeConverter : BasicTypeConverter
	{
		protected Type cType = typeof(object);

		public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
		{
			if (sourceType.Equals(typeof(string)))
				return true;
			return base.CanConvertFrom(context,sourceType);
		}

		public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
		{
			if(destinationType.Equals(typeof(string)))
				return true;
			return base.CanConvertTo(context,destinationType);
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
				try
				{
					MethodInfo tryParse = GetTryParseMethod(cType);
					object[] paras = new object[] { value, t };
					bool parseSucceeded = (bool)tryParse.Invoke(t, paras);
					if (parseSucceeded)
					{
						return paras[1];
					}
				}
				catch
				{
					return t;
				}
			}
			return base.ConvertFrom(context, culture, value);

		}

		public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
		{
			if (destinationType == typeof(string))
				return "" + value;
			return base.ConvertTo(context, culture, value, destinationType);
		}

		public static MethodInfo GetTryParseMethod(Type typeWithMethods)
		{
			MethodInfo method = typeWithMethods.GetMethod("TryParse", BindingFlags.Static | BindingFlags.Public);
			if (method == null) return null;
			if (method.ReturnType != typeof(bool)) return null;
			return method;
		}
	}

	public class BasicTypeConverter : TypeConverter
	{

		public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
		{
			if (destinationType == typeof(InstanceDescriptor))
				return true;
			return base.CanConvertTo(context, destinationType);
		}

		public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
		{
			if (destinationType == typeof(InstanceDescriptor))
			{
				return new InstanceDescriptor(value.GetType().GetConstructor(new Type[] { }), null, false);
			}
			return base.ConvertTo(context, culture, value, destinationType);
		}

		public override bool GetPropertiesSupported(ITypeDescriptorContext context)
		{
			return true;
		}

		public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object value, Attribute[] attributes)
		{
			return TypeDescriptor.GetProperties(value);
		}
	}
}
