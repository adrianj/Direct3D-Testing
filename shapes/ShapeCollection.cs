using System;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.ComponentModel.Design.Serialization;
using System.Windows.Forms;
using System.Globalization;

namespace Direct3DLib
{
	public class ShapeCollectionEditor : CollectionEditor
	{
		private Type[] types;
		public ShapeCollectionEditor(Type type)
			: base(type)
		{
			types = new Type[] {typeof(ComplexShape), typeof(Pipe), typeof(ClosedPipe), typeof(Sphere), };
		}

		protected override Type[] CreateNewItemTypes()
		{
			return types;
		}

		protected override object CreateInstance(Type itemType)
		{
			MessageBox.Show("Creating a new " + itemType);

			return Activator.CreateInstance(itemType);
		}

		protected override void DestroyInstance(object instance)
		{
			MessageBox.Show("disposing " + instance);
			if (instance is IDisposable)
			{
				IDisposable s = instance as IDisposable;
				s.Dispose();
			}
			base.DestroyInstance(instance);
		}

	}
}


