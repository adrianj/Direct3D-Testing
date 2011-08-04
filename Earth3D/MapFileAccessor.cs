using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Drawing;

namespace Direct3DLib
{
	public class MapFileAccessor
	{
		public Image GetImage(MapDescriptor descriptor)
		{
			string filename = descriptor.CalculateFilename();
			if (File.Exists(filename))
			{
				descriptor.MapState = MapDescriptor.MapImageState.Correct;
				return Bitmap.FromFile(filename);
			}
			descriptor.MapState = MapDescriptor.MapImageState.Empty;
			return null;
		}

	}
}
