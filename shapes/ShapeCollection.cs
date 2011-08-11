using System;

namespace Direct3DLib
{
	public class ShapeCollectionEditor : ObjectCollectionEditor
	{
		public ShapeCollectionEditor(Type type) : base(type) { }
		protected override Type[] GetTypes()
		{
			return new Type[] { typeof(ComplexShape), typeof(Pipe), typeof(ClosedPipe), typeof(Sphere) };
		}
	}

}


