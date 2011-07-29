using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Reflection;

namespace Direct3DLib
{

	public class GenericTypeConverter<T> : TypeConverter
	{
		public override bool GetPropertiesSupported(ITypeDescriptorContext context)
		{
			return true;
		}

		public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object value, Attribute[] attributes)
		{
			return TypeDescriptor.GetProperties(typeof(T));
		}

		public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
		{
			if (sourceType.Equals(typeof(T)))
				return true;
			if (sourceType.Equals(typeof(string)))
			{
				MethodInfo parseMethod = GetTryParseMethod(typeof(T));
				if(parseMethod != null)
					return true;
			}
			return false;
		}

		public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
		{
			if(!CanConvertFrom(context,value.GetType())) return base.ConvertFrom(context,culture,value);
			if (value.GetType() == typeof(T))
				return value;
			T t = default(T);
			if (value.GetType() == typeof(string))
			{
				MethodInfo tryParse = GetTryParseMethod(typeof(T)); 
				//System.Windows.Forms.MessageBox.Show("Can convert "+value+" to "+typeof(T)+"?");
				object [] paras = new object[]{value,t};
				bool parseSucceeded = (bool)tryParse.Invoke(null, paras);
				if (parseSucceeded)
					return paras[1];
			}
			
			throw new ArgumentException("Could not convert '" + value + "' to type '" + typeof(T) + "'");
		}


		public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
		{
			if (destinationType == typeof(T)) return true;
			if (destinationType == typeof(string)) return true;
			return false;
		}

		public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
		{
			if(destinationType == typeof(T))
				return value;
			if (destinationType == typeof(string))
				return value.ToString();
			throw new ArgumentException("Could not convert ("+typeof(T)+") '" + value + "' to type '" + destinationType + "'");
		}

		public static MethodInfo GetTryParseMethod(Type typeWithMethods)
		{
			MethodInfo method = typeWithMethods.GetMethod("TryParse", BindingFlags.Static | BindingFlags.Public);
			//System.Windows.Forms.MessageBox.Show("found a method to convert " + typeWithMethods + " : " + method);
			if (method == null) return null;
			//Console.WriteLine("got a method!" + method);
			if (method.ReturnType != typeof(bool)) return null;
			return method;
		}
	}
}
