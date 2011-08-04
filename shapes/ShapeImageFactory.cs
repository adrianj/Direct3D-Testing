using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace Direct3DLib
{
	public class ShapeImageFactory
	{
		public static Shape CreateFromFile(string filename)
		{
			Image image = Bitmap.FromFile(filename);
			throw new NotImplementedException();
		}
	}
}
